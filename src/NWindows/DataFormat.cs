// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

public abstract record DataFormat
{
    internal DataFormat(string mime)
    {
        Mime = mime;
    }

    public string Mime { get; }

    public bool IsSupported { get; internal init; }

    public void VerifySupported()
    {
        if (!IsSupported) throw new ArgumentException($"The format {Mime} is not supported");
    }

    public sealed override string ToString()
    {
        return Mime;
    }
}

public sealed record DataFormat<T>(string Mime) : DataFormat(Mime) where T : class;