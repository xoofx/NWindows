// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// An enum describing how the theme should be changed for a window. Sets via <see cref="Window.ThemeSyncMode"/>.
/// </summary>
public enum WindowThemeSyncMode
{
    /// <summary>
    /// The theme of the window is automatically synchronized with the current system settings.
    /// </summary>
    Auto,

    /// <summary>
    /// The theme selected is light.
    /// </summary>
    Light,

    /// <summary>
    /// The theme selected is dark.
    /// </summary>
    Dark,
}