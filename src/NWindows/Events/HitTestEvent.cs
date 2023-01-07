// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Events;

public record HitTestEvent() : WindowEvent(WindowEventKind.HitTest)
{
    public PointF MousePosition;

    public SizeF WindowSize;

    public HitTest Result;

    public bool Handled;

    // TODO: ToText to HitTest
}