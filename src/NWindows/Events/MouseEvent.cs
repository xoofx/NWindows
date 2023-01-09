// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Input;

namespace NWindows.Events;

public record MouseEvent() : WindowEvent(WindowEventKind.Mouse)
{
    public MouseEventKind MouseKind;

    public MouseButton Button;

    public MouseButtonFlags Pressed;

    public PointF Position;

    public Point WheelDelta;
}