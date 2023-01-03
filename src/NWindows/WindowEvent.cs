// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace NWindows;

[StructLayout(LayoutKind.Explicit)]
public struct WindowEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Undefined;

    [Obsolete("This constructor cannot be used directly. Use specific WindowEvent.", true)]
    public WindowEvent()
    {
    }

    internal WindowEvent(WindowEventKind kind)
    {
        _kind = kind;
    }
    
    [FieldOffset(0)]
    private readonly WindowEventKind _kind;
    public WindowEventKind Kind => _kind;

    [FieldOffset(0)] internal MouseEvent Mouse;
    [FieldOffset(0)] internal FrameEvent Frame;
    [FieldOffset(0)] internal KeyEvent Key;
    [FieldOffset(0)] internal SystemEvent System;
    [FieldOffset(0)] internal PaintEvent Paint;
    [FieldOffset(0)] internal HitTestEvent HitTest;
    [FieldOffset(0)] internal CloseEvent Close;

    public override string ToString()
    {
        return Kind switch
        {
            WindowEventKind.Undefined => "Kind: undefined",
            WindowEventKind.Idle => $"{nameof(Kind)}: {nameof(WindowEventKind.Idle)}",
            WindowEventKind.Shutdown => $"{nameof(Kind)}: {nameof(WindowEventKind.Shutdown)}",
            WindowEventKind.System => this.Cast<SystemEvent>().ToString(),
            WindowEventKind.Application => $"{nameof(Kind)}: {nameof(WindowEventKind.Application)}",
            WindowEventKind.Frame => this.Cast<FrameEvent>().ToString(),
            WindowEventKind.Paint => this.Cast<PaintEvent>().ToString(),
            WindowEventKind.HitTest => this.Cast<HitTestEvent>().ToString(),
            WindowEventKind.Keyboard => $"{nameof(Kind)}: {nameof(WindowEventKind.Keyboard)}",
            WindowEventKind.Mouse => this.Cast<MouseEvent>().ToString(),
            WindowEventKind.Close => this.Cast<CloseEvent>().ToString(),
            _ => "Kind: unknown"
        };
    }
}