// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Events;
using NWindows.Input;
using NWindows.Threading;
using NWindows.Threading.Events;

namespace NWindows.Tests;

public class AppProgram
{
    private readonly WindowEventHub _eventHub;
    private int _eventId;

    public static void Main(string[] args)
    {
        Console.WriteLine(Environment.OSVersion.Version);

        var program = new AppProgram();
        program.Run();
    }
    
    public AppProgram()
    {
        _eventHub = new WindowEventHub();
        _eventHub.All += WindowEventDelegate;
    }

    private void LogEvent(object evt)
    {
        _eventId++;
        Console.WriteLine($"[{_eventId}] {evt}");
    }

    public void Run()
    {
        DisplayScreens();

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        int countTimer = 0;
        timer.Tick += (sender, args) =>
        {
            countTimer++;
            Console.WriteLine($"Timer {countTimer} event");
        };
        timer.Start();

        Dispatcher.Current.Events.All += (dispatcher, evt) =>
        {
            //if (evt is IdleEvent idleEvent)
            //{
            //    idleEvent.Continuous = true;
            //}
            LogEvent(evt);
        };

        //Dispatcher.Current.UnhandledException += (sender, args) =>
        //{
        //    args.Handled = args.Exception is ApplicationException;
        //};

        _mainWindow = Window.Create(new WindowCreateOptions() 
        {
            Events = _eventHub
            //ShowIcon = false,
            //ShowInTaskBar = false
            //Decorations = false
        });

        Dispatcher.Current.Run();
    }

    private Window CreatePopupWindow()
    {
        return Window.Create(new WindowCreateOptions()
        {
            Events = _eventHub,
            Kind = WindowKind.Popup,
            MinimumSize = new SizeF(100, 100),
            MaximumSize = new SizeF(500, 500),
            BackgroundColor = Color.FromArgb(0x1e1e1e),
            Parent = _mainWindow!,
            Title = "Popup",
            Decorations = false
        });
    }

    private Point deltaMousePosition;
    private Window? _mainWindow;

    private void WindowEventDelegate(Window window, WindowEvent evt)
    {
        LogEvent(evt);
        if (evt.Kind == WindowEventKind.System)
        {
            DisplayScreens();
        }
        else if (evt.Kind == WindowEventKind.Mouse)
        {
            var mouseEvt = (MouseEvent)evt;
            var mousePositionOnScreen = window.ClientToScreen(mouseEvt.Position);
            if ((mouseEvt.Button & MouseButtonFlags.LeftButton) != 0)
            {
                window.Title = $"{window.Kind} Pressed: {_eventId}";

                deltaMousePosition = new Point(window.Position.X - mousePositionOnScreen.X, window.Position.Y - mousePositionOnScreen.Y);
            }
            if ((mouseEvt.Pressed & MouseButtonFlags.LeftButton) != 0)
            {
                window.Position = new Point(mousePositionOnScreen.X + deltaMousePosition.X, mousePositionOnScreen.Y + deltaMousePosition.Y);
            }

            if ((mouseEvt.WheelDelta.Y != 0))
            {
                window.Opacity += 0.1f * mouseEvt.WheelDelta.Y;
            }

            if ((mouseEvt.Button & MouseButtonFlags.RightButton) != 0 && mouseEvt.SubKind == MouseEventKind.ButtonDown)
            {
                window.TopMost = !window.TopMost;
            }

            if ((mouseEvt.Button & MouseButtonFlags.MiddleButton) != 0 && mouseEvt.SubKind == MouseEventKind.ButtonDown)
            {
                window.Resizeable = !window.Resizeable;
            }
            
            if ((mouseEvt.Button & MouseButtonFlags.Button1) != 0 && mouseEvt.SubKind == MouseEventKind.ButtonDown)
            {
                if (window.TopLevel)
                {
                    var popupWindow = CreatePopupWindow();
                    popupWindow.ShowDialog();
                }
                else
                {
                    window.Modal = !window.Modal;
                }
            }

            if ((mouseEvt.Button & MouseButtonFlags.Button2) != 0 && mouseEvt.SubKind == MouseEventKind.ButtonDown)
            {
                window.Close();
            }
        }
        else if (evt.Kind == WindowEventKind.HitTest)
        {
            var barHitTestEvent = (HitTestEvent)evt;
            if (barHitTestEvent.MousePosition.Y < 20)
            {
                barHitTestEvent.Result = HitTest.Caption;
                barHitTestEvent.Handled = true;
            }
        }
        else if (evt.Kind == WindowEventKind.Keyboard)
        {
            var keyboardEvent = (KeyboardEvent)evt;
            if (keyboardEvent.IsUp && window.TopLevel)
            {
                switch (keyboardEvent.Key)
                {
                    case Key.H:
                        window.ShowInTaskBar = !window.ShowInTaskBar;
                        break;
                    case Key.E:
                        throw new ApplicationException("This is a test application exception");
                        break;
                }
            }
        }
    }

    static void DisplayScreens()
    {
        foreach (var screen in Screen.Items)
        {
            Console.WriteLine($"{screen.Name} primary: {screen.IsPrimary} position: {screen.Position} size: {screen.Size}");
        }

        Console.WriteLine($"Virtual Screen position: {Screen.VirtualPosition} size: {Screen.VirtualSize}");
    }

}