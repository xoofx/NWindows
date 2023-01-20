// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct HWND {
    public readonly void* Value;

    public HWND(void* value)
    {
        Value = value;
    }

    public static HWND NULL => new HWND(null);

    public static bool operator ==(HWND left, HWND right) => left.Value == right.Value;

    public static bool operator !=(HWND left, HWND right) => left.Value != right.Value;

    public static explicit operator HWND(int value) => new HWND(unchecked((void*)(value)));

    public static explicit operator HWND(nint value) => new HWND(unchecked((void*)(value)));

    public static implicit operator nint(HWND value) => (nint)(value.Value);

    public int CompareTo(object? obj)
    {
            if (obj is HWND other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of HWND.");
    }

    public int CompareTo(HWND other) => ((nuint)(Value)).CompareTo((nuint)(other.Value));

    public override bool Equals(object? obj) => (obj is HWND other) && Equals(other);

    public bool Equals(HWND other) => ((nuint)(Value)).Equals((nuint)(other.Value));

    public override int GetHashCode() => ((nuint)(Value)).GetHashCode();

    public override string ToString() => ((nuint)(Value)).ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => ((nuint)(Value)).ToString(format, formatProvider);
}
