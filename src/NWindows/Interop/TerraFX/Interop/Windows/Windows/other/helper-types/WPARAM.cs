// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct WPARAM {
    public readonly nuint Value;

    public WPARAM(nuint value)
    {
        Value = value;
    }

    public static implicit operator WPARAM(byte value) => new WPARAM(value);

    public static explicit operator WPARAM(int value) => new WPARAM(unchecked((nuint)(value)));

    public static explicit operator int(WPARAM value) => (int)(value.Value);

    public static explicit operator uint(WPARAM value) => (uint)(value.Value);

    public static implicit operator ulong(WPARAM value) => value.Value;

    public static implicit operator nuint(WPARAM value) => value.Value;

    public int CompareTo(object? obj)
    {
            if (obj is WPARAM other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of WPARAM.");
    }

    public int CompareTo(WPARAM other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is WPARAM other) && Equals(other);

    public bool Equals(WPARAM other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
