// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

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
    Restored,
    FocusGained,
    FocusLost,
    Close,
    TakeFocus,
}