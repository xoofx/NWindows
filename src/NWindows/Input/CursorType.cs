// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Input;

/// <summary>
/// An enumeration of the supported cursor types.
/// </summary>
public enum CursorType
{
    /// <summary>
    /// a value indicating that no cursor should be displayed at all.
    /// </summary>
    None = 0,

    /// <summary>
    /// Standard No Cursor.
    /// </summary>
    No,

    /// <summary>
    /// Standard arrow cursor.
    /// </summary>
    Arrow,

    /// <summary>
    /// CrossHair cursor.
    /// </summary>        
    Cross,

    /// <summary>
    /// Text I-Beam cursor.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    IBeam,

    /// <summary>
    /// Four-way pointing cursor.
    /// </summary>
    SizeAll,

    /// <summary>
    /// Double arrow pointing NE and SW.
    /// </summary>
    SizeNESW,

    /// <summary>
    /// Double arrow pointing N and S.
    /// </summary>
    SizeNS,

    /// <summary>
    /// Double arrow pointing NW and SE.
    /// </summary>
    SizeNWSE,

    /// <summary>
    /// Double arrow pointing W and E.
    /// </summary>
    SizeWE,

    /// <summary>
    /// Hourglass cursor.
    /// </summary>
    Wait,

    /// <summary>
    /// Hand cursor.
    /// </summary>
    Hand,
}