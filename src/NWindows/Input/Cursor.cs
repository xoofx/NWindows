// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using NWindows.Interop.Win32;
// ReSharper disable InconsistentNaming

namespace NWindows.Input;

public sealed class Cursor
{
    private static readonly CursorImpl Impl = GetCursorManager();

    internal Cursor(CursorType cursorType, nint handle)
    {
        CursorType = cursorType;
        Handle = handle;
        FileName = string.Empty;
    }

    public CursorType CursorType { get; }

    public string FileName { get; }

    public nint Handle { get; }

    public static Cursor None => CursorNone.Instance;

    public static Cursor Arrow => CursorArrow.Instance;

    public static Cursor IBeam => CursorIBeam.Instance;

    public static Cursor Wait => CursorWait.Instance;

    public static Cursor Cross => CursorCross.Instance;

    public static Cursor SizeNWSE => CursorSizeNWSE.Instance;

    public static Cursor SizeNESW => CursorSizeNESW.Instance;

    public static Cursor SizeWE => CursorSizeWE.Instance;

    public static Cursor SizeNS => CursorSizeNS.Instance;

    public static Cursor SizeAll => CursorSizeAll.Instance;

    public static Cursor No => CursorNo.Instance;

    public static Cursor Hand => CursorHand.Instance;

    public static Cursor LoadFromFile(string fileName) => Impl.LoadFromFile(fileName);

    private static CursorImpl GetCursorManager()
    {
        if (OperatingSystem.IsWindows()) return new Win32Cursor();
        throw new PlatformNotSupportedException();
    }

    // Following are the cursors instances, one per static class to avoid loading all of them
    // if we are only using one of them (or none)

    private static class CursorNone
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.None);
    }

    private static class CursorArrow
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.Arrow);
    }

    private static class CursorIBeam
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.IBeam);
    }

    private static class CursorWait
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.Wait);
    }

    private static class CursorCross
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.Cross);
    }

    private static class CursorSizeNWSE
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.SizeNWSE);
    }

    private static class CursorSizeNESW
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.SizeNESW);
    }

    private static class CursorSizeWE
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.SizeWE);
    }

    private static class CursorSizeNS
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.SizeNS);
    }

    private static class CursorSizeAll
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.SizeAll);
    }

    private static class CursorNo
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.No);
    }

    private static class CursorHand
    {
        public static readonly Cursor Instance = Impl.GetCursor(CursorType.Hand);
    }
}