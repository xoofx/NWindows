// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace NWindows;

/// <summary>
/// Contains a set of Clipboard data value to set.
/// </summary>
public class ClipboardData : IEnumerable
{
    internal readonly Dictionary<DataFormat, object> MapDataFormatToValue;

    public ClipboardData()
    {
        MapDataFormatToValue = new Dictionary<DataFormat, object>();
    }

    /// <summary>
    /// Add the specified value to set to the clipboard.
    /// </summary>
    /// <typeparam name="T">Type of the value to set.</typeparam>
    /// <param name="format">The data format to use.</param>
    /// <param name="data">The data value.</param>
    /// <exception cref="ArgumentException">If the format is not supported</exception>
    public void Add<T>(DataFormat<T> format, T data) where T: class
    {
        if (!format.IsSupported) throw new ArgumentException($"Format {format} is not supported");
        MapDataFormatToValue.Add(format, data);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}