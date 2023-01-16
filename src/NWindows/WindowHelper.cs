// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

public static class WindowHelper
{
    public static Size Max(Size left, Size right)
    {
        return new Size(Math.Max(left.Width, right.Width), Math.Max(left.Height, right.Height));
    }

    public static Size Min(Size left, Size right)
    {
        return new Size(Math.Min(left.Width, right.Width), Math.Min(left.Height, right.Height));
    }
}