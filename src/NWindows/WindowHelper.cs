// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows;

public static class WindowHelper
{
    public static float PixelToLogical(int pixel, int dpi)
    {
        return dpi == 96 ? pixel : pixel * 96.0f / dpi;
    }

    public static PointF PixelToLogical(Point point, Point dpi)
    {
        return new PointF(PixelToLogical(point.X, dpi.X), PixelToLogical(point.Y, dpi.Y));
    }

    public static SizeF PixelToLogical(Size size, Point dpi)
    {
        return new SizeF(PixelToLogical(size.Width, dpi.X), PixelToLogical(size.Height, dpi.Y));
    }

    public static int LogicalToPixel(float logicalValue, int dpi)
    {
        return dpi == 96 ? (int)logicalValue : (int)(logicalValue * dpi / 96.0f);
    }

    public static Point LogicalToPixel(PointF point, Point dpi)
    {
        return new Point(LogicalToPixel(point.X, dpi.X), LogicalToPixel(point.Y, dpi.Y));
    }

    public static Size LogicalToPixel(SizeF size, Point dpi)
    {
        return new Size(LogicalToPixel(size.Width, dpi.X), LogicalToPixel(size.Height, dpi.Y));
    }
}