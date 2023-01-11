// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.CF;
using System.Threading;

namespace NWindows.Interop.Win32;

internal static unsafe class Win32Ole
{
    private const string LibraryOle32 = "Ole32";

    [DllImport(LibraryOle32, EntryPoint = nameof(RegisterDragDrop))]
    public static extern int RegisterDragDrop(nint hWnd, nint pDropTarget);

    [DllImport(LibraryOle32, EntryPoint = nameof(RevokeDragDrop))]
    public static extern int RevokeDragDrop(nint hWnd);

    [DllImport(LibraryOle32, EntryPoint = nameof(OleInitialize))]
    private static extern int OleInitialize(nint pvReserved);

    [DllImport(LibraryOle32, EntryPoint = nameof(ReleaseStgMedium))]
    public static extern void ReleaseStgMedium(STGMEDIUM* p);

    [DllImport(LibraryOle32, EntryPoint = nameof(OleGetClipboard))]
    public static extern int OleGetClipboard(nint ppDataObj);

    public static void Initialize()
    {
        if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
        {
            Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
            Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
        }
        Win32Ole.OleInitialize(NULL);
    }

    public static FileTransferList? GetDropFiles(IDataObject* pDataObj)
    {
        FORMATETC fmte = default;
        fmte.cfFormat = CF.CF_HDROP;
        fmte.dwAspect = 1; // DVASPECT_CONTENT
        fmte.lindex = -1;
        fmte.tymed = (int)TYMED.TYMED_HGLOBAL;

        STGMEDIUM stgm = default;
        if (pDataObj->GetData(&fmte, &stgm).SUCCEEDED)
        {
            var files = new FileTransferList();
            GetDropFiles(new HDROP(stgm.hGlobal), files);
            ReleaseStgMedium(&stgm);
            return files;
        }

        return null;
    }

    public static HDROP CreateDropFiles(IReadOnlyList<string> list)
    {
        // Create a DROPFILES as a HDROP
        // https://learn.microsoft.com/en-us/windows/win32/shell/clipboard?redirectedfrom=MSDN#cf_hdrop
        // https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/ns-shlobj_core-dropfiles
        var fileSize = 1; // need a 0 at the end
        foreach (var file in list)
        {
            if (string.IsNullOrEmpty(file)) continue;
            fileSize += file.Length + 1;
        }

        var totalSize = sizeof(DROPFILES) + fileSize * 2;

        var hGlobal = Win32Helper.AllocHGlobalMoveable(totalSize);
        var pDropFiles = (DROPFILES*)GlobalLock(hGlobal);

        *pDropFiles = default;
        pDropFiles->pFiles = (uint)sizeof(DROPFILES);
        pDropFiles->fWide = true;

        var pData = (char*)((byte*)pDropFiles + pDropFiles->pFiles);
        foreach (var file in list)
        {
            if (string.IsNullOrEmpty(file)) continue;
            file.CopyTo(new Span<char>(pData, file.Length));
            pData += file.Length + 1;
        }

        GlobalUnlock(hGlobal);

        return (HDROP)(nint)hGlobal;
    }


    public static void GetDropFiles(HDROP hDrop, FileTransferList files)
    {
        char* path = stackalloc char[MAX.MAX_PATH];
        var fileCount = DragQueryFileW(hDrop, uint.MaxValue, null, 0);
        for (uint i = 0; i < fileCount; i++)
        {
            var cch = DragQueryFileW(hDrop, i, (ushort*)path, MAX.MAX_PATH);
            if (cch > 0 && cch < MAX.MAX_PATH)
            {
                files.Add(new string(path, 0, (int)cch));
            }
        }
    }
}