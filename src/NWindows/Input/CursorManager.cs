// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Input;

internal abstract class CursorManager
{
    public abstract Cursor GetCursor(CursorType cursorType);

    public abstract Cursor LoadFromFile(string fileName);
}