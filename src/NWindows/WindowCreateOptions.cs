// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;

namespace NWindows;

/// <summary>
/// Options for creating a window.
/// </summary>
public sealed record WindowCreateOptions
{
    /// <summary>
    /// Sets a kind of window. Default is <see cref="WindowKind.TopLevel"/>
    /// </summary>
    public WindowKind Kind { get; init; } = WindowKind.TopLevel;

    /// <summary>
    /// Sets the desired background color for the window. Default is system dependent.
    /// </summary>
    public Color? BackgroundColor { get; init; } = null;
    
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
    /// Sets the icon of the window. The default is the default application icon.
    /// </summary>
    public Icon? Icon { get; init; } = null;

    /// <summary>
    /// Sets the native parent window handle. Required for a popup window.
    /// </summary>
    public INativeWindow? Parent { get; init; } = null;

    /// <summary>
    /// Only valid for a top level window, the window will be shown in the task bar when created. Default is <c>true</c>.
    /// </summary>
    public bool ShowInTaskBar { get; init; } = true;
    
    /// <summary>
    /// Sets whether drag and drop for files is supported for the window. Default is <c>false</c>.
    /// </summary>
    public bool DragDrop { get; init; } = false;

    /// <summary>
    /// Sets the window event delegate that will receive events.
    /// </summary>
    public WindowEventHub Events { get; init; } = new WindowEventHub();
    
    /// <summary>
    /// Sets the window start position (default, center parent, center screen)
    /// </summary>
    public WindowStartPosition StartPosition { get; init; } = WindowStartPosition.Default;

    /// <summary>
    /// Sets the factor relative to the screen size if the size is not specified. Default is <c>0.6f</c>.
    /// </summary>
    /// <remarks>
    /// Maximum is 1.0f, minimum factor is 0.1f.
    /// </remarks>
    public PointF DefaultSizeFactor { get; init; } = new PointF(0.6f, 0.6f);

    /// <summary>
    /// Verify options and throw an exception if an invalid setup is provided.
    /// </summary>
    public void Verify()
    {
        if ((Kind == WindowKind.Popup || Kind == WindowKind.Child) && Parent is null)
        {
            throw new InvalidOperationException("Invalid options. A non TopLevel window must have a Parent window.");
        }
    }
}