// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Events;
using NWindows.Threading;

namespace NWindows;

/// <summary>
/// This class provides event handlers for the various events published by a <see cref="Window"/>.
/// </summary>
public sealed class WindowEventHub : DispatcherObject
{
    private WindowEventHandler? _all;
    private MouseEventHandler? _mouse;
    private FrameEventHandler? _frame;
    private KeyboardEventHandler? _keyboard;
    private SystemEventHandler? _system;
    private PaintEventHandler? _paint;
    private HitTestEventHandler? _hitTest;
    private CloseEventHandler? _close;
    private TextEventHandler? _text;

    /// <summary>
    /// Creates a new instance of this hub associated with the current dispatcher.
    /// </summary>
    public WindowEventHub()
    {
    }

    /// <summary>
    /// Creates a new instance of this hub associated with the specified dispatcher.
    /// </summary>
    /// <param name="dispatcher">The dispatcher.</param>
    public WindowEventHub(Dispatcher dispatcher) : base(dispatcher)
    {
    }

    /// <summary>
    /// Adds or removes an event handler for all Window events published.
    /// </summary>
    public event WindowEventHandler All
    {
        add
        {
            VerifyAccess();
            _all += value;
        }
        remove
        {
            VerifyAccess();
            _all -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for all mouse events published.
    /// </summary>
    public event MouseEventHandler Mouse
    {
        add
        {
            VerifyAccess();
            _mouse += value;
        }
        remove
        {
            VerifyAccess();
            _mouse -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for all frame events published.
    /// </summary>
    public event FrameEventHandler Frame
    {
        add
        {
            VerifyAccess();
            _frame += value;
        }
        remove
        {
            VerifyAccess();
            _frame -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for all keyboard events published.
    /// </summary>
    public event KeyboardEventHandler Keyboard
    {
        add
        {
            VerifyAccess();
            _keyboard += value;
        }
        remove
        {
            VerifyAccess();
            _keyboard -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for all system events published.
    /// </summary>
    public event SystemEventHandler System
    {
        add
        {
            VerifyAccess();
            _system += value;
        }
        remove
        {
            VerifyAccess();
            _system -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for all paint events published.
    /// </summary>
    public event PaintEventHandler Paint
    {
        add
        {
            VerifyAccess();
            _paint += value;
        }
        remove
        {
            VerifyAccess();
            _paint -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for all hittest events published.
    /// </summary>
    public event HitTestEventHandler HitTest
    {
        add
        {
            VerifyAccess();
            _hitTest += value;
        }
        remove
        {
            VerifyAccess();
            _hitTest -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for the close event published.
    /// </summary>
    public event CloseEventHandler Close
    {
        add
        {
            VerifyAccess();
            _close += value;
        }
        remove
        {
            VerifyAccess();
            _close -= value;
        }
    }

    /// <summary>
    /// Adds or removes an event handler for the text events published.
    /// </summary>
    public event TextEventHandler Text
    {
        add
        {
            VerifyAccess();
            _text += value;
        }
        remove
        {
            VerifyAccess();
            _text -= value;
        }
    }

    internal void OnWindowEvent(Window window, WindowEvent evt)
    {
        _all?.Invoke(window, evt);
        switch (evt.Kind)
        {
            case WindowEventKind.System:
                _system?.Invoke(window, (SystemEvent)evt);
                break;
            case WindowEventKind.Frame:
                _frame?.Invoke(window, (FrameEvent)evt);
                break;
            case WindowEventKind.Paint:
                _paint?.Invoke(window, (PaintEvent)evt);
                break;
            case WindowEventKind.HitTest:
                _hitTest?.Invoke(window, (HitTestEvent)evt);
                break;
            case WindowEventKind.Keyboard:
                _keyboard?.Invoke(window, (KeyboardEvent)evt);
                break;
            case WindowEventKind.Mouse:
                _mouse?.Invoke(window, (MouseEvent)evt);
                break;
            case WindowEventKind.Close:
                _close?.Invoke(window, (CloseEvent)evt);
                break;
            case WindowEventKind.Text:
                _text?.Invoke(window, (TextEvent)evt);
                break;
        }
    }

    /// <summary>
    /// Handle for window events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void WindowEventHandler(Window window, WindowEvent evt);

    /// <summary>
    /// Handle for moues events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void MouseEventHandler(Window window, MouseEvent evt);

    /// <summary>
    /// Handle for frame events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void FrameEventHandler(Window window, FrameEvent evt);

    /// <summary>
    /// Handle for keyboard events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void KeyboardEventHandler(Window window, KeyboardEvent evt);

    /// <summary>
    /// Handle for system events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void SystemEventHandler(Window window, SystemEvent evt);

    /// <summary>
    /// Handle for paint events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void PaintEventHandler(Window window, PaintEvent evt);

    /// <summary>
    /// Handle for hittest events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void HitTestEventHandler(Window window, HitTestEvent evt);

    /// <summary>
    /// Handle for close events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void CloseEventHandler(Window window, CloseEvent evt);

    /// <summary>
    /// Handle for text events.
    /// </summary>
    /// <param name="window">The window that published this event.</param>
    /// <param name="evt">The associated event.</param>
    public delegate void TextEventHandler(Window window, TextEvent evt);
}