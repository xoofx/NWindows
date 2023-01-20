// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

/// <summary>
/// Describes the data format of a <see cref="Clipboard"/> value.
/// </summary>
public abstract record DataFormat
{
    internal DataFormat(string mime)
    {
        Mime = mime;
    }

    /// <summary>
    /// The mime description.
    /// </summary>
    public string Mime { get; }

    /// <summary>
    /// A boolean indicating if this data format is supported by this library.
    /// </summary>
    public bool IsSupported { get; internal init; }

    /// <summary>
    /// Throws an argument exception if the data format is not supported.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public void VerifySupported()
    {
        if (!IsSupported) throw new ArgumentException($"The format {Mime} is not supported");
    }

    public sealed override string ToString()
    {
        return Mime;
    }
}

/// <summary>
/// Describes the data format of a <see cref="Clipboard"/> value with the associated managed type.
/// </summary>
/// <typeparam name="T">The type of the data.</typeparam>
/// <param name="Mime">The mime of the data.</param>
public sealed record DataFormat<T>(string Mime) : DataFormat(Mime) where T : class;