// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System;

namespace NWindows;

public class DispatcherTimer
{
    private readonly object _timerLock;
    private TimeSpan _interval;
    private bool _isEnabled;

    public DispatcherTimer() : this(Dispatcher.Current)
    {
    }

    public DispatcherTimer(Dispatcher dispatcher) : this(TimeSpan.Zero, dispatcher)
    {
    }

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

    public Dispatcher Dispatcher { get; private set; }

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

    public event EventHandler? Tick;

    public object? Tag { get; set; }

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