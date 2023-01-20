// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/oleidl.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal static partial class Windows
{
    public const int DROPEFFECT_NONE = (0);
    public const int DROPEFFECT_COPY = (1);
    public const int DROPEFFECT_MOVE = (2);
    public const int DROPEFFECT_LINK = (4);
    public const uint DROPEFFECT_SCROLL = (0x80000000);
}
