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

    public static Point PrimaryDpi => Primary?.Dpi ?? new(96, 96);

    public static ReadOnlySpan<Screen> Items => Dispatcher.Current.ScreenManager.GetAllScreens();

    public abstract bool IsValid { get; }

    public abstract bool IsPrimary { get; }

    public abstract string Name { get; }

    public IntPtr Handle { get; protected set; }

    public abstract Point Position { get; }

    public abstract Size SizeInPixels { get; }

    public abstract SizeF Size { get; }
    
    public abstract Point Dpi { get; }

    public override string ToString()
    {
        return $"Screen {nameof(Name)}: {Name}, {nameof(IsPrimary)}: {IsPrimary}, {nameof(Position)}: {Position}, {nameof(SizeInPixels)}: {SizeInPixels}, {nameof(Size)}: {Size}, {nameof(Dpi)}: {Dpi}";
    }
}