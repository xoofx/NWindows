// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared/winerror.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal static partial class E
{
    public const int E_UNEXPECTED = unchecked((int)(0x8000FFFF));
    public const int E_INVALIDARG = unchecked((int)(0x80070057));
    public const int E_NOINTERFACE = unchecked((int)(0x80004002));
}
