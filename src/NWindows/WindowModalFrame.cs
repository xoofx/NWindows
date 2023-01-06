// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using NWindows.Threading;

namespace NWindows;

public sealed class WindowModalFrame : DispatcherFrame
{
    public WindowModalFrame(Dispatcher dispatcher, Window window) : base(dispatcher, false)
    {
        Window = window;
    }

    public Window Window { get; }

    protected override void Enter()
    {
        Window.Modal = true;
    }
    
    protected override void Leave()
    {
        // The Window has been destroyed if we leave, so we don't need to modify the modal of the parent
    }
}