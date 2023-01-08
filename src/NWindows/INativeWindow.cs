// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;

namespace NWindows;

/// <summary>
/// Interface for a window with its platform dependent main handle.
/// </summary>
public interface INativeWindow
{
    /// <summary>
    /// Gets the platform dependent main handle for this Window.
    /// </summary>
    IntPtr Handle { get; }
}