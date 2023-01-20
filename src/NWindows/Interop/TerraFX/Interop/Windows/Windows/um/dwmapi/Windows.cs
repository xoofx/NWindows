// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/dwmapi.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DwmExtendFrameIntoClientArea"]/*' />
    [DllImport("dwmapi", ExactSpelling = true)]
    public static extern HRESULT DwmExtendFrameIntoClientArea(HWND hWnd, MARGINS* pMarInset);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DwmIsCompositionEnabled"]/*' />
    [DllImport("dwmapi", ExactSpelling = true)]
    public static extern HRESULT DwmIsCompositionEnabled(BOOL* pfEnabled);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DwmSetWindowAttribute"]/*' />
    [DllImport("dwmapi", ExactSpelling = true)]
    public static extern HRESULT DwmSetWindowAttribute(HWND hwnd, uint dwAttribute, void* pvAttribute, uint cbAttribute);
}
