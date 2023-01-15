// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows;

public record struct ScreenMode(int Width, int Height, int BitsPerPixel, int Frequency, DisplayOrientation Orientation)
{
    public Size Size => new (Width, Height);
}