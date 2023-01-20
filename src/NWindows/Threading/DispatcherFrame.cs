using System;
using System.Runtime.ExceptionServices;

namespace NWindows.Threading;

/// <summary>
/// A frame used to implement a message loop in a <see cref="Dispatcher"/>.
/// </summary>
public class DispatcherFrame : DispatcherObject
{
    private readonly bool _exitOnShutdown;
    private bool _continue;
    private bool _entered;
    private ExceptionDispatchInfo? _pendingException;

    /// <summary>
    /// Creates a new instance of this frame.
    /// </summary>
    /// <param name="dispatcher">The associated dispatcher.</param>
    public DispatcherFrame(Dispatcher dispatcher) : this(dispatcher, true)
    {
    }

    /// <summary>
    /// Creates a new instance of this frame.
    /// </summary>
    /// <param name="dispatcher">The associated dispatcher.</param>
    /// <param name="exitOnShutdown"><c>true</c> to force exit when this frame is finishing.</param>
    public DispatcherFrame(Dispatcher dispatcher, bool exitOnShutdown) : base(dispatcher)
    {
        _exitOnShutdown = exitOnShutdown;
        _continue = true;
    }

    /// <summary>
    /// Gets or sets a boolean indicating that this frame should continue the loop.
    /// </summary>
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

    /// <summary>
    /// A callback called when entering the frame.
    /// </summary>
    protected virtual void Enter()
    {
    }

    /// <summary>
    /// A callback called when leaving the frame.
    /// </summary>
    protected virtual void Leave()
    {
    }
}