// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

/// <summary>
/// An enumeration of states of keyboard and mouse.
/// </summary>
[Flags]
public enum DragDropKeyStates
{
    /// <summary>
    /// No state set.
    /// </summary>
    None = 0,
    /// <summary>
    /// The left mouse button.  
    /// </summary>
    LeftMouseButton = 1,
    /// <summary>
    /// The right mouse button.   
    /// </summary>
    RightMouseButton = 2,
    /// <summary>
    /// The SHIFT key.   
    /// </summary>
    ShiftKey = 4,
    /// <summary>
    /// The CTRL key.
    /// </summary>
    ControlKey = 8,
    /// <summary>
    /// The middle mouse button.
    /// </summary>
    MiddleMouseButton = 16,
    /// <summary>
    /// The ALT key.   
    /// </summary>
    AltKey = 32,
}