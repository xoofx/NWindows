// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NWindows.Threading;

/// <summary>
/// The dispatcher provides the infrastructure to manage objects with thread affinity.
/// </summary>
public abstract partial class Dispatcher
{
    private readonly DispatcherTaskScheduler _taskScheduler;
    private readonly ConcurrentQueue<DispatcherJob> _queue;
    private readonly List<DispatcherFrame> _frames;

    private readonly DispatcherUnhandledExceptionFilterEventArgs _defaultDispatcherUnhandledExceptionFilterEventArgs;
    private readonly DispatcherUnhandledExceptionEventArgs _defaultDispatcherUnhandledExceptionEventArgs;

    private event EventHandler<DispatcherUnhandledExceptionEventArgs>? _unhandledException;
    private event EventHandler<DispatcherUnhandledExceptionFilterEventArgs>? _unhandledExceptionFilter;

    public event EventHandler<DispatcherUnhandledExceptionEventArgs> UnhandledException
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

    public event EventHandler<DispatcherUnhandledExceptionFilterEventArgs> UnhandledExceptionFilter
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

    public void Run()
    {
        VerifyAccess();
        PushFrameImpl(new DispatcherFrame(this));

        _shutdownFinished?.Invoke(this, EventArgs.Empty);
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

        _frames.Add(frame);
        try
        {
            frame.EnterInternal();

            while (!_hasShutdownStarted)
            {
                // If the current frame request to exit, we exit immediately from the frame
                if (!frame.Continue || !WaitAndDispatchMessage())
                {
                    break;
                }
                // handle shutdown
                // handle queue
                // handle timer
                // handle idle
            }
        }
        finally
        {
            Debug.Assert(_frames.Count > 0);
            _frames.RemoveAt(_frames.Count - 1);
            frame.LeaveInternal();
        }
    }

    internal abstract bool WaitAndDispatchMessage();

    private void VerifyRunning()
    {
        if (_frames.Count > 0)
        {
            throw new InvalidOperationException("Hub is not running");
        }
    }
    
    public void Invoke(Action action, CancellationToken? cancellationToken = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (CheckAccess())
        {
            if (cancellationToken is null)
            {
                action();
            }
            else
            {
                // No need to wait for task, as the frame will close as soon as the task is completed
                var task = new Task(action, cancellationToken ?? CancellationToken.None);
                var frame = new TaskDispatcherFrame(this, task, _taskScheduler);
                PushFrame(frame);
            }
        }
        else
        {
            var task = InvokeAsync(action, cancellationToken);
            task.Wait();
        }
    }

