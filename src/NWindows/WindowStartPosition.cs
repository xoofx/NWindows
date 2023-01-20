// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// An enum to specify how a Window will be positioned at creation time. Sets this value in <see cref="WindowCreateOptions.StartPosition"/>.
/// </summary>
public enum WindowStartPosition
{
    /// <summary>
    /// Default position, unspecified.
    /// </summary>
    Default,

    /// <summary>
    /// Centers the position of the window to the parent window (or the screen if no parent).
    /// </summary>
    CenterParent,

    /// <summary>
    /// Centers the position of the window to the screen.
    /// </summary>
    CenterScreen
}