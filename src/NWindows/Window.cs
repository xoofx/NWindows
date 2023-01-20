// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Events;
using NWindows.Platforms.Win32;
using NWindows.Threading;

namespace NWindows;

/// <summary>
/// The Window class associated with a native Window.
/// </summary>
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

    /// <summary>
    /// Gets the hub for the events attached to this Window.
    /// </summary>
    public WindowEventHub Events { get; }

    /// <summary>
    /// Gets the native handle associated to this window. This is platform dependent (see remarks).
    /// </summary>
    /// <remarks>
    /// - On Windows: This handle is a HWND.
    /// </remarks>
    public IntPtr Handle { get; protected set; }

    /// <summary>
    /// Gets the kind of this window.
    /// </summary>
    public WindowKind Kind { get; protected set; }

    /// <summary>
    /// Gets or sets a boolean indicating if the window has default decorations (title caption, resize grips, minimize/maximize/close buttons).
    /// </summary>
    public abstract bool Decorations { get; set; }

    /// <summary>
    /// Gets or sets the DPI associated to this window.
    /// </summary>
    /// <remarks>
    /// In order to manually override the system DPI, you must set the <see cref="DpiMode"/> to manual.
    /// </remarks>
    public abstract Dpi Dpi { get; set; }

    /// <summary>
    /// Gets or sets the DPI mode. Default is auto (in sync with the OS).
    /// </summary>
    public abstract DpiMode DpiMode { get; set; }

    /// <summary>
    /// Gets or sets how the theme of the Window is synced. Default is auto.
    /// </summary>
    public abstract WindowThemeSyncMode ThemeSyncMode { get; set; }

    /// <summary>
    /// Gets or sets the background color.
    /// </summary>
    public abstract Color BackgroundColor { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating that this window is enabled or disabled.
    /// </summary>
    public abstract bool Enable { get; set; }

    /// <summary>
    /// Gets a boolean indicating that this Windows has been disposed.
    /// </summary>
    public abstract bool IsDisposed { get; }

    /// <summary>
    /// Gets or sets the title of this window.
    /// </summary>
    public abstract string Title { get; set; }

    /// <summary>
    /// Gets or sets the size of this window. The size is in logical value according to the <see cref="Dpi"/> of this window.
    /// </summary>
    public abstract SizeF Size { get; set; }

    /// <summary>
    /// Gets the size in pixels of this window.
    /// </summary>
    public Size SizeInPixels => Dpi.LogicalToPixel(Size);

    /// <summary>
    /// Gets or sets the logical client size (without the decorations).
    /// </summary>
    /// <remarks>
    /// If the window does not have decoration, the client size is equal to the <see cref="Size"/> of this window.
    /// </remarks>
    public abstract SizeF ClientSize { get; set; }

    /// <summary>
    /// Gets or sets the virtual position in pixels of this window.
    /// </summary>
    public abstract Point Position { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if this window is visible or not.
    /// </summary>
    public abstract bool Visible { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if this window accepts drag and drop or not.
    /// </summary>
    public abstract bool DragDrop { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if this window can be resized.
    /// </summary>
    public abstract bool Resizeable { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if this window can be maximized.
    /// </summary>
    public abstract bool Maximizeable { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if this window can be minimized.
    /// </summary>
    public abstract bool Minimizeable { get; set; }

    /// <summary>
    /// Gets the parent window. Is not null if <see cref="Kind"/> is <see cref="WindowKind.TopLevel"/>.
    /// </summary>
    public abstract INativeWindow? Parent { get; }

    /// <summary>
    /// Gets or sets the state of the window (normal, minimized, maximized, fullscreen).
    /// </summary>
    public abstract WindowState State { get; set; }

    /// <summary>
    /// Gets or sets the opacity of this window.
    /// </summary>
    public abstract float Opacity { get; set; }

    /// <summary>
    /// Gets a boolean indicating if this window is a top level window.
    /// </summary>
    public bool TopLevel => Kind == WindowKind.TopLevel;

    /// <summary>
    /// Gets or sets a boolean indicating that this window is top most.
    /// </summary>
    public abstract bool TopMost { get; set; }

    /// <summary>
    /// Brings the focus to this window.
    /// </summary>
    public abstract void Focus();

    /// <summary>
    /// Activates this window.
    /// </summary>
    public abstract void Activate();

    /// <summary>
    /// Closes this window.
    /// </summary>
    /// <returns>If the window was successfully closed.</returns>
    public abstract bool Close();

    /// <summary>
    /// Gets or sets the logical maximum size of this window.
    /// </summary>
    public abstract SizeF MinimumSize { get; set; }

    /// <summary>
    /// Gets or sets the logical minimum size of this window.
    /// </summary>
    public abstract SizeF MaximumSize { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if this window is modal or not.
    /// </summary>
    public abstract bool Modal { get; set; }

    /// <summary>
    /// Gets or sets a boolean indicating if the icon of window is visible on the task bar.
    /// </summary>
    public abstract bool ShowInTaskBar { get; set; }

    /// <summary>
    /// Converts a screen position to a position within the client area of the window.
    /// </summary>
    /// <param name="position">A screen position.</param>
    /// <returns>A logical position in the client area.</returns>
    public abstract PointF ScreenToClient(Point position);

    /// <summary>
    /// Converts a logical position in the client area to a pixel position on the screen.
    /// </summary>
    /// <param name="position">A logical position in the client area.</param>
    /// <returns>The equivalent screen position.</returns>
    public abstract Point ClientToScreen(PointF position);

    /// <summary>
    /// Gets the associated screen with this window.
    /// </summary>
    /// <returns></returns>
    public abstract Screen? GetScreen();

    /// <summary>
    /// Gets a screen associated to this window or the primary screen if no screen is associated.
    /// </summary>
    /// <returns></returns>
    public Screen? GetScreenOrPrimary() => GetScreen() ?? Screen.Primary;

    /// <summary>
    /// Centers this window according to its parent window. If this window does not have a parent, it will be centered to the screen.
    /// </summary>
    public abstract void CenterToParent();

    /// <summary>
    /// Centers this window according to the screen.
    /// </summary>
    public abstract void CenterToScreen();

    /// <summary>
    /// Sets the icon of this window.
    /// </summary>
    /// <param name="icon">The icon to set.</param>
    public abstract void SetIcon(Icon icon);

    /// <summary>
    /// Show this window as a dialog.
    /// </summary>
    /// <remarks>>
    /// The window must be a <see cref="WindowKind.Popup"/> window.
    /// </remarks>
    public void ShowDialog()
    {
        VerifyAccess();
        VerifyPopup();

        Visible = true;
        var frame = new ModalFrame(Dispatcher, this);
        WindowEventHub.FrameEventHandler destroyFrame = (Window window, FrameEvent evt) =>
        {
            frame.Continue = evt.ChangeKind != FrameChangeKind.Destroyed;
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

    /// <summary>
    /// Creates a window with the specified options.
    /// </summary>
    /// <param name="options">The options of the window.</param>
    /// <returns>The window created.</returns>
    /// <exception cref="PlatformNotSupportedException">If the platform is not supported.</exception>
    public static Window Create(WindowCreateOptions options)
    {
        options.Verify();

        if (OperatingSystem.IsWindows())
        {
            return new Win32Window(options);
        }

        throw new PlatformNotSupportedException();
    }

    internal void VerifyPopup()
    {
        if (Kind != WindowKind.Popup) throw new InvalidOperationException("Window is not a Popup. Expecting the window to be a Popup for this operation.");
    }

    internal void VerifyResizeable()
    {
        if (!Resizeable) throw new InvalidOperationException("Window is not resizable");
    }

    internal void VerifyTopLevel()
    {
        if (Kind != WindowKind.TopLevel) throw new InvalidOperationException("Window is not a TopLevel. Expecting the window to be a TopLevel for this operation.");
    }

    internal void VerifyNotChild()
    {
        if (Kind == WindowKind.Win32Child) throw new InvalidOperationException("Cannot perform this operation on a child window.");
    }
    
    internal void OnWindowEvent(WindowEvent evt)
    {
        Events.OnWindowEvent(this, evt);
    }

    internal void OnFrameEvent(FrameChangeKind frameChangeKind)
    {
        _frameEvent.ChangeKind = frameChangeKind; 
        OnWindowEvent(_frameEvent);
    }
    internal bool OnPaintEvent(in RectangleF bounds)
    {
        _paintEvent.Handled = false;
        _paintEvent.Bounds = bounds;
        OnWindowEvent(_paintEvent);
        return _paintEvent.Handled;
    }

    internal void CenterPositionFromBounds(Rectangle bounds)
    {
        var position = bounds.Location;
        var size = bounds.Size;

        var currentSize = Dpi.LogicalToPixel(Size);
        if (currentSize.IsEmpty) return;

        var dpi = Dpi;
        bool changed = false;
        var currentPosition = Position;
        if (currentSize.Width <= size.Width)
        {
            currentPosition.X = position.X + (size.Width - currentSize.Width) / 2;
            changed = true;
        }

        if (currentSize.Height <= size.Height)
        {
            currentPosition.Y = position.Y + (size.Height - currentSize.Height) / 2;
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