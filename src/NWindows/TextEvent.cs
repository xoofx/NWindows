// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Text;

namespace NWindows;

public struct TextEvent : IWindowEvent
{
    static WindowEventKind IWindowEvent.StaticKind => WindowEventKind.Text;

    public TextEvent()
    {
        Kind = WindowEventKind.Text;
    }

    public WindowEventKind Kind { get; }

    public Rune Rune;
    
    public override string ToString()
    {
        return $"{nameof(Kind)}: {Kind}, {nameof(Rune)}: {Rune}";
    }
}