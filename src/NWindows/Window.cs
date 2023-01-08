// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Events;
using NWindows.Threading;

namespace NWindows;

public abstract class Window : DispatcherObject, INativeWindow
{
    // The following events are cached per Window to avoid allocations
    // ReSharper disable InconsistentNaming
    internal readonly CloseEvent _closeEvent;
    internal readonly FrameEvent _frameEvent;
    internal readonly HitTestEvent _hitTestEvent;
    internal readonly KeyboardEvent _keyboardEvent;
    internal readonly MouseEvent _mouseEvent;
    internal readonly PaintEvent _paintEvent;
    internal readonly TextEvent _textEvent;
    // ReSharper restore InconsistentNaming

    internal Window(WindowCreateOptions options)
    {
        Events = options.Events;
        _closeEvent = new CloseEvent();
        _frameEvent = new FrameEvent();
        _hitTestEvent = new HitTestEvent();
        _keyboardEvent = new KeyboardEvent();
        _mouseEvent = new MouseEvent();
        _paintEvent = new PaintEvent();
        _textEvent = new TextEvent();
    }

    public WindowEventHub Events { get; }

    public IntPtr Handle { get; protected set; }

    public WindowKind Kind { get; protected set; }

    public abstract Point Dpi { get; }

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

    public abstract INativeWindow? Parent { get; }
    
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

    public abstract Screen? GetScreen();

    public abstract void CenterToParent();

    public void CenterToScreen()
    {
        var screen = GetScreen();
        if (screen != null)
        {
            CenterPositionFromBounds(screen.Position, screen.Size);
        }
    }

    public void ShowDialog()
    {
        VerifyAccess();
        VerifyPopup();

        var frame = new ModalFrame(Dispatcher, this);
        WindowEventHub.FrameEventHandler destroyFrame = (Window window, FrameEvent evt) =>
        {
            frame.Continue = evt.FrameKind != FrameEventKind.Destroyed;
        };

        Events.Frame += destroyFrame;
        try
        {
            // Block until this window is closed
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            Events.Frame -= destroyFrame;
        }
    }

    public static Window Create(WindowCreateOptions options)
    {
        options.Verify();

        if (OperatingSystem.IsWindows())
        {
            return new Interop.Win32.Win32Window(options);
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

    internal void OnWindowEvent(WindowEvent evt)
    {
        Events.OnWindowEvent(this, evt);
    }

    internal void OnFrameEvent(FrameEventKind frameEventKind)
    {
        _frameEvent.FrameKind = frameEventKind; 
        OnWindowEvent(_frameEvent);
    }
    internal bool OnPaintEvent(in RectangleF bounds)
    {
        _paintEvent.Handled = false;
        _paintEvent.Bounds = bounds;
        OnWindowEvent(_paintEvent);
        return _paintEvent.Handled;
    }

    internal void CenterPositionFromBounds(Point position, SizeF size)
    {
        var currentSize = Size;
        if (currentSize.IsEmpty) return;

        var dpi = Dpi;
        bool changed = false;
        var currentPosition = Position;
        if (currentSize.Width <= size.Width)
        {
            currentPosition.X = position.X + WindowHelper.LogicalToPixel((size.Width - currentSize.Width) / 2, dpi.X);
            changed = true;
        }

        if (currentSize.Height <= size.Height)
        {
            currentPosition.Y = position.Y + WindowHelper.LogicalToPixel((size.Height - currentSize.Height) / 2, dpi.Y);
            changed = true;
        }

        if (changed)
        {
            Position = currentPosition;
        }
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