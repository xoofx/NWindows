// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// The orientation of the display returned used by a <see cref="Screen"/>.
/// </summary>
public enum DisplayOrientation
{
    /// <summary>
    /// No rotation.
    /// </summary>
    Default,

    /// <summary>
    /// 90 degrees rotation.
    /// </summary>
    Rotate90,

    /// <summary>
    /// 180 degrees rotation.
    /// </summary>
    Rotate180,

    /// <summary>
    /// 270 degrees rotation.
    /// </summary>
    Rotate270,
}