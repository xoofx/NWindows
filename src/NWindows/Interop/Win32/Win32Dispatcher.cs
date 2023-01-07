// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using NWindows.Events;
using NWindows.Threading;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.CS;
using static TerraFX.Interop.Windows.GWLP;
using static TerraFX.Interop.Windows.IDC;
using static TerraFX.Interop.Windows.PM;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;

namespace NWindows.Interop.Win32;

/// <summary>
/// Implementation of the Dispatcher for Windows.
/// </summary>
internal unsafe class Win32Dispatcher : Dispatcher
{
    private WndMsg _previousMessage;
    internal HWND Hwnd;
    private nuint _timerId;
    private readonly Dictionary<nuint, DispatcherTimer> _mapTimerIdToTimer;
    private readonly Dictionary<DispatcherTimer, nuint> _mapTimerToTimerId;

    private readonly List<Win32Window> _windows;
    private readonly Dictionary<HWND, Win32Window> _mapHandleToWindow;

    private readonly uint _uxdDisplayChangeMessage;
    private readonly uint _hotplugDetected;
    private int _runMessageLoop;
    private readonly uint WM_DISPATCHER_QUEUE;
    private readonly SystemEvent _systemEvent;
    
    [ThreadStatic] internal static GCHandle CreatedWindowHandle;

    public Win32Dispatcher(Thread thread) : base(thread)
    {
        _windows = new List<Win32Window>();
        _mapHandleToWindow = new Dictionary<HWND, Win32Window>();
        _mapTimerIdToTimer = new Dictionary<nuint, DispatcherTimer>();
        _mapTimerToTimerId = new Dictionary<DispatcherTimer, nuint>(ReferenceEqualityComparer.Instance);
        _systemEvent = new SystemEvent();

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
                hCursor = LoadCursorW(HINSTANCE.NULL, IDC_ARROW),
                hbrBackground = (HBRUSH)(COLOR.COLOR_WINDOW + 1), // Default background color, we should not change this but change it in the Window Create options
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
            _uxdDisplayChangeMessage = RegisterWindowMessageW((ushort*)lpszClassName);
        }

        fixed (char* lpszClassName = "HotplugDetected")
        {
            _hotplugDetected = RegisterWindowMessageW((ushort*)lpszClassName);
        }

        ScreenManager = new Win32ScreenManager();
        InputManager = new Win32InputManager(this);

        fixed (char* lpszWindowMessage = "NWindows.DispatcherProcessQueue")
            WM_DISPATCHER_QUEUE = RegisterWindowMessageW((ushort*)lpszWindowMessage);

        var windowName = $"NWindows-Dispatcher-{guidAsString}";
        fixed (char* lpWindowName = windowName)
            Hwnd = CreateWindowEx(0, (ushort*)ClassAtom, (ushort*)lpWindowName, 0, 0, 0, 0, 0, HWND.HWND_MESSAGE, HMENU.NULL, HINSTANCE.NULL, null);
    }

    ~Win32Dispatcher()
    {
        ReleaseUnmanagedResources();
    }

    public ushort ClassAtom { get; }

    private new static Win32Dispatcher Current => (Win32Dispatcher)Dispatcher.Current;

    internal override Win32ScreenManager ScreenManager { get; }

    internal override Win32InputManager InputManager { get; }

    internal override void WaitAndDispatchMessage(bool blockOnWait)
    {
        MSG msg;

        // We process as many messages as we can in a loop
        while (true)
        {
            // Use Peek/Get to handle idle
            var msgResult = blockOnWait
                ? GetMessageW(&msg, HWND.NULL, wMsgFilterMin: WM_NULL, wMsgFilterMax: WM_NULL)
                : PeekMessageW(&msg, HWND.NULL, wMsgFilterMin: WM_NULL, wMsgFilterMax: WM_NULL, wRemoveMsg: PM_REMOVE);

            if (msgResult)
            {
                // MSDN documentation says that if it returns -1, there is something wrong
                if (msgResult.Value == -1)
                {
                    throw new InvalidOperationException("Unexpected error while processing Win32 message");
                }

                if (msg.message == WM_QUIT)
                {
                    var frame = CurrentFrame;
                    if (frame != null)
                    {
                        frame.Continue = false;
                    }
                }
                else
                {
                    TranslateMessage(&msg);
                    _ = DispatchMessageW(&msg);
                }

                if (blockOnWait)
                {
                    // Check if we have any pending messages in case of blocking
                    // If we don't have any messages, exit this loop to give the Idle event a chance to trigger
                    msgResult = PeekMessageW(&msg, HWND.NULL, wMsgFilterMin: WM_NULL, wMsgFilterMax: WM_NULL, wRemoveMsg: PM_NOREMOVE);
                    if (!msgResult)
                    {
                        break;
                    }
                }
            }
            else
            {
                break;
            }
        }
    }

    internal override void NotifyJobQueue()
    {
        PostMessage(Hwnd, WM_DISPATCHER_QUEUE, 0, 0);
    }

    internal override void PostQuitToMessageLoop()
    {
        PostQuitMessage(0);
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
            case WM_DEVICECHANGE:
            case WM_SETTINGCHANGE:
            case WM_DISPLAYCHANGE:
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

    private LRESULT WindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        // Install our synchronization context
        SynchronizationContext.SetSynchronizationContext(DispatcherSynchronizationContext);

        if (Hwnd == 0)
        {
            Hwnd = hWnd;
        }

        LRESULT result = -1;
        try
        {
            // Output debug messages if requested
            if (DebugInternal && DebugOutput is { } debugOutput)
            {
                var newMessage = new WndMsg(hWnd, message, wParam, lParam);
                if (_previousMessage != newMessage || message == WM_TIMER)
                {
                    debugOutput.WriteLine(newMessage);
                    _previousMessage = newMessage;
                }
            }

            if (hWnd == Hwnd)
            {
                if (message == WM_DISPATCHER_QUEUE)
                {
                    ProcessJobQueue();
                    result = 0;
                }
                else if (message == WM_TIMER)
                {
                    // We are calling the timer directly instead of going through the queue
                    if (_mapTimerIdToTimer.TryGetValue((nuint)wParam, out var timer))
                    {
                        timer.OnTick();
                    }
                    result = 0;
                }
            }
            else
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
                            result = winWindow.WindowProc(hWnd, message, wParam, lParam);
                            UnRegisterWindow(winWindow);

                            // If we don't have anymore windows running, then we shutdown the loop
                            // TODO: We might want to change this default behavior and so make it more pluggable
                            if (_windows.Count == 0)
                            {
                                Shutdown();
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
        }
        catch (Exception ex) when (FilterException(ex))
        {
            if (!HandleException(ex))
            {
                // If we have an unhandled exception pass it back to the frame (outside of the wndproc)
                // to properly flow it back to the render loop
                CurrentFrame?.CaptureException(ExceptionDispatchInfo.Capture(ex));
            }
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

        if (SetTimer(Hwnd, timerId, (uint)millis, null) == 0)
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
            KillTimer(Hwnd, timerId);
        }
    }

    public void OnSystemEvent(SystemEventKind systemEventKind)
    {
        _systemEvent.SubKind = systemEventKind;
        foreach (var window in _windows)
        {
            window.OnWindowEvent(_systemEvent);
        }
    }
}