using System;
using System.Runtime.ExceptionServices;

namespace NWindows.Threading;

public class DispatcherFrame : DispatcherObject
{
    private readonly bool _exitOnShutdown;
    private bool _continue;
    private bool _entered;
    private ExceptionDispatchInfo? _pendingException;

    public DispatcherFrame(Dispatcher dispatcher) : this(dispatcher, true)
    {
    }

    public DispatcherFrame(Dispatcher dispatcher, bool exitOnShutdown) : base(dispatcher)
    {
        _exitOnShutdown = exitOnShutdown;
        _continue = true;
    }

    public bool Continue
    {
        get
        {
            // We can continue if we don't have any exceptions
            bool shouldContinue = _continue && _pendingException is null;
            if (shouldContinue)
            {
                if (_exitOnShutdown)
                {
                    if (Dispatcher.HasShutdownStarted)
                    {
                        shouldContinue = false;
                    }
                }
            }

            return shouldContinue;
        }

        set
        {
            _continue = value;
            Dispatcher.NotifyJobQueue();
        }
    }

    internal void CaptureException(ExceptionDispatchInfo exceptionDispatchInfo)
    {
        _pendingException = exceptionDispatchInfo;
    }
    
    internal void EnterInternal()
    {
        if (_entered)
        {
            throw new InvalidOperationException("This frame has been already pushed to a Dispatcher");
        }
        _entered = true;
        Enter();
    }

    internal void LeaveInternal()
    {
        Leave();
        var pendingException = _pendingException;
        // Reset all variables (in case of reusing a frame)
        _entered = false;
        _pendingException = null;
        _continue = true;
        pendingException?.Throw();
    }

    protected virtual void Enter()
    {
    }
    protected virtual void Leave()
    {
    }
}