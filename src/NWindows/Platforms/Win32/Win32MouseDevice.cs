// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using NWindows.Input;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Platforms.Win32;

internal sealed class Win32MouseDevice : MouseDevice
{
    public Win32MouseDevice(InputManager manager) : base(manager)
    {
        // TODO: Anything to make it non active? (e.g no mouse device)
        IsActive = true;
    }

    public override bool IsActive { get; }

    public override void SetCursor(Cursor cursor)
    {
        if (!IsActive) return;

        Windows.SetCursor((HCURSOR)cursor.Handle);
    }

    public override unsafe Point Position
    {
        get
        {
            if (!IsActive) return default;

            var point = new Point();
            GetCursorPos((POINT*)&point);
            return point;
        }
        set
        {
            if (!IsActive) return;

            SetCursorPos(value.X, value.Y);
        }
    }

    public override MouseButtonState GetButtonState(MouseButton mouseButton)
    {
        MouseButtonState mouseButtonState = MouseButtonState.Released;

        if (IsActive)
        {
            int virtualKeyCode = 0;

            switch (mouseButton)
            {
                case MouseButton.Left:
                    virtualKeyCode = VK.VK_LBUTTON;
                    break;
                case MouseButton.Right:
                    virtualKeyCode = VK.VK_RBUTTON;
                    break;
                case MouseButton.Middle:
                    virtualKeyCode = VK.VK_MBUTTON;
                    break;
                case MouseButton.XButton1:
                    virtualKeyCode = VK.VK_XBUTTON1;
                    break;
                case MouseButton.XButton2:
                    virtualKeyCode = VK.VK_XBUTTON2;
                    break;
            }

            mouseButtonState = (GetKeyState(virtualKeyCode) & 0x8000) != 0 ? MouseButtonState.Pressed : MouseButtonState.Released;
        }

        return mouseButtonState;
    }
}