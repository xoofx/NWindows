// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

[Flags]
public enum ModifierKeys
{
    /// <summary>
    ///    No modifiers are pressed.
    /// </summary>
    None = 0,

    /// <summary>
    ///    An alt key.
    /// </summary>
    Alt = 1,

    /// <summary>
    ///    A control key.
    /// </summary>
    Control = 2,

    /// <summary>
    ///    A shift key.
    /// </summary>
    Shift = 4,

    /// <summary>
    ///    A windows key.
    /// </summary>
    Windows = 8
}