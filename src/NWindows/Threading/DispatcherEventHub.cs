// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Threading.Events;

namespace NWindows.Threading;

/// <summary>
/// This class provides event handlers for the various events published by a <see cref="Dispatcher"/>.
/// </summary>
public sealed class DispatcherEventHub : DispatcherObject
{
    private DispatcherEventHandler? _all;
    private IdleEventHandler? _idle;
    private UnhandledExceptionFilterEventHandler? _unhandledExceptionFilter;
    private UnhandledExceptionEventHandler? _unhandledException;
    private ShutdownEventHandler? _shutdownStarted;
    private ShutdownEventHandler? _shutdownFinished;

    internal DispatcherEventHub(Dispatcher dispatcher) : base(dispatcher)
    {
    }

    /// <summary>
    /// Adds or removes an event handler for all Dispatcher events published.
    /// </summary>
    public event DispatcherEventHandler All
    {
        add
        {
            VerifyAccess();
            _all += value;
        }
        remove
        {
            VerifyAccess();
            _all -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for idle events published.
    /// </summary>
    public event IdleEventHandler Idle
    {
        add
        {
            VerifyAccess();
            _idle += value;
        }
        remove
        {
            VerifyAccess();
            _idle -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for shutdown started events published.
    /// </summary>
    public event ShutdownEventHandler ShutdownStarted
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

    /// <summary>
    /// Adds or removes an event handler for shutdown finished events published.
    /// </summary>
    public event ShutdownEventHandler ShutdownFinished
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
    /// Adds or removes an event handler for filtering unhandled exceptions.
    /// </summary>
    public event UnhandledExceptionFilterEventHandler UnhandledExceptionFilter
    {
        add
        {
            VerifyAccess();
            _unhandledExceptionFilter += value;
        }
        remove
        {
            VerifyAccess();
            _unhandledExceptionFilter -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for handling unhandled exceptions.
    /// </summary>
    public event UnhandledExceptionEventHandler UnhandledException
    {
        add
        {
            VerifyAccess();
            _unhandledException += value;
        }
        remove
        {
            VerifyAccess();
            _unhandledException -= value;
        }
    }

    /// <summary>
    /// Clears all the event handlers.
    /// </summary>
    public void Clear()
    {
        VerifyAccess();
        _all = null;
        _idle = null;
        _shutdownStarted = null;
        _shutdownFinished = null;
        _unhandledExceptionFilter = null;
        _unhandledException = null;
    }

    internal void OnDispatcherEvent(DispatcherEvent evt)
    {
        _all?.Invoke(Dispatcher, evt);
        switch (evt.Kind)
        {
            case DispatcherEventKind.Idle:
                _idle?.Invoke(Dispatcher, (IdleEvent)evt);
                break;
            case DispatcherEventKind.ShutdownStarted:
                _shutdownStarted?.Invoke(Dispatcher, (ShutdownEvent)evt);
                break;
            case DispatcherEventKind.ShutdownFinished:
                _shutdownFinished?.Invoke(Dispatcher, (ShutdownEvent)evt);
                break;
            case DispatcherEventKind.UnhandledException:
                _unhandledException?.Invoke(Dispatcher, (UnhandledExceptionEvent)evt);
                break;
            case DispatcherEventKind.UnhandledExceptionFilter:
                _unhandledExceptionFilter?.Invoke(Dispatcher, (UnhandledExceptionFilterEvent)evt);
                break;
        }
    }

    /// <summary>
    /// Handle for dispatcher events.
    /// </summary>
    /// <param name="dispatcher">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void DispatcherEventHandler(Dispatcher dispatcher, DispatcherEvent evt);

    /// <summary>
    /// Handle for idle events.
    /// </summary>
    /// <param name="dispatcher">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void IdleEventHandler(Dispatcher dispatcher, IdleEvent evt);

    /// <summary>
    /// Handle for filtering unhandled exception events.
    /// </summary>
    /// <param name="dispatcher">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void UnhandledExceptionFilterEventHandler(Dispatcher dispatcher, UnhandledExceptionFilterEvent evt);

    /// <summary>
    /// Handle for unhandled exception handler events.
    /// </summary>
    /// <param name="dispatcher">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void UnhandledExceptionEventHandler(Dispatcher dispatcher, UnhandledExceptionEvent evt);

    /// <summary>
    /// Handle for shutdown events events.
    /// </summary>
    /// <param name="dispatcher">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void ShutdownEventHandler(Dispatcher dispatcher, ShutdownEvent evt);
}