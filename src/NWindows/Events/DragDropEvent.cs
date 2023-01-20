// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using System.Text;

namespace NWindows.Events;

/// <summary>
/// The drag and drop event is triggered when a file or list of files are dropped on a window that is supporting drag and drop (set via <see cref="Window.DragDrop"/>).
/// </summary>
public record DragDropEvent() : WindowEvent(WindowEventKind.DragDrop)
{
    /// <summary>
    /// Gets the kind of drag and drop.
    /// </summary>
    public DragDropKind DragDropKind { get; set; }

    /// <summary>
    /// Gets the state of the key.
    /// </summary>
    public DragDropKeyStates KeyStates { get; set; }

    /// <summary>
    /// Gets the effects associated with this drag/drop operation.
    /// </summary>
    public DataTransferEffects Effects { get; set; }

    /// <summary>
    /// Gets the position of the drag-drop.
    /// </summary>
    public PointF Position;

    /// <summary>
    /// Gets the associated data. See <see cref="FileTransferList"/> for a list of files drag/drop.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Gets a boolean indicating whether this event is handled by the handler.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Print members of this record.
    /// </summary>
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