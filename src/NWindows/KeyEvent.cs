// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public struct KeyEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Keyboard;

    public KeyEvent()
    {
        Kind = WindowEventKind.Keyboard;
    }

    public WindowEventKind Kind { get; }

    public KeyState State;

    public ushort ScanCode;

    public Key Key;

    public ModifierKeys Modifiers;
}