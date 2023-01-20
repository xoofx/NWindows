// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/winuser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short GET_WHEEL_DELTA_WPARAM(WPARAM wParam) => unchecked((short)HIWORD((uint)wParam));
    public static nint GetWindowLongPtrW(HWND hWnd, int nIndex)
    {
        if (sizeof(nint) == 4)
        {
            return GetWindowLongW(hWnd, nIndex);
        }
        else
        {
            [DllImport("user32", EntryPoint = "GetWindowLongPtrW", ExactSpelling = true)]
            static extern nint _GetWindowLongPtrW(HWND hWnd, int nIndex);

            return _GetWindowLongPtrW(hWnd, nIndex);
        }
    }
    public static delegate*<HWND, int, nint, nint> SetWindowLongPtr => &SetWindowLongPtrW;
    public static nint SetWindowLongPtrW(HWND hWnd, int nIndex, nint dwNewLong)
    {
        if (sizeof(nint) == 4)
        {
            return SetWindowLongW(hWnd, nIndex, (int)dwNewLong);
        }
        else
        {
            [DllImport("user32", EntryPoint = "SetWindowLongPtrW", ExactSpelling = true)]
            static extern nint _SetWindowLongPtrW(HWND hWnd, int nIndex, nint dwNewLong);

            return _SetWindowLongPtrW(hWnd, nIndex, dwNewLong);
        }
    }
}
