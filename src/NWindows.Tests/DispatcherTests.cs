// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Runtime.CompilerServices;
using NUnit.Framework;
using NWindows.Threading;

namespace NWindows.Tests;

public class DispatcherTests
{
    [Test, RequiresThread]
    public void TestShutdown()
    {
        // Run Twice the shutdown sequence to check that Reset is working correctly
        bool shutdownStarted = false;
        bool shutdownFinished = false;
        bool idleCalled = false;
        Dispatcher.Current.Events.ShutdownStarted += (dispatcher, evt) =>
        {
            Assert.True(idleCalled);
            Assert.False(shutdownStarted);
            shutdownStarted = true;
        };
        Dispatcher.Current.Events.ShutdownFinished += (dispatcher, evt) =>
        {
            Assert.True(idleCalled);
            Assert.False(shutdownFinished);
            shutdownFinished = true;
        };

        Dispatcher.Current.Events.Idle += (dispatcher, evt) =>
        {
            idleCalled = true;
            Dispatcher.Current.Shutdown();
            evt.SkipWaitForNextMessage = true;
        };

        SetupTimeOut();
        Dispatcher.Current.Run();

        Assert.True(Dispatcher.Current.HasShutdownStarted);
    }

    [Test, RequiresThread]
    public void TestExceptionCatchNoHandle() => RunExceptionOnIdle(true, false);

    [Test, RequiresThread]
    public void TestExceptionCatchHandle() => RunExceptionOnIdle(true, true);

    [Test, RequiresThread]
    public void TestExceptionNoCatchNoHandle() => RunExceptionOnIdle(false, false);

    private void RunExceptionOnIdle(bool catchException, bool handle)
    {
        Dispatcher.Current.Events.UnhandledExceptionFilter += (dispatcher, evt) =>
        {
            Assert.NotNull(evt.Exception);
            Assert.IsInstanceOf<ApplicationException>(evt.Exception);
            evt.RequestCatch = catchException;
        };

        if (handle)
        {
            Dispatcher.Current.Events.UnhandledException += (dispatcher, evt) =>
            {
                Assert.NotNull(evt.Exception);
                Assert.IsInstanceOf<ApplicationException>(evt.Exception);
                evt.Handled = true;
                dispatcher.Shutdown();
            };
        }

        Dispatcher.Current.Events.Idle += (dispatcher, evt) => throw new ApplicationException("Hello World");

        SetupTimeOut();
        if (!catchException || !handle)
        {
            Assert.Throws<ApplicationException>(() => Dispatcher.Current.Run());
        }
        else
        {
            Assert.DoesNotThrow(() => Dispatcher.Current.Run());
        }
    }

    [Test, RequiresThread]
    public void TestTimer()
    {
        //Dispatcher.Current.EnableDebug = true;
        //Dispatcher.Current.DebugOutput = TestContext.WriteLine;

        var timer = new DispatcherTimer();

        int tick = 0;
        timer.Tick += (sender, e) =>
        {
            tick++;
            if (tick == 10)
            {
                Dispatcher.Current.Shutdown();
            }
        };

        timer.Interval = TimeSpan.FromMilliseconds(10.0);
        timer.Start();

        Thread.Sleep(100);

        // Tick should only start with dispatching
        Assert.Zero(tick);

        // Start dispatching
        SetupTimeOut();
        Dispatcher.Current.Run();

        Assert.AreEqual(10, tick);
    }

    [Test, RequiresThread]
    public void TestTimerCatchNoHandle() => RunTimerTest(true, false);

    [Test, RequiresThread]
    public void TestTimerCatchHandle() => RunTimerTest(true, true);

    [Test, RequiresThread]
    public void TestTimerNoCatchNoHandle() => RunTimerTest(false, false);

    private void RunTimerTest(bool catchException, bool handle)
    {
        var timer = new DispatcherTimer();

        bool hasThrown = false;
        timer.Tick += (sender, e) =>
        {
            if (hasThrown)
            {
                Dispatcher.Current.Shutdown();
            }
            else
            {
                hasThrown = true;
                throw new ApplicationException("Hello from Timer");
            }
        };

        timer.Interval = TimeSpan.FromMilliseconds(10.0);
        timer.Start();


        Dispatcher.Current.Events.UnhandledExceptionFilter += (dispatcher, evt) => { evt.RequestCatch = catchException; };

        Dispatcher.Current.Events.UnhandledException += (dispatcher, evt) => { evt.Handled = handle; };

        SetupTimeOut();
        if (!catchException || !handle)
        {
            Assert.Throws<ApplicationException>(() => Dispatcher.Current.Run());
        }
        else
        {
            Assert.DoesNotThrow(() => Dispatcher.Current.Run());
        }
    }

    [Test, RequiresThread]
    public void TestInvokeFromSameThread()
    {
        int count = 0;
        Dispatcher.Current.Events.Idle += (dispatcher, evt) =>
        {
            Dispatcher.Current.Invoke(() => count++);
            Dispatcher.Current.Shutdown();
            evt.SkipWaitForNextMessage = true;
        };

        SetupTimeOut();
        Dispatcher.Current.Run();

        Assert.AreEqual(1, count);
    }

    [Test, RequiresThread]
    public void TestInvokeFromSeparateThread()
    {
        int count = 0;
        var dispatcher = Dispatcher.Current;
        var thread = new Thread(() => dispatcher.Invoke(() =>
        {
            count++;
            dispatcher.Shutdown();
        }))
        {
            IsBackground = true,
            Name = "Test thread for Dispatcher"
        };
        thread.Start();
        
        SetupTimeOut();
        Dispatcher.Current.Run();
        thread.Join();

        Assert.AreEqual(1, count);
    }

    public static void SetupTimeOut([CallerMemberName] string? callerName = null)
    {
        const int timeInMillis = 1000;

        var dispatcher = Dispatcher.Current;

        bool timeOut = false;
        dispatcher.Events.ShutdownFinished += (dispatcher1, evt) =>
        {
            if (timeOut)
            {
                Assert.Fail($"Unexpected timeout after {timeInMillis}ms for test {callerName}");
            }
        };

        var thread = new Thread(() =>
        {
            Thread.Sleep(timeInMillis);
            if (!dispatcher.HasShutdownStarted)
            {
                dispatcher.InvokeAsyncAndForget(() =>
                {
                    timeOut = true;
                    dispatcher.Shutdown();
                });
            }
        })
        {
            IsBackground = true,
            Name = $"TimeOut Thread - {callerName}"
        };

        thread.Start();
    }
}
