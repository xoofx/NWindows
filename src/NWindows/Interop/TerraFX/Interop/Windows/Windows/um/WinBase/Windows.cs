// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/WinBase.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{
    /// <include file='Windows.xml' path='doc/member[@name="Windows.GlobalAlloc"]/*' />
    [DllImport("kernel32", ExactSpelling = true)]
    public static extern HGLOBAL GlobalAlloc(uint uFlags, nuint dwBytes);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GlobalSize"]/*' />
    [DllImport("kernel32", ExactSpelling = true)]
    public static extern nuint GlobalSize(HGLOBAL hMem);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GlobalUnlock"]/*' />
    [DllImport("kernel32", ExactSpelling = true)]
    public static extern BOOL GlobalUnlock(HGLOBAL hMem);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GlobalLock"]/*' />
    [DllImport("kernel32", ExactSpelling = true)]
    public static extern void* GlobalLock(HGLOBAL hMem);
}
