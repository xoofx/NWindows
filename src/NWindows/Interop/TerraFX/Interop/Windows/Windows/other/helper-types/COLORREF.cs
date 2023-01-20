// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct COLORREF {
    public readonly uint Value;

    public COLORREF(uint value)
    {
        Value = value;
    }

    public static explicit operator ushort(COLORREF value) => (ushort)(value.Value);

    public static implicit operator COLORREF(uint value) => new COLORREF(value);

    public static implicit operator uint(COLORREF value) => value.Value;

    public int CompareTo(object? obj)
    {
            if (obj is COLORREF other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of COLORREF.");
    }

    public int CompareTo(COLORREF other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is COLORREF other) && Equals(other);

    public bool Equals(COLORREF other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("X8");

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
