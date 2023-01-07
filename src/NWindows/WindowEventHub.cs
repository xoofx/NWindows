// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

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
    private IdleEventHandler? _idle;

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

    public event IdleEventHandler Idle
    {
        add
        {
            VerifyAccess();
            _idle += value;
        }
        remove
        {
            VerifyAccess();
            _idle -= value;
        }
    }

    internal void OnWindowEvent(Window window, ref WindowEvent evt)
    {
        _all?.Invoke(window, ref evt);
        switch (evt.Kind)
        {
            case WindowEventKind.Idle:
                _idle?.Invoke(window, ref evt.Idle);
                break;
            case WindowEventKind.System:
                _system?.Invoke(window, ref evt.System);
                break;
            case WindowEventKind.Frame:
                _frame?.Invoke(window, ref evt.Frame);
                break;
            case WindowEventKind.Paint:
                _paint?.Invoke(window, ref evt.Paint);
                break;
            case WindowEventKind.HitTest:
                _hitTest?.Invoke(window, ref evt.HitTest);
                break;
            case WindowEventKind.Keyboard:
                _keyboard?.Invoke(window, ref evt.Keyboard);
                break;
            case WindowEventKind.Mouse:
                _mouse?.Invoke(window, ref evt.Mouse);
                break;
            case WindowEventKind.Close:
                _close?.Invoke(window, ref evt.Close);
                break;
            case WindowEventKind.Text:
                _text?.Invoke(window, ref evt.Text);
                break;
        }
    }

    public delegate void WindowEventHandler(Window window, ref WindowEvent evt);

    public delegate void MouseEventHandler(Window window, ref MouseEvent evt);

    public delegate void FrameEventHandler(Window window, ref FrameEvent evt);

    public delegate void KeyboardEventHandler(Window window, ref KeyboardEvent evt);

    public delegate void SystemEventHandler(Window window, ref SystemEvent evt);

    public delegate void PaintEventHandler(Window window, ref PaintEvent evt);

    public delegate void HitTestEventHandler(Window window, ref HitTestEvent evt);

    public delegate void CloseEventHandler(Window window, ref CloseEvent evt);

    public delegate void TextEventHandler(Window window, ref TextEvent evt);

    public delegate void IdleEventHandler(Window window, ref IdleEvent evt);
}