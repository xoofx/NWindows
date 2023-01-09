// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
using System.Text;

namespace NWindows;

public abstract record WindowEvent
{
    internal WindowEvent(WindowEventKind kind)
    {
        Kind = kind;
    }

    public WindowEventKind Kind { get; }
}