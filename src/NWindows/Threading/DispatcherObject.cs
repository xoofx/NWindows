// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Runtime.CompilerServices;

namespace NWindows.Threading;

public abstract class DispatcherObject
{
    protected DispatcherObject() : this(Dispatcher.Current)
    {
    }

    protected DispatcherObject(Dispatcher dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public Dispatcher Dispatcher { get; }

    /// <summary>
    /// Checks that the current thread owns this hub.
    /// </summary>
    /// <returns><c>true</c> if the current thread owns this hub, <c>false</c> otherwise.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CheckAccess() => Dispatcher.CheckAccess();

    /// <summary>
    /// Verifies that the current thread owns this hub or throw an exception if not.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void VerifyAccess() => Dispatcher.VerifyAccess();
}