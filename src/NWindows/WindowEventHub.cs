// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Events;
using NWindows.Threading;

namespace NWindows;

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

    public WindowEventHub()
    {
    }

    public WindowEventHub(Dispatcher dispatcher) : base(dispatcher)
    {
    }

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

    public delegate void WindowEventHandler(Window window, WindowEvent evt);

    public delegate void MouseEventHandler(Window window, MouseEvent evt);

    public delegate void FrameEventHandler(Window window, FrameEvent evt);

    public delegate void KeyboardEventHandler(Window window, KeyboardEvent evt);

    public delegate void SystemEventHandler(Window window, SystemEvent evt);

    public delegate void PaintEventHandler(Window window, PaintEvent evt);

    public delegate void HitTestEventHandler(Window window, HitTestEvent evt);

    public delegate void CloseEventHandler(Window window, CloseEvent evt);

    public delegate void TextEventHandler(Window window, TextEvent evt);
}