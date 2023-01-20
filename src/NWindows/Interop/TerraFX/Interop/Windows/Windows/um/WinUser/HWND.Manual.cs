// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/winuser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal unsafe partial struct HWND
{
    public static HWND HWND_MESSAGE => ((HWND)(-3));
    public static HWND HWND_TOP => ((HWND)(0));
    public static HWND HWND_TOPMOST => ((HWND)(-1));
    public static HWND HWND_NOTOPMOST => ((HWND)(-2));
}
