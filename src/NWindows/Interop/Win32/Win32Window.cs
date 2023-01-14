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

// Check about hittest https://github.com/microsoft/terminal/blob/547349af77df16d0eed1c73ba3041c84f7b063da/src/cascadia/WindowsTerminal/NonClientIslandWindow.cpp

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

    private Color _backgroundColor;
    private HBRUSH _backgroundColorBrush;
    private bool _isDefaultColorBrush;
    private bool _showInTaskBar;
    private bool _initialShowInTaskBar;
    private Point _dpi;
    private readonly Win32DropTarget _dropTarget;

    private bool _isFullscreen;
    private RECT _windowRectBeforeFullScreen;
    private bool _hasDecorationsBeforeFullScreen;
    private bool _hasMaximizeableBeforeFullScreen;
    private bool _hasMinimizeableBeforeFullScreen;
    private bool _hasResizeableBeforeFullScreen;
    private Point _dpiBeforeFullScreen;

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
            if (value != _enable)
            {
                UpdateDecorations(value);
            }
        }
    }

    public override Point Dpi
    {
        get
        {
            VerifyAccess();
            return _dpi;
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
            if (value != _size)
            {
                UpdateSize(value);
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
            VerifyResizeable();

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
            var pixelPosition = new Point(WindowHelper.LogicalToPixel(position.X, screen.Dpi.X), WindowHelper.LogicalToPixel(position.Y, screen.Dpi.Y));
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
            Windows.ScreenToClient(HWnd, (POINT*)&position);
            return new PointF(WindowHelper.PixelToLogical(position.X, screen.Dpi.X), WindowHelper.PixelToLogical(position.Y, screen.Dpi.Y));
        }
        else
        {
            return default;
        }
    }

    public override Screen? GetScreen()
    {
        VerifyAccessAndNotDestroyed();
        var handle = MonitorFromWindow(HWnd, MONITOR.MONITOR_DEFAULTTONEAREST);
        Dispatcher.ScreenManager.TryGetScreen(handle, out var screen);
        return screen;
    }

    public override void CenterToParent()
    {
        VerifyNotChild();

        var parent = Parent;
        if (parent != null && Win32Helper.TryGetPositionSizeDpiAndRECT((HWND)parent.Handle, false, out var bounds))
        {
            CenterPositionFromBounds(bounds.Item1, bounds.Item2);
        }
        else
        {
            CenterToScreen();
        }
    }

    /// <summary>
    ///  Gets the system's default minimum tracking dimensions of a window in pixels.
    /// </summary>
    private static SizeF MinWindowTrackSize
    {
        get
        {
            var dpi = Screen.PrimaryDpi;
            return new SizeF(WindowHelper.PixelToLogical(GetSystemMetrics(SM.SM_CXMINTRACK), dpi.X),
                WindowHelper.PixelToLogical(GetSystemMetrics(SM.SM_CYMINTRACK), dpi.Y));
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
                HandleGetMinMaxInfo((MINMAXINFO*)lParam);
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
                UpdateDpi(HIWORD(wParam), in *(RECT*)lParam);
                result = 0;
                break;

            case WM_CLIPBOARDUPDATE:
                OnFrameEvent(FrameEventKind.ClipboardChanged);
                result = 0;
                break;
        }

        return result;
    }

    private void UpdateDpi(int dpiX, in RECT suggestedRect)
    {
        var dpi = new Point(dpiX, dpiX);
        if (dpi != _dpi)
        {
            var previous = _dpi;
            _dpi = dpi;
            OnWindowEvent(new DpiChangedEvent() { PreviousValue = previous, NewValue = dpi });

            var w = suggestedRect.right - suggestedRect.left;
            var h = suggestedRect.bottom - suggestedRect.top;

            SetWindowPos(HWnd,
                HWND.NULL,
                suggestedRect.left,
                suggestedRect.top,
                w,
                h,
                SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);
        }
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
                HandleBorderLessNonClientCalculateClientSize(wParam, (NCCALCSIZE_PARAMS*)lParam);
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
    private void HandleGetMinMaxInfo(MINMAXINFO* info)
    {
        // This is called at the creation of the window
        if (_dpi.IsEmpty)
        {
            var dpiX = Win32Helper.GetDpiForWindowSafe(HWnd);
            _dpi = new Point(dpiX, dpiX);
        }
        
        if (_resizeable)
        {
            if (!_minimumSize.IsEmpty)
            {
                *(Size*)&info->ptMinTrackSize = WindowHelper.LogicalToPixel(_minimumSize, _dpi);

                // When the MinTrackSize is set to a value larger than the screen
                // size but the MaxTrackSize is not set to a value equal to or greater than the
                // MinTrackSize and the user attempts to "grab" a resizing handle, Windows makes
                // the window move a distance equal to either the width when attempting to resize
                // horizontally or the height of the window when attempting to resize vertically.
                // So, the workaround to prevent this problem is to set the MaxTrackSize to something
                // whenever the MinTrackSize is set to a value larger than the respective dimension
                // of the virtual screen.
                var screen = GetScreen();
                if (_maximumSize.IsEmpty && screen != null)
                {
                    // Only set the max track size dimensions if the min track size dimensions
                    // are larger than the VirtualScreen dimensions.
                    Size virtualScreen = WindowHelper.LogicalToPixel(screen.Size, _dpi);
                    if (_minimumSize.Height > virtualScreen.Height)
                    {
                        info->ptMaxTrackSize.y = int.MaxValue;
                    }

                    if (_minimumSize.Width > virtualScreen.Width)
                    {
                        info->ptMaxTrackSize.x = int.MaxValue;
                    }
                }
            }

            if (!_maximumSize.IsEmpty)
            {
                var size = WindowHelper.LogicalToPixel(_maximumSize, _dpi);
                // TODO: Max on SystemInformation.MinWindowTrackSize
                *(Size*)&info->ptMaxTrackSize = size;
            }

            // TODO: set MaxSize/MaxPosition
        }
        else
        {
            Size sizeInPixel;
            // Force the size to full screen
            if (_isFullscreen && GetScreen() is {} screen)
            {
                sizeInPixel = screen.SizeInPixels;
            }
            else
            {
                sizeInPixel = WindowHelper.LogicalToPixel(_size, _dpi);
            }

            info->ptMaxSize.x = sizeInPixel.Width;
            info->ptMaxSize.y = sizeInPixel.Height;
            info->ptMaxPosition.x = _position.X;
            info->ptMaxPosition.y = _position.Y;
            info->ptMinTrackSize.x = sizeInPixel.Width;
            info->ptMinTrackSize.y = sizeInPixel.Height;
            info->ptMaxTrackSize.x = sizeInPixel.Width;
            info->ptMaxTrackSize.y = sizeInPixel.Height;
        }
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

    private unsafe void HandleBorderLessNonClientCalculateClientSize(WPARAM wParam, NCCALCSIZE_PARAMS* pnCsp)
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

            var screen = GetScreen();
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
        if (IsMaximized(HWnd) || _isFullscreen)
        {
            return HTCLIENT;
        }

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
        hitTestEvent.WindowSize = WindowHelper.PixelToLogical(new Size(rcWindow.right - rcWindow.left, rcWindow.bottom - rcWindow.top), _dpi);
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
        rectF = new RectangleF(WindowHelper.PixelToLogical(rect->left, dpi.X), WindowHelper.PixelToLogical(rect->top, dpi.Y), WindowHelper.PixelToLogical(rect->right - rect->left, dpi.X),
            WindowHelper.PixelToLogical(rect->bottom - rect->top, dpi.Y));
    }

    internal void HandleSizeChanged(int sizeKind)
    {
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

        var dpi = _dpi;
        var size = new SizeF(WindowHelper.PixelToLogical(pos->cx, dpi.X), WindowHelper.PixelToLogical(pos->cy, dpi.Y));

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

        mouse.Position = WindowHelper.PixelToLogical(new Point(pixelPositionX, pixelPositionY), _dpi);

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
        var screenSize = WindowHelper.LogicalToPixel(value, _dpi);
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
        // If we arrive here, It is necessarily:
        // - previous: false, next: true
        // - previous: true, next: false
        bool previousFullScreen = _state.IsFullScreen;
        bool nextFullScreen = state.IsFullScreen;
        
        // Non fullscreen mode cases
        switch (state.Kind)
        {
            case WindowStateKind.Normal:
                if (previousFullScreen)
                {
                    RestoreWindowBeforeFullScreen();
                }
                ShowWindow(HWnd, SW.SW_NORMAL);
                break;
            case WindowStateKind.Minimized:
                if (previousFullScreen)
                {
                    RestoreWindowBeforeFullScreen();
                }
                ShowWindow(HWnd, SW.SW_MINIMIZE);
                break;
            case WindowStateKind.Maximized:
                if (_maximumSize.IsEmpty)
                {
                    if (previousFullScreen)
                    {
                        RestoreWindowBeforeFullScreen();
                    }
                    ShowWindow(HWnd, SW.SW_MAXIMIZE);
                }
                break;
            case WindowStateKind.FullScreen:
            case WindowStateKind.ExclusiveFullScreen:
                // Try to switch to fullscreen, we could be not successful so
                // in that case we have to return without changing the state
                if (!SwitchToFullScreen(state))
                {
                    return;
                }
                break;
        }

        _state = state;
        OnFrameEvent(FrameEventKind.StateChanged);
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

    private bool SwitchToFullScreen(WindowState state)
    {
        var screen = GetScreen();
        if (screen == null) return false;

        // The size of the window must match the size of the screen if we haven't specified an exclusive size
        var currentMode = screen.CurrentDisplayMode;
        var requestedMode = currentMode;

        bool changeOfResolution = false;

        // In case of exclusive, we make sure that the mode is supported
        if (state is WindowState.ExclusiveFullScreen exclusive)
        {
            requestedMode = exclusive.Mode;
            var indexOfScreen = screen.DisplayModes.IndexOf(requestedMode);
            if (indexOfScreen < 0) return false;

            changeOfResolution = currentMode.Width != requestedMode.Width || currentMode.Height != requestedMode.Height;
        }

        // If we are switching to fullscreen (exclusive or not), save the size/decorations of the window 
        if (!_state.IsFullScreen)
        {
            RECT windowRect;
            GetWindowRect(HWnd, &windowRect);
            _windowRectBeforeFullScreen = windowRect;
            _hasDecorationsBeforeFullScreen = _hasDecorations;
            _hasMinimizeableBeforeFullScreen = _minimizeable;
            _hasMaximizeableBeforeFullScreen = _maximizeable;
            _hasResizeableBeforeFullScreen = _resizeable;
            _dpiBeforeFullScreen = _dpi;

            // The window won't have any decorations and won't be maximizeable in fullscreen mode
            _isFullscreen = true;
        }

        // Keep the dpi
        if (changeOfResolution)
        {
            var oldDpi = _dpi;
            _dpi = new Point(96, 96);
            if (oldDpi != _dpi)
            {
                OnWindowEvent(new DpiChangedEvent() { PreviousValue = oldDpi, NewValue = _dpi });
            }
        }

        UpdateMaximizeable(false, false);
        UpdateMinimizeable(false, false);
        UpdateResizeable(false, false);
        UpdateDecorations(false, true, false);

        bool success = true;
        if (state is WindowState.ExclusiveFullScreen)
        {
            // Set the window position
            SetWindowPos(HWnd, HWND.NULL, screen.Position.X, screen.Position.Y, requestedMode.Width, requestedMode.Height, SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);

            fixed (char* displayName = screen.Name)
            {
                DEVMODEW devModeW = default;
                devModeW.dmSize = (ushort)sizeof(DEVMODEW);

                // Save the mouse position before switching mode
                var position = Mouse.Position;
                var mouseRelativePosition = new PointF((float)(position.X - screen.Position.X) / screen.SizeInPixels.Width, (float)(position.Y - screen.Position.Y) / screen.SizeInPixels.Height);

                EnumDisplaySettingsExW((ushort*)displayName, ENUM.ENUM_CURRENT_SETTINGS, &devModeW, 0);

                devModeW.dmBitsPerPel = (uint)requestedMode.BitsPerPixel;
                devModeW.dmPelsWidth = (uint)requestedMode.Width;
                devModeW.dmPelsHeight = (uint)requestedMode.Height;
                devModeW.dmDisplayFrequency = (uint)requestedMode.Frequency;
                devModeW.dmFields =
                    DM.DM_PELSWIDTH |
                    DM.DM_PELSHEIGHT |
                    DM.DM_BITSPERPEL |
                    DM.DM_DISPLAYFREQUENCY;

                var result = ChangeDisplaySettingsExW((ushort*)displayName, &devModeW, HWND.NULL, CDS.CDS_FULLSCREEN, null);
                success = result == DISP.DISP_CHANGE_SUCCESSFUL;

                // Similar to https://github.com/rust-windowing/winit/blob/9225b2812edcdc8abc049f4811361e10cc94cee0/src/platform_impl/windows/window.rs#LL555-L562C39
                // Seems that `ChangeDisplaySettingsExW can take a long time enough that DWM
                // will think that our program is frozen and will take over and won't make our Window fully fullscreen.
                //
                // Calling `PeekMessageW` here notifies Windows that our process is still running
                // fine, taking control back from the DWM and ensuring that the `SetWindowPos` call
                // below goes through.
                MSG msg;
                PeekMessageW(&msg, HWND.NULL, wMsgFilterMin: WM_NULL, wMsgFilterMax: WM_NULL, wRemoveMsg: PM.PM_NOREMOVE);

                // Make sure to reset the window
                SetWindowPos(HWnd, HWND.NULL, screen.Position.X, screen.Position.Y, requestedMode.Width, requestedMode.Height, SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);

                // Restore the mouse position after with similar relative coordinates
                if (mouseRelativePosition.X >= 0 && mouseRelativePosition.Y >= 0 && mouseRelativePosition.X <= 1.0f && mouseRelativePosition.Y <= 1.0f)
                {
                    Mouse.Position = new Point((int)(mouseRelativePosition.X * requestedMode.Width) + screen.Position.X, (int)(mouseRelativePosition.Y * requestedMode.Height) + screen.Position.Y);
                }
            }
        }
        else
        {
            ExitExclusiveFullScreen();

            // If we switch from exclusive to fullscreen, restore the DPI
            var oldDpi = _dpi;
            _dpi = _dpiBeforeFullScreen;
            if (oldDpi != _dpi)
            {
                OnWindowEvent(new DpiChangedEvent() { PreviousValue = oldDpi, NewValue = _dpi });
            }

            // Set the window position
            SetWindowPos(HWnd, HWND.NULL, screen.Position.X, screen.Position.Y, screen.SizeInPixels.Width, screen.SizeInPixels.Height, SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);
        }

        if (success)
        {
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }
        else
        {
            RestoreWindowBeforeFullScreen();
        }

        return success;
    }

    private void RestoreWindowBeforeFullScreen()
    {
        Debug.Assert(_isFullscreen);
        _isFullscreen = false;

        // If we were previously exclusive, restore non exclusive
        if (_state is WindowState.ExclusiveFullScreen)
        {
            ExitExclusiveFullScreen();
        }

        var oldDpi = _dpi;
        _dpi = _dpiBeforeFullScreen;
        if (oldDpi != _dpi)
        {
            OnWindowEvent(new DpiChangedEvent() { PreviousValue = oldDpi, NewValue = _dpi });
        }
        
        var width = _windowRectBeforeFullScreen.right - _windowRectBeforeFullScreen.left;
        var height = _windowRectBeforeFullScreen.bottom - _windowRectBeforeFullScreen.top;

        UpdateMinimizeable(_hasMinimizeableBeforeFullScreen, false);
        UpdateMaximizeable(_hasMaximizeableBeforeFullScreen, false);
        UpdateResizeable(_hasResizeableBeforeFullScreen, false);
        UpdateDecorations(_hasDecorationsBeforeFullScreen, invalidate: false);

        // Restore the Window position
        SetWindowPos(HWnd, HWND.NULL, _windowRectBeforeFullScreen.left, _windowRectBeforeFullScreen.top, width, height, SWP.SWP_NOZORDER | SWP.SWP_NOACTIVATE);

        InvalidateRect(HWnd, null, false);
        UpdateWindow(HWnd);
        
        _hasMinimizeableBeforeFullScreen = default;
        _hasMaximizeableBeforeFullScreen = default;
        _hasDecorationsBeforeFullScreen = default;
        _windowRectBeforeFullScreen = default;
        _dpiBeforeFullScreen = default;
    }

    private void ExitExclusiveFullScreen()
    {
        // Save the mouse position before switching mode
        var screen = GetScreen();

        var position = Mouse.Position;
        var mouseRelativePosition = new PointF(-1.0f, -1.0f);
        if (screen != null)
        {
            mouseRelativePosition = new PointF((float)(position.X - screen.Position.X) / screen.SizeInPixels.Width, (float)(position.Y - screen.Position.Y) / screen.SizeInPixels.Height);
        }
        
        var displayName = screen?.Name ?? null;
        fixed (char* pDisplayName = displayName)
        {
            // Restore default display settings
            ChangeDisplaySettingsExW((ushort*)pDisplayName, null, HWND.NULL, 0, null);
        }

        // Make sure that we update screen coordinates (so that the following screen instance will have been changed)
        if (Dispatcher.ScreenManager.TryUpdateScreens())
        {
            Dispatcher.OnSystemEvent(SystemEventKind.ScreenChanged);
        }

        // Restore the mouse position after with similar relative coordinates
        if (screen != null && mouseRelativePosition.X >= 0 && mouseRelativePosition.Y >= 0 && mouseRelativePosition.X <= 1.0f && mouseRelativePosition.Y <= 1.0f)
        {
            Mouse.Position = new Point((int)(mouseRelativePosition.X * screen.SizeInPixels.Width) + screen.Position.X, (int)(mouseRelativePosition.Y * screen.SizeInPixels.Height) + screen.Position.Y);
        }
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
            if (!options.Decorations && screen != null)
            {
                if (width == CW_USEDEFAULT || height == CW_USEDEFAULT)
                {
                    width = WindowHelper.LogicalToPixel(screen.Size.Width * 0.80f, screen.Dpi.X);
                    height = WindowHelper.LogicalToPixel(screen.Size.Height * 0.80f, screen.Dpi.Y);
                }

                if (positionX == CW_USEDEFAULT || positionY == CW_USEDEFAULT)
                {
                    positionX = WindowHelper.LogicalToPixel(screen.Size.Width * 0.10f, screen.Dpi.X);
                    positionY = WindowHelper.LogicalToPixel(screen.Size.Height * 0.10f, screen.Dpi.Y);
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
            if (Win32Helper.TryGetPositionSizeDpiAndRECT(HWnd, Kind == WindowKind.Child, out var bounds))
            {
                _position = bounds.Item1;
                _size = bounds.Item2;
                UpdateDpi(bounds.Item3, bounds.Item4);
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
}
