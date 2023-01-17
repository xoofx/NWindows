// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Threading;

namespace NWindows;

public static class WindowSettings
{
    public static WindowTheme Theme => Dispatcher.Current.WindowSettings.Theme;

    public static Color AccentColor => Dispatcher.Current.WindowSettings.AccentColor;

    public static Color BackgroundColor => Dispatcher.Current.WindowSettings.BackgroundColor;

    public static Color ForegroundColor => Dispatcher.Current.WindowSettings.ForegroundColor;
}