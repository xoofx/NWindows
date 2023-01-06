// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics;

namespace NWindows.Threading;

public class DispatcherEventArgs : EventArgs
{
    internal DispatcherEventArgs(Dispatcher dispatcher)
    {
        Debug.Assert(dispatcher != null);
        Dispatcher = dispatcher;
    }

    public Dispatcher Dispatcher { get; }
}