// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// A system event.
/// </summary>
public record SystemEvent() : WindowEvent(WindowEventKind.System)
{
    /// <summary>
    /// Gets the kind of change.
    /// </summary>
    public SystemChangeKind ChangeKind { get; set; }
}