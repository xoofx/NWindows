// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using NWindows.Events;
using NWindows.Input;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.WS;
using static TerraFX.Interop.Windows.HWND;
using static TerraFX.Interop.Windows.MK;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.TME;
using System.Diagnostics;

namespace NWindows.Interop.Win32;

// TODO to handle:
// - WM_POINTERUPDATE https://learn.microsoft.com/en-us/windows/win32/inputmsg/wm-pointerupdate
// - WM_INPUT https://learn.microsoft.com/en-us/windows/win32/inputdev/wm-input

internal unsafe class Win32Window : Window
{
    private bool _hasDecorations;
    private float _mouseLastX;
    private float _mouseLastY;
    private bool _mouseTracked;
    private SizeF _size;
    private bool _visible;
    private Point _position;
    private WindowState _state;
    private bool _isCompositionEnabled;
    private bool _isThemeActive;
    private bool _resizeable;
    private int _mouseCapture; // Number of mouse captures in flight (i.e number of buttons down)
    private float _opacity;
    private bool _topMost;
    private SizeF _minimumSize;
    private SizeF _maximumSize;
    private bool _modal;
    private readonly Win32Window? _parentWindow;
    private bool _enable;
    private bool _closed;
    private bool _disposed;
    private GCHandle _thisGcHandle;
    private string _title;
    private char _currentCharHighSurrogate;
    private bool _maximizeable;
    private bool _minimizeable;
    private bool _allowDragAndDrop;
    private bool _windowIsActive;

    private Color _backgroundColor;
    private HBRUSH _backgroundColorBrush;
    private bool _isDefaultColorBrush;
    private bool _showInTaskBar;
    private bool _initialShowInTaskBar;
    private Dpi _dpi;
    private DpiMode _dpiMode;
    private readonly Win32DropTarget _dropTarget;

    private HICON _windowIcon;

    private bool _isFullscreen;
    private Rectangle _windowRectBeforeFullScreen;
    private bool _hasDecorationsBeforeFullScreen;
    private bool _hasMaximizeableBeforeFullScreen;
    private bool _hasMinimizeableBeforeFullScreen;
    private bool _hasResizeableBeforeFullScreen;
    private Dpi _dpiBeforeFullScreen;

    public Win32Window(WindowCreateOptions options) : base(options)
    {
        _hasDecorations = options.Decorations;
        _title = options.Title;
        _resizeable = options.Resizable;
        _maximizeable = options.Maximizable;
        _minimizeable = options.Minimizable;
        _mouseLastX = -1;
        _mouseLastY = -1;
        _opacity = 1.0f;
        _enable = true;
        _initialShowInTaskBar = options.ShowInTaskBar;
        _state = options.WindowState;
        if (options.BackgroundColor.HasValue)
        {
            UpdateBackgroundColor(options.BackgroundColor.Value, false);
        }
        else
        {
            _backgroundColor = GetDefaultWindowBackgroundColor();
            _backgroundColorBrush = GetSysColorBrush(COLOR.COLOR_WINDOW);
            _isDefaultColorBrush = true;
        }
        _parentWindow = (Win32Window?)options.Parent;
        Kind = options.Kind;

        // Create Handle for this window
        CreateWindowHandle(options);

        // Setup DragDrop
        _dropTarget = new Win32DropTarget(this);
        UpdateDragDrop(options.DragDrop, false);
    }

    public new Win32Dispatcher Dispatcher => (Win32Dispatcher)base.Dispatcher;

    public HWND HWnd => (HWND)Handle;

    public override bool Enable
    {
        get
        {
            VerifyAccess();
            return _enable;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotFullScreen();

            if (value != _enable)
            {
                UpdateEnable(value);
            }
        }
    }

    public override bool HasDecorations
    {
        get
        {
            VerifyAccess();
            return _hasDecorations;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotChild();
            VerifyNotFullScreen();

            if (value != _hasDecorations)
            {
                UpdateDecorations(value);
            }
        }
    }

    public override Dpi Dpi
    {
        get
        {
            VerifyAccess();
            return _dpi;
        }
        set
        {
            VerifyAccessAndNotDestroyed();

            if (_dpiMode == DpiMode.Auto) throw new ArgumentException("Cannot change the dpi when the DpiMode is set to Auto", nameof(value));

            if (value != _dpi)
            {
                UpdateDpi(value);
            }
        }
    }

    public override DpiMode DpiMode
    {
        get
        {
            VerifyAccess();
            return _dpiMode;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            if (value != _dpiMode)
            {
                UpdateDpiMode(value);
            }
        }
    }

    public override Color BackgroundColor
    {
        get
        {
            VerifyAccess();
            return _backgroundColor;
        }
        set
        {
            VerifyAccess();

            if (_backgroundColor != value)
            {
                UpdateBackgroundColor(value);
            }
        }
    }

    public override bool IsDisposed => _disposed;

    public override string Title
    {
        get
        {
            VerifyAccess();
            return _title;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotChild();

            if (!string.Equals(_title, value, StringComparison.Ordinal))
            {
                UpdateTitle(value);
            }
        }
    }

    public override SizeF Size
    {
        get
        {
            VerifyAccess();
            return _size;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotChild();
            VerifyNotFullScreen();

            if (value != _size)
            {
                UpdateSize(value);
            }
        }
    }

    public override SizeF ClientSize
    {
        get
        {
            VerifyAccessAndNotDestroyed();

            RECT rect;
            if (GetClientRect(HWnd, &rect))
            {
                return _dpi.PixelToLogical(rect.ToRectangle().Size);
            }

            return default;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotFullScreenAndNotMaximized();

            var newSize = _dpi.LogicalToPixel(value);
            RECT rect;

            if (!_hasDecorations)
            {
                if (GetWindowRect(HWnd, &rect))
                {
                    // This will notify a change of Size, not client size
                    SetWindowPos(HWnd, HWND.NULL, 0, 0, newSize.Width, newSize.Height, SWP.SWP_NOMOVE | SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER);
                }
            }
            else
            {
                rect.left = 0;
                rect.top = 0;
                rect.right = newSize.Width;
                rect.bottom = newSize.Height;

                var style = GetWindowStyle(HWnd);
                var exStyle = GetWindowExStyle(HWnd);

                // We take into account DPI just above, so we are not using AdjustWindowsRectExForDPI.
                if (AdjustWindowRectEx(&rect, style, false, exStyle))
                {
                    // This will notify a change of Size, not client size
                    SetWindowPos(HWnd, HWND.NULL, 0, 0, rect.right - rect.left, rect.bottom - rect.top, SWP.SWP_NOMOVE | SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER);
                }
            }
        }
    }

    public override Point Position
    {
        get
        {
            VerifyAccess();
            return _position;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotChild();

            if (value != _position)
            {
                UpdatePosition(value);
            }
        }
    }

    public override bool Visible
    {
        get
        {
            VerifyAccess();
            return _visible;
        }

        set
        {
            VerifyAccessAndNotDestroyed();
            if (_visible != value)
            {
                UpdateVisible(value);
            }
        }
    }

    public override bool DragDrop
    {
        get
        {
            VerifyAccess();
            return _allowDragAndDrop;
        }

        set
        {
            VerifyAccessAndNotDestroyed();
            if (_allowDragAndDrop != value)
            {
                UpdateDragDrop(value, true);
            }
        }
    }

    public override bool Resizeable
    {
        get
        {
            VerifyAccess();
            return _resizeable;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotFullScreen();

            if (_resizeable != value)
            {
                UpdateResizeable(value);
            }
        }
    }

    public override bool Maximizeable
    {
        get
        {
            VerifyAccess();
            return _maximizeable;
        }
        set
        {
            VerifyAccess();
            VerifyNotChild();
            VerifyResizeable();
            VerifyNotFullScreen();

            if (_maximizeable != value)
            {
                UpdateMaximizeable(value);
            }
        }
    }


    public override bool Minimizeable
    {
        get
        {
            VerifyAccess();
            return _minimizeable;
        }
        set
        {
            VerifyAccess();
            VerifyNotChild();
            VerifyNotFullScreen();

            if (_minimizeable != value)
            {
                UpdateMinimizeable(value);
            }
        }
    }

    public override INativeWindow? Parent => _parentWindow;

