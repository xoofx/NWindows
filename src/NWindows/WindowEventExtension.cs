// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.
namespace NWindows;

public static class WindowEventExtension
{
    public static string ToText(this WindowEventKind kind)
    {
        return kind switch
        {
            WindowEventKind.System => nameof(WindowEventKind.System),
            WindowEventKind.Frame => nameof(WindowEventKind.Frame),
            WindowEventKind.Paint => nameof(WindowEventKind.Paint),
            WindowEventKind.HitTest => nameof(WindowEventKind.HitTest),
            WindowEventKind.Keyboard => nameof(WindowEventKind.Keyboard),
            WindowEventKind.Mouse => nameof(WindowEventKind.Mouse),
            WindowEventKind.Close => nameof(WindowEventKind.Close),
            WindowEventKind.Text => nameof(WindowEventKind.Text),
            _ => nameof(WindowEventKind.Undefined)
        };
    }
}