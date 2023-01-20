// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace NWindows.Threading;

/// <summary>
/// A timer class that allows to received timer events from a <see cref="Dispatcher"/>.
/// </summary>
public class DispatcherTimer
{
    private readonly object _timerLock;
    private TimeSpan _interval;
    private bool _isEnabled;

    /// <summary>
    /// Creates a new instance of this timer.
    /// </summary>
    public DispatcherTimer() : this(Dispatcher.Current)
    {
    }

    /// <summary>
    /// Creates a new instance of this timer with the specified dispatcher.
    /// </summary>
    public DispatcherTimer(Dispatcher dispatcher) : this(TimeSpan.Zero, dispatcher)
    {
    }

    /// <summary>
    /// Creates a new instance of this timer with the specified internal, dispatcher and callback.
    /// </summary>
    public DispatcherTimer(TimeSpan interval, Dispatcher dispatcher, EventHandler? callback = null) :
        this(dispatcher, callback, interval)
    {
    }

    private DispatcherTimer(Dispatcher dispatcher, EventHandler? callback = null, TimeSpan? interval = null)
    {
        _timerLock = new object();
        Dispatcher = dispatcher;

        if (interval.HasValue)
        {
            _interval = interval.Value;
        }

        if (callback != null)
        {
            Tick += callback;
            Start();
        }
    }

    /// <summary>
    /// The associated dispatcher.
    /// </summary>
    public Dispatcher Dispatcher { get; private set; }

    /// <summary>
    /// Gets or sets a boolean to enable or disable this timer.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;

        set
        {
            lock (_timerLock)
            {
                if (value && !_isEnabled)
                {
                    Start();
                }
                else if (!value && _isEnabled)
                {
                    Stop();
                }
            }
        }
    }

    /// <summary>
    /// An event handler called when the timer is ticking.
    /// </summary>

    public event EventHandler? Tick;

    /// <summary>
    /// Gets or sets an object associated with this timer.
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Gets or sets the internal of this timer.
    /// </summary>
    public TimeSpan Interval
    {
        get => _interval;

        set
        {
            lock (_timerLock)
            {
                _interval = value;

                if (_isEnabled)
                {
                    CreateOrReset();
                }
            }
        }
    }

    /// <summary>
    /// Resets this timer.
    /// </summary>
    public void Reset()
    {
        lock (_timerLock)
        {
            if (_isEnabled)
            {
                CreateOrReset();
            }
        }
    }

    /// <summary>
    /// Starts this timer.
    /// </summary>
    public void Start()
    {
        lock (_timerLock)
        {
            if (!_isEnabled)
            {
                CreateOrReset();
                _isEnabled = true;
            }
        }
    }

    /// <summary>
    /// Stops this timer.
    /// </summary>
    public void Stop()
    {
        lock (_timerLock)
        {
            if (_isEnabled)
            {
                _isEnabled = false;
                if (Dispatcher.CheckAccess())
                {
                    Dispatcher.DestroyTimer(this);
                }
                else
                {
                    Dispatcher.Invoke(() => Dispatcher.DestroyTimer(this));
                }
            }
        }
    }

    internal void OnTick()
    {
        if (_isEnabled)
        {
            Tick?.Invoke(this, EventArgs.Empty);
        }
    }

    private void CreateOrReset()
    {
        var timeMillis = CalculateMilliseconds();
        if (Dispatcher.CheckAccess())
        {
            Dispatcher.CreateOrResetTimer(this, timeMillis);
        }
        else
        {
            Dispatcher.Invoke(() => Dispatcher.CreateOrResetTimer(this, timeMillis));
        }
    }

    private int CalculateMilliseconds()
    {
        var totalMillis = _interval.TotalMilliseconds;
        if (totalMillis < 0) return 0;
        if (totalMillis > int.MaxValue) return int.MaxValue;
        return (int)totalMillis;
    }
}