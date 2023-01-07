// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Input;

namespace NWindows.Events;

public record KeyboardEvent() : WindowEvent(WindowEventKind.Keyboard)
{
    public Key Key;

    public KeyStates State;

    public ModifierKeys Modifiers;

    public ushort ScanCode;

    public bool IsExtended;

    public bool IsSystem;

    public int Repeat;

    public bool Handled;

    public bool IsDown => (State & KeyStates.Down) != 0;

    public bool IsUp => (State & KeyStates.Down) == 0;

    public bool IsToggled => (State & KeyStates.Toggled) != 0;

    // TODO: ToText for Enums
}