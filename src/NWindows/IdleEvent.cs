// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public struct IdleEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Idle;

    public IdleEvent()
    {
        Kind = WindowEventKind.Idle;
    }

    public WindowEventKind Kind { get; }

    public bool Continuous;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(Continuous)}: {Continuous}";
    }
}