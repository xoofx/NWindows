// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace NWindows.Interop.Win32;

internal sealed unsafe class Win32ScreenManager : IScreenManager
{
    private readonly Dictionary<HMONITOR, Win32Screen> _mapMonitorToScreen;
    private readonly List<Screen> _tempCollectScreens;
    private bool _screenAddedOrUpdated;
    private readonly GCHandle _thisGcHandle;
    private Screen[] _items;
    private Screen? _primaryScreen;
    private Point _virtualScreenPosition;
    private Size _virtualScreenSize;

    public Win32ScreenManager()
    {
        _mapMonitorToScreen = new Dictionary<HMONITOR, Win32Screen>();
        _thisGcHandle = GCHandle.Alloc(this);
        _tempCollectScreens = new List<Screen>(4);
        _items = Array.Empty<Screen>();

        _ = TryUpdateScreens();
    }

    public ReadOnlySpan<Screen> GetAllScreens()
    {
        return _items;
    }

    public Screen? GetPrimaryScreen()
    {
        return _primaryScreen;
    }

    public Point GetVirtualScreenPosition() => _virtualScreenPosition;

    public Size GetVirtualScreenSize() => _virtualScreenSize;

    public bool TryGetScreen(HMONITOR monitorHandle, [NotNullWhen(true)] out Win32Screen? screen)
    {
        return _mapMonitorToScreen.TryGetValue(monitorHandle, out screen);
    }

    public bool TryUpdateScreens()
    {
        // Clear the data before collecting information about screens
        _tempCollectScreens.Clear();
        _screenAddedOrUpdated = false;

        _ = Windows.EnumDisplayMonitors(HDC.NULL, (RECT*)null, &EnumDisplayMonitorProc, GCHandle.ToIntPtr(_thisGcHandle));
        var updated = _screenAddedOrUpdated;

        var primaryScreen = _primaryScreen;

        foreach (var screen in _tempCollectScreens)
        {
            if (screen.IsPrimary)
            {
                if (primaryScreen != screen)
                {
                    primaryScreen = screen;
                    updated = true;
                }
            }

            _mapMonitorToScreen.Remove((HMONITOR)screen.Handle);
        }

        if (_tempCollectScreens.Count == 0)
        {
            if (primaryScreen is { })
            {
                primaryScreen = null;
                updated = true;
            }
        }

        if (_mapMonitorToScreen.Count > 0)
        {
            updated = true;
        }

        _mapMonitorToScreen.Clear();
        foreach (var screen in _tempCollectScreens)
        {
            _mapMonitorToScreen[(HMONITOR)screen.Handle] = (Win32Screen)screen;
        }

        var virtualScreenPosition = new Point(Windows.GetSystemMetrics(SM.SM_XVIRTUALSCREEN), Windows.GetSystemMetrics(SM.SM_YVIRTUALSCREEN));
        var virtualScreenSize = new Size(Windows.GetSystemMetrics(SM.SM_CXVIRTUALSCREEN), Windows.GetSystemMetrics(SM.SM_CYVIRTUALSCREEN));

        if (!_virtualScreenPosition.Equals(virtualScreenPosition))
        {
            _virtualScreenPosition = virtualScreenPosition;
            updated = true;
        }

        if (!_virtualScreenSize.Equals(virtualScreenSize))
        {
            _virtualScreenSize = virtualScreenSize;
            updated = true;
        }

        _primaryScreen = primaryScreen;

        if (updated)
        {
            // TODO: avoid the ToArray
            _items = _tempCollectScreens.ToArray();
        }

        return updated;
    }

    [UnmanagedCallersOnly]
    private static BOOL EnumDisplayMonitorProc(HMONITOR monitor, HDC hdc, RECT* lpRect, LPARAM lParam)
    {
        var manager = (Win32ScreenManager)GCHandle.FromIntPtr((nint)lParam).Target!;

        Win32ScreenData.TryGetScreenState(monitor, out var screenData);

        if (!manager._mapMonitorToScreen.TryGetValue(monitor, out var screen))
        {
            screen = new Win32Screen(monitor, screenData);
            manager._screenAddedOrUpdated = true;
        }
        else if (screenData != screen.InternalData)
        {
            screen.InternalData = screenData;
            manager._screenAddedOrUpdated = true;
        }
        manager._tempCollectScreens.Add(screen);

        return true;
    }
}