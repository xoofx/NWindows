// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Events;

/// <summary>
/// An event triggered for a window that has no decorations to specify which part of the window the moues point to.
/// </summary>
public record HitTestEvent() : WindowEvent(WindowEventKind.HitTest)
{
    /// <summary>
    /// Gets the position of the mouse within the window.
    /// </summary>
    public PointF MousePosition;

    /// <summary>
    /// Gets the size of the window.
    /// </summary>
    public SizeF WindowSize;

    /// <summary>
    /// Gets or sets the result of the hit-test. Must be set by the handler.
    /// </summary>
    public HitTest Result { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if the hit-test was handled.
    /// </summary>
    public bool Handled { get; set; }

    // TODO: ToText to HitTest
}