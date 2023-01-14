// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

public readonly struct DpiScale : IEquatable<DpiScale>
{
    private const float DefaultDpi = 96.0f;
    private const int DefaultDpiAsInt = 96;
    private const int MaxDefaultDpiAsInt = DefaultDpiAsInt * 4;

    public static readonly DpiScale Default = new(DefaultDpiAsInt, DefaultDpiAsInt);

    public DpiScale(Point dpi) : this(dpi.X, dpi.Y)
    {
    }

    public DpiScale(int dpiX, int dpiY)
    {
        dpiX = Math.Clamp(dpiX, DefaultDpiAsInt, MaxDefaultDpiAsInt);
        dpiY = Math.Clamp(dpiY, DefaultDpiAsInt, MaxDefaultDpiAsInt);

        DpiX = dpiX;
        DpiY = dpiY;

        if (dpiX == DefaultDpiAsInt && dpiY == DefaultDpiAsInt)
        {
            ScaleLogicalToPixel = new PointF(1.0f, 1.0f);
            ScalePixelToLogical = ScaleLogicalToPixel;
        }
        else
        {
            ScaleLogicalToPixel = new PointF(dpiX / DefaultDpi, dpiY / DefaultDpi);
            ScalePixelToLogical = new PointF(DefaultDpi / dpiX, DefaultDpi / dpiY);
        }
    }

    public bool IsEmpty => DpiX == 0;

    public int DpiX { get; }

    public int DpiY { get; }

    public PointF ScaleLogicalToPixel { get; }

    public PointF ScalePixelToLogical { get; }

    public PointF PixelToLogical(Point point) => new(point.X * ScalePixelToLogical.X, point.Y * ScalePixelToLogical.Y);

    public Point LogicalToPixel(PointF point) => new((int)(point.X * ScaleLogicalToPixel.X), (int)(point.Y * ScaleLogicalToPixel.Y));

    public SizeF PixelToLogical(Size point) => new(point.Width * ScalePixelToLogical.X, point.Height * ScalePixelToLogical.Y);

    public Size LogicalToPixel(SizeF point) => new((int)(point.Width * ScaleLogicalToPixel.X), (int)(point.Height * ScaleLogicalToPixel.Y));

    public bool Equals(DpiScale other)
    {
        return DpiX == other.DpiX && DpiY == other.DpiY;
    }

    public override bool Equals(object? obj)
    {
        return obj is DpiScale other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DpiX, DpiY);
    }

    public static bool operator ==(DpiScale left, DpiScale right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DpiScale left, DpiScale right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{nameof(DpiX)}: {DpiX}, {nameof(DpiY)}: {DpiY}, Scale: ({ScaleLogicalToPixel.X * 100:##0}%, {ScaleLogicalToPixel.Y * 100:##0}%)";
    }
}