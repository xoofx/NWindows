// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Input;

namespace NWindows;

public struct MouseEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Mouse;

    public MouseEvent()
    {
        Kind = WindowEventKind.Mouse;
    }

    public WindowEventKind Kind { get; }

    public MouseEventKind SubKind;

    public MouseButtonFlags Button;

    public MouseButtonFlags Pressed;

    public PointF Position;

    public Point WheelDelta;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(SubKind)}: {SubKind}, {nameof(Button)}: {Button}, {nameof(Pressed)}: {Pressed}, {nameof(Position)}: {Position}, {nameof(WheelDelta)}: {WheelDelta}";
    }
}