// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows;

public struct BarHitTestEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.BarHitTest;

    public BarHitTestEvent()
    {
        Kind = WindowEventKind.BarHitTest;
    }

    public WindowEventKind Kind { get; }

    public PointF MousePosition;

    public SizeF WindowSize;

    public BarHitTest Result;

    public bool Handled;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(MousePosition)}: {MousePosition}, {nameof(WindowSize)}: {WindowSize}, {nameof(Result)}: {Result}, {nameof(Handled)}: {Handled}";
    }
}