    public override WindowState State
    {
        get
        {
            VerifyAccess();
            return _state;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotChild();
            
            if (_state != value)
            {
                UpdateWindowState(value);
            }
        }
    }

    public override float Opacity
    {
        get
        {
            VerifyAccess();
            return _opacity;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            if (_opacity != value)
            {
                UpdateOpacity(value);
            }
        }
    }

    public override bool TopMost
    {
        get
        {
            VerifyAccess();
            return _topMost;
        }
        set
        {
            VerifyAccessAndNotDestroyed();

            if (_topMost != value)
            {
                UpdateTopMost(value);
            }
        }
    }

    public override SizeF MinimumSize
    {
        get
        {
            VerifyAccess();
            return _minimumSize;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotFullScreen();

            if (!_minimumSize.Equals(value))
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MinimumSize));
                }

                UpdateMinimumSize(value);
            }
        }
    }

    public override SizeF MaximumSize
    {
        get
        {
            VerifyAccess();
            return _maximumSize;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyNotFullScreenAndNotMaximized();

            if (!_maximumSize.Equals(value))
            {
                if (value.Width < 0 || value.Height < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(MaximumSize));
                }

                UpdateMaximumSize(value);
            }
        }
    }

    public override bool Modal
    {
        get
        {
            VerifyAccess();
            return _modal;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyPopup();

            if (_modal != value)
            {
                UpdateModal(value);
            }
        }
    }

    public override bool ShowInTaskBar
    {
        get
        {
            VerifyAccess();
            return _showInTaskBar;
        }
        set
        {
            VerifyAccessAndNotDestroyed();
            VerifyTopLevel();

            if (_showInTaskBar != value)
            {
                UpdateShowInTaskBar(value);
            }
        }
    }

    public override void Focus()
    {
        VerifyAccessAndNotDestroyed();
        SetFocus(HWnd);
    }

    public override void Activate()
    {
        VerifyAccessAndNotDestroyed();
        if (_visible)
        {
            SetForegroundWindow(HWnd);
        }
    }

    public override bool Close()
    {
        VerifyAccessAndNotDestroyed();
        SendMessage(HWnd, WM_CLOSE, (WPARAM)0, (LPARAM)0);
        return _closed;
    }

    public override Point ClientToScreen(PointF position)
    {
        VerifyAccessAndNotDestroyed();

        var screen = GetScreen();
        if (screen != null)
        {
            var screenDpi = screen.Dpi;
            var pixelPosition = screenDpi.LogicalToPixel(position);
            Windows.ClientToScreen(HWnd, (POINT*)&pixelPosition);
            return pixelPosition;
        }
        else
        {
            return default;
        }
    }

    public override PointF ScreenToClient(Point position)
    {
        VerifyAccessAndNotDestroyed();

        var screen = GetScreen();
        if (screen != null)
        {
            var screenDpi = screen.Dpi;
            Windows.ScreenToClient(HWnd, (POINT*)&position);
            return screenDpi.PixelToLogical(position);
        }
        else
        {
            return default;
        }
    }

    public override Screen? GetScreen()
    {
        VerifyAccessAndNotDestroyed();
        return GetScreenFromHWnd(HWnd);
    }

    private Screen? GetScreenFromHWnd(HWND hWnd)
    {
        if (hWnd == IntPtr.Zero)
        {
            System.Diagnostics.Debugger.Break();
        }
        var handle = MonitorFromWindow(hWnd, MONITOR.MONITOR_DEFAULTTONULL);
        Dispatcher.ScreenManager.TryGetScreen(handle, out var screen);
        return screen;
    }

    public override void CenterToParent()
    {
        VerifyAccessAndNotDestroyed();
        VerifyNotChild();
        VerifyNotFullScreenAndNotMaximized();

        var parent = Parent;
        if (parent != null && Win32Helper.TryGetWindowPositionAndSize((HWND)parent.Handle, out var bounds))
        {
            CenterPositionFromBounds(bounds);
        }
        else
        {
            CenterToScreen();
        }
    }

    public override void CenterToScreen()
    {
        VerifyAccessAndNotDestroyed();
        VerifyNotChild();
        VerifyNotFullScreenAndNotMaximized();

        var screen = GetScreenOrPrimary();
        if (screen != null)
        {
            CenterPositionFromBounds(screen.Bounds);
        }
    }

    public override void SetIcon(Icon icon)
    {
        VerifyAccessAndNotDestroyed();

        var nativeIcon = Win32Icon.CreateIcon(icon);

        if (nativeIcon != HICON.NULL)
        {
            SendMessage(HWnd, WM_SETICON, ICON_BIG, (LPARAM)nativeIcon);
            SendMessage(HWnd, WM_SETICON, ICON_SMALL, (LPARAM)nativeIcon);

            if (_windowIcon != HICON.NULL)
            {
                DestroyIcon(_windowIcon);
            }
            _windowIcon = nativeIcon;
        }
    }

    /// <summary>
    ///  Gets the system's default minimum tracking dimensions of a window in pixels.
    /// </summary>
    private static Size MinWindowTrackSize
    {
        get
        {
            var size = new Size(GetSystemMetrics(SM.SM_CXMINTRACK), GetSystemMetrics(SM.SM_CYMINTRACK));
            return size;
        }
    }

    private static Size MaxWindowTrackSize
    {
        get
        {
            var size = new Size(GetSystemMetrics(SM.SM_CXMAXTRACK), GetSystemMetrics(SM.SM_CYMAXTRACK));
            return size;
        }
    }

    internal LRESULT WindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        // Process potential screen changes
        if (Dispatcher.TryHandleScreenChanges(hWnd, message, wParam, lParam))
        {
            Dispatcher.OnSystemEvent(SystemEventKind.ScreenChanged);
        }

        if (message >= WM_MOUSEFIRST && message <= WM_MOUSELAST || message == WM_MOUSELEAVE)
        {
            return HandleMouse(hWnd, message, wParam, lParam);
        }

        LRESULT result;
        if (!_hasDecorations)
        {
            result = HandleBorderLessWindowProc(hWnd, message, wParam, lParam);
            if (result >= 0)
            {
                return result;
            }
        }

        result = -1;

        switch (message)
        {
            case WM_NCDESTROY:
                HandleDispose();
                result = 0;
                break;

            case WM_CREATE:
                // https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-create
                HandleCreate();
                result = 0;
                break;

            case WM_SHOWWINDOW:
                // https://learn.microsoft.com/en-us/windows/win32/winmsg/wm-showwindow
                _visible = (BOOL)wParam;
                OnFrameEvent(_visible ? FrameEventKind.Shown : FrameEventKind.Hidden);
                result = 0;
                break;

            case WM_CLOSE:
                HandleClose();
                result = 0;
                break;

            case WM_DESTROY:
                HandleDestroy();
                result = 0;
                break;

            case WM_SETFOCUS:
                OnFrameEvent(FrameEventKind.FocusGained);
                result = 0;
                break;

            case WM_KILLFOCUS:
                OnFrameEvent(FrameEventKind.FocusLost);
                result = 0;
                break;

            case WM_GETMINMAXINFO:
                HandleGetMinMaxInfo(hWnd, (MINMAXINFO*)lParam);
                result = 0;
                break;

            case WM_WINDOWPOSCHANGED:
                HandlePositionChanged((WINDOWPOS*)lParam);
                // We don't handle it because we need to recover the maximized information in WM_SIZE
                break;

            case WM_CANCELMODE:
                HandleCancelMode();
                result = 0;
                break;

            case WM_ENABLE:
                _enable = (BOOL)wParam;
                OnFrameEvent(_enable ? FrameEventKind.Enabled : FrameEventKind.Disabled);
                result = 0;
                break;

            case WM_ERASEBKGND:
                // Do nothing on erase background
                // TODO: Check if we need to handle this
                result = 0;
                break;

            case WM_MOVE:
                result = 0;
                break;

            case WM_SIZE:
                HandleSizeChanged((int)wParam);
                result = 0;
                break;

            case WM_PAINT:
                HandlePaint();
                result = 0;
                break;

            case WM_DWMCOMPOSITIONCHANGED:
                UpdateCompositionEnabled(_hasDecorations, _isFullscreen);
                result = 0;
                break;

            case WM_THEMECHANGED:
                UpdateThemeActive();
                result = 0;
                break;

            case WM_KEYUP:
            case WM_KEYDOWN:
            case WM_SYSKEYUP:
            case WM_SYSKEYDOWN:
                result = HandleKey(message, wParam, lParam);
                break;

            case WM_CHAR:
                HandleChar(wParam, lParam);
                result = 0;
                break;

            case WM_DPICHANGED:
                if (_dpiMode == DpiMode.Auto)
                {
                    var dpiX = HIWORD(wParam);
                    UpdateDpi(new Dpi(dpiX, dpiX));
                    SetWindowBounds((*(RECT*)lParam).ToRectangle());
                }
                result = 0;
                break;

            case WM_CLIPBOARDUPDATE:
                OnFrameEvent(FrameEventKind.ClipboardChanged);
                result = 0;
                break;

            case WM_ACTIVATEAPP:
                _windowIsActive = ((BOOL)wParam);
                // Mark the dispatcher to not process Windows key if we have an active window in fullscreen
                //Dispatcher.HasActiveExclusiveFullScreenWindow = _windowIsActive && _state.Kind == WindowState.ExclusiveFullScreen;
                break;
        }

        return result;
    }

    private void SetWindowBounds(Rectangle bounds)
    {
        SetWindowPos(HWnd,
            HWND.NULL,
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);

        _position = bounds.Location;
        _size = _dpi.PixelToLogical(bounds.Size);
    }

    private void HandleChar(WPARAM wParam, LPARAM lParam)
    {
        var c = (char)wParam;
        if (char.IsHighSurrogate(c))
        {
            _currentCharHighSurrogate = c;
            return;
        }

        Rune rune;
        if (char.IsSurrogatePair(_currentCharHighSurrogate, c))
        {
            rune = new Rune(char.ConvertToUtf32(_currentCharHighSurrogate, c));
            _currentCharHighSurrogate = (char)0;
        }
        else
        {
            rune = new Rune(c);
        }

        _textEvent.Rune = rune;
        OnWindowEvent(_textEvent);
    }

    private LRESULT HandleKey(uint message, WPARAM wParam, LPARAM lParam)
    {
        var virtualKey = GetVirtualKey(wParam, lParam);

        // If false, up
        var isDown = message == WM_KEYDOWN || message == WM_SYSKEYDOWN;

        var keyEvent = _keyboardEvent;
        keyEvent.Key = Win32KeyInterop.VirtualKeyToKey(virtualKey);
        keyEvent.State = isDown ? KeyStates.Down : KeyStates.None;
        keyEvent.Modifiers = Win32KeyInterop.GetSystemModifierKeys();
        keyEvent.ScanCode = (ushort)GetScanCode(wParam, lParam);
        keyEvent.IsExtended = IsExtendedKey(lParam);
        keyEvent.IsSystem = message == WM_SYSKEYDOWN || message == WM_SYSKEYUP;
        keyEvent.Repeat = (ushort)lParam & 0x7FFF;
        keyEvent.Handled = false;

        // Only keep track of toggle state for the following keys
        if (virtualKey == VK.VK_CAPITAL || virtualKey == VK.VK_SCROLL || virtualKey == VK.VK_NUMLOCK)
        {
            if ((GetKeyState(virtualKey) & 1) != 0)
            {
                keyEvent.State |= KeyStates.Toggled;
            }
        }

        OnWindowEvent(keyEvent);

        return keyEvent.Handled ? 0 : -1;
    }

    internal static int GetVirtualKey(WPARAM wParam, LPARAM lParam)
    {
        int virtualKey = (int)wParam;
        int keyData = (int)lParam;

        // Find the left/right instance SHIFT keys.
        if (virtualKey == VK.VK_SHIFT)
        {
            var scanCode = (uint)((keyData & 0xFF0000) >> 16);
            virtualKey = unchecked((int)MapVirtualKeyW(scanCode, MAPVK_VSC_TO_VK_EX));
            if (virtualKey == 0)
            {
                virtualKey = VK.VK_LSHIFT;
            }
        }

        // Find the left/right instance ALT keys.
        if (virtualKey == VK.VK_MENU)
        {
            bool right = (keyData & 0x1000000) >> 24 != 0;

            if (right)
            {
                virtualKey = VK.VK_RMENU;
            }
            else
            {
                virtualKey = VK.VK_LMENU;
            }
        }

        // Find the left/right instance CONTROL keys.
        if (virtualKey == VK.VK_CONTROL)
        {
            bool right = (keyData & 0x1000000) >> 24 != 0;

            if (right)
            {
                virtualKey = VK.VK_RCONTROL;
            }
            else
            {
                virtualKey = VK.VK_LCONTROL;
            }
        }

        return virtualKey;
    }

    internal static int GetScanCode(WPARAM wParam, LPARAM lParam)
    {
        int keyData = (int)wParam;

        int scanCode = (keyData & 0xFF0000) >> 16;
        if (scanCode == 0)
        {
            int virtualKey = GetVirtualKey(wParam, lParam);
            scanCode = (int)MapVirtualKeyW((uint)virtualKey, MAPVK_VK_TO_VSC);
        }

        return scanCode;
    }

    internal static bool IsExtendedKey(LPARAM lParam)
    {
        int keyData = (int)lParam;
        return (keyData & 0x01000000) != 0 ? true : false;
    }

    private void HandleDispose()
    {
        _disposed = true;
        _thisGcHandle.Free();
        _thisGcHandle = default;
        _dropTarget.Dispose();

        if (_windowIcon != HICON.NULL)
        {
            DestroyIcon(_windowIcon);
        }

        if (!_isDefaultColorBrush)
        {
            DeleteObject(_backgroundColorBrush);
        }
        Handle = default;
    }

    private void HandleClose()
    {
        _closeEvent.Cancel = false;
        OnWindowEvent(_closeEvent);
        if (!_closeEvent.Cancel)
        {
            _closed = true;
            DestroyWindow(HWnd);
        }
    }

    private void HandleCancelMode()
    {
        // Cancel any pending capture
        if (_mouseCapture > 0)
        {
            ReleaseCapture();
            _mouseCapture = 0;
        }
    }

    private void HandleDestroy()
    {
        // Make sure that we re-enable a parent window if a window was modal.
        if (_modal)
        {
            _parentWindow!.Enable = true;
            SetActiveWindow(_parentWindow!.HWnd);
        }

        _dropTarget.UnRegister();

        OnFrameEvent(FrameEventKind.Destroyed);
    }

    private LRESULT HandleBorderLessWindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        // Tried to use some of the tips from:
        // https://github.com/rossy/borderless-window/blob/master/borderless-window.c

        const int WM_NCUAHDRAWCAPTION = 0x00AE;
        const int WM_NCUAHDRAWFRAME = 0x00AF;

        switch (message)
        {
            case WM_NCCALCSIZE:
                // https://learn.microsoft.com/en-us/windows/win32/dwm/customframe
                HandleBorderLessNonClientCalculateClientSize(hWnd, wParam, (NCCALCSIZE_PARAMS*)lParam);
                return 0;

            case WM_NCHITTEST:
                return HitTestNonClientAreaBorderLess(hWnd, wParam, lParam);

            case WM_NCACTIVATE:
                // DefWindowProc won't repaint the window border if lParam (normally a HRGN) is -1.
                // This is recommended in: https://blogs.msdn.microsoft.com/wpfsdk/2008/09/08/custom-window-chrome-in-wpf/
                return DefWindowProcW(hWnd, message, wParam, -1);

            case WM_NCPAINT:
                // Only block WM_NCPAINT when composition is disabled. If it's blocked when composition is enabled, the window shadow won't be drawn.
                if (!_isCompositionEnabled)
                {
                    return 0;
                }
                break;

            case WM_NCUAHDRAWCAPTION:
            case WM_NCUAHDRAWFRAME:
                /* These undocumented messages are sent to draw themed window borders.
                   Block them to prevent drawing borders over the client area. */
                return 0;

            case WM_SETICON:
            case WM_SETTEXT:
                /* Disable painting while these messages are handled to prevent them
                   from drawing a window caption over the client area, but only when
                   composition and theming are disabled. These messages don't paint
                   when composition is enabled and blocking WM_NCUAHDRAWCAPTION should
                   be enough to prevent painting when theming is enabled. */
                if (!_isCompositionEnabled && !_isThemeActive)
                {
                    var old_style = GetWindowLongPtrW(HWnd, GWL.GWL_STYLE);

                    /* Prevent Windows from drawing the default title bar by temporarily
                       toggling the WS_VISIBLE style. This is recommended in:
                       https://blogs.msdn.microsoft.com/wpfsdk/2008/09/08/custom-window-chrome-in-wpf/ */
                    SetWindowLongPtrW(HWnd, GWL.GWL_STYLE, old_style & ~WS_VISIBLE);
                    LRESULT result = DefWindowProcW(HWnd, message, wParam, lParam);
                    SetWindowLongPtrW(HWnd, GWL.GWL_STYLE, old_style);
                    return result;
                }
                break;
        }

        return -1;
    }
    private void HandleGetMinMaxInfo(HWND hWnd, MINMAXINFO* info)
    {
        // This is called at the creation of the window
        // By default DPI mode must be Auto
        if (_dpi.IsEmpty)
        {
            Debug.Assert(_dpiMode == DpiMode.Auto);
            var dpiX = Win32Helper.GetDpiForWindowSafe(hWnd);
            _dpi = new Dpi(dpiX, dpiX);
        }
        
        if (_resizeable)
        {
            var minimumWindowTrackSize = MinWindowTrackSize;
            var minimumSize = _dpi.LogicalToPixel(_minimumSize);
            minimumSize = new Size(Math.Max(minimumWindowTrackSize.Width, minimumSize.Width), Math.Max(minimumWindowTrackSize.Height, minimumSize.Height));

            // We only apply minimum size to top level window
            if (Kind == WindowKind.TopLevel)
            {
                * (Size*)&info->ptMinTrackSize = minimumSize;
                // When the MinTrackSize is set to a value larger than the screen
                // size but the MaxTrackSize is not set to a value equal to or greater than the
                // MinTrackSize and the user attempts to "grab" a resizing handle, Windows makes
                // the window move a distance equal to either the width when attempting to resize
                // horizontally or the height of the window when attempting to resize vertically.
                // So, the workaround to prevent this problem is to set the MaxTrackSize to something
                // whenever the MinTrackSize is set to a value larger than the respective dimension
                // of the virtual screen.
                var screen = GetScreenFromHWnd(hWnd);
                if (_maximumSize.IsEmpty && screen != null)
                {
                    // Only set the max track size dimensions if the min track size dimensions
                    // are larger than the VirtualScreen dimensions.
                    Size virtualScreen = _dpi.LogicalToPixel(screen.Size);
                    if (minimumSize.Height > virtualScreen.Height)
                    {
                        info->ptMaxTrackSize.y = int.MaxValue;
                    }

                    if (minimumSize.Width > virtualScreen.Width)
                    {
                        info->ptMaxTrackSize.x = int.MaxValue;
                    }
                }
            }

            if (!_maximumSize.IsEmpty)
            {
                var maximumSize = _dpi.LogicalToPixel(_maximumSize);
                maximumSize = new Size(Math.Max(minimumSize.Width, maximumSize.Width), Math.Max(minimumSize.Height, maximumSize.Height));
                *(Size*)&info->ptMaxTrackSize = maximumSize;
            }
        }
        else
        {
            var sizeInPixel = _dpi.LogicalToPixel(_size);
            if (_isFullscreen)
            {
                // We let the min and max size flexible as we need to adjust it
                // accordingly to the screen resolution
                var minWindowTrackSize = MinWindowTrackSize;
                var maxWindowTrackSize = MaxWindowTrackSize;
                info->ptMinTrackSize.x = minWindowTrackSize.Width;
                info->ptMinTrackSize.y = minWindowTrackSize.Height;
                info->ptMaxTrackSize.x = maxWindowTrackSize.Width;
                info->ptMaxTrackSize.y = maxWindowTrackSize.Height;
            }
            else
            {
                // Otherwise we force the window to keep its current size
                info->ptMinTrackSize.x = sizeInPixel.Width;
                info->ptMinTrackSize.y = sizeInPixel.Height;
                info->ptMaxTrackSize.x = sizeInPixel.Width;
                info->ptMaxTrackSize.y = sizeInPixel.Height;
            }
        }

        // How does the window manager adjust ptMaxSize and ptMaxPosition for multiple monitors?
        // See https://devblogs.microsoft.com/oldnewthing/20150501-00/?p=44964
        var screenForMaxPosition = Screen.Primary;
        if (screenForMaxPosition == null) return;

        // From: https://learn.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-minmaxinfo
        // For systems with multiple monitors, the ptMaxSize and ptMaxPosition members describe the maximized size
        // and position of the window on the primary monitor, even if the window ultimately maximizes onto
        // a secondary monitor.
        // In that case, the window manager adjusts these values to compensate for differences between the primary
        // monitor and the monitor that displays the window.
        // Thus, if the user leaves ptMaxSize untouched, a window on a monitor larger than the primary monitor
        // maximizes to the size of the larger monitor.

        info->ptMaxSize.x = screenForMaxPosition.SizeInPixels.Width;
        info->ptMaxSize.y = screenForMaxPosition.SizeInPixels.Height;
        info->ptMaxPosition.x = screenForMaxPosition.Position.X;
        info->ptMaxPosition.y = screenForMaxPosition.Position.Y;
    }

    private void UpdateThemeActive()
    {
        _isThemeActive = IsThemeActive();
    }

    private void UpdateCompositionEnabled(bool hasDecorations, bool fullscreen)
    {
        BOOL enabled = FALSE;
        DwmIsCompositionEnabled(&enabled);
        _isCompositionEnabled = enabled;

        if (_isCompositionEnabled)
        {
            // The window needs a frame to show a shadow, so give it the smallest amount of frame possible
            MARGINS margins = default;

            if (!fullscreen)
            {
                margins.cyTopHeight = 1;
            }

            DwmExtendFrameIntoClientArea(HWnd, &margins);
            DWMNCRENDERINGPOLICY attributes = DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED;
            DwmSetWindowAttribute(HWnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, &attributes, sizeof(DWMNCRENDERINGPOLICY));
        }
    }

    private void HandleCreate()
    {
        OnFrameEvent(FrameEventKind.Created);
    }

    private unsafe void HandleBorderLessNonClientCalculateClientSize(HWND hWnd, WPARAM wParam, NCCALCSIZE_PARAMS* pnCsp)
    {
        ref var rect = ref pnCsp->rgrc[0];

        var nonClient = rect;
        // DefWindowProc must be called in both the maximized and non - maximized cases, otherwise tile/ cascade windows won't work 
        DefWindowProcW(HWnd, WM_NCCALCSIZE, wParam, (LPARAM)pnCsp);
        var client = rect;

        if (IsMaximized(HWnd))
        {
            WINDOWINFO wi = default;
            wi.cbSize = (uint)sizeof(WINDOWINFO);
            GetWindowInfo(HWnd, &wi);

            /* Maximized windows always have a non-client border that hangs over
               the edge of the screen, so the size proposed by WM_NCCALCSIZE is
               fine. Just adjust the top border to remove the window title. */
            rect.left = client.left;
            rect.top = nonClient.top; // + (int)wi.cyWindowBorders;
            rect.right = client.right;
            rect.bottom = client.bottom;

            var screen = GetScreenFromHWnd(hWnd);
            if (screen != null)
            {
                MONITORINFO mi = default;
                mi.cbSize = (uint)sizeof(MONITORINFO);
                GetMonitorInfoW((HMONITOR)screen.Handle, &mi);

                /* If the client rectangle is the same as the monitor's rectangle,
                   the shell assumes that the window has gone fullscreen, so it removes
                   the topmost attribute from any auto-hide appbars, making them
                   inaccessible. To avoid this, reduce the size of the client area by
                   one pixel on a certain edge. The edge is chosen based on which side
                   of the monitor is likely to contain an auto-hide appbar, so the
                   missing client area is covered by it. */
                if (rect.Equals(mi.rcMonitor))
                {
                    if (HasAutoHideAppBar(ABE_BOTTOM, mi.rcMonitor))
                        rect.bottom--;
                    else if (HasAutoHideAppBar(ABE_LEFT, mi.rcMonitor))
                        rect.left++;
                    else if (HasAutoHideAppBar(ABE_TOP, mi.rcMonitor))
                        rect.top++;
                    else if (HasAutoHideAppBar(ABE_RIGHT, mi.rcMonitor))
                        rect.right--;
                }
            }
        }
        else
        {
            rect = nonClient;
        }
    }

    private static bool HasAutoHideAppBar(uint edge, RECT mon)
    {
        APPBARDATA appBarData = default;
        appBarData.cbSize = (uint)sizeof(APPBARDATA);
        appBarData.uEdge = edge;
        appBarData.rc = mon;
        return SHAppBarMessage(ABM.ABM_GETAUTOHIDEBAREX, &appBarData) != 0;
    }

    // Hit test the frame for resizing and moving.
    private LRESULT HitTestNonClientAreaBorderLess(HWND hWnd, WPARAM wParam, LPARAM lParam)
    {
        // Mouse with screen coordinates
        POINT ptMouse = new POINT() { x = GET_X_LPARAM(lParam), y = GET_Y_LPARAM(lParam) };

        // Get the window rectangle.
        RECT rcWindow;
        GetWindowRect(hWnd, &rcWindow);

        int frameSize = GetSystemMetrics(SM.SM_CXFRAME) +
                         GetSystemMetrics(SM.SM_CXPADDEDBORDER);

        // In a borderless Delegate the detection of the bar to the event handler
        var hitTestEvent = _hitTestEvent;
        hitTestEvent.Handled = false;
        hitTestEvent.WindowSize = _dpi.PixelToLogical(new Size(rcWindow.right - rcWindow.left, rcWindow.bottom - rcWindow.top));
        hitTestEvent.MousePosition = ScreenToClient(new Point(ptMouse.x, ptMouse.y));

        OnWindowEvent(hitTestEvent);
        if (hitTestEvent.Handled)
        {
            switch (hitTestEvent.Result)
            {
                case HitTest.None:
                    return HTNOWHERE;
                case HitTest.Menu:
                    return HTMENU;
                case HitTest.Help:
                    return HTHELP;
                case HitTest.Caption:
                    return HTCAPTION;
                case HitTest.MinimizeButton:
                    return HTMINBUTTON;
                case HitTest.MaximizeButton:
                    return HTMAXBUTTON;
                case HitTest.CloseButton:
                    return HTCLOSE;
                case HitTest.BorderLeft:
                    return HTLEFT;
                case HitTest.BorderRight:
                    return HTRIGHT;
                case HitTest.BorderTop:
                    return HTTOP;
                case HitTest.BorderBottom:
                    return HTBOTTOM;
                case HitTest.BorderTopLeft:
                    return HTTOPLEFT;
                case HitTest.BorderTopRight:
                    return HTTOPRIGHT;
                case HitTest.BorderBottomLeft:
                    return HTBOTTOMLEFT;
                case HitTest.BorderBottomRight:
                    return HTBOTTOMRIGHT;
                case HitTest.Client:
                    return HTCLIENT;
            }
        }

        // The following provides a default handling for the borders
        // Unless we are maximized or fullscreen
        bool isMaximized = IsMaximized(HWnd);
        if (!_isFullscreen && !isMaximized)
        {
            // Diagonal are wider than the frame
            int diagonalWidth = frameSize * 2 + GetSystemMetrics(SM.SM_CXBORDER);

            // Top
            if (ptMouse.y >= rcWindow.top)
            {
                bool isTopBorder = ptMouse.y < rcWindow.top + frameSize;

                if (isTopBorder)
                {
                    if (ptMouse.x < rcWindow.left + diagonalWidth)
                    {
                        return HTTOPLEFT;
                    }

                    if (ptMouse.x > rcWindow.right - diagonalWidth)
                    {
                        return HTTOPRIGHT;
                    }

                    return HTTOP;
                }
            }

            // Bottom
            if (ptMouse.y >= rcWindow.bottom - frameSize)
            {
                if (ptMouse.x < rcWindow.left + diagonalWidth)
                {
                    return HTBOTTOMLEFT;
                }

                return ptMouse.x > rcWindow.right - diagonalWidth ? HTBOTTOMRIGHT : HTBOTTOM;
            }

            // Left or Right
            if (ptMouse.x < rcWindow.left + frameSize)
            {
                return HTLEFT;
            }
            else if (ptMouse.x > rcWindow.right - frameSize)
            {
                return HTRIGHT;
            }
        }

        return HTCLIENT;
    }

    internal void HandlePaint()
    {
        RECT rect;
        if (GetUpdateRect(HWnd, &rect, FALSE))
        {
            ToRectangleLogical(&rect, out var rectF);

            // If the event did not handle it, assume that nothing is being drawn
            // So we clear at least the background
            if (!OnPaintEvent(rectF))
            {
                ClearBackground();
            }

            ValidateRect(HWnd, &rect);
        }
    }

    internal void ClearBackground()
    {
        RECT rect;
        if (GetClientRect(HWnd, &rect))
        {
            PAINTSTRUCT ps;
            var hdc = BeginPaint(HWnd, &ps);
            FillRect(hdc, &rect, _backgroundColorBrush);
            EndPaint(HWnd, &ps);
        }
    }

    private void ToRectangleLogical(RECT* rect, out RectangleF rectF)
    {
        var dpi = _dpi;

        var position = new Point(rect->left, rect->top);
        var size = new Size(rect->right - rect->left, rect->bottom - rect->top);
        rectF = new RectangleF(dpi.PixelToLogical(position), dpi.PixelToLogical(size));
    }

    internal void HandleSizeChanged(int sizeKind)
    {
        // We don't change the state while in fullscreen mode
        if (_isFullscreen) return;

        if (sizeKind == SIZE_MAXIMIZED)
        {
            _state = WindowState.Maximized;
            OnFrameEvent(FrameEventKind.Maximized);
        }
        else if (sizeKind == SIZE_MINIMIZED)
        {
            _state = WindowState.Minimized;
            OnFrameEvent(FrameEventKind.Minimized);
        }
        else if (sizeKind == SIZE_RESTORED)
        {
            if (_state != WindowState.Normal)
            {
                _state = WindowState.Normal;
                OnFrameEvent(FrameEventKind.Restored);
            }
        }
    }

    internal void HandlePositionChanged(WINDOWPOS* pos)
    {
        var position = new Point(pos->x, pos->y);
        var size = _dpi.PixelToLogical(new Size(pos->cx, pos->cy));

        bool posChanged = false;
        bool sizeChanged = false;

        if (position != _position)
        {
            _position = position;
            posChanged = true;
        }

        if (_size != size)
        {
            _size = size;
            sizeChanged = true;
        }

        // Notify events
        if (posChanged)
        {
            OnFrameEvent(sizeChanged ? FrameEventKind.PositionAndSizeChanged : FrameEventKind.Moved);
        }
        else if (sizeChanged)
        {
            OnFrameEvent(FrameEventKind.Resized);
        }
    }

    internal LRESULT HandleMouse(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        var mouse = _mouseEvent;
        mouse.Button = MouseButton.None;
        mouse.Pressed = MouseButtonFlags.None;
        mouse.Position = default;
        mouse.WheelDelta = default;

        SetMouseButtonStates(wParam, mouse);

        var pixelPositionX = GET_X_LPARAM(lParam);
        var pixelPositionY = GET_Y_LPARAM(lParam);

        mouse.Position = _dpi.PixelToLogical(new Point(pixelPositionX, pixelPositionY));

        if (message == WM_MOUSEMOVE && !_mouseTracked)
        {
            TRACKMOUSEEVENT trackMouseEvent;
            trackMouseEvent.cbSize = (uint)sizeof(TRACKMOUSEEVENT);
            trackMouseEvent.hwndTrack = HWnd;
            trackMouseEvent.dwHoverTime = HOVER_DEFAULT;
            //trackMouseEvent.dwFlags = TME_LEAVE | TME_NONCLIENT | TME_CANCEL;
            trackMouseEvent.dwFlags = TME_LEAVE;
            _mouseTracked = TrackMouseEvent(&trackMouseEvent);

            if (_mouseTracked)
            {
                // Send a mouse enter
                mouse.MouseKind = MouseEventKind.Enter;
                OnWindowEvent(mouse);
            }
            _mouseLastX = -1.0f;
            _mouseLastY = -1.0f;
            return 0;
        }

        if (message == WM_MOUSELEAVE)
        {
            _mouseTracked = false;
            // Send a mouse leave
            mouse.MouseKind = MouseEventKind.Leave;
            mouse.Position.X = _mouseLastX;
            mouse.Position.Y = _mouseLastY;
            OnWindowEvent(mouse);
            _mouseLastX = -1.0f;
            _mouseLastY = -1.0f;
            return 0;
        }

        switch (message)
        {
            case WM_MOUSEMOVE:
                // Don't send a mouse move if position has not changed
                if (_mouseLastX == mouse.Position.X && _mouseLastY == mouse.Position.Y)
                {
                    return 0;
                }
                mouse.MouseKind = MouseEventKind.Move;
                break;
            case WM_LBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = MouseButton.Left;
                mouse.MouseKind = MouseEventKind.ButtonDown;
                break;
            case WM_LBUTTONUP:
                EndCaptureMouse();
                mouse.Button = MouseButton.Left;
                mouse.MouseKind = MouseEventKind.ButtonUp;
                break;
            case WM_LBUTTONDBLCLK:
                mouse.Button = MouseButton.Left;
                mouse.MouseKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_RBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = MouseButton.Right;
                mouse.MouseKind = MouseEventKind.ButtonDown;
                break;
            case WM_RBUTTONUP:
                EndCaptureMouse();
                mouse.Button = MouseButton.Right;
                mouse.MouseKind = MouseEventKind.ButtonUp;
                break;
            case WM_RBUTTONDBLCLK:
                mouse.Button = MouseButton.Right;
                mouse.MouseKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_MBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = MouseButton.Middle;
                mouse.MouseKind = MouseEventKind.ButtonDown;
                break;
            case WM_MBUTTONUP:
                EndCaptureMouse();
                mouse.Button = MouseButton.Middle;
                mouse.MouseKind = MouseEventKind.ButtonUp;
                break;
            case WM_MBUTTONDBLCLK:
                mouse.Button = MouseButton.Middle;
                mouse.MouseKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_XBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = HIWORD(wParam) == XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2;
                mouse.MouseKind = MouseEventKind.ButtonDown;
                break;
            case WM_XBUTTONUP:
                EndCaptureMouse();
                mouse.Button = HIWORD(wParam) == XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2;
                mouse.MouseKind = MouseEventKind.ButtonUp;
                break;
            case WM_XBUTTONDBLCLK:
                mouse.Button = HIWORD(wParam) == XBUTTON1 ? MouseButton.XButton1 : MouseButton.XButton2;
                mouse.MouseKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_MOUSEWHEEL:
            case WM_MOUSEHWHEEL:
                mouse.MouseKind = MouseEventKind.Wheel;

                // Mouse wheel mouse coords are relative to screen, so convert them back to the client area
                mouse.Position = ScreenToClient(new Point(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)));
                mouse.WheelDelta = new Point(0, GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA);
                break;
        }

        // Notify the mouse event
        _mouseLastX = mouse.Position.X;
        _mouseLastY = mouse.Position.Y;
        OnWindowEvent(mouse);

        return 0;

        static void SetMouseButtonStates(WPARAM wParam, MouseEvent evt)
        {
            // Gets the buttons clicked
            if ((wParam & MK_LBUTTON) != 0)
                evt.Pressed |= MouseButtonFlags.Left;
            if ((wParam & MK_MBUTTON) != 0)
                evt.Pressed |= MouseButtonFlags.Middle;
            if ((wParam & MK_RBUTTON) != 0)
                evt.Pressed |= MouseButtonFlags.Right;
            if ((wParam & MK_XBUTTON1) != 0)
                evt.Pressed |= MouseButtonFlags.XButton1;
            if ((wParam & MK_XBUTTON2) != 0)
                evt.Pressed |= MouseButtonFlags.XButton2;
        }
    }

    private void BeginCaptureMouse()
    {
        if (_mouseCapture == 0)
        {
            SetCapture(HWnd);
        }
        _mouseCapture++;

    }

    private void EndCaptureMouse()
    {
        _mouseCapture--;
        if (_mouseCapture == 0)
        {
            ReleaseCapture();
        }
    }

    private void UpdateEnable(bool enable)
    {
        EnableWindow(HWnd, enable);
        // Notify is handled by WindowProc
    }

    private void UpdateDecorations(bool value, bool notify = true, bool invalidate = true)
    {
        UpdateCompositionEnabled(value, _isFullscreen);

        bool changed = _hasDecorations != value;
        _hasDecorations = value;

        // Notify with a frame changed event
        SetWindowPos(HWnd, HWND.NULL, 0, 0, 0, 0, SWP.SWP_NOMOVE | SWP.SWP_NOSIZE | SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER | SWP.SWP_FRAMECHANGED);

        if (invalidate)
        {
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }

        if (notify && changed)
        {
            OnFrameEvent(FrameEventKind.DecorationsChanged);
        }
    }
    private void UpdateDpi(Dpi dpi)
    {
        _dpi = dpi;
        if (_dpi != dpi)
        {
            OnFrameEvent(FrameEventKind.DpiChanged);
        }
    }

    private void UpdateDpiMode(DpiMode dpiMode)
    {
        // If we are switching from Manual to Auto DPI, we need to reset the DPI
        // accordingly
        _dpiMode = dpiMode;
        if (dpiMode == DpiMode.Auto)
        {
            var defaultDpiX = Win32Helper.GetDpiForWindowSafe(HWnd);
            var defaultDpi = new Dpi(defaultDpiX, defaultDpiX);
            UpdateDpi(defaultDpi);
        }

        if (_dpiMode != dpiMode)
        {
            OnFrameEvent(FrameEventKind.DpiModeChanged);
        }
    }

    private void UpdateBackgroundColor(Color value, bool notify = true)
    {
        // If we don't have a default color brush
        if (!_isDefaultColorBrush)
        {
            DeleteObject(_backgroundColorBrush);
        }

        _backgroundColor = value;
        _backgroundColorBrush = CreateSolidBrush(Win32Helper.FromColor(value));
        _isDefaultColorBrush = false;

        if (notify)
        {
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
            SendMessage(HWnd, WM_ERASEBKGND, 0, 0);
            OnFrameEvent(FrameEventKind.BackgroundColorChanged);
        }
    }

    private void UpdateTitle(string title)
    {
        fixed (char* pTitle = title)
        {
            SetWindowTextW(HWnd, (ushort*)pTitle);
        }

        _title = title;
        OnFrameEvent(FrameEventKind.TitleChanged);
    }

    private void UpdateSize(SizeF value)
    {
        var screenSize = _dpi.LogicalToPixel(value);
        SetWindowPos(HWnd, HWND.NULL, 0, 0, screenSize.Width, screenSize.Height, SWP.SWP_NOMOVE | SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER);
        // Notify is handled by HandlePositionChanged
    }

    private void UpdatePosition(Point value)
    {
        SetWindowPos(HWnd, HWND.NULL, value.X, value.Y, 0, 0, SWP.SWP_NOSIZE | SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER);
        // Notify is handled by HandlePositionChanged
    }

    private void UpdateVisible(bool value)
    {
        ShowWindow(HWnd, value ? SW.SW_SHOW : SW.SW_HIDE);
        // Notify is handled by WindowProc
    }

    private void UpdateDragDrop(bool value, bool notify)
    {
        var exStyle = GetWindowExStyle(HWnd);
        if (value)
        {
            _dropTarget.Register();
            exStyle |= WS_EX_ACCEPTFILES;
        }
        else
        {
            _dropTarget.UnRegister();
            exStyle &= ~(uint)WS_EX_ACCEPTFILES;
        }
        SetWindowLongPtr(HWnd, GWL.GWL_EXSTYLE, (nint)exStyle);

        _allowDragAndDrop = value;
        if (notify)
        {
            OnFrameEvent(FrameEventKind.DragDropChanged);
        }
    }

    private void UpdateResizeable(bool value, bool invalidate = true)
    {
        var style = GetWindowStyle(HWnd);
        if (value)
        {
            style |= WS_SIZEBOX;
            // We can maximize only if there is no MaximumSize defined
            if (_maximumSize.IsEmpty)
            {
                style |= WS_MAXIMIZEBOX;
            }
        }
        else
        {
            // Disable size grip AND MaximizeBox 
            style &= ~(uint)(WS_SIZEBOX | WS_MAXIMIZEBOX);
        }
        SetWindowLongPtr(HWnd, GWL.GWL_STYLE, (nint)style);

        if (invalidate)
        {
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }

        bool changed = _resizeable != value;
        _resizeable = value;
        if (changed)
        {
            OnFrameEvent(FrameEventKind.ResizeableChanged);
        }
    }

    private void UpdateMaximizeable(bool value, bool invalidate = true)
    {
        var style = GetWindowStyle(HWnd);
        if (value)
        {
            style |= WS_MAXIMIZEBOX;
        }
        else
        {
            style &= ~(uint)WS_MAXIMIZEBOX;
        }
        SetWindowLongPtr(HWnd, GWL.GWL_STYLE, (nint)style);

        if (invalidate)
        {
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }

        bool changed = _maximizeable != value;
        _maximizeable = value;
        if (changed)
        {
            OnFrameEvent(FrameEventKind.MaximizeableChanged);
        }
    }

    private void UpdateMinimizeable(bool value, bool invalidate = true)
    {
        var style = GetWindowStyle(HWnd);
        if (value)
        {
            style |= WS_MINIMIZEBOX;
        }
        else
        {
            style &= ~(uint)WS_MINIMIZEBOX;
        }
        SetWindowLongPtr(HWnd, GWL.GWL_STYLE, (nint)style);

        if (invalidate)
        {
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }

        _minimizeable = value;
        OnFrameEvent(FrameEventKind.MinimizeableChanged);
    }

    private void UpdateWindowState(WindowState state)
    {
        switch (state)
        {
            case WindowState.Normal:
                if (_isFullscreen)
                {
                    RestoreWindowBeforeFullScreen();
                }
                ShowWindow(HWnd, SW.SW_NORMAL);
                break;
            case WindowState.Minimized:
                if (_isFullscreen)
                {
                    RestoreWindowBeforeFullScreen();
                }
                ShowWindow(HWnd, SW.SW_MINIMIZE);
                break;
            case WindowState.Maximized:
                if (_maximumSize.IsEmpty)
                {
                    if (_isFullscreen)
                    {
                        RestoreWindowBeforeFullScreen();
                    }
                    ShowWindow(HWnd, SW.SW_MAXIMIZE);
                }
                break;
            case WindowState.FullScreen:
                if (!_isFullscreen)
                {
                    SwitchToFullScreen();
                }
                break;
        }

        if (_state != state)
        {
            _state = state;
            OnFrameEvent(FrameEventKind.StateChanged);
        }
    }

    private void UpdateOpacity(float value)
    {
        value = Math.Clamp(value, 0.0f, 1.0f);

        bool hasOpacity = value < 1.0f;

        var exStyle = GetWindowExStyle(HWnd);
        if (hasOpacity)
        {
            exStyle |= WS_EX_LAYERED;
            SetWindowLongPtr(HWnd, GWL.GWL_EXSTYLE, (nint)exStyle);
            var fixedValue = (byte)(value * 255);
            if (fixedValue == 0)
            {
                fixedValue = 1;
            }
            SetLayeredWindowAttributes(HWnd, new COLORREF(), fixedValue, LWA.LWA_ALPHA);
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }
        else
        {
            SetLayeredWindowAttributes(HWnd, new COLORREF(), 255, LWA.LWA_ALPHA);
            exStyle &= ~(uint)WS_EX_LAYERED;
            SetWindowLongPtr(HWnd, GWL.GWL_EXSTYLE, (nint)exStyle);
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }

        _opacity = value;
        OnFrameEvent(FrameEventKind.OpacityChanged);
    }

    private void UpdateTopMost(bool value)
    {
        SetWindowPos(HWnd, value ? HWND_TOPMOST : HWND_NOTOPMOST, 0, 0, 0, 0, SWP.SWP_NOMOVE | SWP.SWP_NOSIZE);
        _topMost = value;
        OnFrameEvent(FrameEventKind.TopMostChanged);
    }

    private void UpdateMinimumSize(SizeF value, bool updateWindowSize = true, bool notify = true)
    {
        _minimumSize = value;

        // Make sure the size is no smaller than the default minimum track size
        var minWindowTrackSize = MinWindowTrackSize;
        _minimumSize.Width = Math.Max(minWindowTrackSize.Width, _minimumSize.Width);
        _minimumSize.Height = Math.Max(minWindowTrackSize.Height, _minimumSize.Height);

        // Bump maximum size if necessary
        if (!_maximumSize.IsEmpty && !value.IsEmpty)
        {
            if (_maximumSize.Width < value.Width)
            {
                _maximumSize.Width = value.Width;
            }

            if (_maximumSize.Height < value.Height)
            {
                _maximumSize.Height = value.Height;
            }
        }

        if (updateWindowSize)
        {
            // Keep form size within new limits
            SizeF size = Size;
            if (size.Width < value.Width || size.Height < value.Height)
            {
                Size = new SizeF(Math.Max(size.Width, value.Width), Math.Max(size.Height, value.Height));
            }
        }

        if (notify)
        {
            OnFrameEvent(FrameEventKind.MinimumSizeChanged);
        }
    }

    private void UpdateMaximumSize(SizeF value, bool updateWindowSize = true, bool notify = true)
    {
        _maximumSize = value;

        // Bump minimum size if necessary
        if (!_minimumSize.IsEmpty && !value.IsEmpty)
        {
            if (_minimumSize.Width > value.Width)
            {
                _minimumSize.Width = value.Width;
            }

            if (_minimumSize.Height > value.Height)
            {
                _minimumSize.Height = value.Height;
            }
        }

        if (updateWindowSize)
        {
            // Keep form size within new limits
            SizeF currentSize = Size;
            if (!value.IsEmpty && (currentSize.Width > value.Width || currentSize.Height > value.Height))
            {
                Size = new SizeF(Math.Min(currentSize.Width, value.Width), Math.Min(currentSize.Height, value.Height));
            }
        }

        if (notify)
        {
            OnFrameEvent(FrameEventKind.MaximumSizeChanged);
        }
    }

    private void UpdateModal(bool modal)
    {
        var parentWindow = _parentWindow!;

        // When making a window modal, disable the parent window
        parentWindow.Enable = !modal;

        _modal = modal;
        OnFrameEvent(FrameEventKind.ModalChanged);
    }

    private void UpdateShowInTaskBar(bool value)
    {
        var exStyle = GetWindowExStyle(HWnd);
        if (value)
        {
            exStyle |= WS_EX_APPWINDOW;
        }
        else
        {
            exStyle &= ~(uint)WS_EX_APPWINDOW;
        }

        // For some reason, if the window was created without the WS_EX_APPWINDOW
        // We need to force hide/show to add the WS_EX_APPWINDOW at least once to be able to toggle it after
        if (!_initialShowInTaskBar)
        {
            ShowWindow(HWnd, SW.SW_HIDE);
        }

        SetWindowLongPtr(HWnd, GWL.GWL_EXSTYLE, (nint)exStyle);

        if (!_initialShowInTaskBar)
        {
            ShowWindow(HWnd, SW.SW_SHOW);
            // Once we have assigned WS_EX_APPWINDOW at least once, we don't need to hack with show/hide
            // for subsequent ShowInTaskBar changes
            _initialShowInTaskBar = true;
        }

        _showInTaskBar = value;
        OnFrameEvent(FrameEventKind.ShowInTaskBarChanged);
    }

    private void SwitchToFullScreen()
    {
        if (_state == WindowState.FullScreen) return;

        var screen = GetScreenOrPrimary();
        if (screen == null) return;

        // If we are switching to fullscreen (exclusive or not), save the size/decorations of the window 
        RECT windowRect;
        GetWindowRect(HWnd, &windowRect);
        _windowRectBeforeFullScreen = windowRect.ToRectangle();
        _hasDecorationsBeforeFullScreen = _hasDecorations;
        _hasMinimizeableBeforeFullScreen = _minimizeable;
        _hasMaximizeableBeforeFullScreen = _maximizeable;
        _hasResizeableBeforeFullScreen = _resizeable;
        _dpiBeforeFullScreen = _dpi;
        _isFullscreen = true;

        // Update the style of the window
        UpdateMaximizeable(false, false);
        UpdateMinimizeable(false, false);
        UpdateResizeable(false, false);
        UpdateDecorations(false, true, false);

        // Set the window position
        SetWindowPos(HWnd, HWND.NULL, screen.Position.X, screen.Position.Y, screen.SizeInPixels.Width, screen.SizeInPixels.Height, SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);
        InvalidateRect(HWnd, null, false);
        UpdateWindow(HWnd);

        // Make sure that the Dispatcher is informed that we might have an active window in exclusive fullscreen and we want to disable WIN keys
        //Dispatcher.HasActiveExclusiveFullScreenWindow = _windowIsActive && state.Kind == WindowState.ExclusiveFullScreen;
    }

    private void RestoreWindowBeforeFullScreen()
    {
        if (_dpiMode == DpiMode.Auto)
        {
            UpdateDpi(_dpiBeforeFullScreen);
        }

        UpdateMinimizeable(_hasMinimizeableBeforeFullScreen, false);
        UpdateMaximizeable(_hasMaximizeableBeforeFullScreen, false);
        UpdateResizeable(_hasResizeableBeforeFullScreen, false);
        UpdateDecorations(_hasDecorationsBeforeFullScreen, false);
        _isFullscreen = false;

        // Restore the Window position
        SetWindowPos(HWnd, HWND.NULL, _windowRectBeforeFullScreen.X, _windowRectBeforeFullScreen.Y, _windowRectBeforeFullScreen.Width, _windowRectBeforeFullScreen.Height, SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);

        InvalidateRect(HWnd, null, false);
        UpdateWindow(HWnd);

        _hasMinimizeableBeforeFullScreen = default;
        _hasMaximizeableBeforeFullScreen = default;
        _hasDecorationsBeforeFullScreen = default;
        _windowRectBeforeFullScreen = default;
        _dpiBeforeFullScreen = default;
    }

    private void CreateWindowHandle(WindowCreateOptions options)
    {
        _minimumSize = options.MinimumSize ?? SizeF.Empty;
        _maximumSize = options.MaximumSize ?? SizeF.Empty;

        // Make sure that the size we have can't be smaller than the default
        UpdateMinimumSize(_minimumSize, false, false);
        UpdateMaximumSize(_maximumSize, false, false);

        var positionX = CW_USEDEFAULT;
        var positionY = CW_USEDEFAULT;
        int width;
        int height;

        if (options.Kind == WindowKind.Child)
        {
            var parentWindowHandle = options.Parent!.Handle;

            RECT parentRect = default;
            GetClientRect((HWND)parentWindowHandle, &parentRect);
            positionX = parentRect.left;
            positionY = parentRect.top;
            width = parentRect.right - parentRect.left;
            height = parentRect.bottom - parentRect.top;
        }
        else
        {

            if (options.Position is { } pos)
            {
                positionX = pos.X;
                positionY = pos.Y;
            }

            if (options.Size is { } size)
            {
                width = size.Width;
                height = size.Height;
            }
            else if (Screen.Primary is { } primary)
            {
                var factor = options.DefaultSizeFactor;
                factor.X = Math.Clamp(factor.X, 0.1f, 1.0f);
                factor.Y = Math.Clamp(factor.X, 0.1f, 1.0f);

                width = (int)(primary.SizeInPixels.Width * factor.X);
                height = (int)(primary.SizeInPixels.Height * factor.Y);
            }
            else
            {
                width = 640;
                height = 320;
            }

            // Popup windows don't have a default size with CreateWindowEx, so force one
            if (options.Kind == WindowKind.Popup)
            {
                if (width == CW_USEDEFAULT)
                {
                    width = 512;
                }

                if (height == CW_USEDEFAULT)
                {
                    height = 256;
                }
            }

            var screen = Screen.Primary;
            if (screen != null)
            {
                if (width == CW_USEDEFAULT || height == CW_USEDEFAULT)
                {
                    width = (int)(screen.SizeInPixels.Width * 0.80f);
                    height = (int)(screen.SizeInPixels.Height * 0.80f);
                }

                if (positionX == CW_USEDEFAULT || positionY == CW_USEDEFAULT)
                {
                    positionX = (int)(screen.SizeInPixels.Width * 0.10f);
                    positionY = (int)(screen.SizeInPixels.Height * 0.10f);
                }
            }

            // By default, a top level window is visible on the taskbar
            if (options.Kind == WindowKind.TopLevel)
            {
                _showInTaskBar = options.ShowInTaskBar;
            }
        }

        // We are using the hidden Dispatcher HWND for a top level window in order to be able to hide the Window from the taskbar
        // via ShowInTaskBar
        var hWndParent = options.Parent is { } parentWindow ? (HWND)parentWindow.Handle : Dispatcher.Hwnd;

        var (style, styleEx) = GetStyleAndStyleExFromOptions(options);
        fixed (char* lpWindowName = options.Title)
        {
            _thisGcHandle = GCHandle.Alloc(this);
            Win32Dispatcher.CreatedWindowHandle = _thisGcHandle;
            Handle = CreateWindowExW(
                styleEx,
                (ushort*)Dispatcher.ClassAtom,
                (ushort*)lpWindowName,
                style,
                X: positionX,
                Y: positionY,
                nWidth: width,
                nHeight: height,
                hWndParent: hWndParent,
                hMenu: HMENU.NULL,
                hInstance: Win32Helper.ModuleHandle,
                lpParam: (void*)null
            );
            Win32Dispatcher.CreatedWindowHandle = default;

            UpdateThemeActive();
            UpdateCompositionEnabled(_hasDecorations, false);

            // Check for clipboard events
            AddClipboardFormatListener(HWnd);

            // Update the location and size of the window after creation and before making it visible
            if (Win32Helper.TryGetWindowPositionAndSize(HWnd, out var bounds))
            {
                SetWindowBounds(bounds);
            }

            if (Kind != WindowKind.Child)
            {
                switch (options.StartPosition)
                {
                    case WindowStartPosition.CenterParent:
                        CenterToParent();
                        break;
                    case WindowStartPosition.CenterScreen:
                        CenterToScreen();
                        break;
                }
            }

            // Sets the icon if specified
            if (options.Icon is { } icon)
            {
                SetIcon(icon);
            }

            if (options.Visible)
            {
                ShowWindow(HWnd, SW.SW_SHOWDEFAULT);
                InvalidateRect(HWnd, null, false);
                UpdateWindow(HWnd);
                _visible = options.Visible;
            }
        }
    }

    private static (uint, uint) GetStyleAndStyleExFromOptions(in WindowCreateOptions options)
    {
        uint style = WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        uint styleEx = 0;

        // options.Visible is set after the window is created

        switch (options.Kind)
        {
            case WindowKind.TopLevel:
            {
                style |= WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU;
                if (options.ShowInTaskBar)
                {
                    styleEx |= WS_EX_APPWINDOW;
                }

                break;
            }
            case WindowKind.Popup:
                style |= WS_POPUP | WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU;
                break;

            case WindowKind.Child:
                style |= WS_CHILD;
                break;
        }

        if (options.Kind != WindowKind.Child)
        {
            if (options.Resizable)
            {
                style |= WS_SIZEBOX;
            }

            if (options.Maximizable && !options.MaximumSize.HasValue)
            {
                style |= WS_MAXIMIZEBOX;
            }

            if (options.Minimizable)
            {
                style |= WS_MINIMIZEBOX;
            }

            if (options.WindowState == WindowState.Maximized && !options.MaximumSize.HasValue)
            {
                style |= WS_MAXIMIZE;
            }
            else if (options.WindowState == WindowState.Minimized)
            {
                style |= WS_MINIMIZE;
            }

            if (options.Transparent)
            {
                styleEx |= WS_EX_TRANSPARENT;
            }
        }

        return (style, styleEx);
    }

    private Color GetDefaultWindowBackgroundColor()
    {
        // This is matching what is declared in Win32Dispatcher for the Window Class
        return Win32Helper.ToColor((COLORREF)GetSysColor(COLOR.COLOR_WINDOW));
    }

    private void VerifyAccessAndNotDestroyed()
    {
        VerifyAccess();
        if (_disposed)
        {
            throw new InvalidOperationException("This window has been closed and destroyed.");
        }
    }

    private void VerifyNotFullScreen()
    {
        if (_isFullscreen) throw new ArgumentException("Cannot change this property when the screen is in fullscreen");
    }

    private void VerifyNotFullScreenAndNotMaximized()
    {
        VerifyNotFullScreen();
        if (_state == WindowState.Maximized) throw new ArgumentException("Cannot change this property when the screen is maximized");
    }
}
