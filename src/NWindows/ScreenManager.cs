// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

internal abstract class ScreenManager
{
    public abstract Point GetVirtualScreenPosition();

    public abstract Size GetVirtualScreenSizeInPixels();

    public abstract bool TryUpdateScreens();

    public abstract ReadOnlySpan<Screen> GetAllScreens();

    public abstract Screen? GetPrimaryScreen();
}