// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public static class DataFormats
{
    public static readonly DataFormat<string> Text = new("text/plain") { IsSupported = true };

    public static readonly DataFormat<string> UnicodeText = new("text/unicode") { IsSupported = true };

    public static readonly DataFormat<FileTransferList> File = new("application/x-moz-file") { IsSupported = true };

    public static readonly DataFormat<string> Html = new ("text/html") { IsSupported = true };

    public static readonly DataFormat<byte[]> RichText = new("text/richtext") { IsSupported = true };

    public static readonly DataFormat<byte[]> Png = new("image/png") { IsSupported = true };
}
