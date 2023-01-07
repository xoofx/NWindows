// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using NWindows.Input;
using NWindows.Threading.Events;

namespace NWindows.Threading;

/// <summary>
/// The dispatcher provides the infrastructure to manage objects with thread affinity.
/// </summary>
public abstract partial class Dispatcher
{
    private bool _hasShutdownStarted;
    private bool _hasShutdownFinished;
    private readonly IdleEvent _idleEvent;

    /// <summary>
    /// Shared synchronization context for a dispatcher
    /// </summary>
    protected readonly DispatcherSynchronizationContext DispatcherSynchronizationContext;

    // We use a fast static instance for the current dispatcher 
    // We check that we are on the same thread otherwise we override it
    [ThreadStatic]
    private static Dispatcher? _tlsCurrentDispatcher; // TODO: we might want to optimize it with a static field as it is done in WPF

    // internal list storing all dispatchers
    private static readonly List<WeakReference<Dispatcher>> Dispatchers = new();

    /// <summary>
    /// Set to true to allow per platform debugging
    /// </summary>
    internal bool DebugInternal;

    /// <summary>
    /// The writer used to log debug messages
    /// </summary>
    internal TextWriter? DebugOutputInternal;

    protected Dispatcher(Thread thread)
    {
        Thread = thread;
        _taskScheduler = new DispatcherTaskScheduler(this);
        _queue = new ConcurrentQueue<DispatcherJob>();
        _frames = new List<DispatcherFrame>();

        DispatcherSynchronizationContext = new DispatcherSynchronizationContext(this);

        // Pre-allocate events
        _idleEvent = new IdleEvent();
        _defaultUnhandledExceptionFilterEvent = new UnhandledExceptionFilterEvent();
        _defaultUnhandledExceptionEvent = new UnhandledExceptionEvent();

        // Make sure that the TLS dispatcher is set
        _tlsCurrentDispatcher = this;

        Events = new DispatcherEventHub(this);
    }

    /// <summary>
    /// Gets the event hub for this dispatcher.
    /// </summary>
    public DispatcherEventHub Events { get; }
    
    /// <summary>
    /// Gets the current dispatcher.
    /// </summary>
    /// <value>The current dispatcher.</value>
    /// <remarks>
    /// The current dispatcher is attached to the current thread.
    /// </remarks>
    public static Dispatcher Current => _tlsCurrentDispatcher ?? FromThread(Thread.CurrentThread);

    /// <summary>
    /// Gets the dispatcher from the specified thread or create one if not it does not exist.
    /// </summary>
    /// <param name="thread">The thread.</param>
    /// <returns>Dispatcher.</returns>
    public static Dispatcher FromThread(Thread thread)
    {
        Dispatcher? dispatcher = null;
        lock (Dispatchers)
        {
            for (int i = 0; i < Dispatchers.Count; i++)
            {
                var weakref = Dispatchers[i];
                if (weakref.TryGetTarget(out dispatcher))
                {
                    if (dispatcher.Thread == thread)
                    {
                        break;
                    }
                    else
                    {
                        dispatcher = null;
                    }
                }
                else
                {
                    Dispatchers.RemoveAt(i);
                    i--;
                }
            }

            if (dispatcher == null)
            {
                dispatcher = CreateDispatcher(thread);
                Dispatchers.Add(new WeakReference<Dispatcher>(dispatcher));
            }
        }
        return dispatcher;
    }

    private static Dispatcher CreateDispatcher(Thread thread)
    {
        if (OperatingSystem.IsWindows())
        {
            return new Interop.Win32.Win32Dispatcher(thread);
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Gets the thread this dispatcher is attached to.
    /// </summary>
    public Thread Thread { get; }

    public bool HasShutdownStarted => _hasShutdownStarted;

    public bool HasShutdownFinished => _hasShutdownFinished;
    
    public bool EnableDebug
    {
        get
        {
            VerifyAccess();
            return DebugInternal;
        }
        set
        {
            VerifyAccess();
            DebugInternal = value;
        }
    }

    public TextWriter? DebugOutput
    {
        get
        {
            VerifyAccess();
            return DebugOutputInternal;
        }
        set
        {
            VerifyAccess();
            DebugOutputInternal = value;
        }
    }

    /// <summary>
    /// Checks that the current thread owns this dispatcher.
    /// </summary>
    /// <returns><c>true</c> if the current thread owns this dispatcher, <c>false</c> otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess()
    {
        return Thread == Thread.CurrentThread;
    }

    /// <summary>
    /// Verifies that the current thread owns this dispatcher or throw an exception if not.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void VerifyAccess()
    {
        if (!CheckAccess())
        {
            ThrowInvalidAccess();
        }
    }

    public void Shutdown()
    {
        VerifyAccess();

        PostQuitToMessageLoop();

        Events.OnDispatcherEvent(ShutdownEvent.ShutdownStarted);
        _hasShutdownStarted = true;
    }

    public void Run()
    {
        VerifyAccess();
        PushFrameImpl(new DispatcherFrame(this));

        Events.OnDispatcherEvent(ShutdownEvent.ShutdownFinished);
        _hasShutdownFinished = true;
    }

    public void PushFrame(DispatcherFrame frame)
    {
        VerifyAccess();
        VerifyRunning();
        PushFrameImpl(frame);
    }

    private void PushFrameImpl(DispatcherFrame frame)
    {
        if (_hasShutdownStarted)
        {
            throw new InvalidOperationException("Can't push a new frame. The shutdown has been already started");
        }

        if (frame.Dispatcher != this)
        {
            throw new InvalidOperationException("Dispatcher frame is not attached to this dispatcher");
        }

        var frameCount = _frames.Count;
        _frames.Add(frame);
        try
        {
            frame.EnterInternal();

            bool blockOnWait = true;
            while (!_hasShutdownStarted && frame.Continue)
            {
                // If any frame already running are asking to shutdown, we need to close this running frame as well
                // For example, a modal window pushed with WindowModalFrame should force the exit of the frame and any subsequent frame
                for (int i = 0; i < frameCount; i++)
                {
                    if (!_frames[i].Continue)
                    {
                        frame.Continue = false;
                        break;
                    }
                }

                if (!frame.Continue)
                {
                    break;
                }

                WaitAndDispatchMessage(blockOnWait);

                // Handle idle
                // Reset the state of the idle
                _idleEvent.Continuous = false;
                Events.OnDispatcherEvent(_idleEvent);
                blockOnWait = !_idleEvent.Continuous;
            }
        }
        finally
        {
            Debug.Assert(_frames.Count > 0);
            _frames.RemoveAt(_frames.Count - 1);
            frame.LeaveInternal();
        }
    }
   
    internal DispatcherFrame? CurrentFrame => _frames.Count > 0 ? _frames[_frames.Count - 1] : null;

    internal abstract void WaitAndDispatchMessage(bool blockOnWait);

    internal abstract void CreateOrResetTimer(DispatcherTimer timer, int millis);

    internal abstract void DestroyTimer(DispatcherTimer timer);
    
    internal abstract void PostQuitToMessageLoop();

    internal abstract IScreenManager ScreenManager { get; }

    internal abstract InputManager InputManager { get; }
    
    [DoesNotReturn]
    private static void ThrowInvalidAccess()
    {
        throw new InvalidOperationException("Invalid Access from this thread. This object must be accessed from the UI main Thread");
    }
}
