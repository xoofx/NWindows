// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

/// <summary>
/// A DPI (Dots per inch) value that allows to convert in and out logical/pixel values.
/// </summary>
public readonly struct Dpi : IEquatable<Dpi>
{
    private const float DefaultDpi = 96.0f;
    private const int DefaultDpiAsInt = 96;
    private const int MaxDefaultDpiAsInt = DefaultDpiAsInt * 4;

    /// <summary>
    /// The default DPI is 96.
    /// </summary>
    public static readonly Dpi Default = new(DefaultDpiAsInt, DefaultDpiAsInt);

    /// <summary>
    /// Creates a new instance of this class with the specified DPI on x and y.
    /// </summary>
    /// <param name="dpi">The point with the DPI specified on x and y.</param>
    public Dpi(Point dpi) : this(dpi.X, dpi.Y)
    {
    }

    /// <summary>
    /// Creates a new instance of this class with the specified DPI on x and y.
    /// </summary>
    /// <param name="x">The DPI on x.</param>
    /// <param name="y">The DPI on y.</param>
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

    /// <summary>
    /// A boolean indicating if this DPI is empty (x == 0).
    /// </summary>
    public bool IsEmpty => X == 0;

    /// <summary>
    /// Gets the DPI on X.
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Gets the DPI on Y.
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Gets the scaling factor to convert from logical to pixel values.
    /// </summary>
    public PointF ScaleLogicalToPixel { get; }

    /// <summary>
    /// Gets the scaling factor to convert from pixel to logical values.
    /// </summary>
    public PointF ScalePixelToLogical { get; }

    /// <summary>
    /// Converts the specified value from pixel to logical.
    /// </summary>
    /// <param name="point">A pixel point.</param>
    /// <returns>A logical point.</returns>
    public PointF PixelToLogical(Point point) => new(point.X * ScalePixelToLogical.X, point.Y * ScalePixelToLogical.Y);

    /// <summary>
    /// Converts the specified value from logical to pixel.
    /// </summary>
    /// <param name="point">A logical point.</param>
    /// <returns>A pixel point.</returns>
    public Point LogicalToPixel(PointF point) => new((int)(point.X * ScaleLogicalToPixel.X), (int)(point.Y * ScaleLogicalToPixel.Y));

    /// <summary>
    /// Converts the specified value from pixel to logical.
    /// </summary>
    /// <param name="size">A pixel size.</param>
    /// <returns>A logical size.</returns>
    public SizeF PixelToLogical(Size size) => new(size.Width * ScalePixelToLogical.X, size.Height * ScalePixelToLogical.Y);

    /// <summary>
    /// Converts the specified value from logical to pixel.
    /// </summary>
    /// <param name="size">A logical size.</param>
    /// <returns>A pixel size.</returns>
    public Size LogicalToPixel(SizeF size) => new((int)(size.Width * ScaleLogicalToPixel.X), (int)(size.Height * ScaleLogicalToPixel.Y));

    /// <summary>
    /// Converts the specified value from pixel to logical.
    /// </summary>
    /// <param name="bounds">A pixel rectangle.</param>
    /// <returns>A logical rectangle.</returns>
    public Rectangle LogicalToPixel(RectangleF bounds) => new Rectangle(LogicalToPixel(bounds.Location), LogicalToPixel(bounds.Size));

    /// <summary>
    /// Converts the specified value from logical to pixel.
    /// </summary>
    /// <param name="bounds">A logical rectangle.</param>
    /// <returns>A pixel rectangle.</returns>
    public RectangleF PixelToLogical(Rectangle bounds) => new RectangleF(PixelToLogical(bounds.Location), PixelToLogical(bounds.Size));

    /// <inheritdoc />
    public bool Equals(Dpi other)
    {
        return X == other.X && Y == other.Y;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Dpi other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    /// <summary>
    /// Standard equal operator.
    /// </summary>
    public static bool operator ==(Dpi left, Dpi right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Standard un-equal operator.
    /// </summary>
    public static bool operator !=(Dpi left, Dpi right)
    {
        return !left.Equals(right);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(X)}: {X}, {nameof(Y)}: {Y}, Scale: ({ScaleLogicalToPixel.X * 100:##0}%, {ScaleLogicalToPixel.Y * 100:##0}%)";
    }
}