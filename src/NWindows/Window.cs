// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Threading;
using TerraFX.Interop.Windows;

namespace NWindows;


// Missing options:
// - DefaultSize (in options)
// - WindowStartPosition (e.g Default, CenterParent, CenterScreen)
//
// Missing properties:
//
// - DesktopBounds (Gets or sets the size and location of the form on the Windows desktop.)
// - DesktopLocation (Gets or sets the location of the form on the Windows desktop )
// - TransparencyKey
//
// Missing methods:
// - CenterToParent
// - CenterToScreen
//
// Missing events:
// - DpiChanged: See https://learn.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows
// - ResizeBegin / ResizeEnd
//
// 

public abstract class Window : DispatcherObject
{
    internal Window(WindowCreateOptions options)
    {
        Events = options.Events;
    }

    public WindowEventHub Events { get; }

    public IntPtr Handle { get; protected set; }

    public WindowKind Kind { get; protected set; }

    public abstract Color BackgroundColor { get; set; }

    public abstract bool Enable { get; set; }

    public abstract bool IsDisposed { get; }

    public abstract string Title { get; set; }

    public abstract SizeF Size { get; set; }
    
    public abstract Point Position { get; set; }

    public abstract bool Visible { get; set; }

    public abstract bool Resizeable { get; set; }

    public abstract bool Maximizeable { get; set; }

    public abstract bool Minimizeable { get; set; }

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

    public abstract bool ShowInTaskBar { get; set; }

    public abstract PointF ScreenToClient(Point position);

    public abstract Point ClientToScreen(PointF position);

    public Point CurrentDpi => GetScreen() is {} screen ? screen.Dpi : new Point(96, 96);
    
    public abstract Screen? GetScreen();

    public void ShowDialog()
    {
        VerifyAccess();
        VerifyPopup();

        var frame = new ModalFrame(Dispatcher, this);
        WindowEventHub.CloseEventHandler stopFrame = (Window window, ref CloseEvent evt) =>
        {
            frame.Continue = false;
        };

        Events.Close += stopFrame;
        try
        {
            // Block until this window is closed
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            Events.Close -= stopFrame;
        }
    }

    public static Window Create(WindowCreateOptions options)
    {
        options.Verify();

        if (OperatingSystem.IsWindows())
        {
            return new Win32.Win32Window(options);
        }

        throw new PlatformNotSupportedException();
    }

    internal void VerifyPopup()
    {
        if (Kind != WindowKind.Popup) throw new InvalidOperationException("Window is not a Popup. Expecting the window to be a Popup for this operation");
    }

    internal void VerifyResizeable()
    {
        if (!Resizeable) throw new InvalidOperationException("Window is not resizable");
    }

    internal void VerifyTopLevel()
    {
        if (Kind != WindowKind.TopLevel) throw new InvalidOperationException("Window is not a TopLevel. Expecting the window to be a TopLevel for this operation");
    }

    internal void OnWindowEvent(ref WindowEvent evt)
    {
        Events.OnWindowEvent(this, ref evt);
    }

    internal void OnFrameEvent(FrameEventKind frameEventKind)
    {
        var frameEvent = new WindowEvent(WindowEventKind.Frame);
        frameEvent.Frame.SubKind = frameEventKind;
        OnWindowEvent(ref frameEvent);
    }
    internal bool OnPaintEvent(in RectangleF bounds)
    {
        var paintEvent = new WindowEvent(WindowEventKind.Paint);
        paintEvent.Paint.Bounds = bounds;
        OnWindowEvent(ref paintEvent);
        return paintEvent.Paint.Handled;
    }

    private sealed class ModalFrame : DispatcherFrame
    {
        private readonly Window _window;

        public ModalFrame(Dispatcher dispatcher, Window window) : base(dispatcher, false)
        {
            _window = window;
        }

        protected override void Enter()
        {
            _window.Modal = true;
        }

        protected override void Leave()
        {
            // The Window has been destroyed if we leave, so we don't need to modify the modal of the parent
        }
    }
}