// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct HICON {
    public readonly void* Value;

    public HICON(void* value)
    {
        Value = value;
    }

    public static HICON NULL => new HICON(null);

    public static bool operator ==(HICON left, HICON right) => left.Value == right.Value;

    public static bool operator !=(HICON left, HICON right) => left.Value != right.Value;

    public static explicit operator HICON(HANDLE value) => new HICON(value);

    public int CompareTo(object? obj)
    {
            if (obj is HICON other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of HICON.");
    }

    public int CompareTo(HICON other) => ((nuint)(Value)).CompareTo((nuint)(other.Value));

    public override bool Equals(object? obj) => (obj is HICON other) && Equals(other);

    public bool Equals(HICON other) => ((nuint)(Value)).Equals((nuint)(other.Value));

    public override int GetHashCode() => ((nuint)(Value)).GetHashCode();

    public override string ToString() => ((nuint)(Value)).ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => ((nuint)(Value)).ToString(format, formatProvider);
}
