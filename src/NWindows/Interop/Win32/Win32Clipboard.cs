// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.CF;

namespace NWindows.Interop.Win32;

internal sealed unsafe class Win32Clipboard : ClipboardImpl
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable IdentifierTypo
    private static readonly uint CF_PREFERREDDROPEFFECT;
    private static readonly uint CF_PERFORMEDDROPEFFECT;
    private static readonly uint CF_PASTESUCCEEDED;

    private static readonly uint CF_NWINDOWS_CLIPBOARD_ID;
    // ReSharper restore IdentifierTypo
    // ReSharper restore InconsistentNaming

    // Some references:
    // https://github.com/mozilla/gecko-dev/blob/master/widget/windows/nsClipboard.cpp
    // https://learn.microsoft.com/en-us/windows/win32/dataxchg/standard-clipboard-formats
    // Mime type:
    // https://github.com/mozilla/gecko-dev/blob/master/widget/nsITransferable.idl

    private static readonly Dictionary<uint, DataFormat> MapFormatToMime = new()
    {
        { CF_UNICODETEXT, DataFormats.UnicodeText },
        { CF_TEXT, DataFormats.Text },
        { CF_HDROP, DataFormats.File },
        { CF_DIB, DataFormats.Png },
        { CF_DIBV5, DataFormats.Png  },
        // Png, HTML, RTF are mapped in the cctor
    };

    private static readonly Dictionary<DataFormat, uint> MapMimeToFormat = new()
    {
        { DataFormats.UnicodeText, CF_UNICODETEXT },
        { DataFormats.Text, CF_TEXT },
        { DataFormats.File, CF_HDROP },
        // Png, HTML, RTF are mapped in the cctor
    };

    static Win32Clipboard()
    {
        // Statically register known formats that we support
        MapClipboardFormat("PNG", DataFormats.Png);
        MapClipboardFormat("HTML Format", DataFormats.Html);
        MapClipboardFormat("Rich Text Format", DataFormats.RichText);

        CF_PREFERREDDROPEFFECT = RegisterClipboardFormat(CFSTR.CFSTR_PREFERREDDROPEFFECT);
        CF_PERFORMEDDROPEFFECT = RegisterClipboardFormat(CFSTR.CFSTR_PERFORMEDDROPEFFECT);
        CF_PASTESUCCEEDED = RegisterClipboardFormat(CFSTR.CFSTR_PASTESUCCEEDED);

        CF_NWINDOWS_CLIPBOARD_ID = RegisterClipboardFormat("NWindowsClipboardId");
    }

    private readonly object _lock;
    private readonly Dictionary<DataFormat, uint> _mapMimeToFormat;
    private readonly Dictionary<uint, DataFormat> _mapFormatToMime;
    private readonly Dictionary<DataFormat, DataFormatSerializerProxy> _mapMimeToSerializerProxy;

    public Win32Clipboard()
    {
        Win32Ole.Initialize();
        _lock = new object();
        _mapMimeToFormat = new Dictionary<DataFormat, uint>();
        _mapFormatToMime = new Dictionary<uint, DataFormat>();
        _mapMimeToSerializerProxy = new Dictionary<DataFormat, DataFormatSerializerProxy>();
    }

    public override void Clear()
    {
        if (!TryOpenClipboard()) return;
        EmptyClipboard();
        CloseClipboard();
    }

    public override T? GetData<T>(DataFormat<T> mimeFormat) where T: class
    {
        var nativeFormat = GetDefaultNativeFormatForMime(mimeFormat);
        // If we don't have at least one native format for a mime format
        // We don't support it at all
        if (nativeFormat == 0) return null;


        // We try first to 
        if (!TryOpenClipboard())
        {
            return null;
        }

        object? result = null;
        try
        {
            // Collect the supported native formats from a mime format
            var supportedNativeFormats = new List<ushort> { nativeFormat };
            GetSupportedNativeFormatsFromMime(supportedNativeFormats, mimeFormat);

            // Go through the supported formats for one mime format
            // The list should be ordered in preferred order (because of EnumClipboardFormats)
            foreach (var nFormat in supportedNativeFormats)
            {
                var hGlobal = (HGLOBAL)GetClipboardData(nFormat);
                if (hGlobal != nint.Zero)
                {
                    result = GetData(hGlobal, nFormat, mimeFormat);
                    if (result != null)
                    {
                        // Retrieve the status
                        if (mimeFormat == DataFormats.File)
                        {
                            var dropFileList = (FileTransferList)result;
                            dropFileList.PreferredDataTransferEffects = GetClipboardValue<DataTransferEffects>(CF_PREFERREDDROPEFFECT);

                            // Get or set the guid retrieved from the clipboard
                            var guid = GetClipboardValue<Guid>(CF_NWINDOWS_CLIPBOARD_ID);
                            if (guid == Guid.Empty)
                            {
                                // Set the clipboard id for this transfer (so that we can match it again)
                                SetClipboardValue(CF_NWINDOWS_CLIPBOARD_ID, dropFileList.Id);
                            }
                            else
                            {
                                dropFileList.Id = guid;
                            }
                        }
                        break;
                    }
                }
            }

        }
        finally
        {
            CloseClipboard();
        }

        return result as T;
    }

    public override List<DataFormat> GetDataFormats()
    {
        var supportedList = new List<DataFormat>();
        var unSupportedList = new List<DataFormat>();

        if (CountClipboardFormats() == 0 || !TryOpenClipboard()) return supportedList;

        var uFormat = EnumClipboardFormats(0);

        while (uFormat != 0)
        {
            var dataFormat = GetFormatFromNative((ushort)uFormat);
            if (dataFormat != null)
            {
                var list = (dataFormat.IsSupported ? supportedList : unSupportedList);
                if (!list.Contains(dataFormat))
                {
                    list.Add(dataFormat);
                }
            }
            uFormat = EnumClipboardFormats(uFormat);
        }

        CloseClipboard();

        // Append unsupported list AFTER the supported list
        supportedList.AddRange(unSupportedList);

        return supportedList;
    }

    public override void Notify(DataTransferResult result)
    {
        IDataObject* pDataObj;
        IDataObject** ppDataObj = &pDataObj;

        if (!Win32Helper.TryRepeatedly(() => ((HRESULT)Win32Ole.OleGetClipboard((nint)ppDataObj)).SUCCEEDED))
        {
            return;
        }

        SetData(pDataObj, (ushort)CF_PASTESUCCEEDED, Win32Helper.AllocHGlobalMoveableWithValue((uint)result));
        pDataObj->Release();
    }

    public override DataFormat<T> Register<T>(string mimeName, IDataFormatSerializer<T> serializer)
    {
        var mime = new DataFormat<T>(mimeName) { IsSupported = true };

        // TODO: This doesn't check for native mime
        if (MapMimeToFormat.ContainsKey(mime)) throw new ArgumentException($"The mime `{mimeName}` is reserved.");

        lock (_lock)
        {
            if (_mapMimeToFormat.ContainsKey(mime)) throw new ArgumentException($"The mime `{mimeName}` has been already registered");

            // In case we request a native format that we don't support out of the box
            // we will use the native format, otherwise we register it.
            if (!NativeFormats.TryGetNativeFormatFromMime(mimeName, out var format))
            {
                format = (uint)RegisterClipboardFormat(mimeName);
            }

            _mapMimeToFormat[mime] = format;
            _mapFormatToMime[format] = mime;
            _mapMimeToSerializerProxy[mime] = new DataFormatSerializerProxy<T>(serializer);
        }

        return mime;
    }

    private static void SetData(IDataObject* pDataObj, ushort uFormat, HGLOBAL hGlobal)
    {
        FORMATETC fmt = default;
        fmt.cfFormat = uFormat;
        fmt.dwAspect = 1; // DVASPECT_CONTENT
        fmt.lindex = -1;
        fmt.tymed = (int)TYMED.TYMED_HGLOBAL;

        STGMEDIUM stgm = default;
        stgm.tymed = (int)TYMED.TYMED_HGLOBAL;
        stgm.hGlobal = hGlobal;

        var result = pDataObj->SetData(&fmt, &stgm, true);
    }

    //private static object? GetClipboardObject(IDataObject* pDataObj, ushort nFormat, string mimeFormat)
    //{
    //    object? result = null;
    //    HGLOBAL hGlobal;
    //    if (pDataObj != null)
    //    {
    //        FORMATETC fmt = default;
    //        FORMATETC* pfmt = &fmt;
    //        fmt.cfFormat = (ushort)nFormat;
    //        fmt.dwAspect = 1; // DVASPECT_CONTENT
    //        fmt.lindex = -1;
    //        fmt.tymed = (int)TYMED.TYMED_HGLOBAL;

    //        if (pDataObj->QueryGetData(&fmt).FAILED) return null;

    //        STGMEDIUM stgm = default;
    //        STGMEDIUM* pstgm = &stgm;

    //        if (!Win32Helper.TryRepeatedly(() => pDataObj->GetData(pfmt, pstgm).SUCCEEDED) || stgm.tymed != (uint)TYMED.TYMED_HGLOBAL)
    //        {
    //            return null;
    //        }

    //        hGlobal = stgm.hGlobal;
    //    }
    //    else
    //    {
    //        // Get the hGlobal directly from the ClipboardData
    //        hGlobal = (HGLOBAL)GetClipboardData(nFormat);
    //    }

    //    if (hGlobal != nint.Zero)
    //    {
    //        result = GetData(hGlobal, nFormat, mimeFormat);
    //    }
    //    return result;
    //}

    private void GetSupportedNativeFormatsFromMime(List<ushort> supportedFormats, DataFormat expectedMimeFormat)
    {
        var uFormat = EnumClipboardFormats(0);

        while (uFormat != 0)
        {
            bool supported = MapFormatToMime.TryGetValue(uFormat, out var mimeFormat) && expectedMimeFormat == mimeFormat;
            if (!supported)
            {
                // Check for custom registered data format
                lock (_lock)
                {
                    supported = _mapFormatToMime.TryGetValue(uFormat, out mimeFormat) && expectedMimeFormat == mimeFormat;
                }
            }

            if (supported)
            {
                var formatToAdd = (ushort)uFormat;
                if (!supportedFormats.Contains(formatToAdd))
                {
                    supportedFormats.Add(formatToAdd);
                }
            }

            uFormat = EnumClipboardFormats(uFormat);
        }
    }

    public override bool SetData<T>(DataFormat<T> mimeFormat, T data) where T: class
    {
        return TryOpenClipboardToSet(() => { SetDataInternal(mimeFormat, data); });
    }

    private static void SetDataString(DataFormat format, string data)
    {
        if (format == DataFormats.Text)
        {
                var dataCount = Encoding.ASCII.GetByteCount(data);
                var hGlobal = Win32Helper.AllocHGlobalMoveable(dataCount + 1);
                var ptr = (byte*)GlobalLock(hGlobal);
                Encoding.ASCII.GetBytes(data, new Span<byte>(ptr, dataCount));
                ptr[dataCount] = 0;
                GlobalUnlock(hGlobal);

                SetClipboardData(CF_TEXT, hGlobal);
        }
        else if (format == DataFormats.UnicodeText) 
        {
            var dataCount = (data.Length + 1) * 2;
            var hGlobal = Win32Helper.AllocHGlobalMoveable(dataCount);
            var ptr = (char*)GlobalLock(hGlobal);
            ptr[data.Length] = (char)0;
            data.CopyTo(new Span<char>(ptr, data.Length));
            GlobalUnlock(hGlobal);

            SetClipboardData(CF_UNICODETEXT, hGlobal);
        }
        else if (format == DataFormats.Html)
        {
            var fragmentCount = Encoding.UTF8.GetByteCount(data);
            var preHtml = PreHtml();
            var postHtml = PostHtml();
            var totalCount = fragmentCount + preHtml.Length + postHtml.Length;
            var hGlobal = Win32Helper.AllocHGlobalMoveable(totalCount);
            var ptr = (char*)GlobalLock(hGlobal);

            var header = new Span<byte>(ptr, preHtml.Length);
            preHtml.CopyTo(header);

            var footer = new Span<byte>((byte*)ptr + preHtml.Length + fragmentCount, postHtml.Length);
            postHtml.CopyTo(footer);

            Encoding.UTF8.GetBytes(data, new Span<byte>((byte*)ptr + preHtml.Length, fragmentCount));

            const int endHtmlValueIndex = 41;
            const int endFragmentValueIndex = 89;
            SetValue(header, endHtmlValueIndex, totalCount);
            SetValue(header, endFragmentValueIndex, totalCount - postHtml.Length);
            GlobalUnlock(hGlobal);

            SetClipboardData((ushort)MapMimeToFormat[DataFormats.Html], hGlobal);
        }

        // Pre-calculated header for HTML Format
        //                                      0         1          2         3          4         5          6         7          8         9          A          B         C         D
        //                                      012345678901 234567890123456789012 3456789012345678901 2345678901234567890123456 78901234567890123456789 0123456 789012345678901234567890123
        static ReadOnlySpan<byte> PreHtml() => "Version:0.9\nStartHTML:0000000100\nEndHTML:0000000000\nStartFragment:0000000133\nEndFragment:0000000000\n<html>\n<body><!--StartFragment-->"u8;
        static ReadOnlySpan<byte> PostHtml() => "<!--EndFragment-->\n</body>\n</html>"u8;
    }

    private static void SetValue(Span<byte> bytes, int index, int value)
    {
        index += 9;
        while (value != 0)
        {
            bytes[index] = (byte)((value % 10) + '0');
            value /=10;
            index--;
        }
    }

    private static void SetDataFiles(FileTransferList fileList)
    {
        // TODO: Use SHCreateDataObject instead to support feedback?
        var drop = Win32Ole.CreateDropFiles(fileList);
        SetClipboardData(CF_HDROP, drop);
        SetClipboardValue(CF_NWINDOWS_CLIPBOARD_ID, fileList.Id);
    }
    
    private static void SetDataBinary(DataFormat format, byte[] data)
    {
        var hGlobal = Win32Helper.AllocHGlobalMoveable(data.Length);
        var ptr = (byte*)GlobalLock(hGlobal);
        data.AsSpan().CopyTo(new Span<byte>(ptr, data.Length));
        GlobalUnlock(hGlobal);
        SetClipboardData(MapMimeToFormat[format], hGlobal);
    }

    private static bool TryOpenClipboardToSet(Action action)
    {
        if (!TryOpenClipboard())
        {
            return false;
        }

        EmptyClipboard();

        try
        {
            action();
        }
        finally
        {
            CloseClipboard();
        }

        return true;
    }

    private void SetDataInternal(DataFormat format, object data)
    {
        if (format == DataFormats.Text || format == DataFormats.UnicodeText || format == DataFormats.Html)
        {
            SetDataString(format, (string)data);
        }
        else if (format == DataFormats.File)
        {
            SetDataFiles((FileTransferList)data);
        }
        else if (format == DataFormats.Png || format == DataFormats.RichText)
        {
            SetDataBinary(format, (byte[])data);
        }
        else
        {
            DataFormatSerializerProxy proxySerializer;
            ushort uFormat;
            lock (_lock)
            {
                proxySerializer = _mapMimeToSerializerProxy[format];
                uFormat = (ushort)_mapMimeToFormat[format];
            }

            SetCustomData(uFormat, data, proxySerializer);
        }
    }

    private static void SetCustomData(ushort uFormat, object data, DataFormatSerializerProxy proxySerializer)
    {
        var size = proxySerializer.CalculateSize(data);
        var hGlobal = Win32Helper.AllocHGlobalMoveable(size);
        var pData = (byte*)GlobalLock(hGlobal);
        var span = new Span<byte>(pData, size);
        proxySerializer.Serialize(data, span);
        GlobalLock(hGlobal);

        SetClipboardData(uFormat, hGlobal);
    }

    public override void SetData(ClipboardData data)
    {
        TryOpenClipboardToSet(() =>
            {
                foreach (var pairMimeData in data.MapDataFormatToValue)
                {
                    SetDataInternal(pairMimeData.Key, pairMimeData.Value);
                }
            }
        );
    }

    private ushort GetDefaultNativeFormatForMime(DataFormat format)
    {
        if (MapMimeToFormat.TryGetValue(format, out var result))
        {
            return (ushort)result;
        }

        lock (_lock)
        {
            if (_mapMimeToFormat.TryGetValue(format, out result))
            {
                return (ushort)result;
            }
        }

        return 0;
    }

    private object? GetData(HGLOBAL hGlobal, ushort nativeFormat, DataFormat mime)
    {
        switch (nativeFormat)
        {
            case CF_UNICODETEXT:
            {
                return Win32Helper.GetGlobalDataAsUnicodeString(hGlobal);
            }
                break;
            case CF_TEXT:
            {
                return Win32Helper.GetGlobalDataAsAnsiString(hGlobal);
            }
                break;
            case CF_DIB:
            {
                // TODO: handle image format;
                return null;
            }
                break;
            case CF_DIBV5:
            {
                // TODO: handle image format;
                return null;
            }
                break;

            case CF_HDROP:
            {
                var files = new FileTransferList();
                Win32Ole.GetDropFiles(new HDROP(hGlobal), files);
                return files;
            }
                break;
            default:
                if (mime == DataFormats.Png || mime == DataFormats.RichText)
                {
                    return Win32Helper.GetGlobalData(hGlobal);
                }

                if (mime == DataFormats.Html)
                {
                    return Win32Helper.GetGlobalDataAsHtmlFormat(hGlobal);
                }

                DataFormatSerializerProxy proxySerializer;
                lock (_lock)
                {
                    proxySerializer = _mapMimeToSerializerProxy[mime];
                }
                return GetCustomData(hGlobal, proxySerializer);
        }

        return null;
    }

    private static object? GetCustomData(HGLOBAL hGlobal, DataFormatSerializerProxy proxySerializer)
    {
        var pData = (byte*)GlobalLock(hGlobal);
        var size = GlobalSize(hGlobal);
        var result = proxySerializer.Deserialize(new Span<byte>(pData, (int)size));
        GlobalUnlock(hGlobal);
        return result;
    }

    private static bool TryGetClipboardValue<T>(uint uFormat, out T value) where T: unmanaged
    {
        value = default;
        var handle = (HGLOBAL)GetClipboardData(uFormat);
        if (handle == nint.Zero) return false;

        var pValue = (T*)GlobalLock(handle);
        value = *pValue;
        GlobalUnlock(handle);
        return true;
    }

    private static T GetClipboardValue<T>(uint uFormat) where T : unmanaged
    {
        _= TryGetClipboardValue<T>(uFormat, out var value);
        return value;
    }

    private static void SetClipboardValue<T>(uint uFormat, T value) where T: unmanaged
    {
        var hGlobal = Win32Helper.AllocHGlobalMoveableWithValue(value);
        SetClipboardData(uFormat, hGlobal);
    }

    private static bool TryOpenClipboard()
    {
        // Open the clipboard
        return Win32Helper.TryRepeatedly(() => OpenClipboard(HWND.NULL));
    }

    // Use SkipLocalsInit to avoid clearing the stackalloc
    [SkipLocalsInit]
    private DataFormat? GetFormatFromNative(ushort uFormat)
    {
        if (MapFormatToMime.TryGetValue(uFormat, out var dataFormat))
        {
            return dataFormat;
        }

        lock (_lock)
        {
            if (_mapFormatToMime.TryGetValue(uFormat, out var dataFormat2))
            {
                return dataFormat2;
            }
        }

        const int maxCharCount = 256;
        char* formatName = stackalloc char[maxCharCount];
        var cc = GetClipboardFormatNameW(uFormat, (ushort*)formatName, maxCharCount);
        return cc > 0 ? new UnknownDataFormat($"unknown/{new string(formatName, 0, cc)}") : null;
    }
    
    private static void MapClipboardFormat(string nativeName, DataFormat dataFormat)
    {
        var format = RegisterClipboardFormat(nativeName);
        MapFormatToMime[format] = dataFormat;
        MapMimeToFormat[dataFormat] = format;
    }

    private static uint RegisterClipboardFormat(string nativeName)
    {
        fixed (char* pName = nativeName)
        {
            return Windows.RegisterClipboardFormat((ushort*)pName);
        }
    }

    private static class NativeFormats
    {
        private static readonly Dictionary<string, uint> MapNameToNativeFormat = new();

        public static readonly NativeDataFormat CF_NULL = new($"native/{nameof(CF_NULL)}");
        public static readonly NativeDataFormat CF_TEXT = new($"native/${CF_TEXT}");
        public static readonly NativeDataFormat CF_BITMAP = new($"native/{nameof(CF_BITMAP)}");
        public static readonly NativeDataFormat CF_METAFILEPICT = new($"native/{nameof(CF_METAFILEPICT)}");
        public static readonly NativeDataFormat CF_SYLK = new($"native/{nameof(CF_SYLK)}");
        public static readonly NativeDataFormat CF_DIF = new($"native/{nameof(CF_DIF)}");
        public static readonly NativeDataFormat CF_TIFF = new($"native/{nameof(CF_TIFF)}");
        public static readonly NativeDataFormat CF_OEMTEXT = new($"native/{nameof(CF_OEMTEXT)}");
        public static readonly NativeDataFormat CF_DIB = new($"native/{nameof(CF_DIB)}");
        public static readonly NativeDataFormat CF_PALETTE = new($"native/{nameof(CF_PALETTE)}");
        public static readonly NativeDataFormat CF_PENDATA = new($"native/{nameof(CF_PENDATA)}");
        public static readonly NativeDataFormat CF_RIFF = new($"native/{nameof(CF_RIFF)}");
        public static readonly NativeDataFormat CF_WAVE = new($"native/{nameof(CF_WAVE)}");
        public static readonly NativeDataFormat CF_UNICODETEXT = new($"native/{nameof(CF_UNICODETEXT)}");
        public static readonly NativeDataFormat CF_ENHMETAFILE = new($"native/{nameof(CF_ENHMETAFILE)}");
        public static readonly NativeDataFormat CF_HDROP = new($"native/{nameof(CF_HDROP)}");
        public static readonly NativeDataFormat CF_LOCALE = new($"native/{nameof(CF_LOCALE)}");
        public static readonly NativeDataFormat CF_DIBV5 = new($"native/{nameof(CF_DIBV5)}");
        public static readonly NativeDataFormat CF_MAX = new($"native/{nameof(CF_MAX)}");
        public static readonly NativeDataFormat CF_OWNERDISPLAY = new($"native/{nameof(CF_OWNERDISPLAY)}");
        public static readonly NativeDataFormat CF_DSPTEXT = new($"native/{nameof(CF_DSPTEXT)}");
        public static readonly NativeDataFormat CF_DSPBITMAP = new($"native/{nameof(CF_DSPBITMAP)}");
        public static readonly NativeDataFormat CF_DSPMETAFILEPICT = new($"native/{nameof(CF_DSPMETAFILEPICT)}");
        public static readonly NativeDataFormat CF_DSPENHMETAFILE = new($"native/{nameof(CF_DSPENHMETAFILE)}");
        public static readonly NativeDataFormat CF_PRIVATEFIRST = new($"native/{nameof(CF_PRIVATEFIRST)}");
        public static readonly NativeDataFormat CF_PRIVATELAST = new($"native/{nameof(CF_PRIVATELAST)}");
        public static readonly NativeDataFormat CF_GDIOBJFIRST = new($"native/{nameof(CF_GDIOBJFIRST)}");
        public static readonly NativeDataFormat CF_GDIOBJLAST = new($"native/{nameof(CF_GDIOBJLAST)}");

        static NativeFormats()
        {
            MapNameToNativeFormat[CF_NULL.Mime] = CF.CF_NULL;
            MapNameToNativeFormat[CF_TEXT.Mime] = CF.CF_TEXT;
            MapNameToNativeFormat[CF_BITMAP.Mime] = CF.CF_BITMAP;
            MapNameToNativeFormat[CF_METAFILEPICT.Mime] = CF.CF_METAFILEPICT;
            MapNameToNativeFormat[CF_SYLK.Mime] = CF.CF_SYLK;
            MapNameToNativeFormat[CF_DIF.Mime] = CF.CF_DIF;
            MapNameToNativeFormat[CF_TIFF.Mime] = CF.CF_TIFF;
            MapNameToNativeFormat[CF_OEMTEXT.Mime] = CF.CF_OEMTEXT;
            MapNameToNativeFormat[CF_DIB.Mime] = CF.CF_DIB;
            MapNameToNativeFormat[CF_PALETTE.Mime] = CF.CF_PALETTE;
            MapNameToNativeFormat[CF_PENDATA.Mime] = CF.CF_PENDATA;
            MapNameToNativeFormat[CF_RIFF.Mime] = CF.CF_RIFF;
            MapNameToNativeFormat[CF_WAVE.Mime] = CF.CF_WAVE;
            MapNameToNativeFormat[CF_UNICODETEXT.Mime] = CF.CF_UNICODETEXT;
            MapNameToNativeFormat[CF_ENHMETAFILE.Mime] = CF.CF_ENHMETAFILE;
            MapNameToNativeFormat[CF_HDROP.Mime] = CF.CF_HDROP;
            MapNameToNativeFormat[CF_LOCALE.Mime] = CF.CF_LOCALE;
            MapNameToNativeFormat[CF_DIBV5.Mime] = CF.CF_DIBV5;
            MapNameToNativeFormat[CF_MAX.Mime] = CF.CF_MAX;
            MapNameToNativeFormat[CF_OWNERDISPLAY.Mime] = CF.CF_OWNERDISPLAY;
            MapNameToNativeFormat[CF_DSPTEXT.Mime] = CF.CF_DSPTEXT;
            MapNameToNativeFormat[CF_DSPBITMAP.Mime] = CF.CF_DSPBITMAP;
            MapNameToNativeFormat[CF_DSPMETAFILEPICT.Mime] = CF.CF_DSPMETAFILEPICT;
            MapNameToNativeFormat[CF_DSPENHMETAFILE.Mime] = CF.CF_DSPENHMETAFILE;
            MapNameToNativeFormat[CF_PRIVATEFIRST.Mime] = CF.CF_PRIVATEFIRST;
            MapNameToNativeFormat[CF_PRIVATELAST.Mime] = CF.CF_PRIVATELAST;
            MapNameToNativeFormat[CF_GDIOBJFIRST.Mime] = CF.CF_GDIOBJFIRST;
            MapNameToNativeFormat[CF_GDIOBJLAST.Mime] = CF.CF_GDIOBJLAST;
        }

        public static bool TryGetNativeFormatFromMime(string mime, out uint format)
        {
            return MapNameToNativeFormat.TryGetValue(mime, out format);
        }
        
        public static NativeDataFormat? GetNativeDataFormat(uint uFormat)
        {
            switch (uFormat)
            {
                case CF.CF_NULL: return CF_NULL;
                case CF.CF_TEXT: return CF_TEXT;
                case CF.CF_BITMAP: return CF_BITMAP;
                case CF.CF_METAFILEPICT: return CF_METAFILEPICT;
                case CF.CF_SYLK: return CF_SYLK;
                case CF.CF_DIF: return CF_DIF;
                case CF.CF_TIFF: return CF_TIFF;
                case CF.CF_OEMTEXT: return CF_OEMTEXT;
                case CF.CF_DIB: return CF_DIB;
                case CF.CF_PALETTE: return CF_PALETTE;
                case CF.CF_PENDATA: return CF_PENDATA;
                case CF.CF_RIFF: return CF_RIFF;
                case CF.CF_WAVE: return CF_WAVE;
                case CF.CF_UNICODETEXT: return CF_UNICODETEXT;
                case CF.CF_ENHMETAFILE: return CF_ENHMETAFILE;
                case CF.CF_HDROP: return CF_HDROP;
                case CF.CF_LOCALE: return CF_LOCALE;
                case CF.CF_DIBV5: return CF_DIBV5;
                case CF.CF_MAX: return CF_MAX;
                case CF.CF_OWNERDISPLAY: return CF_OWNERDISPLAY;
                case CF.CF_DSPTEXT: return CF_DSPTEXT;
                case CF.CF_DSPBITMAP: return CF_DSPBITMAP;
                case CF.CF_DSPMETAFILEPICT: return CF_DSPMETAFILEPICT;
                case CF.CF_DSPENHMETAFILE: return CF_DSPENHMETAFILE;
                case CF.CF_PRIVATEFIRST: return CF_PRIVATEFIRST;
                case CF.CF_PRIVATELAST: return CF_PRIVATELAST;
                case CF.CF_GDIOBJFIRST: return CF_GDIOBJFIRST;
                case CF.CF_GDIOBJLAST: return CF_GDIOBJLAST;
            }

            return null;
        }
    }

    private record NativeDataFormat(string Mime) : DataFormat(Mime);

    private record UnknownDataFormat(string Mime) : DataFormat(Mime);

    private abstract class DataFormatSerializerProxy
    {
        public abstract int CalculateSize(object value);

        public abstract void Serialize(object value, Span<byte> buffer);

        public abstract object? Deserialize(Span<byte> buffer);
    }

    private sealed class DataFormatSerializerProxy<T> : DataFormatSerializerProxy where T : class
    {
        private readonly IDataFormatSerializer<T> _serializer;

        public DataFormatSerializerProxy(IDataFormatSerializer<T> serializer)
        {
            _serializer = serializer;
        }

        public override int CalculateSize(object value) => _serializer.CalculateSize((T)value);

        public override void Serialize(object value, Span<byte> buffer) => _serializer.Serialize((T)value, buffer);

        public override object? Deserialize(Span<byte> buffer) => _serializer.Deserialize(buffer);
    }
}
