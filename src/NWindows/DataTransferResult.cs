// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

/// <summary>
/// An enumeration to notify back a transfer result to the clipboard <see cref="Clipboard.Notify"/>
/// </summary>
public enum DataTransferResult
{
    /// <summary>
    /// No action has been performed.
    /// </summary>
    None = 0,
    /// <summary>
    /// A copy action has been performed.
    /// </summary>
    Copy = DataTransferEffects.Copy,
    /// <summary>
    /// A move action has been performed.
    /// </summary>
    Move = DataTransferEffects.Move,
    /// <summary>
    /// A link action has been performed.
    /// </summary>
    Link = DataTransferEffects.Link,
}