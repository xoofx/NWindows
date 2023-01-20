// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct HBITMAP {
    public readonly void* Value;

    public HBITMAP(void* value)
    {
        Value = value;
    }

    public static HBITMAP NULL => new HBITMAP(null);

    public static bool operator ==(HBITMAP left, HBITMAP right) => left.Value == right.Value;

    public static bool operator !=(HBITMAP left, HBITMAP right) => left.Value != right.Value;

    public static implicit operator HANDLE(HBITMAP value) => new HANDLE(value.Value);

    public int CompareTo(object? obj)
    {
            if (obj is HBITMAP other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of HBITMAP.");
    }

    public int CompareTo(HBITMAP other) => ((nuint)(Value)).CompareTo((nuint)(other.Value));

    public override bool Equals(object? obj) => (obj is HBITMAP other) && Equals(other);

    public bool Equals(HBITMAP other) => ((nuint)(Value)).Equals((nuint)(other.Value));

    public override int GetHashCode() => ((nuint)(Value)).GetHashCode();

    public override string ToString() => ((nuint)(Value)).ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => ((nuint)(Value)).ToString(format, formatProvider);
}
