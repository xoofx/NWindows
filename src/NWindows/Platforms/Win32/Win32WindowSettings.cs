// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using TerraFX.Interop.Windows;
using TerraFX.Interop.WinRT;

namespace NWindows.Platforms.Win32;

internal unsafe class Win32WindowSettings : WindowSettingsImpl
{
    private readonly IUISettings3* _uiSettings3;

    public Win32WindowSettings()
    {
        const string uiSettingsClassName = "Windows.UI.ViewManagement.UISettings";
        HSTRING hstring;
        fixed (char* name = uiSettingsClassName)
        {
            ThrowIfFailed(WinRT.WindowsCreateString((ushort*)name, (uint)uiSettingsClassName.Length, &hstring));
        }

        IInspectable* inspectable;
        ThrowIfFailed(WinRT.RoActivateInstance(hstring, &inspectable));
        WinRT.WindowsDeleteString(hstring);

        var guid = new Guid("03021BE4-5254-4781-8194-5168F7D06D7B");
        IUISettings3* uiSettings3;
        ThrowIfFailed(inspectable->QueryInterface(&guid, (void**)&uiSettings3));
        _uiSettings3 = uiSettings3;
    }

    public override WindowTheme Theme => IsLightColor(ForegroundColor) ? WindowTheme.Dark : WindowTheme.Light;

    public override Color AccentColor => _uiSettings3->GetColorValue(UIColorType.Accent);

    public override Color BackgroundColor => _uiSettings3->GetColorValue(UIColorType.Background);

    public override Color ForegroundColor => _uiSettings3->GetColorValue(UIColorType.Foreground);

    private static bool IsLightColor(Color color) => (((5 * color.G) + (2 * color.R) + color.B) > (8 * 128));

    // Extract of the IUISettings3 from windows.ui.viewmanagement.idl
    private struct IUISettings3
    {
        public void** lpVtbl;

        public Color GetColorValue(UIColorType desiredColor)
        {
            UIColor value;
            // The GetColorValue method comes right after IInspectable and is at VTBL slot 6
            ((delegate* unmanaged<IUISettings3*, UIColorType, UIColor*, int>)(lpVtbl[6]))((IUISettings3*)Unsafe.AsPointer(ref this), desiredColor, &value);
            return Color.FromArgb(*(int*)&value);
        }
    }

    private enum UIColorType
    {
        Background = 0,
        Foreground = 1,
        AccentDark3 = 2,
        AccentDark2 = 3,
        AccentDark1 = 4,
        Accent = 5,
        AccentLight1 = 6,
        AccentLight2 = 7,
        AccentLight3 = 8,
        Complement = 9
    };

    private readonly record struct UIColor(byte A, byte R, byte G, byte B);

    private static void ThrowIfFailed(HRESULT result)
    {
        if (result.FAILED)
        {
            throw new PlatformNotSupportedException("Unable to access `Windows.UI.ViewManagement.UISettings`. Platform not supported");
        }
    }
}