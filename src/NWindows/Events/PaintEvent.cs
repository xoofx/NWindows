// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Events;

/// <summary>
/// A paint event. This event must be handled for custom rendering.
/// </summary>
public record PaintEvent() : WindowEvent(WindowEventKind.Paint)
{
    /// <summary>
    /// Gets the boundaries of the window to paint.
    /// </summary>
    public RectangleF Bounds;

    /// <summary>
    /// Gets or sets a boolean indicating that this event has been handled by the paint handler.
    /// </summary>
    public bool Handled { get; set; }
}