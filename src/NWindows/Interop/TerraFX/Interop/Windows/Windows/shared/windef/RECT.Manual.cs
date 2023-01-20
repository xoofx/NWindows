// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from shared/windef.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;

namespace TerraFX.Interop.Windows;
internal partial struct RECT {

    public static bool operator ==(in RECT l, in RECT r)
    {
        return (l.left == r.left)
            && (l.top == r.top)
            && (l.right == r.right)
            && (l.bottom == r.bottom);
    }

    public static bool operator !=(in RECT l, in RECT r)
        => !(l == r);

    public override bool Equals(object? obj) => (obj is RECT other) && Equals(other);

    public bool Equals(RECT other) => this == other;

    public override int GetHashCode() => HashCode.Combine(left, top, right, bottom);
}
