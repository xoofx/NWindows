// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// Defines the way the Dpi is handled by a Window.
/// </summary>
public enum DpiMode
{
    /// <summary>
    /// Automatically adjust the DPI based on system settings or the state of fullscreen/resolution of the Window.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// The DPI is set manually directly on the Window.
    /// </summary>
    Manual = 1,
}