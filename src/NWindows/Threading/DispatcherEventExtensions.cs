// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace NWindows.Threading;

/// <summary>
/// Extension method for dispatcher event.
/// </summary>
public static class DispatcherEventExtension
{
    /// <summary>
    /// Converts an enum to a string.
    /// </summary>
    /// <param name="kind">The enum to convert.</param>
    /// <returns>The string representation.</returns>
    public static string ToText(this DispatcherEventKind kind)
    {
        return kind switch
        {
            DispatcherEventKind.Idle => nameof(DispatcherEventKind.Idle),
            DispatcherEventKind.ShutdownStarted => nameof(DispatcherEventKind.ShutdownStarted),
            DispatcherEventKind.ShutdownFinished => nameof(DispatcherEventKind.ShutdownFinished),
            DispatcherEventKind.UnhandledException => nameof(DispatcherEventKind.UnhandledException),
            DispatcherEventKind.UnhandledExceptionFilter => nameof(DispatcherEventKind.UnhandledExceptionFilter),
            _ => nameof(DispatcherEventKind.Undefined)
        };
    }
}