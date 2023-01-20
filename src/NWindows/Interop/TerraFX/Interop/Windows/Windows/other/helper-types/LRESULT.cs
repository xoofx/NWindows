// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly partial struct LRESULT {
    public readonly nint Value;

    public LRESULT(nint value)
    {
        Value = value;
    }

    public static bool operator <(LRESULT left, LRESULT right) => left.Value < right.Value;

    public static bool operator <=(LRESULT left, LRESULT right) => left.Value <= right.Value;

    public static bool operator >(LRESULT left, LRESULT right) => left.Value > right.Value;

    public static bool operator >=(LRESULT left, LRESULT right) => left.Value >= right.Value;

    public static implicit operator LRESULT(int value) => new LRESULT(value);

    public int CompareTo(object? obj)
    {
            if (obj is LRESULT other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of LRESULT.");
    }

    public int CompareTo(LRESULT other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is LRESULT other) && Equals(other);

    public bool Equals(LRESULT other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString();

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
