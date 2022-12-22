// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public interface IWindowEvent
{
    public WindowEventKind Kind { get; }

    internal static abstract WindowEventKind StaticKind { get; }
}