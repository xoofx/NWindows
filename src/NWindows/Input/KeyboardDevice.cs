// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows.Input;

/// <summary>
/// Abstract class that represents a keyboard device.
/// </summary>
public abstract class KeyboardDevice : InputDevice
{
    internal KeyboardDevice(InputManager manager) : base(manager)
    {
    }

    /// <summary>
    /// Gets the modifier keys pressed.
    /// </summary>
    public ModifierKeys Modifiers
    {
        get
        {
            VerifyAccess();

            var modifiers = ModifierKeys.None;
            if (IsKeyDownInternal(Key.LeftAlt) || IsKeyDownInternal(Key.RightAlt))
            {
                modifiers |= ModifierKeys.Alt;
            }
            if (IsKeyDownInternal(Key.LeftCtrl) || IsKeyDownInternal(Key.RightCtrl))
            {
                modifiers |= ModifierKeys.Control;
            }
            if (IsKeyDownInternal(Key.LeftShift) || IsKeyDownInternal(Key.RightShift))
            {
                modifiers |= ModifierKeys.Shift;
            }

            return modifiers;
        }
    }

    /// <summary>
    /// Gets the state of the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The state of the specified key.</returns>
    public KeyStates GetKeyStates(Key key)
    {
        ValidateKey(key);
        return GetKeyStatesFromSystem(key);
    }

    /// <summary>
    /// Gets a boolean indicating whether the specified key is pressed down.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the specified key is pressed down; otherwise <c>false</c>.</returns>
    public bool IsKeyDown(Key key)
    {
        ValidateKey(key);
        return IsKeyDownInternal(key);
    }

    /// <summary>
    /// Gets a boolean indicating whether the specified key is toggled.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the specified key is toggled down; otherwise <c>false</c>.</returns>
    public bool IsKeyToggled(Key key)
    {
        ValidateKey(key);
        return IsKeyToggledInternal(key);
    }

    /// <summary>
    /// Gets a boolean indicating whether the specified key is pressed up.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns><c>true</c> if the specified key is pressed up; otherwise <c>false</c>.</returns>
    public bool IsKeyUp(Key key)
    {
        ValidateKey(key);
        return IsKeyUpInternal(key);
    }

    internal abstract KeyStates GetKeyStatesFromSystem(Key key);

    private bool IsKeyDownInternal(Key key) => (GetKeyStatesFromSystem(key) & KeyStates.Down) != 0;
    private bool IsKeyToggledInternal(Key key) => (GetKeyStatesFromSystem(key) & KeyStates.Toggled) != 0;
    private bool IsKeyUpInternal(Key key) => (GetKeyStatesFromSystem(key) & KeyStates.Down) == 0;

    private void ValidateKey(Key key)
    {
        VerifyAccess();
        if (key > Key.DeadCharProcessed) throw new ArgumentOutOfRangeException(nameof(key), $"The key code 0x{(ushort)key:x4} is out of range (Maximum value: 0x{(ushort)Key.DeadCharProcessed:x4})");
    }
}