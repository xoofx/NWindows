// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace NWindows.Threading;

public static class DispatcherEventExtension
{
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