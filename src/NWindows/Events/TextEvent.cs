// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Text;

namespace NWindows.Events;

/// <summary>
/// A text event that provide the interpreted visual character from a keyboard.
/// </summary>
public record TextEvent() : WindowEvent(WindowEventKind.Text)
{
    /// <summary>
    /// Gets the <see cref="Rune"/> character associated with the keyboard events.
    /// </summary>
    public Rune Rune { get; set; }
}