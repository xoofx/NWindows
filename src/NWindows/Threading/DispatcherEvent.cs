// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Text;

namespace NWindows.Threading;

public record DispatcherEvent
{
    internal DispatcherEvent(DispatcherEventKind kind)
    {
        Kind = kind;
    }

    public DispatcherEventKind Kind { get; }

    protected virtual bool PrintMembers(StringBuilder builder)
    {
        // Use ToText to make it compatible with NativeAOT - Reflection Free mode
        builder.Append("Kind = ");
        builder.Append(Kind.ToText());
        return true;
    }
}