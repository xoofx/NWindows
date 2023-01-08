// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Input;
using TerraFX.Interop.Windows;

namespace NWindows.Interop.Win32;

internal class Win32KeyboardDevice : KeyboardDevice
{
    public Win32KeyboardDevice(InputManager manager) : base(manager)
    {
        IsActive = true;
    }

    public override bool IsActive { get; }

    internal override KeyStates GetKeyStatesFromSystem(Key key)
    {
        KeyStates keyStates = KeyStates.None;

        int virtualKeyCode = Win32KeyInterop.VirtualKeyFromKey(key);
        int nativeKeyState = Windows.GetKeyState(virtualKeyCode);

        if ((nativeKeyState & 0x00008000) == 0x00008000)
            keyStates |= KeyStates.Down;

        if ((nativeKeyState & 0x00000001) == 0x00000001)
            keyStates |= KeyStates.Toggled;

        return keyStates;
    }

}