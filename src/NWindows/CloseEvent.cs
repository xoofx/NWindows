// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public struct CloseEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Close;

    public CloseEvent()
    {
        Kind = WindowEventKind.Close;
    }

    public WindowEventKind Kind { get; }

    public bool Cancel;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(Cancel)}: {Cancel}";
    }
}