// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

/// <summary>
/// Extension classes.
/// </summary>
public static class WindowHelper
{
    /// <summary>
    /// Calculates the max of the specified size.
    /// </summary>
    /// <param name="left">The size to calculate the max with.</param>
    /// <param name="right">The size to calculate the max with.</param>
    /// <returns>The maximum of the width/height.</returns>
    public static Size Max(Size left, Size right)
    {
        return new Size(Math.Max(left.Width, right.Width), Math.Max(left.Height, right.Height));
    }

    /// <summary>
    /// Calculates the min of the specified size.
    /// </summary>
    /// <param name="left">The size to calculate the min with.</param>
    /// <param name="right">The size to calculate the min with.</param>
    /// <returns>The minimum of the width/height.</returns>
    public static Size Min(Size left, Size right)
    {
        return new Size(Math.Min(left.Width, right.Width), Math.Min(left.Height, right.Height));
    }
}