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
    private WndMsg _previousMessage;
    private HWND _thisHwnd;
    private nuint _timerId;
    private readonly Dictionary<nuint, DispatcherTimer> _mapTimerIdToTimer;
    private readonly Dictionary<DispatcherTimer, nuint> _mapTimerToTimerId;

    private readonly List<Win32Window> _windows;
    private readonly Dictionary<HWND, Win32Window> _mapHandleToWindow;

    private readonly uint _uxdDisplayChangeMessage;
    private readonly uint _hotplugDetected;
    private int _runMessageLoop;
    private bool _exitFromCurrentMessageLoop;
    private readonly uint WM_DISPATCHER_QUEUE;

    [ThreadStatic] internal static GCHandle CreatedWindowHandle;

    public Win32Dispatcher(Thread thread) : base(thread)
    {
        _windows = new List<Win32Window>();
        _mapHandleToWindow = new Dictionary<HWND, Win32Window>();
        _mapTimerIdToTimer = new Dictionary<nuint, DispatcherTimer>();
        _mapTimerToTimerId = new Dictionary<DispatcherTimer, nuint>(ReferenceEqualityComparer.Instance);

        var guidAsString = Guid.NewGuid().ToString("N");
        var className = $"NWindows-{guidAsString}";
        fixed (char* lpszClassName = className)
        {
            // Initialize the window class.
            var windowClass = new WNDCLASSEXW
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                style = CS_DBLCLKS | CS_HREDRAW | CS_VREDRAW,
                lpfnWndProc = &StaticWindowProc,
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
        InputManager = new Win32InputManager(this);

        fixed (char* lpszWindowMessage = "NWindows.DispatcherProcessQueue")
            WM_DISPATCHER_QUEUE = RegisterWindowMessageW((ushort*)lpszWindowMessage);

        var windowName = $"NWindows-Dispatcher-{guidAsString}";
        fixed (char* lpWindowName = windowName)
            _thisHwnd = CreateWindowEx(0, (ushort*)ClassAtom, (ushort*)lpWindowName, 0, 0, 0, 0, 0, HWND.HWND_MESSAGE, HMENU.NULL, HINSTANCE.NULL, null);
    }

    ~Win32Dispatcher()
    {
        ReleaseUnmanagedResources();
    }

    public ushort ClassAtom { get; }
    
    private new static Win32Dispatcher Current => (Win32Dispatcher)(Dispatcher.Current);

    internal override Win32ScreenManager ScreenManager { get; }

    internal override Win32InputManager InputManager { get; }

    internal override bool WaitAndDispatchMessage()
    {
        MSG msg;
        // Use Peek/Get and handle idle
        if (PeekMessageW(&msg, HWND.NULL, wMsgFilterMin: WM_NULL, wMsgFilterMax: WM_NULL, wRemoveMsg: PM_REMOVE))
        {
            if (msg.message == WM_QUIT)
            {
                return false;
            }
            else
            {
                TranslateMessage(&msg);
                _ = DispatchMessageW(&msg);
            }
        }

        return true;
    }

    internal override void NotifyJobQueue()
    {
        PostMessage(_thisHwnd, (uint)WM_DISPATCHER_QUEUE, 0, 0);
    }

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

    private LRESULT GlobalWindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        LRESULT result = -1;
        try
        {
            if (message == WM_DISPATCHER_QUEUE)
            {
                ProcessJobQueue();
            }
            else if (message == WM_TIMER)
            {
                // We are calling the timer directly instead of going through the queue
                if (_mapTimerIdToTimer.TryGetValue((nuint)wParam, out var timer))
                {
                    timer.OnTick();
                }
            }
            else if (message == WM_NCDESTROY)
            {
                //
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

    private LRESULT WindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        // Install our synchronization context
        SynchronizationContext.SetSynchronizationContext(DispatcherSynchronizationContext);

        if (_thisHwnd == 0)
        {
            _thisHwnd = hWnd;
        }

        // Log message that are only different
        var newMessage = new WndMsg(hWnd, message, wParam, lParam);
        if (_previousMessage != newMessage || message == WM_TIMER)
        {
            Win32Helper.OutputDebugWinProc(newMessage);
            _previousMessage = newMessage;
        }

        if (hWnd == _thisHwnd)
        {
            return GlobalWindowProc(hWnd, message, wParam, lParam);
        }

        // Handle for Windows
        LRESULT result = -1;
        try
        {
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
                    RegisterWindow(winWindow);
                    result = winWindow.WindowProc(hWnd, message, wParam, lParam);
                    break;

                case WM_DESTROY:
                    {
                        var popup = winWindow.Kind == WindowKind.Popup;
                        result = winWindow.WindowProc(hWnd, message, wParam, lParam);
                        UnRegisterWindow(winWindow);

                        // TODO: Allow to customize this behavior
                        if (_windows.Count == 0)
                        {
                            PostQuitMessage(0);
                        }
                        else
                        {
                            if (popup)
                            {
                                _exitFromCurrentMessageLoop = true;
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

    [UnmanagedCallersOnly]
    private static LRESULT StaticWindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        return Current.WindowProc(hWnd, message, wParam, lParam);
    }

    private void ReleaseUnmanagedResources()
    {
        if (ClassAtom != 0)
        {
            UnregisterClassW((ushort*)ClassAtom, Win32Shared.ModuleHandle);
        }
    }

    internal override void CreateOrResetTimer(DispatcherTimer timer, int millis)
    {
        bool created = false;
        if (!_mapTimerToTimerId.TryGetValue(timer, out var timerId))
        {
            _timerId++;
            timerId = _timerId;
            created = true;
        }

        if (SetTimer(_thisHwnd, timerId, (uint)millis, null) == 0)
        {
            throw new InvalidOperationException("Unable to create/reset timer.");
        }

        if (created)
        {
            _mapTimerIdToTimer.Add(timerId, timer);
            _mapTimerToTimerId.Add(timer, timerId);
        }
    }

    internal override void DestroyTimer(DispatcherTimer timer)
    {
        if (_mapTimerToTimerId.TryGetValue(timer, out var timerId))
        {
            _mapTimerToTimerId.Remove(timer);
            _mapTimerIdToTimer.Remove(timerId);
            KillTimer(_thisHwnd, timerId);
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