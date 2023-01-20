// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/WinUser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    /// <include file='Windows.xml' path='doc/member[@name="Windows.RegisterWindowMessageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern uint RegisterWindowMessageW(ushort* lpString);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.TrackMouseEvent"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL TrackMouseEvent(TRACKMOUSEEVENT* lpEventTrack);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetMessageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetMessageW(MSG* lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.TranslateMessage"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL TranslateMessage(MSG* lpMsg);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DispatchMessageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern LRESULT DispatchMessageW(MSG* lpMsg);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.PeekMessageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL PeekMessageW(MSG* lpMsg, HWND hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SendMessageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern LRESULT SendMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.PostMessageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL PostMessageW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DefWindowProcW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern LRESULT DefWindowProcW(HWND hWnd, uint Msg, WPARAM wParam, LPARAM lParam);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.PostQuitMessage"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern void PostQuitMessage(int nExitCode);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.UnregisterClassW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL UnregisterClassW(ushort* lpClassName, HINSTANCE hInstance);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.RegisterClassExW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern ushort RegisterClassExW(WNDCLASSEXW* param0);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CreateWindowExW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HWND CreateWindowExW(uint dwExStyle, ushort* lpClassName, ushort* lpWindowName, uint dwStyle, int X, int Y, int nWidth, int nHeight, HWND hWndParent, HMENU hMenu, HINSTANCE hInstance, void* lpParam);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DestroyWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL DestroyWindow(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.ShowWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL ShowWindow(HWND hWnd, int nCmdShow);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetLayeredWindowAttributes"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL SetLayeredWindowAttributes(HWND hwnd, COLORREF crKey, byte bAlpha, uint dwFlags);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetWindowPos"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.IsZoomed"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL IsZoomed(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.OpenClipboard"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL OpenClipboard(HWND hWndNewOwner);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CloseClipboard"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL CloseClipboard();

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetClipboardData"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HANDLE SetClipboardData(uint uFormat, HANDLE hMem);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetClipboardData"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HANDLE GetClipboardData(uint uFormat);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.RegisterClipboardFormatW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern uint RegisterClipboardFormatW(ushort* lpszFormat);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CountClipboardFormats"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern int CountClipboardFormats();

    /// <include file='Windows.xml' path='doc/member[@name="Windows.EnumClipboardFormats"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern uint EnumClipboardFormats(uint format);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetClipboardFormatNameW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern int GetClipboardFormatNameW(uint format, ushort* lpszFormatName, int cchMaxCount);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.EmptyClipboard"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL EmptyClipboard();

    /// <include file='Windows.xml' path='doc/member[@name="Windows.AddClipboardFormatListener"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL AddClipboardFormatListener(HWND hwnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetFocus"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HWND SetFocus(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetKeyState"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern short GetKeyState(int nVirtKey);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.MapVirtualKeyW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern uint MapVirtualKeyW(uint uCode, uint uMapType);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetCapture"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HWND SetCapture(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.ReleaseCapture"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL ReleaseCapture();

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetTimer"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern nuint SetTimer(HWND hWnd, nuint nIDEvent, uint uElapse, delegate* unmanaged<HWND, uint, nuint, uint, void> lpTimerFunc);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.KillTimer"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL KillTimer(HWND hWnd, nuint uIDEvent);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.EnableWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL EnableWindow(HWND hWnd, BOOL bEnable);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetSystemMetrics"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern int GetSystemMetrics(int nIndex);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.UpdateWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL UpdateWindow(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetActiveWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HWND SetActiveWindow(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetForegroundWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL SetForegroundWindow(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.BeginPaint"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HDC BeginPaint(HWND hWnd, PAINTSTRUCT* lpPaint);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.EndPaint"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL EndPaint(HWND hWnd, PAINTSTRUCT* lpPaint);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetUpdateRect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetUpdateRect(HWND hWnd, RECT* lpRect, BOOL bErase);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.InvalidateRect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL InvalidateRect(HWND hWnd, RECT* lpRect, BOOL bErase);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.ValidateRect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL ValidateRect(HWND hWnd, RECT* lpRect);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetWindowTextW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL SetWindowTextW(HWND hWnd, ushort* lpString);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetClientRect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetClientRect(HWND hWnd, RECT* lpRect);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetWindowRect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetWindowRect(HWND hWnd, RECT* lpRect);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.AdjustWindowRectEx"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL AdjustWindowRectEx(RECT* lpRect, uint dwStyle, BOOL bMenu, uint dwExStyle);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetCursorPos"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL SetCursorPos(int X, int Y);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetCursor"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HCURSOR SetCursor(HCURSOR hCursor);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetCursorPos"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetCursorPos(POINT* lpPoint);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.ClientToScreen"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL ClientToScreen(HWND hWnd, POINT* lpPoint);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.ScreenToClient"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL ScreenToClient(HWND hWnd, POINT* lpPoint);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetSysColor"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    [SuppressGCTransition]
    public static extern uint GetSysColor(int nIndex);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetSysColorBrush"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HBRUSH GetSysColorBrush(int nIndex);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.FillRect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern int FillRect(HDC hDC, RECT* lprc, HBRUSH hbr);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetWindowLongW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern int GetWindowLongW(HWND hWnd, int nIndex);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetWindowLongW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern int SetWindowLongW(HWND hWnd, int nIndex, int dwNewLong);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetParent"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HWND GetParent(HWND hWnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetWindowsHookExW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HHOOK SetWindowsHookExW(int idHook, delegate* unmanaged<int, WPARAM, LPARAM, LRESULT> lpfn, HINSTANCE hmod, uint dwThreadId);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CallNextHookEx"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern LRESULT CallNextHookEx(HHOOK hhk, int nCode, WPARAM wParam, LPARAM lParam);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.LoadCursorW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HCURSOR LoadCursorW(HINSTANCE hInstance, ushort* lpCursorName);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.LoadIconW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HICON LoadIconW(HINSTANCE hInstance, ushort* lpIconName);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.DestroyIcon"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL DestroyIcon(HICON hIcon);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.LoadImageW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HANDLE LoadImageW(HINSTANCE hInst, ushort* name, uint type, int cx, int cy, uint fuLoad);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.CreateIconIndirect"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HICON CreateIconIndirect(ICONINFO* piconinfo);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetIconInfo"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetIconInfo(HICON hIcon, ICONINFO* piconinfo);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.EnumDisplaySettingsExW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL EnumDisplaySettingsExW(ushort* lpszDeviceName, uint iModeNum, DEVMODEW* lpDevMode, uint dwFlags);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.MonitorFromWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern HMONITOR MonitorFromWindow(HWND hwnd, uint dwFlags);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetMonitorInfoW"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetMonitorInfoW(HMONITOR hMonitor, MONITORINFO* lpmi);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.EnumDisplayMonitors"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL EnumDisplayMonitors(HDC hdc, RECT* lprcClip, delegate* unmanaged<HMONITOR, HDC, RECT*, LPARAM, BOOL> lpfnEnum, LPARAM dwData);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.SetThreadDpiAwarenessContext"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    [SupportedOSPlatform("windows10.0.14393.0")]
    public static extern DPI_AWARENESS_CONTEXT SetThreadDpiAwarenessContext(DPI_AWARENESS_CONTEXT dpiContext);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetThreadDpiAwarenessContext"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    [SupportedOSPlatform("windows10.0.14393.0")]
    public static extern DPI_AWARENESS_CONTEXT GetThreadDpiAwarenessContext();

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetDpiForWindow"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    [SupportedOSPlatform("windows10.0.14393.0")]
    public static extern uint GetDpiForWindow(HWND hwnd);

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetDpiForSystem"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    [SupportedOSPlatform("windows10.0.14393.0")]
    public static extern uint GetDpiForSystem();

    /// <include file='Windows.xml' path='doc/member[@name="Windows.GetWindowInfo"]/*' />
    [DllImport("user32", ExactSpelling = true)]
    public static extern BOOL GetWindowInfo(HWND hwnd, WINDOWINFO* pwi);
    public const int HC_ACTION = 0;
    public const int WHEEL_DELTA = 120;
    public const int XBUTTON1 = 0x0001;
    public const int HTNOWHERE = 0;
    public const int HTCLIENT = 1;
    public const int HTCAPTION = 2;
    public const int HTMENU = 5;
    public const int HTMINBUTTON = 8;
    public const int HTMAXBUTTON = 9;
    public const int HTLEFT = 10;
    public const int HTRIGHT = 11;
    public const int HTTOP = 12;
    public const int HTTOPLEFT = 13;
    public const int HTTOPRIGHT = 14;
    public const int HTBOTTOM = 15;
    public const int HTBOTTOMLEFT = 16;
    public const int HTBOTTOMRIGHT = 17;
    public const int HTCLOSE = 20;
    public const int HTHELP = 21;
    public const int ICON_SMALL = 0;
    public const int ICON_BIG = 1;
    public const int SIZE_RESTORED = 0;
    public const int SIZE_MINIMIZED = 1;
    public const int SIZE_MAXIMIZED = 2;
    public const uint HOVER_DEFAULT = 0xFFFFFFFF;
    public static delegate*<HWND, uint, WPARAM, LPARAM, LRESULT> SendMessage => &SendMessageW;
    public static delegate*<HWND, uint, WPARAM, LPARAM, BOOL> PostMessage => &PostMessageW;
    public const int CW_USEDEFAULT = unchecked((int)(0x80000000));
    public static delegate*<uint, ushort*, ushort*, uint, int, int, int, int, HWND, HMENU, HINSTANCE, void*, HWND> CreateWindowEx => &CreateWindowExW;
    public static delegate*<ushort*, uint> RegisterClipboardFormat => &RegisterClipboardFormatW;
    public const int MAPVK_VK_TO_VSC = (0);
    public const int MAPVK_VSC_TO_VK_EX = (3);
    public static delegate*<HWND, int, int> GetWindowLong => &GetWindowLongW;
    public const int MONITORINFOF_PRIMARY = 0x00000001;
}
