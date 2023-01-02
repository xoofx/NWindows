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
// - FormBorderStyle
// - DesktopBounds (Gets or sets the size and location of the form on the Windows desktop.)
// - DesktopLocation (Gets or sets the location of the form on the Windows desktop )
// - MaximumSize
// - MinimumSize
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
        WindowEvent += options.WindowEventDelegate;
    }

    public event WindowEventDelegate WindowEvent;

    public IntPtr Handle { get; protected set; }

    public abstract SizeF Size { get; set; }

    public abstract Point Position { get; set; }

    public abstract bool Visible { get; set; }
    
    public abstract WindowState State { get; set; }

    public abstract float Opacity { get; set; }

    public abstract bool TopLevel { get; }

    public abstract bool TopMost { get; set; }
    
    public abstract void Focus();

    public abstract void Activate();

    public abstract SizeF MinimumSize { get; set; }

    public abstract SizeF MaximumSize { get; set; }

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
        WindowEvent.Invoke(this, ref evt);
    }

    protected void OnFrameEvent(FrameEventKind frameEventKind)
    {
        var localEvent = new WindowEvent(WindowEventKind.Frame);
        ref var frameEvent = ref localEvent.Cast<FrameEvent>();
        frameEvent.SubKind = frameEventKind;
        OnWindowEvent(ref localEvent);
    }
    protected bool OnPaintEvent(in RectangleF bounds)
    {
        var localEvent = new WindowEvent(WindowEventKind.Paint);
        ref var painEvent = ref localEvent.Cast<PaintEvent>();
        painEvent.Bounds = bounds;
        OnWindowEvent(ref localEvent);
        return painEvent.Handled;
    }
}