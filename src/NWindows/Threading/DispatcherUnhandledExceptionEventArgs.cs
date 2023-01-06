// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows.Threading;

public sealed class DispatcherUnhandledExceptionEventArgs : DispatcherEventArgs
{
    internal DispatcherUnhandledExceptionEventArgs(Dispatcher dispatcher)
        : base(dispatcher)
    {
    }

    public Exception? Exception { get; internal set; }

    public bool Handled { get; set; }
}