    public T Invoke<T>(Func<T> func, CancellationToken? cancellationToken = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));
        Task<T> task;
        if (CheckAccess())
        {
            if (cancellationToken is null)
            {
                return func();
            }
            else
            {
                // No need to wait for task, as the frame will close as soon as the task is completed
                task = new Task<T>(func, cancellationToken ?? CancellationToken.None);
                var frame = new TaskDispatcherFrame(this, task, _taskScheduler);
                PushFrame(frame);
            }
        }
        else
        {
            task = InvokeAsync(func, cancellationToken);
            task.Wait();
        }
        return task.Result;
    }

    public void InvokeAsyncAndForget(Action action, CancellationToken? cancellationToken = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        QueueDispatcherJob(action, cancellationToken);
    }

    public Task InvokeAsync(Action action, CancellationToken? cancellationToken = null)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        var task = new Task(action, cancellationToken ?? CancellationToken.None);
        task.Start(_taskScheduler);

        return task;
    }

    public Task<T> InvokeAsync<T>(Func<T> func, CancellationToken? cancellationToken = null)
    {
        if (func == null) throw new ArgumentNullException(nameof(func));

        var task = new Task<T>(func, cancellationToken ?? CancellationToken.None);
        task.Start(_taskScheduler);

        return task;
    }

    private void QueueDispatcherJob(object action, CancellationToken? cancelToken = null)
    {

        var job = action is Task ? new DispatcherJob((Task)action) : new DispatcherJob((Action)action, cancelToken ?? CancellationToken.None);

        _queue.Enqueue(job);

        NotifyJobQueue();
    }


    internal void ProcessJobQueue()
    {
        var hasStillJobs = _queue.TryDequeue(out var job);

        if (job.Callback != null)
        {
            try
            {
                switch (job.Callback)
                {
                    case Task task:
                        _taskScheduler.ExecuteTask(task);
                        break;

                    case Action action:
                    {
                        if (!job.CancellationToken.IsCancellationRequested)
                        {
                            action();
                        }

                        break;
                    }
                }
            }
            catch (Exception ex) when (FilterException(ex))
            {
                if (!HandleException(ex))
                {
                    throw;
                }
            }
        }

        if (hasStillJobs)
        {
            NotifyJobQueue();
        }
    }
    
    private bool HandleException(Exception exception)
    {
        var unhandled = _unhandledException;
        if (unhandled != null)
        {
            _defaultDispatcherUnhandledExceptionEventArgs.Exception = exception;
            _defaultDispatcherUnhandledExceptionEventArgs.Handled = false;
            unhandled(this, _defaultDispatcherUnhandledExceptionEventArgs);
            return _defaultDispatcherUnhandledExceptionEventArgs.Handled;
        }
        return false;
    }

    private bool FilterException(Exception exception)
    {
        var unhandledFilter = _unhandledExceptionFilter;

        if (unhandledFilter != null)
        {
            _defaultDispatcherUnhandledExceptionFilterEventArgs.Exception = exception;
            _defaultDispatcherUnhandledExceptionFilterEventArgs.RequestCatch = false;
            unhandledFilter(this, _defaultDispatcherUnhandledExceptionFilterEventArgs);
            return _defaultDispatcherUnhandledExceptionFilterEventArgs.RequestCatch;
        }

        return true;
    }

    internal abstract void NotifyJobQueue();

    private class DispatcherTaskScheduler : TaskScheduler
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherTaskScheduler(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return Enumerable.Empty<Task>();
        }

        public void ExecuteTask(Task task)
        {
            TryExecuteTask(task);
        }

        protected override void QueueTask(Task task)
        {
            _dispatcher.QueueDispatcherJob(task);
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!taskWasPreviouslyQueued && _dispatcher.Thread == Thread.CurrentThread)
            {
                return TryExecuteTask(task);
            }

            return false;
        }
    }

    internal readonly struct DispatcherJob
    {
        public DispatcherJob(Task task)
        {
            Callback = task;
            CancellationToken = CancellationToken.None;
        }

        public DispatcherJob(Action action, CancellationToken cancellationToken)
        {
            Callback = action;
            CancellationToken = cancellationToken;
        }

        public readonly object Callback;

        public readonly CancellationToken CancellationToken;
    }

    private class TaskDispatcherFrame : DispatcherFrame
    {
        private readonly Task _task;
        private readonly TaskScheduler _scheduler;

        public TaskDispatcherFrame(Dispatcher hub, Task task, TaskScheduler scheduler) : base(hub)
        {
            _task = task;
            _scheduler = scheduler;
        }

        private void ContinuationAction(Task task)
        {
            Continue = false;
        }

        protected override void Enter()
        {
            var continuationTask = _task.ContinueWith(ContinuationAction, default, TaskContinuationOptions.AttachedToParent, _scheduler);
            _task.Start(_scheduler);
        }
    }
}


public sealed class DispatcherUnhandledExceptionFilterEventArgs : DispatcherEventArgs
{
    internal DispatcherUnhandledExceptionFilterEventArgs(Dispatcher dispatcher)
        : base(dispatcher)
    {
    }

    public Exception Exception { get; internal set; }

    public bool RequestCatch { get; set; }
}

public class DispatcherEventArgs : EventArgs
{
    internal DispatcherEventArgs(Dispatcher dispatcher)
    {
        Debug.Assert(dispatcher != null);
        Dispatcher = dispatcher;
    }

    public Dispatcher Dispatcher { get; }
}

public sealed class DispatcherUnhandledExceptionEventArgs : DispatcherEventArgs
{
    internal DispatcherUnhandledExceptionEventArgs(Dispatcher dispatcher)
        : base(dispatcher)
    {
    }

    public Exception Exception { get; internal set; }

    public bool Handled { get; set; }
}