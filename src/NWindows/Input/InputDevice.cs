// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

namespace NWindows.Input;

// https://learn.microsoft.com/en-us/dotnet/api/system.windows.input.inputdevice?view=windowsdesktop-7.0

/// <summary>
/// Abstract class that describes an input device.
/// </summary>
public abstract class InputDevice : DispatcherObject
{
    internal InputDevice(InputManager manager) : base(manager.Dispatcher)
    {
        Manager = manager;
    }

    public InputManager Manager { get; }
}