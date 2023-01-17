// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using System.Runtime.CompilerServices;
using NWindows.Threading;

namespace NWindows.Tests;

public class WindowTests
{
    [Test]
    public Task TestSimpleWindow()
    {
        return RunDispatcher(writer =>
        {
            var windowEventHub = new WindowEventHub();
            windowEventHub.All += (dispatcher, evt) => writer.WriteLine($"Window Event: {evt}");

            var window = Window.Create(new()
            {
                Visible = false,
                Events = windowEventHub
            });

            bool isWindowClosed = false;
            Dispatcher.Current.Events.Idle += (dispatcher, evt) =>
            {
                if (!isWindowClosed)
                {
                    window.Close();
                }
            };
        });
    }

    //[Test]
    public Task TestMultiEventWindow()
    {
        return RunDispatcher(writer =>
        {
            var windowEventHub = new WindowEventHub();
            windowEventHub.All += (window, evt) => writer.WriteLine($"Window Event: {evt} Position: {window.Position} Size: {window.Size} Title: {window.Title}");

            var window = Window.Create(new()
            {
                Position = new Point(4, 4),
                Size = new Size(32, 32),
                Visible = false,
                DpiMode = DpiMode.Manual,
                Events = windowEventHub,
                StartPosition = WindowStartPosition.Default,
            });

            bool isWindowClosed = false;
            Dispatcher.Current.Events.Idle += (dispatcher, evt) =>
            {
                window.Position = new Point(11, 12);
                window.Size = new SizeF(64, 128);
                window.Title = "Hello from there";
                window.ClientSize = new SizeF(64, 128);

                if (!isWindowClosed)
                {
                    window.Close();
                }
            };
        });
    }

    public Task VerifyOutput(string output) {
        return Verify(output, settings: GetVerifySettings());
    }

    private VerifySettings GetVerifySettings()
    {
        var settings = new VerifySettings();
        settings.UseDirectory("Snapshots");
        settings.DisableDiff();
        return settings;
    }

    private Task RunDispatcher(Action<StringWriter> setup, [CallerMemberName] string? callerMember = null)
    {
        var writer = new StringWriter();
        var thread = new Thread(() =>
            {
                //Dispatcher.Current.EnableDebug = true;
                //Dispatcher.Current.DebugOutput = writer.WriteLine;
                
                Dispatcher.Current.Events.All += (dispatcher, evt) => writer.WriteLine($"Dispatcher Event: {evt}");
                setup(writer);
                DispatcherTests.SetupTimeOut();
                Dispatcher.Current.Run();
            }
        )
        {
            IsBackground = true,
            Name = $"WindowTests Thread {callerMember}",
        };
        thread.Start();
        thread.Join();
        return VerifyOutput(writer.ToString());
    }
}