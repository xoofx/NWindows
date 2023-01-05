// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows.Input;

/// <summary>
///     The KeyStates enumeration describes the state that keyboard keys
///     can be in.
/// </summary>
[Flags]
public enum KeyStates : byte
{
    /// <summary>
    ///     No state (same as up).
    /// </summary>
    None = 0,

    /// <summary>
    ///    The key is down.
    /// </summary>
    Down = 1,

    /// <summary>
    ///    The key is toggled on.
    /// </summary>
    Toggled = 2
}