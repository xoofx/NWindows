// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// The kind of <see cref="WindowEvent"/>.
/// </summary>
public enum WindowEventKind
{
    /// <summary>
    /// Undefined
    /// </summary>
    Undefined = 0,

    /// <summary>
    /// A system event.
    /// </summary>
    System,

    /// <summary>
    /// A frame event.
    /// </summary>
    Frame,

    /// <summary>
    /// A paint event.
    /// </summary>
    Paint,

    /// <summary>
    /// A HitTest event.
    /// </summary>
    HitTest,

    /// <summary>
    /// A Keyboard event.
    /// </summary>
    Keyboard,

    /// <summary>
    /// A mouse event.
    /// </summary>
    Mouse,

    /// <summary>
    /// A close event.
    /// </summary>
    Close,

    /// <summary>
    /// A text event.
    /// </summary>
    Text,

    /// <summary>
    /// A drag and drop event.
    /// </summary>
    DragDrop,

    // TODO: add touch, gesture, clipboard, drag and drop
}