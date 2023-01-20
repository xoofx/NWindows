// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/wingdi.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Runtime.CompilerServices;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static COLORREF RGB(byte r, byte g, byte b) => r | ((uint)g << 8) | (((uint)b) << 16);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetRValue(COLORREF rgb) => LOBYTE((ushort)rgb);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetGValue(COLORREF rgb) => LOBYTE((ushort)(rgb >> 8));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte GetBValue(COLORREF rgb) => LOBYTE((ushort)(rgb >> 16));
}
