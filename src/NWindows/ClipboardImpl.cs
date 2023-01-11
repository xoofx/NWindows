// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;

namespace NWindows;

internal abstract class ClipboardImpl
{
    public abstract void Clear();

    public abstract T? GetData<T>(DataFormat<T> mimeFormat) where T : class;

    public abstract List<DataFormat> GetDataFormats();

    public abstract void Notify(DataTransferResult result);

    public abstract DataFormat<T> Register<T>(string mimeName, IDataFormatSerializer<T> serializer) where T : class;

    public abstract bool SetData<T>(DataFormat<T> mimeFormat, T data) where T : class;

    public abstract void SetData(ClipboardData data);
}