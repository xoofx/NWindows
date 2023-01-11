// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

public interface IDataFormatSerializer<T> where T: class
{
    int CalculateSize(T value);

    void Serialize(T value, Span<byte> buffer);

    T? Deserialize(Span<byte> buffer);
}