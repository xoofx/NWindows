// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows;

public struct PaintEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Paint;

    public PaintEvent()
    {
        Kind = WindowEventKind.Paint;
    }

    public WindowEventKind Kind { get; }

    public RectangleF Bounds;

    public bool Handled;

    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(Bounds)}: {Bounds}";
    }
}