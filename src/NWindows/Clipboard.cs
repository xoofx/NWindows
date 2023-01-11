// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using NWindows.Interop.Win32;

namespace NWindows;

public static class Clipboard
{
    private static readonly ClipboardImpl Manager = GetClipboardImpl();

    public static void Clear() => Manager.Clear();

    public static T? GetData<T>(DataFormat<T> format) where T : class
    {
        format.VerifySupported();
        return Manager.GetData(format);
    }

    public static List<DataFormat> GetDataFormats() => Manager.GetDataFormats();

    public static void Notify(DataTransferResult dataTransferResult) => Manager.Notify(dataTransferResult);

    public static DataFormat<T> Register<T>(string mimeName, IDataFormatSerializer<T>? serializer = null) where T: class => Manager.Register(mimeName, serializer ?? DefaultSerializer<T>());

    public static bool SetData<T>(DataFormat<T> format, T data) where T : class
    {
        format.VerifySupported();
        return Manager.SetData(format, data);
    }

    public static void SetData(ClipboardData dataMap) => Manager.SetData(dataMap);

    private static ClipboardImpl GetClipboardImpl()
    {
        if (OperatingSystem.IsWindows())
        {
            return new Win32Clipboard();
        }

        throw new PlatformNotSupportedException();
    }

    private static IDataFormatSerializer<T> DefaultSerializer<T>() where T : class
    {
        if (typeof(T) == typeof(byte[])) return (IDataFormatSerializer<T>)ByteArrayDataFormatSerializer.Default;
        if (typeof(T) == typeof(string)) return (IDataFormatSerializer<T>)StringDataFormatSerializer.Default;
        throw new ArgumentException($"The type {typeof(T).FullName} is not supported as a default serializer. Only byte[] and string are supported. You must provide a custom serializer implementation in that case.", "serializer");
    }

    private class ByteArrayDataFormatSerializer : IDataFormatSerializer<byte[]>
    {
        public static readonly ByteArrayDataFormatSerializer Default = new();

        public int CalculateSize(byte[] value) => value.Length;
        public void Serialize(byte[] value, Span<byte> buffer) => value.CopyTo(buffer);
        public byte[]? Deserialize(Span<byte> buffer) => buffer.ToArray();
    }

    private class StringDataFormatSerializer : IDataFormatSerializer<string>
    {
        public static readonly StringDataFormatSerializer Default = new();

        public int CalculateSize(string value) => Encoding.UTF8.GetBytes(value).Length;

        public void Serialize(string value, Span<byte> buffer) => Encoding.UTF8.GetBytes(value, buffer);

        public string? Deserialize(Span<byte> buffer) => Encoding.UTF8.GetString(buffer);
    }

}