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

    [Test]
    public Task TestWindowPositionSizeTitle()
    {
        return RunDispatcher(writer =>
        {
            var windowEventHub = new WindowEventHub();
            windowEventHub.All += (window, evt) => writer.WriteLine($"Window Event: {evt} Position: {window.Position} Size: {window.Size} Title: \"{window.Title}\"");

            var window = Window.Create(new()
            {
                Position = new Point(4, 4),
                Size = new Size(320, 320),
                Visible = false,
                DpiMode = DpiMode.Manual, // Force manual DPI for the tests
                Events = windowEventHub,
                StartPosition = WindowStartPosition.Default,
            });

            bool isWindowClosed = false;
            Dispatcher.Current.Events.Idle += (dispatcher, evt) =>
            {
                writer.WriteLine("--- Change Position");
                window.Position = new Point(11, 12);
                window.Position = new Point(11, 12);  // Check that setting the same value doesn't duplicate
                writer.WriteLine("--- Change Size");
                window.Size = new SizeF(350, 360);
                window.Size = new SizeF(350, 360);
                writer.WriteLine("--- Change Title");
                window.Title = "Hello from there";
                window.Title = "Hello from there";
                writer.WriteLine("--- Change Client Size");
                window.ClientSize = new SizeF(400, 320);
                window.ClientSize = new SizeF(400, 320);
                writer.WriteLine("--- Change Decoration");
                window.HasDecorations = false;
                window.HasDecorations = false;
                writer.WriteLine("--- Change Maximizeable");
                window.Maximizeable = false;
                window.Maximizeable = false;
                writer.WriteLine("--- Change Minimizeable");
                window.Minimizeable = false;
                window.Minimizeable = false;
                writer.WriteLine("--- Change BackgroundColor");
                window.BackgroundColor = Color.Red;
                window.BackgroundColor = Color.Red;
                writer.WriteLine("--- Change Resizeable");
                window.Resizeable = false;
                window.Resizeable = true;
                window.Resizeable = true;
                if (!isWindowClosed)
                {
                    writer.WriteLine("--- Request Closing");
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