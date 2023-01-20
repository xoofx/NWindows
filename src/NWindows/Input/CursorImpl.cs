// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Input;

/// <summary>
/// Internal Cursor abstract class that requires a platform dependent implementation.
/// </summary>
internal abstract class CursorImpl
{
    public abstract Cursor GetCursor(CursorType cursorType);

    public abstract Cursor LoadFromFile(string fileName);
}