// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// The kind of change for a <see cref="FrameEvent"/>.
/// </summary>
public enum FrameChangeKind
{
    None,
    /// <summary>
    /// The window was created.
    /// </summary>
    Created,

    /// <summary>
    /// The window is destroyed.
    /// </summary>
    Destroyed,

    /// <summary>
    /// The position and size of the window have changed.
    /// </summary>
    PositionAndSizeChanged,

    /// <summary>
    /// The window is visible.
    /// </summary>
    Shown,

    /// <summary>
    /// The window is hidden.
    /// </summary>
    Hidden,

    /// <summary>
    /// The window has been moved (but its size is not modified. See <see cref="PositionAndSizeChanged"/>)
    /// </summary>
    Moved,

    /// <summary>
    /// The window has been resized (but its position is not modified. See <see cref="PositionAndSizeChanged"/>)
    /// </summary>
    Resized,

    /// <summary>
    /// The window has been minimized.
    /// </summary>
    Minimized,

    /// <summary>
    /// The window has been maximized.
    /// </summary>
    Maximized,

    /// <summary>
    /// The window is fullscreen.
    /// </summary>
    FullScreen,

    /// <summary>
    /// The window has been restored to its standard position.
    /// </summary>
    Restored,

    /// <summary>
    /// The window has gained keyboard focus.
    /// </summary>
    FocusGained,

    /// <summary>
    /// The window has lost keyboard focus.
    /// </summary>
    FocusLost,

    /// <summary>
    /// The <see cref="Window.MinimumSize"/> has been changed.
    /// </summary>
    MinimumSizeChanged,

    /// <summary>
    /// The <see cref="Window.MaximumSize"/> has been changed.
    /// </summary>
    MaximumSizeChanged,

    /// <summary>
    /// The <see cref="Window.Resizeable"/> has been changed.
    /// </summary>
    ResizeableChanged,

    /// <summary>
    /// The <see cref="Window.State"/> has been changed.
    /// </summary>
    StateChanged,

    /// <summary>
    /// The <see cref="Window.Opacity"/> has been changed.
    /// </summary>
    OpacityChanged,

    /// <summary>
    /// The <see cref="Window.TopMost"/> has been changed.
    /// </summary>
    TopMostChanged,

    /// <summary>
    /// The <see cref="Window.Modal"/> has been changed.
    /// </summary>
    ModalChanged,

    /// <summary>
    /// The window is enabled.
    /// </summary>
    Enabled,

    /// <summary>
    /// The window is disabled.
    /// </summary>
    Disabled,

    /// <summary>
    /// The <see cref="Window.Title"/> has been changed.
    /// </summary>
    TitleChanged,

    /// <summary>
    /// The <see cref="Window.Maximizeable"/> has been changed.
    /// </summary>
    MaximizeableChanged,

    /// <summary>
    /// The <see cref="Window.Minimizeable"/> has been changed.
    /// </summary>
    MinimizeableChanged,

    /// <summary>
    /// The <see cref="Window.BackgroundColor"/> has been changed.
    /// </summary>
    BackgroundColorChanged,
    
    /// <summary>
    /// The <see cref="Window.ShowInTaskBar"/> has been changed.
    /// </summary>
    ShowInTaskBarChanged,

    /// <summary>
    /// The <see cref="Window.Dpi"/> has been changed.
    /// </summary>
    DpiChanged,

    /// <summary>
    /// The <see cref="Window.DpiMode"/> has been changed.
    /// </summary>
    DpiModeChanged,

    /// <summary>
    /// The <see cref="Window.DragDrop"/> has been changed.
    /// </summary>
    DragDropChanged,

    /// <summary>
    /// The clipboard has changed.
    /// </summary>
    ClipboardChanged,

    /// <summary>
    /// The <see cref="Window.Decorations"/> has been changed.
    /// </summary>
    DecorationsChanged,

    /// <summary>
    /// The Window system theme has changed.
    /// </summary>
    ThemeChanged,

    /// <summary>
    /// The <see cref="Window.ThemeSyncMode"/> has been changed.
    /// </summary>
    ThemeSyncModeChanged
}