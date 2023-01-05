// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Input;

namespace NWindows;

public struct KeyboardEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Keyboard;

    public KeyboardEvent()
    {
        Kind = WindowEventKind.Keyboard;
    }

    public WindowEventKind Kind { get; }

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
    
    public override string ToString()
    {
        // TODO: use ToText for enums to make it compatible with NativeAOT reflection-free
        return $"{nameof(Kind)}: {Kind}, {nameof(Key)}: {Key}, {nameof(State)}: {State}, {nameof(Modifiers)}: {Modifiers}, {nameof(ScanCode)}: {ScanCode}, {nameof(IsExtended)}: {IsExtended}, {nameof(IsSystem)}: {IsSystem}, {nameof(Repeat)}: {Repeat}, {nameof(Handled)}: {Handled}";
    }
}