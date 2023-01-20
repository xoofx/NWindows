// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Text;

namespace NWindows.Threading;

/// <summary>
/// A dispatcher event.
/// </summary>
public record DispatcherEvent
{
    internal DispatcherEvent(DispatcherEventKind kind)
    {
        Kind = kind;
    }

    /// <summary>
    /// Gets the kind of dispatcher event.
    /// </summary>
    public DispatcherEventKind Kind { get; }

    /// <summary>
    /// Print members of this dispatcher event.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    protected virtual bool PrintMembers(StringBuilder builder)
    {
        // Use ToText to make it compatible with NativeAOT - Reflection Free mode
        builder.Append("Kind = ");
        builder.Append(Kind.ToText());
        return true;
    }
}