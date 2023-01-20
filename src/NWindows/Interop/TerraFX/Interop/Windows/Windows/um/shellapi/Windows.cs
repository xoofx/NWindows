// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/shellapi.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DragQueryFileW"]/*' />
    [DllImport("shell32", ExactSpelling = true)]
    public static extern uint DragQueryFileW(HDROP hDrop, uint iFile, ushort* lpszFile, uint cch);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SHAppBarMessage"]/*' />
    [DllImport("shell32", ExactSpelling = true)]
    public static extern nuint SHAppBarMessage(uint dwMessage, void* pData);
    public const int ABE_LEFT = 0;
    public const int ABE_TOP = 1;
    public const int ABE_RIGHT = 2;
    public const int ABE_BOTTOM = 3;
}
