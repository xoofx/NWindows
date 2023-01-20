// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

namespace TerraFX.Interop.Windows;
internal unsafe partial struct WPARAM
{

    public static explicit operator BOOL(WPARAM value) => new BOOL((int)(value.Value));
}
