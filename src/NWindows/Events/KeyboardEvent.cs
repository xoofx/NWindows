// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Input;

namespace NWindows.Events;

/// <summary>
/// A keyboard event.
/// </summary>
public record KeyboardEvent() : WindowEvent(WindowEventKind.Keyboard)
{
    /// <summary>
    /// Gets the key.
    /// </summary>
    public Key Key { get; set; }

    /// <summary>
    /// Gets the states of the key.
    /// </summary>
    public KeyStates State { get; set; }

    /// <summary>
    /// Gets the key modifiers.
    /// </summary>
    public ModifierKeys Modifiers { get; set; }

    /// <summary>
    /// Gets the platform dependent scan code.
    /// </summary>
    public ushort ScanCode { get; set; }

    /// <summary>
    /// Gets a boolean indicating if this key is extended.
    /// </summary>
    public bool IsExtended { get; set; }

    /// <summary>
    /// Gets a boolean indicating if this key is a system key.
    /// </summary>
    public bool IsSystem;

    /// <summary>
    /// Gets the repeat count for this key.
    /// </summary>
    public int Repeat;

    /// <summary>
    /// Gets or sets a boolean indicating if this key event was handled.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Gets a boolean indicating if the key is pressed down.
    /// </summary>
    public bool IsDown => (State & KeyStates.Down) != 0;

    /// <summary>
    /// Gets a boolean indicating if the key is pressed up.
    /// </summary>
    public bool IsUp => (State & KeyStates.Down) == 0;

    /// <summary>
    /// Gets a boolean indicating if the key is toggled.
    /// </summary>
    public bool IsToggled => (State & KeyStates.Toggled) != 0;

    // TODO: ToText for Enums
}