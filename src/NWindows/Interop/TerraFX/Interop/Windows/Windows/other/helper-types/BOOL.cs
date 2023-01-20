// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly partial struct BOOL {
    public readonly int Value;

    public BOOL(int value)
    {
        Value = value;
    }

    public static implicit operator bool(BOOL value) => value.Value != 0;

    public static implicit operator BOOL(bool value) => new BOOL(value ? 1 : 0);

    public static implicit operator BOOL(int value) => new BOOL(value);

    public int CompareTo(object? obj)
    {
            if (obj is BOOL other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of BOOL.");
    }

    public int CompareTo(BOOL other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is BOOL other) && Equals(other);

    public bool Equals(BOOL other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
