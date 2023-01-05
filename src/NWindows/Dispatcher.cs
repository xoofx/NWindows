// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using NWindows.Input;
using NWindows.Win32;

namespace NWindows;

/// <summary>
/// The dispatcher provides the infrastructure to manage objects with thread affinity.
/// </summary>
public abstract partial class Dispatcher
{
    private bool _hasShutdownStarted;
    private bool _hasShutdownFinished;

    /// <summary>
    /// Shared synchronization context for a dispatcher
    /// </summary>
    protected readonly DispatcherSynchronizationContext DispatcherSynchronizationContext;

    // ReSharper disable once InconsistentNaming
#pragma warning disable IDE1006
    private event EventHandler? _shutdownStarted;
    // ReSharper disable once InconsistentNaming
    private event EventHandler? _shutdownFinished;
#pragma warning restore IDE1006

    // We use a fast static instance for the current dispatcher 
    // We check that we are on the same thread otherwise we override it
    [ThreadStatic]
    private static Dispatcher? _tlsCurrentDispatcher; // TODO: we might want to optimize it with a static field as it is done in WPF

    // internal list storing all dispatchers
    private static readonly List<WeakReference<Dispatcher>> Dispatchers = new();

    protected Dispatcher(Thread thread)
    {
        Thread = thread;
        _taskScheduler = new DispatcherTaskScheduler(this);
        _queue = new ConcurrentQueue<DispatcherJob>();
        _frames = new List<DispatcherFrame>();

        DispatcherSynchronizationContext = new DispatcherSynchronizationContext(this);

        // Pre-allocate exception args
        _defaultDispatcherUnhandledExceptionFilterEventArgs = new DispatcherUnhandledExceptionFilterEventArgs(this);
        _defaultDispatcherUnhandledExceptionEventArgs = new DispatcherUnhandledExceptionEventArgs(this);

        // Make sure that the TLS dispatcher is set
        _tlsCurrentDispatcher = this;
    }

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
            return new Win32Dispatcher(thread);
        }

        throw new PlatformNotSupportedException();
    }

    /// <summary>
    /// Gets the thread this dispatcher is attached to.
    /// </summary>
    public Thread Thread { get; }

    public bool HasShutdownStarted => _hasShutdownStarted;

    public bool HasShutdownFinished => _hasShutdownFinished;

    public event EventHandler ShutdownStarted
    {
        add
        {
            VerifyAccess();
            _shutdownStarted += value;
        }
        remove
        {
            VerifyAccess();
            _shutdownStarted -= value;
        }
    }

    public event EventHandler ShutdownFinished
    {
        add
        {
            VerifyAccess();
            _shutdownFinished += value;
        }
        remove
        {
            VerifyAccess();
            _shutdownFinished -= value;
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

        RequestShutdown();
        
        _shutdownStarted?.Invoke(this, EventArgs.Empty);
        _hasShutdownStarted = true;
    }

    internal abstract void CreateOrResetTimer(DispatcherTimer timer, int millis);

    internal abstract void DestroyTimer(DispatcherTimer timer);
    
    protected abstract void RequestShutdown();

    protected abstract void RunMessageLoop(Window? window);

    internal abstract IScreenManager ScreenManager { get; }

    internal abstract InputManager InputManager { get; }
    
    [DoesNotReturn]
    private static void ThrowInvalidAccess()
    {
        throw new InvalidOperationException("Invalid Access from this thread. This object must be accessed from the UI main Thread");
    }
}
