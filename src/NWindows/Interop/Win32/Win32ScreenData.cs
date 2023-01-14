// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Interop.Win32;

internal readonly struct Win32ScreenData : IEquatable<Win32ScreenData>
{
    private Win32ScreenData(string name, bool isPrimary, Point position, SizeF size, Size sizeInPixels, DpiScale dpiScale, in ScreenMode currentDisplayMode, ScreenMode[] screenModes)
    {
        IsValid = true;
        Name = name;
        IsPrimary = isPrimary;
        Position = position;
        Size = size;
        SizeInPixels = sizeInPixels;
        DpiScale = dpiScale;
        CurrentDisplayMode = currentDisplayMode;
        ScreenModes = screenModes;
    }

    private Win32ScreenData(string name)
    {
        IsValid = false;
        Name = name;
        ScreenModes = Array.Empty<ScreenMode>();
    }

    public readonly bool IsValid;

    public readonly string Name;

    public readonly bool IsPrimary;

    public readonly Point Position;

    public readonly SizeF Size;

    public readonly Size SizeInPixels;

    public readonly DpiScale DpiScale;

    public readonly ScreenMode CurrentDisplayMode;

    public readonly ScreenMode[] ScreenModes;

    public bool Equals(Win32ScreenData other)
    {
        return IsValid == other.IsValid && Name == other.Name && IsPrimary == other.IsPrimary && Position.Equals(other.Position) && Size.Equals(other.Size) && DpiScale.Equals(other.DpiScale) && CurrentDisplayMode.Equals(other.CurrentDisplayMode) && ScreenModes.AsSpan().SequenceEqual(other.ScreenModes);
    }

    public override bool Equals(object? obj)
    {
        return obj is Win32ScreenData other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsValid, Name, IsPrimary, Position, Size, DpiScale);
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

            var dpiScale = new DpiScale(dpiX, dpiY);

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

            DEVMODEW devModeW = default;
            devModeW.dmSize = (ushort)sizeof(DEVMODEW);
            uint modeIndex = 0;
            var displayModes = new List<ScreenMode>(128);
            while (EnumDisplaySettingsExW((ushort*)monitorInfo.szDevice, modeIndex, &devModeW, 0))
            {
                // Skip exotic displays (interlaced, greyscale)
                if (devModeW.dmDisplayFlags != 0)
                {
                    continue;
                }

                ToDisplayMode(devModeW, out var displayMode);
                displayModes.Add(displayMode);
                modeIndex++;
            }
            displayModes.Sort(ScreenModeComparer.Instance);

            ScreenMode currentDisplayMode = default;
            if (EnumDisplaySettingsExW((ushort*)monitorInfo.szDevice, ENUM.ENUM_CURRENT_SETTINGS, &devModeW, 0))
            {
                ToDisplayMode(devModeW, out currentDisplayMode);
            }

            screenData = new Win32ScreenData(name, isPrimary, new Point(pixelPositionX, pixelPositionY), new SizeF(width, height), new Size(pixelWidth, pixelHeight), dpiScale, currentDisplayMode, displayModes.ToArray());
            return true;
        }
        else
        {
            screenData = new Win32ScreenData(string.Empty);
        }

        return false;
    }

    private static void ToDisplayMode(in DEVMODEW devModeW, out ScreenMode screenMode)
    {
        var orientation = devModeW.dmOrientation switch
        {
            DMDO_DEFAULT => DisplayOrientation.Default,
            DMDO_90 => DisplayOrientation.Rotate90,
            DMDO_180 => DisplayOrientation.Rotate180,
            DMDO_270 => DisplayOrientation.Rotate270,
            _ => DisplayOrientation.Default,
        };

        screenMode = new ScreenMode((int)devModeW.dmPelsWidth, (int)devModeW.dmPelsHeight, (int)devModeW.dmBitsPerPel, (int)devModeW.dmDisplayFrequency, orientation);
    }

    private class ScreenModeComparer : IComparer<ScreenMode>
    {
        public static readonly ScreenModeComparer Instance = new ScreenModeComparer();

        public int Compare(ScreenMode x, ScreenMode y)
        {
            var widthComparison = x.Width.CompareTo(y.Width);
            if (widthComparison != 0)
            {
                return widthComparison;
            }

            var heightComparison = x.Height.CompareTo(y.Height);
            if (heightComparison != 0)
            {
                return heightComparison;
            }

            var bitsPerPixelComparison = x.BitsPerPixel.CompareTo(y.BitsPerPixel);
            if (bitsPerPixelComparison != 0)
            {
                return bitsPerPixelComparison;
            }

            var frequencyComparison = x.Frequency.CompareTo(y.Frequency);
            if (frequencyComparison != 0)
            {
                return frequencyComparison;
            }

            return x.Orientation.CompareTo(y.Orientation);
        }
    }
}