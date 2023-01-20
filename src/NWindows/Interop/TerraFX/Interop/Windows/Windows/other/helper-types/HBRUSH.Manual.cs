// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal unsafe partial struct HBRUSH {

    public static implicit operator HGDIOBJ(HBRUSH value) => new HGDIOBJ(value.Value);
}
