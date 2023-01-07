// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows.Input;

[Flags]
public enum MouseButtonFlags
{
    None = 0,
    LeftButton = 1 << 0,
    MiddleButton = 1 << 1,
    RightButton = 1 << 2,
    Button1 = 1 << 3,
    Button2 = 1 << 4,
}