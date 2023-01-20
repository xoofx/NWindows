// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct LPARAM {
    public readonly nint Value;

    public LPARAM(nint value)
    {
        Value = value;
    }

    public static implicit operator LPARAM(int value) => new LPARAM(value);

    public static explicit operator int(LPARAM value) => (int)(value.Value);

    public static implicit operator LPARAM(nint value) => new LPARAM(value);

    public static implicit operator nint(LPARAM value) => value.Value;

    public static explicit operator ushort(LPARAM value) => (ushort)(value.Value);

    public int CompareTo(object? obj)
    {
            if (obj is LPARAM other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of LPARAM.");
    }

    public int CompareTo(LPARAM other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is LPARAM other) && Equals(other);

    public bool Equals(LPARAM other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
