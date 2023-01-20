// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct HBRUSH {
    public readonly void* Value;

    public HBRUSH(void* value)
    {
        Value = value;
    }

    public static explicit operator HBRUSH(int value) => new HBRUSH(unchecked((void*)(value)));

    public int CompareTo(object? obj)
    {
            if (obj is HBRUSH other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of HBRUSH.");
    }

    public int CompareTo(HBRUSH other) => ((nuint)(Value)).CompareTo((nuint)(other.Value));

    public override bool Equals(object? obj) => (obj is HBRUSH other) && Equals(other);

    public bool Equals(HBRUSH other) => ((nuint)(Value)).Equals((nuint)(other.Value));

    public override int GetHashCode() => ((nuint)(Value)).GetHashCode();

    public override string ToString() => ((nuint)(Value)).ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => ((nuint)(Value)).ToString(format, formatProvider);
}
