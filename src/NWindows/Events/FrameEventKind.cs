// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

public enum FrameEventKind
{
    None,
    Created,
    Destroyed,
    PositionAndSizeChanged,
    Shown,
    Hidden,
    Moved,
    Resized,
    Minimized,
    Maximized,
    FullScreen,
    Restored,
    FocusGained,
    FocusLost,
    MinimumSizeChanged,
    MaximumSizeChanged,
    ResizeableChanged,
    StateChanged,
    OpacityChanged,
    TopMostChanged,
    ModalChanged,
    Enabled,
    Disabled,
    TitleChanged,
    MaximizeableChanged,
    MinimizeableChanged,
    BackgroundColorChanged,
    ShowInTaskBarChanged,
    DpiChanged,
    DpiModeChanged,
    DragDropChanged,
    ClipboardChanged,
    DecorationsChanged,
    ThemeChanged,
    ThemeSyncModeChanged
}