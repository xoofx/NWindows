// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using System.Text;

namespace NWindows.Events;

public record DragDropEvent() : WindowEvent(WindowEventKind.DragDrop)
{
    public DragDropKind DragDropKind;

    public DragDropKeyStates KeyStates;

    public DataTransferEffects Effects;

    public PointF Position;
    
    public object? Data;

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
        builder.Append(nameof(Data)).Append(" = ");
        if (Data is FileTransferList list)
        {
            builder.Append(list);
        }
        else if (Data is byte[] array)
        {
            builder.Append("byte[").Append(array.Length).Append(']');
        }
        else
        {
            builder.Append(Data);
        }

        builder.Append(", ");

        builder.Append(nameof(Handled)).Append(" = ").Append(Handled);
        return true;
    }
}