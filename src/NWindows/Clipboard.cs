// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text;
using NWindows.Platforms.Win32;

namespace NWindows;

/// <summary>
/// Main clipboard class to interact with the clipboard.
/// </summary>
public static class Clipboard
{
    private static readonly ClipboardImpl Manager = GetClipboardImpl();

    /// <summary>
    /// Clears the clipboard.
    /// </summary>
    public static void Clear() => Manager.Clear();

    /// <summary>
    /// Gets a data from the clipboard using the specified data format.
    /// </summary>
    /// <typeparam name="T">The managed type of the data.</typeparam>
    /// <param name="format">The data format. See <see cref="DataFormats"/> for a list of predefined formats.</param>
    /// <returns>The value or null if no data available for the specified format.</returns>
    public static T? GetData<T>(DataFormat<T> format) where T : class
    {
        format.VerifySupported();
        return Manager.GetData(format);
    }

    /// <summary>
    /// Get the list of data formats supported by the current data on the clipboard.
    /// </summary>
    /// <returns>A list of data formats supported by the current data on the clipboard</returns>
    public static List<DataFormat> GetDataFormats() => Manager.GetDataFormats();

    /// <summary>
    /// Notify the result of a cut/paste operation involving file paths.
    /// </summary>
    /// <param name="dataTransferResult">The result of the transfer.</param>
    public static void Notify(DataTransferResult dataTransferResult) => Manager.Notify(dataTransferResult);

    /// <summary>
    /// Registers a custom data format.
    /// </summary>
    /// <typeparam name="T">The type of the data format.</typeparam>
    /// <param name="mimeName">The mime to register for this data format.</param>
    /// <param name="serializer">The implementation of the serializer. If this serializer is null, this method provide an implementation for deserializing a string and a byte[] buffer.</param>
    /// <returns>A supported data format.</returns>
    public static DataFormat<T> Register<T>(string mimeName, IDataFormatSerializer<T>? serializer = null) where T: class => Manager.Register(mimeName, serializer ?? DefaultSerializer<T>());

    /// <summary>
    /// Sets a data to the clipboard for the specified data format. 
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="format">The data format.</param>
    /// <param name="data">The value to set.</param>
    /// <returns>A boolean indicating whether the operation was successful.</returns>
    public static bool SetData<T>(DataFormat<T> format, T data) where T : class
    {
        format.VerifySupported();
        return Manager.SetData(format, data);
    }

    /// <summary>
    /// Sets several data to the clipboard. 
    /// </summary>
    /// <param name="dataMap">The <see cref="ClipboardData"/> to set.</param>
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

        public int CalculateSizeInBytes(byte[] value) => value.Length;
        public void Serialize(byte[] value, Span<byte> buffer) => value.CopyTo(buffer);
        public byte[]? Deserialize(Span<byte> buffer) => buffer.ToArray();
    }

    private class StringDataFormatSerializer : IDataFormatSerializer<string>
    {
        public static readonly StringDataFormatSerializer Default = new();

        public int CalculateSizeInBytes(string value) => Encoding.UTF8.GetBytes(value).Length;

        public void Serialize(string value, Span<byte> buffer) => Encoding.UTF8.GetBytes(value, buffer);

        public string? Deserialize(Span<byte> buffer) => Encoding.UTF8.GetString(buffer);
    }
}