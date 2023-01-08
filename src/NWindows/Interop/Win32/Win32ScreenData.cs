// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using TerraFX.Interop.Windows;

namespace NWindows.Interop.Win32;

internal readonly struct Win32ScreenData : IEquatable<Win32ScreenData>
{
    private Win32ScreenData(string name, bool isPrimary, Point position, SizeF size, Size sizeInPixels, Point dpi)
    {
        IsValid = true;
        Name = name;
        IsPrimary = isPrimary;
        Position = position;
        Size = size;
        SizeInPixels = sizeInPixels;
        Dpi = dpi;
    }

    private Win32ScreenData(string name)
    {
        IsValid = false;
        Name = name;
    }

    public readonly bool IsValid;

    public readonly string Name;

    public readonly bool IsPrimary;

    public readonly Point Position;

    public readonly SizeF Size;

    public readonly Size SizeInPixels;

    public readonly Point Dpi;

    public bool Equals(Win32ScreenData other)
    {
        return IsValid == other.IsValid && Name == other.Name && IsPrimary == other.IsPrimary && Position.Equals(other.Position) && Size.Equals(other.Size) && Dpi.Equals(other.Dpi);
    }

    public override bool Equals(object? obj)
    {
        return obj is Win32ScreenData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsValid, Name, IsPrimary, Position, Size, Dpi);
    }

    public static bool operator ==(Win32ScreenData left, Win32ScreenData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Win32ScreenData left, Win32ScreenData right)
    {
        return !left.Equals(right);
    }

    public static unsafe bool TryGetScreenState(HMONITOR monitorHandle, out Win32ScreenData screenData)
    {
        MONITORINFOEXW monitorInfo;
        monitorInfo.Base.cbSize = (uint)sizeof(MONITORINFOEXW);

        if (Windows.GetMonitorInfoW(monitorHandle, (MONITORINFO*)&monitorInfo))
        {
            var isPrimary = (monitorInfo.Base.dwFlags & Windows.MONITORINFOF_PRIMARY) != 0;

            // Fetch the DPI
            int dpiX;
            int dpiY;
            if (Windows.GetDpiForMonitor(monitorHandle, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, (uint*)&dpiX, (uint*)&dpiY) != 0)
            {
                dpiX = 96;
                dpiY = 96;
            }

            var span = new ReadOnlySpan<char>((char*)monitorInfo.szDevice, 32);
            var index = span.IndexOf((char)0);
            if (index >= 0)
            {
                span = span.Slice(0, index);
            }

            var name = new string(span);
            var pixelPositionX = monitorInfo.Base.rcMonitor.left;
            var pixelPositionY = monitorInfo.Base.rcMonitor.top;

            var pixelWidth = monitorInfo.Base.rcMonitor.right - monitorInfo.Base.rcMonitor.left;
            var pixelHeight = monitorInfo.Base.rcMonitor.bottom - monitorInfo.Base.rcMonitor.top;
            var width = WindowHelper.PixelToLogical(pixelWidth, dpiX);
            var height = WindowHelper.PixelToLogical(pixelHeight, dpiY);

            screenData = new Win32ScreenData(name, isPrimary, new Point(pixelPositionX, pixelPositionY), new SizeF(width, height), new Size(pixelWidth, pixelHeight), new Point(dpiX, dpiY));
            return true;
        }
        else
        {
            screenData = new Win32ScreenData(string.Empty);
        }

        return false;
    }
}