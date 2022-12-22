// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NWindows;

public static class WindowEventExtension
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref WindowEvent AsWindowEvent<T>(this ref T evt) where T: struct, IWindowEvent
    {
        return ref Unsafe.As<T, WindowEvent>(ref evt);
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static ref T CastRef<T>(this ref WindowEvent evt) where T : struct, IWindowEvent
    //{
    //    if (evt.Kind != T.StaticKind) ThrowInvalidCast(evt.Kind, T.StaticKind);
    //    return ref Unsafe.As<WindowEvent, T>(ref evt);
    //}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Cast<T>(this ref WindowEvent evt) where T : struct, IWindowEvent
    {
        if (evt.Kind != T.StaticKind) ThrowInvalidCast(evt.Kind, T.StaticKind);
        return ref Unsafe.As<WindowEvent, T>(ref evt);
    }
    
    public static string ToText(this WindowEventKind kind)
    {
        return kind switch
        {
            WindowEventKind.Application => "application",
            WindowEventKind.Frame => "control",
            WindowEventKind.Keyboard => "keyboard",
            WindowEventKind.Mouse => "mouse",
            _ => "undefined"
        };
    }

    [DoesNotReturn]
    private static void ThrowInvalidCast(WindowEventKind from, WindowEventKind to)
    {
        throw new ArgumentException($"Invalid cast. Unable to cast message from `{from.ToText()}` to `{to.ToText()}`", "evt");
    }
}