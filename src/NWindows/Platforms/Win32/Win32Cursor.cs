// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using NWindows.Input;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.IDC;

namespace NWindows.Platforms.Win32;

internal unsafe class Win32Cursor : CursorImpl
{
    private readonly Cursor?[] _cursors;

    public Win32Cursor()
    {
        _cursors = new Cursor?[(int)CursorType.Hand + 1];
    }

    public override Cursor GetCursor(CursorType cursorType)
    {
        ref var cursor = ref _cursors[(int)cursorType];
        if (cursor == null)
        {
            var cursorIDC = GetIDCCursor(cursorType);
            var nativeCursor = cursorType == CursorType.None ? nint.Zero : (nint)LoadCursorW(HINSTANCE.NULL, cursorIDC);
            cursor ??= new Cursor(cursorType, nativeCursor);
        }
        return cursor;
    }

    public override Cursor LoadFromFile(string fileName)
    {
        throw new System.NotImplementedException();
    }

    private static ushort* GetIDCCursor(CursorType cursorType)
    {
        return cursorType switch
        {
            CursorType.None => (ushort*)0,
            CursorType.No => IDC_NO,
            CursorType.Arrow => IDC_ARROW,
            CursorType.Cross => IDC_CROSS,
            CursorType.IBeam => IDC_IBEAM,
            CursorType.SizeAll => IDC_SIZEALL,
            CursorType.SizeNESW => IDC_SIZENESW,
            CursorType.SizeNS => IDC_SIZENS,
            CursorType.SizeNWSE => IDC_SIZENWSE,
            CursorType.SizeWE => IDC_SIZEWE,
            CursorType.Wait => IDC_WAIT,
            CursorType.Hand => IDC_HAND,
            _ => throw new ArgumentOutOfRangeException(nameof(cursorType), cursorType, null)
        };
    }
}