// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows;

public record WindowState
{
    internal  WindowState(WindowStateKind kind)
    {
        Kind = kind;
    }
    
    public WindowStateKind Kind { get; }

    public static readonly WindowState Normal = new (WindowStateKind.Normal);

    public static readonly WindowState Minimized = new (WindowStateKind.Minimized);

    public static readonly WindowState Maximized = new (WindowStateKind.Maximized);

    public static readonly WindowState FullScreen = new (WindowStateKind.FullScreen);

    public bool IsFullScreen => Kind is WindowStateKind.FullScreen or WindowStateKind.ExclusiveFullScreen;

    public sealed record ExclusiveFullScreen(ScreenMode Mode) : WindowState(WindowStateKind.ExclusiveFullScreen);
}