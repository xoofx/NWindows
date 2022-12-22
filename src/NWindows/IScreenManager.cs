// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

internal interface IScreenManager
{
    Point GetVirtualScreenPosition();

    Size GetVirtualScreenSize();

    bool TryUpdateScreens();

    ReadOnlySpan<Screen> GetAllScreens();

    Screen? GetPrimaryScreen();
}