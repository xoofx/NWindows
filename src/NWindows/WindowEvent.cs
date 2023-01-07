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

    public WindowEvent(WindowEventKind kind)
    {
        _kind = kind;
    }
    
    [FieldOffset(0)]
    private readonly WindowEventKind _kind;
    public WindowEventKind Kind => _kind;

    [FieldOffset(0)] internal MouseEvent Mouse;
    [FieldOffset(0)] internal FrameEvent Frame;
    [FieldOffset(0)] internal KeyboardEvent Keyboard;
    [FieldOffset(0)] internal SystemEvent System;
    [FieldOffset(0)] internal PaintEvent Paint;
    [FieldOffset(0)] internal HitTestEvent HitTest;
    [FieldOffset(0)] internal CloseEvent Close;
    [FieldOffset(0)] internal TextEvent Text;
    [FieldOffset(0)] internal IdleEvent Idle;

    public override string ToString()
    {
        return Kind switch
        {
            WindowEventKind.Undefined => "Kind: undefined",
            WindowEventKind.Idle => this.Cast<IdleEvent>().ToString(),
            WindowEventKind.System => this.Cast<SystemEvent>().ToString(),
            WindowEventKind.Frame => this.Cast<FrameEvent>().ToString(),
            WindowEventKind.Paint => this.Cast<PaintEvent>().ToString(),
            WindowEventKind.HitTest => this.Cast<HitTestEvent>().ToString(),
            WindowEventKind.Keyboard => this.Cast<KeyboardEvent>().ToString(),
            WindowEventKind.Mouse => this.Cast<MouseEvent>().ToString(),
            WindowEventKind.Close => this.Cast<CloseEvent>().ToString(),
            WindowEventKind.Text => this.Cast<TextEvent>().ToString(),
            _ => "Kind: unknown"
        };
    }
}