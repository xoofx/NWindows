// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/Uxtheme.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    /// <include file='Windows.xml' path='doc/member[@name="Windows.IsThemeActive"]/*' />
    [DllImport("uxtheme", ExactSpelling = true)]
    public static extern BOOL IsThemeActive();
}
