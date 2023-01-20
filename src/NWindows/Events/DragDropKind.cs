// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// The kind of <see cref="DragDropEvent"/>.
/// </summary>
public enum DragDropKind
{
    /// <summary>
    /// The drag and drop operation is entering.
    /// </summary>
    Enter,

    /// <summary>
    /// The drag and drop operation is over-ing.
    /// </summary>
    Over,

    /// <summary>
    /// The drag and drop operation is dropping.
    /// </summary>
    Drop,

    /// <summary>
    /// The drag and drop operation is leaving.
    /// </summary>
    Leave,
}