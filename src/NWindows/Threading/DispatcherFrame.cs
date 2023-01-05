using System;

namespace NWindows.Threading;

public class DispatcherFrame : DispatcherObject
{
    private readonly bool _exitOnShutdown;
    private bool _continue;
    private bool _entered;

    public DispatcherFrame(Dispatcher dispatcher) : this(dispatcher, true)
    {
    }

    public DispatcherFrame(Dispatcher hub, bool exitOnShutdown) : base(hub)
    {
        _exitOnShutdown = exitOnShutdown;
        _continue = true;
    }

    public bool Continue
    {
        get
        {
            bool shouldContinue = _continue;
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
        _entered = false;
    }

    protected virtual void Enter()
    {
    }
    protected virtual void Leave()
    {
    }
}