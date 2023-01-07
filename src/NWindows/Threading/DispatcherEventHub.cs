// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Threading.Events;

namespace NWindows.Threading;

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

    public delegate void DispatcherEventHandler(Dispatcher dispatcher, DispatcherEvent evt);

    public delegate void IdleEventHandler(Dispatcher dispatcher, IdleEvent evt);

    public delegate void UnhandledExceptionFilterEventHandler(Dispatcher dispatcher, UnhandledExceptionFilterEvent evt);

    public delegate void UnhandledExceptionEventHandler(Dispatcher dispatcher, UnhandledExceptionEvent evt);

    public delegate void ShutdownEventHandler(Dispatcher dispatcher, ShutdownEvent evt);
}