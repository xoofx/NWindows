// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Threading;

public sealed record IdleEvent() : DispatcherEvent(DispatcherEventKind.Idle)
{
    public bool Continuous { get; set; }
}