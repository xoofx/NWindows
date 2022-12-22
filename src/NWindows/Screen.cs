// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

public abstract partial class Screen : DispatcherObject
{
    internal Screen()
    {
    }

    public static Point VirtualPosition => Dispatcher.Current.ScreenManager.GetVirtualScreenPosition();

    public static Size VirtualSize => Dispatcher.Current.ScreenManager.GetVirtualScreenSize();

    public static Screen? Primary => Dispatcher.Current.ScreenManager.GetPrimaryScreen();

    public static ReadOnlySpan<Screen> Items => Dispatcher.Current.ScreenManager.GetAllScreens();

    public abstract bool IsValid { get; }

    public abstract bool IsPrimary { get; }

    public abstract string Name { get; }

    public IntPtr Handle { get; protected set; }

    public abstract Point Position { get; }

    public abstract Point Dpi { get; }

    public abstract SizeF Size { get; }
}