// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Tests;

public class AppProgram
{
    public static void Main(string[] args)
    {
        DisplayScreens();


        var mainWindow= Window.Create(new WindowCreateOptions(WindowEventDelegate)
        {
            //Decorations = false
        });
        
        var popupWindow = Window.Create(new WindowCreateOptions(WindowEventDelegate)
        {
            Kind = WindowKind.Popup,
            MinimumSize = new SizeF(100, 100),
            MaximumSize = new SizeF(500, 500),
            Parent = mainWindow,
            Title = "Popup",
            Decorations = false
        });

        Dispatcher.Current.Run();
    }

    private static int EventNumber;

    private static Point deltaMousePosition;

    private static void WindowEventDelegate(Window window, ref WindowEvent evt)
    {
        EventNumber++;
        Console.WriteLine($"[{EventNumber}] {evt} Window Position: {window.Position} Window Size: {window.Size}");
        if (evt.Kind == WindowEventKind.System)
        {
            DisplayScreens();
        }
        else if (evt.Kind == WindowEventKind.Mouse)
        {
            ref var mouseEvt = ref evt.Cast<MouseEvent>();
            var mousePositionOnScreen = window.ClientToScreen(mouseEvt.Position);
            if ((mouseEvt.Button & MouseButtonFlags.LeftButton) != 0)
            {
                window.Title = $"{window.Kind} Pressed: {EventNumber}";

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
                window.Modal = !window.Modal;
            }

            if ((mouseEvt.Button & MouseButtonFlags.Button2) != 0 && mouseEvt.SubKind == MouseEventKind.ButtonDown)
            {
                window.Close();
            }
        }
        else if (evt.Kind == WindowEventKind.HitTest)
        {
            ref var barHitTestEvent = ref evt.Cast<HitTestEvent>();
            if (barHitTestEvent.MousePosition.Y < 20)
            {
                barHitTestEvent.Result = HitTest.Caption;
                barHitTestEvent.Handled = true;
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