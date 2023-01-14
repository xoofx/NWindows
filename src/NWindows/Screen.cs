// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Threading;

namespace NWindows;

public abstract class Screen : DispatcherObject
{
    internal Screen()
    {
    }

    public static Point VirtualPosition => Dispatcher.Current.ScreenManager.GetVirtualScreenPosition();

    public static Size VirtualSizeInPixels => Dispatcher.Current.ScreenManager.GetVirtualScreenSizeInPixels();

    public static Screen? Primary => Dispatcher.Current.ScreenManager.GetPrimaryScreen();

    public static ref readonly Dpi PrimaryDpi
    {
        get
        {
            if (Primary is { } primary)
            {
                return ref primary.Dpi;
            }
            return ref Dpi.Default;
        }
    }

    public static ReadOnlySpan<Screen> List => Dispatcher.Current.ScreenManager.GetAllScreens();

    public abstract bool IsValid { get; }

    public abstract bool IsPrimary { get; }

    public abstract string Name { get; }

    public IntPtr Handle { get; protected set; }

    public abstract Point Position { get; }

    public abstract Size SizeInPixels { get; }

    public Rectangle Bounds => new Rectangle(Position, SizeInPixels);

    public abstract SizeF Size { get; }
    
    public abstract ref readonly Dpi Dpi { get; }

    public abstract ref readonly ScreenMode CurrentDisplayMode { get; }
    
    public abstract ref readonly ScreenMode SystemDisplayMode { get; }

    public abstract ReadOnlySpan<ScreenMode> DisplayModes { get; }

    public override string ToString()
    {
        return $"Screen {nameof(Name)}: {Name}, {nameof(IsPrimary)}: {IsPrimary}, {nameof(Position)}: {Position}, {nameof(SizeInPixels)}: {SizeInPixels}, {nameof(Size)}: {Size}, {nameof(Dpi)}: {Dpi}, {nameof(DisplayModes)}: {DisplayModes.Length}";
    }
}