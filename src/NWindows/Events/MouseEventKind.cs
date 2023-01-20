// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// The kind of a <see cref="MouseEvent"/>.
/// </summary>
public enum MouseEventKind
{
    /// <summary>
    /// The mouse is entering the window.
    /// </summary>
    Enter,

    /// <summary>
    /// The mouse is leaving the window.
    /// </summary>
    Leave,

    /// <summary>
    /// The mouse is being moved in the window.
    /// </summary>
    Move,

    /// <summary>
    /// The mouse wheel is changed.
    /// </summary>
    Wheel,

    /// <summary>
    /// A mouse button has been released.
    /// </summary>
    ButtonUp,

    /// <summary>
    /// A mouse button has been pressed down.
    /// </summary>
    ButtonDown,

    /// <summary>
    /// A mouse button has been double-clicked.
    /// </summary>
    ButtonDoubleClick,
}