// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct HDROP {
    public readonly void* Value;

    public HDROP(void* value)
    {
        Value = value;
    }

    public static implicit operator HANDLE(HDROP value) => new HANDLE(value.Value);

    public static explicit operator HDROP(nint value) => new HDROP(unchecked((void*)(value)));

    public int CompareTo(object? obj)
    {
            if (obj is HDROP other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of HDROP.");
    }

    public int CompareTo(HDROP other) => ((nuint)(Value)).CompareTo((nuint)(other.Value));

    public override bool Equals(object? obj) => (obj is HDROP other) && Equals(other);

    public bool Equals(HDROP other) => ((nuint)(Value)).Equals((nuint)(other.Value));

    public override int GetHashCode() => ((nuint)(Value)).GetHashCode();

    public override string ToString() => ((nuint)(Value)).ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => ((nuint)(Value)).ToString(format, formatProvider);
}
