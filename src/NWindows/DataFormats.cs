// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// This class provides a list of predefined data formats.
/// </summary>
public static class DataFormats
{
    /// <summary>
    /// An ANSI text format. Use <see cref="UnicodeText"/> for international text.
    /// </summary>
    public static readonly DataFormat<string> Text = new("text/plain") { IsSupported = true };

    /// <summary>
    /// An Unicode text format.
    /// </summary>
    public static readonly DataFormat<string> UnicodeText = new("text/unicode") { IsSupported = true };

    /// <summary>
    /// A file transfer data format, used when cut/copy a list of file from e.g a file explorer.
    /// </summary>
    public static readonly DataFormat<FileTransferList> File = new("application/x-moz-file") { IsSupported = true };

    /// <summary>
    /// A HTML text format.
    /// </summary>
    public static readonly DataFormat<string> Html = new ("text/html") { IsSupported = true };

    /// <summary>
    /// A binary rich text format.
    /// </summary>
    public static readonly DataFormat<byte[]> RichText = new("text/richtext") { IsSupported = true };

    /// <summary>
    /// A PNG image format.
    /// </summary>
    public static readonly DataFormat<byte[]> Png = new("image/png") { IsSupported = true };
}
