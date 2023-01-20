// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Platforms.Win32;

internal readonly struct Win32ScreenData : IEquatable<Win32ScreenData>
{
    private Win32ScreenData(string name, bool isPrimary, Point position, SizeF size, Size sizeInPixels, Dpi dpi, int refreshRate, DisplayOrientation displayOrientation)
    {
        IsValid = true;
        Name = name;
        IsPrimary = isPrimary;
        Position = position;
        Size = size;
        SizeInPixels = sizeInPixels;
        Dpi = dpi;
        RefreshRate = refreshRate;
        DisplayOrientation = displayOrientation;
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

    public readonly Dpi Dpi;

    public readonly int RefreshRate;

    public readonly DisplayOrientation DisplayOrientation;

    public bool Equals(Win32ScreenData other)
    {
        return IsValid == other.IsValid && Name == other.Name && IsPrimary == other.IsPrimary && Position.Equals(other.Position) && Size.Equals(other.Size) && Dpi.Equals(other.Dpi) && RefreshRate.Equals(other.RefreshRate) && DisplayOrientation == other.DisplayOrientation;
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

        if (GetMonitorInfoW(monitorHandle, (MONITORINFO*)&monitorInfo))
        {
            var isPrimary = (monitorInfo.Base.dwFlags & MONITORINFOF_PRIMARY) != 0;

            // Fetch the DPI
            int dpiX;
            int dpiY;
            if (GetDpiForMonitor(monitorHandle, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, (uint*)&dpiX, (uint*)&dpiY) != 0)
            {
                dpiX = 96;
                dpiY = 96;
            }

            var dpiScale = new Dpi(dpiX, dpiY);

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
            var width = dpiScale.ScalePixelToLogical.X * pixelWidth;
            var height = dpiScale.ScalePixelToLogical.Y * pixelHeight;

            // Query all supported display mode
            DEVMODEW devModeW = default;
            devModeW.dmSize = (ushort)sizeof(DEVMODEW);

            // Query the current display mode
            int refreshRate = 0;
            DisplayOrientation orientation = DisplayOrientation.Default;
            if (EnumDisplaySettingsExW((ushort*)monitorInfo.szDevice, ENUM.ENUM_CURRENT_SETTINGS, &devModeW, 0))
            {
                refreshRate = (int)devModeW.dmDisplayFrequency;
                orientation = devModeW.dmOrientation switch
                {
                    DMDO_DEFAULT => DisplayOrientation.Default,
                    DMDO_90 => DisplayOrientation.Rotate90,
                    DMDO_180 => DisplayOrientation.Rotate180,
                    DMDO_270 => DisplayOrientation.Rotate270,
                    _ => DisplayOrientation.Default,
                };
            }

            screenData = new Win32ScreenData(name, isPrimary, new Point(pixelPositionX, pixelPositionY), new SizeF(width, height), new Size(pixelWidth, pixelHeight), dpiScale, refreshRate, orientation);
            return true;
        }
        else
        {
            screenData = new Win32ScreenData(string.Empty);
        }

        return false;
    }
}