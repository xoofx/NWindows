// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public enum WindowEventKind
{
    Undefined,
    Idle,
    Shutdown,
    System,
    Application,
    Frame,
    Paint,
    HitTest,
    Keyboard,
    Mouse,
    Close,
    Text,

    // TODO: add touch, gesture, clipboard, drag and drop
}