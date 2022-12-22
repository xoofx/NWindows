// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public struct FrameEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Frame;

    public FrameEvent()
    {
        Kind = WindowEventKind.Frame;
    }

    public WindowEventKind Kind { get; }

    public FrameEventKind SubKind;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(SubKind)}: {SubKind}";
    }
}