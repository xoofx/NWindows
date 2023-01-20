// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from winrt/winstring.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.WinRT;
internal static unsafe partial class WinRT
{
    /// <include file='WinRT.xml' path='doc/member[@name="WinRT.WindowsCreateString"]/*' />
    [DllImport("combase", ExactSpelling = true)]
    public static extern HRESULT WindowsCreateString(ushort* sourceString, uint length, HSTRING* @string);

    /// <include file='WinRT.xml' path='doc/member[@name="WinRT.WindowsDeleteString"]/*' />
    [DllImport("combase", ExactSpelling = true)]
    public static extern HRESULT WindowsDeleteString(HSTRING @string);
}
