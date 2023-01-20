// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/wingdi.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CreateBitmap"]/*' />
    [DllImport("gdi32", ExactSpelling = true)]
    public static extern HBITMAP CreateBitmap(int nWidth, int nHeight, uint nPlanes, uint nBitCount, void* lpBits);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CreateSolidBrush"]/*' />
    [DllImport("gdi32", ExactSpelling = true)]
    public static extern HBRUSH CreateSolidBrush(COLORREF color);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DeleteObject"]/*' />
    [DllImport("gdi32", ExactSpelling = true)]
    public static extern BOOL DeleteObject(HGDIOBJ ho);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetBitmapBits"]/*' />
    [DllImport("gdi32", ExactSpelling = true)]
    public static extern int GetBitmapBits(HBITMAP hbit, int cb, void* lpvBits);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetObjectW"]/*' />
    [DllImport("gdi32", ExactSpelling = true)]
    public static extern int GetObjectW(HANDLE h, int c, void* pv);
    public const int DMDO_DEFAULT = 0;
    public const int DMDO_90 = 1;
    public const int DMDO_180 = 2;
    public const int DMDO_270 = 3;
    public static delegate*<HANDLE, int, void*, int> GetObject => &GetObjectW;
}
