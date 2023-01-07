// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Events;

public record DpiChangedEvent : FrameEvent
{
    public DpiChangedEvent()
    {
        FrameKind = FrameEventKind.DpiChanged;
    }

    public Point PreviousValue;

    public Point NewValue;
}