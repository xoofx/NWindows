// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using NWindows.Platforms.Win32;

// ReSharper disable InconsistentNaming

namespace NWindows.Input;

/// <summary>
/// The cursor class allows to get cursors that can be set by <see cref="Mouse.SetCursor"/>.
/// </summary>
public sealed class Cursor
{
    private static readonly CursorImpl Impl = GetCursorManager();

    internal Cursor(CursorType cursorType, nint handle)
    {
        CursorType = cursorType;
        Handle = handle;
        FileName = string.Empty;
    }

    /// <summary>
    /// Gets the type of cursor.
    /// </summary>
    public CursorType CursorType { get; }

    /// <summary>
    /// Gets the filename associated to a cursor.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the native handle associated to this cursor.
    /// </summary>
    /// <remarks>
    /// - On Windows: This is a HCURSOR.
    /// </remarks>
    public nint Handle { get; }

    /// <summary>
    /// Gets a cursor that should not be displayed at all.
    /// </summary>
    public static Cursor None => CursorNone.Instance;

    /// <summary>
    /// Gets the Standard arrow cursor.
    /// </summary>
    public static Cursor Arrow => CursorArrow.Instance;

    /// <summary>
    /// Gets the Text I-Beam cursor.
    /// </summary>
    public static Cursor IBeam => CursorIBeam.Instance;

    /// <summary>
    /// Gets the Hourglass cursor.
    /// </summary>
    public static Cursor Wait => CursorWait.Instance;

    /// <summary>
    /// Gets the CrossHair cursor.
    /// </summary>
    public static Cursor Cross => CursorCross.Instance;

    /// <summary>
    /// Gets the Double arrow pointing NW and SE.
    /// </summary>
    public static Cursor SizeNWSE => CursorSizeNWSE.Instance;

    /// <summary>
    /// Gets the Double arrow pointing NE and SW.
    /// </summary>
    public static Cursor SizeNESW => CursorSizeNESW.Instance;

    /// <summary>
    /// Gets the Double arrow pointing W and E.
    /// </summary>
    public static Cursor SizeWE => CursorSizeWE.Instance;

    /// <summary>
    /// Gets the Double arrow pointing N and S.
    /// </summary>
    public static Cursor SizeNS => CursorSizeNS.Instance;

    /// <summary>
    /// Gets the Four-way pointing cursor.
    /// </summary>
    public static Cursor SizeAll => CursorSizeAll.Instance;

    /// <summary>
    /// Gets the Standard No Cursor.
    /// </summary>
    public static Cursor No => CursorNo.Instance;

    /// <summary>
    /// Gets the Hand cursor.
    /// </summary>
    public static Cursor Hand => CursorHand.Instance;

    /// <summary>
    /// Loads the specified cursor from a file.
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// TODO: Not implemented yet.
    private static Cursor LoadFromFile(string fileName) => Impl.LoadFromFile(fileName);

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