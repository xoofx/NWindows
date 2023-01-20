// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows.Input;

/// <summary>
/// Flags associated to mouse buttons.
/// </summary>
[Flags]
public enum MouseButtonFlags
{
    /// <summary>
    /// No buttons.
    /// </summary>
    None = 0,

    /// <summary>
    /// Left is active.
    /// </summary>
    Left = 1 << 0,

    /// <summary>
    /// Middle is active.
    /// </summary>
    Middle = 1 << 1,

    /// <summary>
    /// Right is active.
    /// </summary>
    Right = 1 << 2,

    /// <summary>
    /// XButton1 is active.
    /// </summary>
    XButton1 = 1 << 3,

    /// <summary>
    /// XButton2 is active.
    /// </summary>
    XButton2 = 1 << 4,
}