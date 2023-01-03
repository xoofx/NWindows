// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Threading;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.CS;
using static TerraFX.Interop.Windows.GWLP;
using static TerraFX.Interop.Windows.IDC;
using static TerraFX.Interop.Windows.PM;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;

namespace NWindows.Win32;

/// <summary>
/// Implementation of the Dispatcher for Windows.
/// </summary>
internal unsafe class Win32Dispatcher : Dispatcher
{
    [ThreadStatic]
    private static WndMsg _previousMessage;

    private readonly List<Win32Window> _windows;
    private readonly Dictionary<HWND, Win32Window> _mapHandleToWindow;

    private readonly uint _uxdDisplayChangeMessage;
    private readonly uint _hotplugDetected;
    private int _runMessageLoop;
    private bool _exitFromCurrentMessageLoop;

    [ThreadStatic] internal static GCHandle CreatedWindowHandle;

    public Win32Dispatcher(Thread thread) : base(thread)
    {
        _windows = new List<Win32Window>();
        _mapHandleToWindow = new Dictionary<HWND, Win32Window>();

        var className = $"NWindows-{Guid.NewGuid():N}";
        fixed (char* lpszClassName = className)
        {
            // Initialize the window class.
            var windowClass = new WNDCLASSEXW
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = CS_DBLCLKS | CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = &WindowProc,
                hInstance = Win32Shared.ModuleHandle,
                hCursor = LoadCursorW(HINSTANCE.NULL, (ushort*)IDC_ARROW),
                hbrBackground = (HBRUSH)(COLOR.COLOR_WINDOW + 1),
                lpszClassName = (ushort*)lpszClassName
            };

            ClassAtom = RegisterClassExW(&windowClass);
            if (ClassAtom == 0)
            {
                // This should never happen, but in case.
                throw new InvalidOperationException($"Failed to initialize Windows Dispatcher.");
            }
        }

        fixed (char* lpszClassName = "UxdDisplayChangeMessage")
        {
            _uxdDisplayChangeMessage = Windows.RegisterWindowMessageW((ushort*)lpszClassName);
        }

        fixed (char* lpszClassName = "HotplugDetected")
        {
            _hotplugDetected = Windows.RegisterWindowMessageW((ushort*)lpszClassName);
        }

        ScreenManager = new Win32ScreenManager();
    }

    ~Win32Dispatcher()
    {
        ReleaseUnmanagedResources();
    }

    public ushort ClassAtom { get; }

    internal override Win32ScreenManager ScreenManager { get; }

    protected override void RequestShutdown()
    {
        PostQuitMessage(0);
    }

    protected override void RunMessageLoop(Window? window)
    {
        MSG msg;

        _runMessageLoop++;
        if (window != null)
        {
            if (window.Kind == WindowKind.Popup)
            {
                window.Modal = true;
            }
        }

        try
        {
            while (!_exitFromCurrentMessageLoop)
            {
                if (PeekMessageW(&msg, HWND.NULL, wMsgFilterMin: WM_NULL, wMsgFilterMax: WM_NULL, wRemoveMsg: PM_REMOVE))
                {
                    if (msg.message == WM_QUIT)
                    {
                        break;
                    }
                    else
                    {
                        TranslateMessage(&msg);
                        _ = DispatchMessageW(&msg);
                    }
                }
            }
        }
        finally
        {
            _exitFromCurrentMessageLoop = false;
            _runMessageLoop--;
        }
    }

    private void RegisterWindow(Win32Window window)
    {
        _windows.Add(window);
        _mapHandleToWindow[(HWND)window.Handle] = window;
    }


    private void UnRegisterWindow(Win32Window window)
    {
        _windows.Remove(window);
        _mapHandleToWindow.Remove((HWND)window.Handle);
    }

    public bool TryGetWindowByHandle(HWND handle, [NotNullWhen(true)] out Win32Window? window)
    {
        return _mapHandleToWindow.TryGetValue(handle, out window);
    }
    
    internal bool TryHandleScreenChanges(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        // https://stackoverflow.com/a/33762334/1356325
        bool updateScreens = false;
        switch (message)
        {
            case WM.WM_DEVICECHANGE:
            case WM.WM_SETTINGCHANGE:
            case WM.WM_DISPLAYCHANGE:
                updateScreens = true;
                break;
            default:
                if (message == _uxdDisplayChangeMessage)
                {
                    updateScreens = true;
                }
                else if (message == _hotplugDetected)
                {
                    updateScreens = true;
                }
                break;

        }

        return updateScreens && ScreenManager.TryUpdateScreens();
    }

    [UnmanagedCallersOnly]
    private static LRESULT WindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        LRESULT result = -1;
        try
        {
            // Log message that are only different
            var newMessage = new WndMsg(hWnd, message, wParam, lParam);
            if (_previousMessage != newMessage)
            {
                Win32Helper.OutputDebugWinProc(newMessage);
                _previousMessage = newMessage;
            }

            var handle = GetWindowLongPtrW(hWnd, GWLP_USERDATA);
            Win32Window winWindow;
            if (handle == 0)
            {
                winWindow = (Win32Window)CreatedWindowHandle.Target!;
                handle = GCHandle.ToIntPtr(CreatedWindowHandle);
                _ = SetWindowLongPtrW(hWnd, GWLP_USERDATA, handle);
            }
            else
            {
                winWindow = (Win32Window)GCHandle.FromIntPtr(handle).Target!;
            }

            switch (message)
            {
                case WM_CREATE:
                    winWindow.Dispatcher.RegisterWindow(winWindow);
                    result = winWindow.WindowProc(hWnd, message, wParam, lParam);
                    break;

                case WM_DESTROY:
                {
                    var popup = winWindow.Kind == WindowKind.Popup;
                    result = winWindow.WindowProc(hWnd, message, wParam, lParam);
                    winWindow.Dispatcher.UnRegisterWindow(winWindow);

                    // TODO: Allow to customize this behavior
                    if (winWindow.Dispatcher._windows.Count == 0)
                    {
                        PostQuitMessage(0);
                    }
                    else
                    {
                        if (popup)
                        {
                            winWindow.Dispatcher._exitFromCurrentMessageLoop = true;
                        }
                    }
                    result = 0;
                }
                    break;

                default:
                {
                    result = winWindow.WindowProc(hWnd, message, wParam, lParam);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception?
            // We should never crash with an exception in a WindowProc
            result = -1;
        }
        finally
        {
            if (result < 0)
            {
                result = DefWindowProcW(hWnd, message, wParam, lParam);
            }
        }

        return result;
    }

    private void ReleaseUnmanagedResources()
    {
        if (ClassAtom != 0)
        {
            UnregisterClassW((ushort*)ClassAtom, Win32Shared.ModuleHandle);
        }
    }

    public void OnSystemEvent(SystemEventKind systemEventKind)
    {
        var localEvent = new WindowEvent(WindowEventKind.System);
        ref var sysEvent = ref localEvent.Cast<SystemEvent>();
        foreach (var window in _windows)
        {
            sysEvent.SubKind = systemEventKind;
            window.OnWindowEvent(ref localEvent);
        }
    }
}