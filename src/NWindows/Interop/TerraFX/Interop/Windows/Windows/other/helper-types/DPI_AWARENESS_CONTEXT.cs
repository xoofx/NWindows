// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

using System;

namespace TerraFX.Interop.Windows;
internal readonly unsafe partial struct DPI_AWARENESS_CONTEXT {
    public readonly void* Value;

    public DPI_AWARENESS_CONTEXT(void* value)
    {
        Value = value;
    }

    public static bool operator ==(DPI_AWARENESS_CONTEXT left, DPI_AWARENESS_CONTEXT right) => left.Value == right.Value;

    public static bool operator !=(DPI_AWARENESS_CONTEXT left, DPI_AWARENESS_CONTEXT right) => left.Value != right.Value;

    public static implicit operator nint(DPI_AWARENESS_CONTEXT value) => (nint)(value.Value);

    public static explicit operator DPI_AWARENESS_CONTEXT(nuint value) => new DPI_AWARENESS_CONTEXT(unchecked((void*)(value)));

    public int CompareTo(object? obj)
    {
            if (obj is DPI_AWARENESS_CONTEXT other)
        {
            return CompareTo(other);
        }

        return (obj is null) ? 1 : throw new ArgumentException("obj is not an instance of DPI_AWARENESS_CONTEXT.");
    }

    public int CompareTo(DPI_AWARENESS_CONTEXT other) => ((nuint)(Value)).CompareTo((nuint)(other.Value));

    public override bool Equals(object? obj) => (obj is DPI_AWARENESS_CONTEXT other) && Equals(other);

    public bool Equals(DPI_AWARENESS_CONTEXT other) => ((nuint)(Value)).Equals((nuint)(other.Value));

    public override int GetHashCode() => ((nuint)(Value)).GetHashCode();

    public override string ToString() => ((nuint)(Value)).ToString((sizeof(nint) == 4) ? "X8" : "X16");

    public string ToString(string? format, IFormatProvider? formatProvider) => ((nuint)(Value)).ToString(format, formatProvider);
}
