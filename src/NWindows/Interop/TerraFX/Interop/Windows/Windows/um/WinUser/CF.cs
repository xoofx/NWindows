// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/WinUser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal static partial class CF
{
    public const int CF_TEXT = 1;
    public const int CF_BITMAP = 2;
    public const int CF_METAFILEPICT = 3;
    public const int CF_SYLK = 4;
    public const int CF_DIF = 5;
    public const int CF_TIFF = 6;
    public const int CF_OEMTEXT = 7;
    public const int CF_DIB = 8;
    public const int CF_PALETTE = 9;
    public const int CF_PENDATA = 10;
    public const int CF_RIFF = 11;
    public const int CF_WAVE = 12;
    public const int CF_UNICODETEXT = 13;
    public const int CF_ENHMETAFILE = 14;
    public const int CF_HDROP = 15;
    public const int CF_LOCALE = 16;
    public const int CF_DIBV5 = 17;
    public const int CF_MAX = 18;
    public const int CF_OWNERDISPLAY = 0x0080;
    public const int CF_DSPTEXT = 0x0081;
    public const int CF_DSPBITMAP = 0x0082;
    public const int CF_DSPMETAFILEPICT = 0x0083;
    public const int CF_DSPENHMETAFILE = 0x008E;
    public const int CF_PRIVATEFIRST = 0x0200;
    public const int CF_PRIVATELAST = 0x02FF;
    public const int CF_GDIOBJFIRST = 0x0300;
    public const int CF_GDIOBJLAST = 0x03FF;
}
