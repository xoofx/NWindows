// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

public readonly struct Dpi : IEquatable<Dpi>
{
    private const float DefaultDpi = 96.0f;
    private const int DefaultDpiAsInt = 96;
    private const int MaxDefaultDpiAsInt = DefaultDpiAsInt * 4;

    public static readonly Dpi Default = new(DefaultDpiAsInt, DefaultDpiAsInt);

    public Dpi(Point dpi) : this(dpi.X, dpi.Y)
    {
    }

    public Dpi(int x, int y)
    {
        x = Math.Clamp(x, DefaultDpiAsInt, MaxDefaultDpiAsInt);
        y = Math.Clamp(y, DefaultDpiAsInt, MaxDefaultDpiAsInt);

        X = x;
        Y = y;

        if (x == DefaultDpiAsInt && y == DefaultDpiAsInt)
        {
            ScaleLogicalToPixel = new PointF(1.0f, 1.0f);
            ScalePixelToLogical = ScaleLogicalToPixel;
        }
        else
        {
            ScaleLogicalToPixel = new PointF(x / DefaultDpi, y / DefaultDpi);
            ScalePixelToLogical = new PointF(DefaultDpi / x, DefaultDpi / y);
        }
    }

    public bool IsEmpty => X == 0;

    public int X { get; }

    public int Y { get; }

    public PointF ScaleLogicalToPixel { get; }

    public PointF ScalePixelToLogical { get; }

    public PointF PixelToLogical(Point point) => new(point.X * ScalePixelToLogical.X, point.Y * ScalePixelToLogical.Y);

    public Point LogicalToPixel(PointF point) => new((int)(point.X * ScaleLogicalToPixel.X), (int)(point.Y * ScaleLogicalToPixel.Y));

    public SizeF PixelToLogical(Size point) => new(point.Width * ScalePixelToLogical.X, point.Height * ScalePixelToLogical.Y);

    public Size LogicalToPixel(SizeF point) => new((int)(point.Width * ScaleLogicalToPixel.X), (int)(point.Height * ScaleLogicalToPixel.Y));

    public Rectangle LogicalToPixel(RectangleF bounds) => new Rectangle(LogicalToPixel(bounds.Location), LogicalToPixel(bounds.Size));

    public RectangleF PixelToLogical(Rectangle bounds) => new RectangleF(PixelToLogical(bounds.Location), PixelToLogical(bounds.Size));

    public bool Equals(Dpi other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Dpi other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(Dpi left, Dpi right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Dpi left, Dpi right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, Scale: ({ScaleLogicalToPixel.X * 100:##0}%, {ScaleLogicalToPixel.Y * 100:##0}%)";
    }
}