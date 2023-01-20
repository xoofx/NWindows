// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// This close event is triggered when a window is going to be closed. It can be used to cancel the closing event.
/// </summary>
public record CloseEvent() : WindowEvent(WindowEventKind.Close)
{
    /// <summary>
    /// Gets or sets a boolean indicating that the close event should be cancelled. Default is <c>false</c>.
    /// </summary>
    public bool Cancel { get; set; }
}