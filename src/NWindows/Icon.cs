// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Platforms.Win32;

namespace NWindows;

/// <summary>
/// A simple icon class that contains the image representation of an icon for a window.
/// </summary>
public sealed class Icon
{
    private static readonly IconImpl IconImpl = CreateIconImpl();

    private readonly Rgba32[] _buffer;

    /// <summary>
    /// Creates a new instance of this class with the specified width and height.
    /// </summary>
    /// <param name="width">The width of the icon.</param>
    /// <param name="height">The height of the icon.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the width or height is less or equal 0.</exception>
    public Icon(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;

        _buffer = new Rgba32[width * height];
    }

    /// <summary>
    /// Gets the size of the icon.
    /// </summary>
    public Size Size => new Size(Width, Height);

    /// <summary>
    /// Gets the width of the icon.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the icon.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the associated buffer.
    /// </summary>
    public Span<Rgba32> Buffer => _buffer;

    /// <summary>
    /// Gets a reference to the pixel value at the specified position.
    /// </summary>
    /// <param name="x">The x pixel position.</param>
    /// <param name="y">The y pixel position.</param>
    /// <returns>A reference to the pixel value/</returns>
    /// <exception cref="ArgumentOutOfRangeException">If x or y are not within the Width and Height of this icon.</exception>
    public ref Rgba32 PixelAt(int x, int y)
    {
        if (x <= 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x), $"x ({x}) is out of range ({Width}).");
        if (y <= 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y), $"y ({y}) is out of range ({Height}).");

        return ref _buffer[y * Width + x];
    }

    /// <summary>
    /// Gets the default application icon.
    /// </summary>
    /// <returns></returns>
    public static Icon GetApplicationIcon() => IconImpl.GetApplicationIcon();

    /// <summary>
    /// A RGBA Pixel value.
    /// </summary>
    /// <param name="R">The red component.</param>
    /// <param name="G">The green component.</param>
    /// <param name="B">The blue component.</param>
    /// <param name="A">The alpha component.</param>
    public record struct Rgba32(byte R, byte G, byte B, byte A)
    {
        public static implicit operator Rgba32(Color color) => new(color.R, color.G, color.B, color.A);
    }

    private static IconImpl CreateIconImpl()
    {
        if (OperatingSystem.IsWindows()) return new Win32Icon();

        throw new PlatformNotSupportedException();
    }
}