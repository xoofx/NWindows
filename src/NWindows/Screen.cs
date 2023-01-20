// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using NWindows.Threading;

namespace NWindows;

/// <summary>
/// Describes a screen with its properties (width, height...).
/// </summary>
public abstract class Screen : DispatcherObject
{
    internal Screen()
    {
    }

    /// <summary>
    /// Gets the default virtual screen position.
    /// </summary>
    public static Point VirtualPosition => Dispatcher.Current.ScreenManager.GetVirtualScreenPosition();

    /// <summary>
    /// Gets the size in pixel of the virtual screen.
    /// </summary>
    public static Size VirtualSizeInPixels => Dispatcher.Current.ScreenManager.GetVirtualScreenSizeInPixels();

    /// <summary>
    /// Gets the primary screen. Might be null.
    /// </summary>
    public static Screen? Primary => Dispatcher.Current.ScreenManager.GetPrimaryScreen();

    /// <summary>
    /// Gets the primary DPI or a default DPI if no Primary screens are attached.
    /// </summary>
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

    /// <summary>
    /// Gets the list of all active screens.
    /// </summary>
    public static ReadOnlySpan<Screen> List => Dispatcher.Current.ScreenManager.GetAllScreens();

    /// <summary>
    /// Gets a boolean indicating if this instance is valid.
    /// </summary>
    public abstract bool IsValid { get; }

    /// <summary>
    /// Gets a boolean indicating if this screen is the primary screen.
    /// </summary>
    public abstract bool IsPrimary { get; }

    /// <summary>
    /// Gets the name of this screen.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the associated native handle associated to this screen.
    /// </summary>
    /// <remarks>
    /// - On Windows: this is a HMONITOR.
    /// </remarks>
    public IntPtr Handle { get; protected set; }

    /// <summary>
    /// Gets the position of this screen.
    /// </summary>
    public abstract Point Position { get; }

    /// <summary>
    /// Gets the size in pixels of this screen.
    /// </summary>
    public abstract Size SizeInPixels { get; }

    /// <summary>
    /// Gets the boundaries (<see cref="Position"/> and <see cref="SizeInPixels"/>).
    /// </summary>
    public Rectangle Bounds => new Rectangle(Position, SizeInPixels);

    /// <summary>
    /// Gets the logical size according the screens <see cref="Dpi"/>.
    /// </summary>
    public abstract SizeF Size { get; }

    /// <summary>
    /// Gets the current DPI of this screen.
    /// </summary>
    public abstract ref readonly Dpi Dpi { get; }

    /// <summary>
    /// Gets the current refresh rate of this screen.
    /// </summary>
    public abstract int RefreshRate { get; }

    /// <summary>
    /// Gets the current orientation of this screen.
    /// </summary>
    public abstract DisplayOrientation DisplayOrientation { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Screen {nameof(Name)}: {Name}, {nameof(IsPrimary)}: {IsPrimary}, {nameof(Position)}: {Position}, {nameof(SizeInPixels)}: {SizeInPixels}, {nameof(Size)}: {Size}, {nameof(Dpi)}: {Dpi}, {nameof(RefreshRate)}: {RefreshRate}, {nameof(DisplayOrientation)}: {DisplayOrientation}";
    }
}