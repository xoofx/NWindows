// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Threading;

/// <summary>
/// The kind of a <see cref="DispatcherEvent"/>.
/// </summary>
public enum DispatcherEventKind
{
    /// <summary>
    /// Undefined.
    /// </summary>
    Undefined,

    /// <summary>
    /// An idle event.
    /// </summary>
    Idle,

    /// <summary>
    /// A shutdown started event.
    /// </summary>
    ShutdownStarted,

    /// <summary>
    /// A shutdown finished event.
    /// </summary>
    ShutdownFinished,

    /// <summary>
    /// An unhandled exception event.
    /// </summary>
    UnhandledException,

    /// <summary>
    /// An unhandled exception filter event.
    /// </summary>
    UnhandledExceptionFilter
}