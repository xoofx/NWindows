// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Threading;

namespace NWindows.Input;

/// <summary>
/// Abstract class that describes an input device.
/// </summary>
public abstract class InputDevice : DispatcherObject
{
    internal InputDevice(InputManager manager) : base(manager.Dispatcher)
    {
        Manager = manager;
    }

    /// <summary>
    /// Is this device active.
    /// </summary>
    public abstract bool IsActive { get; }
    
    /// <summary>
    /// Gets the associated input manager.
    /// </summary>
    public InputManager Manager { get; }
}