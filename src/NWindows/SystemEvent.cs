// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public struct SystemEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.System;

    public SystemEvent()
    {
        Kind = WindowEventKind.System;
    }

    public WindowEventKind Kind { get; }

    public SystemEventKind SubKind;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(SubKind)}: {SubKind}";
    }
}