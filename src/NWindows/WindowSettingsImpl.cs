// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows;

internal abstract class WindowSettingsImpl
{
    public abstract WindowTheme Theme { get; }

    public abstract Color AccentColor { get; }

    public abstract Color BackgroundColor { get; }

    public abstract Color ForegroundColor { get; }
}