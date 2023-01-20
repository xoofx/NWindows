// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct HRESULT {
    public readonly int Value;

    public HRESULT(int value)
    {
        Value = value;
    }

    public static bool operator ==(HRESULT left, HRESULT right) => left.Value == right.Value;

    public static bool operator !=(HRESULT left, HRESULT right) => left.Value != right.Value;

    public static bool operator <(HRESULT left, HRESULT right) => left.Value < right.Value;

    public static bool operator <=(HRESULT left, HRESULT right) => left.Value <= right.Value;

    public static bool operator >(HRESULT left, HRESULT right) => left.Value > right.Value;

    public static bool operator >=(HRESULT left, HRESULT right) => left.Value >= right.Value;

    public static implicit operator HRESULT(int value) => new HRESULT(value);

    public static implicit operator int(HRESULT value) => value.Value;

    public int CompareTo(object? obj)
    {
            if (obj is HRESULT other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of HRESULT.");
    }

    public int CompareTo(HRESULT other) => Value.CompareTo(other.Value);

    public override bool Equals(object? obj) => (obj is HRESULT other) && Equals(other);

    public bool Equals(HRESULT other) => Value.Equals(other.Value);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value.ToString("X8");

    public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);
}
