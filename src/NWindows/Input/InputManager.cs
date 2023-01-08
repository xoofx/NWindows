// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Threading;

namespace NWindows.Input;

public abstract class InputManager : DispatcherObject
{
    internal InputManager(Dispatcher dispatcher) : base(dispatcher)
    {
    }

    public abstract KeyboardDevice PrimaryKeyboardDevice { get; }

    public abstract MouseDevice PrimaryMouseDevice { get; }
    
    public static InputManager Current => Dispatcher.Current.InputManager;
}