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

    public override string ToString()
    {
        return Kind switch
        {
            WindowEventKind.Undefined => "Kind: undefined",
            WindowEventKind.System => this.System.ToString(),
            WindowEventKind.Frame => this.Frame.ToString(),
            WindowEventKind.Paint => this.Paint.ToString(),
            WindowEventKind.HitTest => this.HitTest.ToString(),
            WindowEventKind.Keyboard => this.Keyboard.ToString(),
            WindowEventKind.Mouse => this.Mouse.ToString(),
            WindowEventKind.Close => this.Close.ToString(),
            WindowEventKind.Text => this.Text.ToString(),
            _ => "Kind: unknown"
        };
    }
}