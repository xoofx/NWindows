// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Threading;

namespace NWindows.Input;

/// <summary>
/// The input manager.
/// </summary>
public abstract class InputManager : DispatcherObject
{
    internal InputManager(Dispatcher dispatcher) : base(dispatcher)
    {
    }

    /// <summary>
    /// Gets the primary keyboard device.
    /// </summary>
    public abstract KeyboardDevice PrimaryKeyboardDevice { get; }

    /// <summary>
    /// Gets the primary mouse device.
    /// </summary>
    public abstract MouseDevice PrimaryMouseDevice { get; }

    /// <summary>
    /// Gets the current input manager associated with the current dispatcher.
    /// </summary>
    public static InputManager Current => Dispatcher.Current.InputManager;
}