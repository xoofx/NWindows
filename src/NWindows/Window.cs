// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;


// Missing options:
// - DefaultSize (in options)
// - WindowStartPosition (e.g Default, CenterParent, CenterScreen)
//
// Missing properties:
//
// - BackColor?
// - FormBorderStyle => Changed to Resizeable?
// - DesktopBounds (Gets or sets the size and location of the form on the Windows desktop.)
// - DesktopLocation (Gets or sets the location of the form on the Windows desktop )
// - MaximizeBox
// - MinimizeBox
// - Modal
// - ShowInTaskbar
// - ShowIcon
// - TransparencyKey
//
// Missing methods:
// - CenterToParent
// - CenterToScreen
// - ShowModal
//
// Missing events:
// - DpiChanged?
// - ResizeBegin / ResizeEnd

public abstract class Window : DispatcherObject
{
    internal Window(in WindowCreateOptions options)
    {
        EventHandler = options.WindowEventHandler;
    }

    public WindowEventDelegate EventHandler { get; }

    public IntPtr Handle { get; protected set; }

    public WindowKind Kind { get; protected set; }

    public abstract bool Enable { get; set; }

    public abstract string Title { get; set; }

    public abstract SizeF Size { get; set; }
    
    public abstract Point Position { get; set; }

    public abstract bool Visible { get; set; }

    public abstract bool Resizeable { get; set; }

    public abstract Window? Parent { get; }
    
    public abstract WindowState State { get; set; }

    public abstract float Opacity { get; set; }

    public bool TopLevel => Kind == WindowKind.TopLevel;

    public abstract bool TopMost { get; set; }

    public abstract void Focus();

    public abstract void Activate();

    public abstract bool Close();

    public abstract SizeF MinimumSize { get; set; }

    public abstract SizeF MaximumSize { get; set; }

    public abstract bool Modal { get; set; }

    public abstract PointF ScreenToClient(Point position);

    public abstract Point ClientToScreen(PointF position);

    public Point CurrentDpi => GetScreen() is {} screen ? screen.Dpi : new Point(96, 96);
    
    public abstract Screen? GetScreen();

    public static Window Create(WindowCreateOptions options)
    {
        if (OperatingSystem.IsWindows())
        {
            return new Win32.Win32Window(options);
        }

        throw new PlatformNotSupportedException();
    }

    protected internal void OnWindowEvent(ref WindowEvent evt)
    {
        EventHandler.Invoke(this, ref evt);
    }

    protected void OnFrameEvent(FrameEventKind frameEventKind)
    {
        var frameEvent = new WindowEvent(WindowEventKind.Frame);
        frameEvent.Frame.SubKind = frameEventKind;
        OnWindowEvent(ref frameEvent);
    }
    protected bool OnPaintEvent(in RectangleF bounds)
    {
        var paintEvent = new WindowEvent(WindowEventKind.Paint);
        paintEvent.Paint.Bounds = bounds;
        OnWindowEvent(ref paintEvent);
        return paintEvent.Paint.Handled;
    }
}