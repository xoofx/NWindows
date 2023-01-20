// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// An enum that describes the state of the window.
/// </summary>
public enum WindowState
{
    /// <summary>
    /// A window in normal mode.
    /// </summary>
    Normal,

    /// <summary>
    /// The window is minimized.
    /// </summary>
    Minimized,

    /// <summary>
    /// The window is maximized.
    /// </summary>
    Maximized,

    /// <summary>
    /// The window is fullscreen.
    /// </summary>
    FullScreen,
}