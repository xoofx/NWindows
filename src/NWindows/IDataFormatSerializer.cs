// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

/// <summary>
/// Interface used for implementing a serializer/deserializer for a data format.
/// </summary>
/// <typeparam name="T">The type of the data to serialize/deserialize.</typeparam>
public interface IDataFormatSerializer<T> where T: class
{
    /// <summary>
    /// Calculates the size in bytes of the specified clipboard value.
    /// </summary>
    /// <param name="value">The clipboard value.</param>
    /// <returns>The size in bytes.</returns>
    int CalculateSizeInBytes(T value);

    /// <summary>
    /// Writes a clipboard value to the specified buffer.
    /// </summary>
    /// <param name="value">The clipboard value.</param>
    /// <param name="buffer">The output buffer receiving a binary representation of the value.</param>
    void Serialize(T value, Span<byte> buffer);

    /// <summary>
    /// Reads a clipboard value from the specified buffer.
    /// </summary>
    /// <param name="buffer">The output buffer that contains a binary representation of the value.</param>
    /// <returns>The value deserialized.</returns>
    T? Deserialize(Span<byte> buffer);
}