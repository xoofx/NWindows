// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Input;

/// <summary>
/// The Keyboard class represents the mouse device to the
/// members of a context.
/// </summary>
/// <remarks>
/// The static members of this class simply delegate to the primary
/// keyboard device of the calling thread's input manager.
/// </remarks>
public static class Keyboard
{
    /// <summary>
    /// The set of modifier keys currently pressed.
    /// </summary>
    public static ModifierKeys Modifiers
    {
        get
        {
            return PrimaryDevice.Modifiers;
        }
    }

    /// <summary>
    /// Returns whether or not the specified key is down.
    /// </summary>
    public static bool IsKeyDown(Key key)
    {
        return PrimaryDevice.IsKeyDown(key);
    }

    /// <summary>
    /// Returns whether or not the specified key is up.
    /// </summary>
    public static bool IsKeyUp(Key key)
    {
        return PrimaryDevice.IsKeyUp(key);
    }

    /// <summary>
    /// Returns whether or not the specified key is toggled.
    /// </summary>
    public static bool IsKeyToggled(Key key)
    {
        return PrimaryDevice.IsKeyToggled(key);
    }

    /// <summary>
    /// Returns the state of the specified key.
    /// </summary>
    public static KeyStates GetKeyStates(Key key)
    {
        return PrimaryDevice.GetKeyStates(key);
    }

    /// <summary>
    /// The primary keyboard device.
    /// </summary>
    public static KeyboardDevice PrimaryDevice => InputManager.Current.PrimaryKeyboardDevice;
}