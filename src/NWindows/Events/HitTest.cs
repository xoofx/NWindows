// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// The result of a hit-test for a <see cref="HitTestEvent"/>.
/// </summary>
public enum HitTest
{
    /// <summary>
    /// The test is inconclusive.
    /// </summary>
    None,

    /// <summary>
    /// The hit-test points to a menu.
    /// </summary>
    Menu,

    /// <summary>
    /// The hit-test points to a help button.
    /// </summary>
    Help,

    /// <summary>
    /// The hit-test points to the caption.
    /// </summary>
    Caption,

    /// <summary>
    /// The hit-test points to the minimize button.
    /// </summary>
    MinimizeButton,

    /// <summary>
    /// The hit-test points to the maximize button.
    /// </summary>
    MaximizeButton,

    /// <summary>
    /// The hit-test points to the close button.
    /// </summary>
    CloseButton,

    /// <summary>
    /// The hit-test points to the left border.
    /// </summary>
    BorderLeft,

    /// <summary>
    /// The hit-test points to the right border.
    /// </summary>
    BorderRight,

    /// <summary>
    /// The hit-test points to the top border.
    /// </summary>
    BorderTop,

    /// <summary>
    /// The hit-test points to the bottom border.
    /// </summary>
    BorderBottom,

    /// <summary>
    /// The hit-test points to the top-left border.
    /// </summary>
    BorderTopLeft,

    /// <summary>
    /// The hit-test points to the top-right border.
    /// </summary>
    BorderTopRight,

    /// <summary>
    /// The hit-test points to the bottom-left border.
    /// </summary>
    BorderBottomLeft,

    /// <summary>
    /// The hit-test points to the bottom-right border.
    /// </summary>
    BorderBottomRight,

    /// <summary>
    /// The hit-test points to the client area of the window.
    /// </summary>
    Client
}