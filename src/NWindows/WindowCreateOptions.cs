// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

/// <summary>
/// Options for creating a window.
/// </summary>
public readonly struct WindowCreateOptions
{
    /// <summary>
    /// Creates a new instance of this object with the specified window event delegate.
    /// </summary>
    /// <param name="windowEventHandler">The window event delegate.</param>
    public WindowCreateOptions(WindowEventDelegate windowEventHandler)
    {
        WindowEventHandler = windowEventHandler;
    }

    /// <summary>
    /// Sets a kind of window. Default is <see cref="WindowKind.TopLevel"/>
    /// </summary>
    public WindowKind Kind { get; init; } = WindowKind.TopLevel;
    
    /// <summary>
    /// Sets a desired default position for the window.
    /// </summary>
    /// <remarks>
    /// If this is not set, some platform-specific position will be chosen.
    /// </remarks>
    public Point? Position { get; init; } = null;

    /// <summary>
    /// Sets a desired default size for the window.
    /// </summary>
    /// <remarks>
    /// If this is not set, some platform-specific dimensions will be used.
    /// </remarks>
    public Size? Size { get; init; } = null;

    /// <summary>
    /// Sets a minimum size for the window.
    /// </summary>
    /// <remarks>
    /// If this is not set, the window will have no minimum dimensions (aside from reserved).
    /// </remarks>
    public SizeF? MinimumSize { get; init; } = null;

    /// <summary>
    /// Sets a the maximum size for the window.
    /// </summary>
    /// <remarks>
    /// If this is not set, the window will have no maximum or will be set to the primary monitor’s dimensions by the platform.
    /// </remarks>
    public SizeF? MaximumSize { get; init; } = null;

    /// <summary>
    /// Sets whether the window is resizable or not. Default is <c>true</c>.
    /// </summary>
    public bool Resizable { get; init; } = true;

    /// <summary>
    /// Sets the initial title of the window in the title bar. The default is "NWindows Window".
    /// </summary>
    public string Title { get; init; } = "NWindows Window";

    /// <summary>
    /// Sets whether the window should go fullscreen upon creation. Default is <c>false</c>.
    /// </summary>
    public bool FullScreen { get; init; } = false;

    /// <summary>
    /// Sets whether the window should be minimized upon creation. Default is <c>false</c>.
    /// </summary>
    public bool Minimized { get; init; } = false;

    /// <summary>
    /// Sets whether the window should be maximized upon creation. Default is <c>false</c>.
    /// </summary>
    public bool Maximized { get; init; } = false;

    /// <summary>
    /// Sets whether the window should be visible upon creation. Default is <c>true</c>.
    /// </summary>
    public bool Visible { get; init; } = true;

    /// <summary>
    /// Sets whether the background of the window should be transparent. Default is <c>false</c>.
    /// </summary>
    public bool Transparent { get; init; } = false;

    /// <summary>
    /// Sets whether the window should have a border, a title bar, etc. Default is <c>true</c>.
    /// </summary>
    public bool Decorations { get; init; } = true;
    
    /// <summary>
    /// Sets whether the window can be maximizable. Default is <c>true</c>.
    /// </summary>
    public bool Maximizable { get; init; } = true;

    /// <summary>
    /// Sets whether the window can be minimizable. Default is <c>true</c>.
    /// </summary>
    public bool Minimizable { get; init; } = true;

    /// <summary>
    /// Sets the icon of the window.
    /// </summary>
    public Icon? Icon { get; init; } = null;

    /// <summary>
    /// Sets the native parent window handle. Required for a popup window.
    /// </summary>
    public Window? ParentWindow { get; init; } = null;

    /// <summary>
    /// Sets the window event delegate that will receive events.
    /// </summary>
    public WindowEventDelegate WindowEventHandler { get; init; }
}