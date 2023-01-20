// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Input;

namespace NWindows.Events;

/// <summary>
/// A mouse event.
/// </summary>
public record MouseEvent() : WindowEvent(WindowEventKind.Mouse)
{
    /// <summary>
    /// Gets the mouse event kind.
    /// </summary>
    public MouseEventKind MouseKind { get; set; }

    /// <summary>
    /// Gets the mouse button that is getting just pressed.
    /// </summary>
    public MouseButton Button { get; set; }

    /// <summary>
    /// Gets the currently pressed mouse buttons.
    /// </summary>
    public MouseButtonFlags Pressed { get; set; }

    /// <summary>
    /// Gets the logical position within the client area of the window.
    /// </summary>
    public PointF Position;

    /// <summary>
    /// Gets the delta for the wheel button.
    /// </summary>
    public Point WheelDelta;
}