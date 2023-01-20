// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/tpcshrd.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal static partial class WM
{
    public const int WM_TABLET_ADDED = (0x02C0 + 8);
    public const int WM_TABLET_DELETED = (0x02C0 + 9);
    public const int WM_TABLET_FLICK = (0x02C0 + 11);
    public const int WM_TABLET_QUERYSYSTEMGESTURESTATUS = (0x02C0 + 12);
}
