// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Platforms.Win32;

namespace NWindows;

public sealed class Icon
{
    private static readonly IconImpl IconImpl = CreateIconImpl();

    private readonly Rgba32[] _buffer;

    public Icon(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

        Width = width;
        Height = height;

        _buffer = new Rgba32[width * height];
    }

    public Size Size => new Size(Width, Height);

    public int Width { get; }

    public int Height { get; }

    public Span<Rgba32> Buffer => _buffer;

    public ref Rgba32 PixelAt(int x, int y)
    {
        if (x <= 0 || x >= Width) throw new ArgumentOutOfRangeException(nameof(x), $"x ({x}) is out of range ({Width}).");
        if (y <= 0 || y >= Height) throw new ArgumentOutOfRangeException(nameof(y), $"y ({y}) is out of range ({Height}).");

        return ref _buffer[y * Width + x];
    }

    public static Icon GetApplicationIcon() => IconImpl.GetApplicationIcon();

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