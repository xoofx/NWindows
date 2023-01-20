// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared/minwindef.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Runtime.CompilerServices;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{
    public const int NULL = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort LOWORD(nuint l) => unchecked((ushort)(((nuint)(l)) & 0xffff));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort LOWORD(nint l) => unchecked(LOWORD((nuint)(l)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort HIWORD(nuint l) => ((ushort)((((nuint)(l)) >> 16) & 0xffff));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort HIWORD(nint l) => unchecked(HIWORD((nuint)(l)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LOBYTE(nuint w) => ((byte)(((nuint)(w)) & 0xff));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte LOBYTE(nint w) => unchecked(LOBYTE((nuint)(w)));
}
