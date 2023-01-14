// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using TerraFX.Interop.Windows;

namespace NWindows.Interop.Win32;

internal sealed class Win32Screen : Screen
{
    internal Win32ScreenData InternalData;

    public Win32Screen(HMONITOR monitor, in Win32ScreenData data)
    {
        Handle = monitor;
        InternalData = data;
    }

    public override bool IsValid
    {
        get
        {
            VerifyAccess();
            return InternalData.IsValid;
        }
    }

    public override bool IsPrimary
    {
        get
        {
            VerifyAccess();
            return InternalData.IsPrimary;
        }
    }

    public override string Name
    {
        get
        {
            VerifyAccess();
            return InternalData.Name;
        }
    }

    public override Point Position
    {
        get
        {
            VerifyAccess();
            return InternalData.Position;
        }
    }

    public override SizeF Size
    {
        get
        {
            VerifyAccess();
            return InternalData.Size;
        }
    }

    public override Size SizeInPixels
    {
        get
        {
            VerifyAccess();
            return InternalData.SizeInPixels;
        }
    }

    public override ref readonly DpiScale DpiScale
    {
        get
        {
            VerifyAccess();
            return ref InternalData.DpiScale;
        }
    }

    public override ref readonly ScreenMode CurrentDisplayMode
    {
        get
        {
            VerifyAccess();
            return ref InternalData.CurrentDisplayMode;
        }
    }

    public override ReadOnlySpan<ScreenMode> DisplayModes
    {
        get
        {
            VerifyAccess();
            return InternalData.ScreenModes;
        }
    }
}