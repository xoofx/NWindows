// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace NWindows.Events;

public record DragDropEvent() : WindowEvent(WindowEventKind.DragDrop)
{
    public DragDropKind DragDropKind;

    public DragDropKeyStates KeyStates;

    public PointF Position;

    public DragDropEffects Effects;

    public List<string> Files { get; } = new();

    public bool Handled;

    protected override bool PrintMembers(StringBuilder builder)
    {
        if (base.PrintMembers(builder))
        {
            builder.Append(", ");
        }
        builder.Append(nameof(DragDropKind)).Append(" = ").Append(DragDropKind).Append(", ");
        builder.Append(nameof(KeyStates)).Append(" = ").Append(KeyStates).Append(", ");
        builder.Append(nameof(Position)).Append(" = ").Append(Position).Append(", ");
        builder.Append(nameof(Effects)).Append(" = ").Append(Effects).Append(", ");
        builder.Append(nameof(Files)).Append(" = [ ");
        for (var i = 0; i < Files.Count; i++)
        {
            var file = Files[i];
            if (i > 0) builder.Append(", ");
            builder.Append(file);
        }

        builder.Append("], ");
        builder.Append(nameof(Handled)).Append(" = ").Append(Handled);
        return true;
    }
}