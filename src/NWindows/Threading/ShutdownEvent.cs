// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Threading;

public sealed record ShutdownEvent : DispatcherEvent
{
    public static readonly DispatcherEvent ShutdownStarted = new ShutdownEvent(DispatcherEventKind.ShutdownStarted);
    public static readonly DispatcherEvent ShutdownFinished = new ShutdownEvent(DispatcherEventKind.ShutdownFinished);

    internal ShutdownEvent(DispatcherEventKind kind) : base(kind)
    {

    }
}