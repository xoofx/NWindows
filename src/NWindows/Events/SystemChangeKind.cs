// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Events;

/// <summary>
/// A kind of change for a <see cref="SystemEvent"/>.
/// </summary>
public enum SystemChangeKind
{
    /// <summary>
    /// No changes.
    /// </summary>
    None = 0,

    /// <summary>
    /// Screens changed (activated, deactivated, resolution changed...)
    /// </summary>
    ScreenChanged,
}