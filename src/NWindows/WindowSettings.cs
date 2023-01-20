// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Threading;

namespace NWindows;

/// <summary>
/// Provides settings related to windows (theme, accent color...)
/// </summary>
public static class WindowSettings
{
    /// <summary>
    /// Gets the current theme for Windows.
    /// </summary>
    public static WindowTheme Theme => Dispatcher.Current.WindowSettings.Theme;

    /// <summary>
    /// Gets the current accent color.
    /// </summary>
    public static Color AccentColor => Dispatcher.Current.WindowSettings.AccentColor;

    /// <summary>
    /// Gets the current default background color.
    /// </summary>
    public static Color BackgroundColor => Dispatcher.Current.WindowSettings.BackgroundColor;

    /// <summary>
    /// Gets the current default foreground color.
    /// </summary>
    public static Color ForegroundColor => Dispatcher.Current.WindowSettings.ForegroundColor;
}