// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Input;
using NWindows.Threading;

namespace NWindows.Interop.Win32;

internal class Win32InputManager : InputManager
{
    public Win32InputManager(Dispatcher dispatcher) : base(dispatcher)
    {
        PrimaryKeyboardDevice = new Win32KeyboardDevice(this);
    }
    public override KeyboardDevice PrimaryKeyboardDevice { get; }
}