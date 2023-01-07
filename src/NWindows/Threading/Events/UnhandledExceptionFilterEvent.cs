// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows.Threading.Events;

public sealed record UnhandledExceptionFilterEvent() : DispatcherEvent(DispatcherEventKind.UnhandledExceptionFilter)
{
    public Exception? Exception { get; internal set; }

    public bool RequestCatch { get; set; }
}