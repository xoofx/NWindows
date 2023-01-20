// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Diagnostics;
using System.Threading;

namespace NWindows.Threading;

/// <summary>
/// A synchronization context associated with a dispatcher.
/// </summary>
[DebuggerDisplay("DispatcherSynchronizationContext {Dispatcher}")]
public class DispatcherSynchronizationContext : SynchronizationContext
{
    /// <summary>
    /// Constructs a new instance of the DispatcherSynchronizationContext
    /// using the current Hub and normal post priority.
    /// </summary>
    public DispatcherSynchronizationContext() : this(Dispatcher.Current)
    {
    }

    /// <summary>
    /// Constructs a new instance of the DispatcherSynchronizationContext
    /// using the specified Hub and normal post priority.
    /// </summary>
    public DispatcherSynchronizationContext(Dispatcher hub)
    {
        Dispatcher = hub;

        // Tell the CLR to call us when blocking.
        SetWaitNotificationRequired();
    }

    public Dispatcher Dispatcher { get; }

    /// <summary>
    /// Synchronously invoke the callback in the SynchronizationContext.
    /// </summary>
    public override void Send(SendOrPostCallback d, object? state)
    {
        Dispatcher.Invoke(() => d(state));
    }

    /// <summary>
    /// Asynchronously invoke the callback in the SynchronizationContext.
    /// </summary>
    public override void Post(SendOrPostCallback d, object? state)
    {
        Dispatcher.InvokeAsync(() => d(state));
    }

    /// <summary>
    /// Create a copy of this SynchronizationContext.
    /// </summary>
    public override SynchronizationContext CreateCopy()
    {
        return new DispatcherSynchronizationContext(Dispatcher);
    }
}