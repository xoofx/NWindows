// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;

namespace NWindows.Input;

public static class Mouse
{
    /// <summary>
    /// Sets the cursor
    /// </summary>
    public static void SetCursor(Cursor cursor) => InputManager.Current.PrimaryMouseDevice.SetCursor(cursor);
    
    /// <summary>
    /// Gets or sets the virtual screen position in pixel.
    /// </summary>
    public static Point Position
    {
        get => InputManager.Current.PrimaryMouseDevice.Position;
        set => InputManager.Current.PrimaryMouseDevice.Position = value;
    }
    
    /// <summary>
    /// The state of the left button.
    /// </summary>
    public static MouseButtonState LeftButton => GetButtonState(MouseButton.Left);

    /// <summary>
    /// The state of the right button.
    /// </summary>
    public static MouseButtonState RightButton => GetButtonState(MouseButton.Right);

    /// <summary>
    /// The state of the middle button.
    /// </summary>
    public static MouseButtonState MiddleButton => GetButtonState(MouseButton.Middle);

    /// <summary>
    /// The state of the first extended button.
    /// </summary>
    public static MouseButtonState XButton1 => GetButtonState(MouseButton.XButton1);

    /// <summary>
    /// The state of the second extended button.
    /// </summary>
    public static MouseButtonState XButton2 => GetButtonState(MouseButton.XButton2);

    private static MouseButtonState GetButtonState(MouseButton mouseButton) => InputManager.Current.PrimaryMouseDevice.GetButtonState(mouseButton);
}