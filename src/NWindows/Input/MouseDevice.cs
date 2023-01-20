// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Input;

/// <summary>
/// The mouse device.
/// </summary>
public abstract class MouseDevice : InputDevice
{
    internal MouseDevice(InputManager manager) : base(manager)
    {
    }

    /// <summary>
    /// Sets the cursor for this device.
    /// </summary>
    /// <param name="cursor">The cursor.</param>
    public abstract void SetCursor(Cursor cursor);

    /// <summary>
    /// Gets or sets the position for this device.
    /// </summary>
    public abstract Point Position { get; set; }

    /// <summary>
    /// Gets the button state for the specified button.
    /// </summary>
    /// <param name="mouseButton">The mouse button</param>
    /// <returns>The state of the button.</returns>
    public abstract MouseButtonState GetButtonState(MouseButton mouseButton);
}