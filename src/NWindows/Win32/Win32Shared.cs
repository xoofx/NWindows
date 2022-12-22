// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Win32;

internal static unsafe class Win32Shared
{
    public static readonly HMODULE ModuleHandle = GetModuleHandleW(null);
}