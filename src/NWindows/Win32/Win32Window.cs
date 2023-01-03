// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.WS;
using static TerraFX.Interop.Windows.HWND;
using static TerraFX.Interop.Windows.MK;
using static TerraFX.Interop.Windows.Windows;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.TME;

namespace NWindows.Win32;

// Check about hittest https://github.com/microsoft/terminal/blob/547349af77df16d0eed1c73ba3041c84f7b063da/src/cascadia/WindowsTerminal/NonClientIslandWindow.cpp

internal unsafe class Win32Window : Window
{
    private readonly bool _hasDecorations;
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

    public Win32Window(in WindowCreateOptions options) : base(options)
    {
        _hasDecorations = options.Decorations;
        _resizeable = options.Resizable;
        _mouseLastX = -1;
        _mouseLastY = -1;
        _opacity = 1.0f;
        _enable = true;
        _parentWindow = (Win32Window?)options.ParentWindow;
        Kind = options.Kind;
        CreateWindowHandle(options);
    }

    public new Win32Dispatcher  Dispatcher => (Win32Dispatcher)base.Dispatcher;

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
            VerifyAccess();
            if (value != _enable)
            {
                UpdateEnable(value);
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
            VerifyAccess();
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
            VerifyAccess();
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
            VerifyAccess();
            if (_visible != value)
            {
                UpdateVisible(value);
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
            VerifyAccess();

            if (_resizeable != value)
            {
                UpdateResizeable(value);
            }
        }
    }

    public override Window? Parent => _parentWindow;

    public override WindowState State
    {
        get
        {
            VerifyAccess();
            return _state;
        }
        set
        {
            VerifyAccess();
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
            VerifyAccess();
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
            VerifyAccess();

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
            VerifyAccess();
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
            VerifyAccess();
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
            VerifyAccess();
            if (Kind != WindowKind.Popup) throw new InvalidOperationException("Cannot make a non popup Window modal. Only Popup Window can be made modal");

            if (_modal != value)
            {
                UpdateModal(value);
            }
        }
    }

    public override void Focus()
    {
        VerifyAccess();
        SetFocus(HWnd);
    }

    public override void Activate()
    {
        VerifyAccess();
        if (_visible)
        {
            SetForegroundWindow(HWnd);
        }
    }
    
    public override Point ClientToScreen(PointF position)
    {
        VerifyAccess();

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
        VerifyAccess();

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
        VerifyAccess();
        var handle = MonitorFromWindow((HWND)Handle, MONITOR.MONITOR_DEFAULTTONEAREST);
        Dispatcher.ScreenManager.TryGetScreen(handle, out var screen);
        return screen;
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

    private void CreateWindowHandle(in WindowCreateOptions options)
    {
        _minimumSize = options.MinimumSize ?? SizeF.Empty;
        _maximumSize = options.MaximumSize ?? SizeF.Empty;

        // Make sure that the size we have can't be smaller than the default
        UpdateMinimumSize(_minimumSize, false, false);
        UpdateMaximumSize(_maximumSize, false, false);

        var positionX = CW_USEDEFAULT;
        var positionY = CW_USEDEFAULT;
        if (options.Position is { } pos)
        {
            positionX = pos.X;
            positionY = pos.Y;
        }

        var width = CW_USEDEFAULT;
        var height = CW_USEDEFAULT;

        if (options.Size is { } size)
        {
            width = size.Width;
            height = size.Height;
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

        var (style, styleEx) = GetStyleAndStyleExFromOptions(options);
        fixed (char* lpWindowName = options.Title)
        {
            Win32Dispatcher.CreatedWindowHandle = GCHandle.Alloc(this);
            Handle = CreateWindowExW(
                styleEx,
                (ushort*)Dispatcher.ClassAtom,
                (ushort*)lpWindowName,
                style,
                X: positionX,
                Y: positionY,
                nWidth: width,
                nHeight: height,
                hWndParent: options.ParentWindow is {} parentWindow ? (HWND)parentWindow.Handle : HWND_DESKTOP,
                hMenu: HMENU.NULL,
                hInstance: Win32Shared.ModuleHandle,
                lpParam: (void*)null
            );
            Win32Dispatcher.CreatedWindowHandle = default;

            UpdateThemeActive();
            UpdateCompositionEnabled();

            if (options.Visible)
            {
                ShowWindow(HWnd, SW.SW_SHOWDEFAULT);
                InvalidateRect(HWnd, null, false);
                UpdateWindow(HWnd);
            }
        }
    }

    internal LRESULT WindowProc(HWND hWnd, uint message, WPARAM wParam, LPARAM lParam)
    {
        // Process potential screen changes
        if (Dispatcher.TryHandleScreenChanges(hWnd, message, wParam, lParam))
        {
            Dispatcher.OnSystemEvent(SystemEventKind.ScreenChanged);
        }
        
        if ((message >= WM_MOUSEFIRST && message <= WM_MOUSELAST) || message == WM_MOUSELEAVE)
        {
            return HandleMouse(hWnd, message, wParam, lParam);
        }

        nint result;
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
                UpdateCompositionEnabled();
                result = 0;
                break;

            case WM_THEMECHANGED:
                UpdateThemeActive();
                result = 0;
                break;
        }

        return result;
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
        if (_resizeable)
        {
            if (!_minimumSize.IsEmpty)
            {
                *((Size*)&info->ptMinTrackSize) = WindowHelper.LogicalToPixel(_minimumSize, CurrentDpi);

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
                    Size virtualScreen = WindowHelper.LogicalToPixel(screen.Size, screen.Dpi);
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
                var size = WindowHelper.LogicalToPixel(_maximumSize, CurrentDpi);
                // TODO: Max on SystemInformation.MinWindowTrackSize
                *((Size*)&info->ptMaxTrackSize) = size;
            }

            // TODO: set MaxSize/MaxPosition
        }
        else
        {
            // Force the size
            var sizeInPixel = WindowHelper.LogicalToPixel(_size, CurrentDpi);

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

    private void UpdateCompositionEnabled()
    {
        BOOL enabled = FALSE;
        DwmIsCompositionEnabled(&enabled);
        _isCompositionEnabled = enabled;

        if (_isCompositionEnabled && !_hasDecorations) {
            // The window needs a frame to show a shadow, so give it the smallest amount of frame possible
            MARGINS margins = default;
            margins.cyTopHeight = 1;
            DwmExtendFrameIntoClientArea(HWnd, &margins);
            DWMNCRENDERINGPOLICY attributes = DWMNCRENDERINGPOLICY.DWMNCRP_ENABLED;
            DwmSetWindowAttribute(HWnd, (uint)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, &attributes, sizeof(DWMNCRENDERINGPOLICY));
        }
        // TODO
        // update_region(data);
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
                if (rect.Equals(mi.rcMonitor)) {
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
        if (IsMaximized(HWnd))
        {
            return HTCLIENT;
        }

        // Mouse with screen coordinates
        POINT ptMouse = new POINT(){ x = GET_X_LPARAM(lParam), y = GET_Y_LPARAM(lParam) };

        // Get the window rectangle.
        RECT rcWindow;
        GetWindowRect(hWnd, &rcWindow);

        int frameSize = GetSystemMetrics(SM.SM_CXFRAME) +
                         GetSystemMetrics(SM.SM_CXPADDEDBORDER);

        // TODO: Handle CAPTION

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

            // In a borderless Delegate the detection of the bar to the event handler
            var barEvent = new WindowEvent(WindowEventKind.BarHitTest);

            barEvent.BarHitTest.WindowSize = WindowHelper.PixelToLogical(new Size(rcWindow.right - rcWindow.left, rcWindow.bottom - rcWindow.top), CurrentDpi);
            barEvent.BarHitTest.MousePosition = ScreenToClient(new Point(ptMouse.x, ptMouse.y));

            OnWindowEvent(ref barEvent);
            if (barEvent.BarHitTest.Handled)
            {
                switch (barEvent.BarHitTest.Result)
                {
                    case BarHitTest.None:
                        break;
                    case BarHitTest.Menu:
                        return HTMENU;
                    case BarHitTest.Help:
                        return HTHELP;
                    case BarHitTest.Caption:
                        return HTCAPTION;
                    case BarHitTest.MinimizeButton:
                        return HTMINBUTTON;
                    case BarHitTest.MaximizeButton:
                        return HTMAXBUTTON;
                    case BarHitTest.CloseButton:
                        return HTCLOSE;
                }
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
            FillRect(hdc, &rect, (HBRUSH)(COLOR.COLOR_WINDOW + 1));
            EndPaint(HWnd, &ps);
        }
    }

    private void ToRectangleLogical(RECT* rect, out RectangleF rectF)
    {
        var screen = GetScreen();
        if (screen == null)
        {
            rectF = default;
            return;
        }
        var dpi = screen.Dpi;
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

        var screen = GetScreen();

        SizeF size;
        if (screen is { })
        {
            var dpi = screen.Dpi;
            size = new SizeF(WindowHelper.PixelToLogical(pos->cx, dpi.X), WindowHelper.PixelToLogical(pos->cy, dpi.Y));
        }
        else
        {
            size = new Size(pos->cx, pos->cy);
        }

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
        var localEvent = new WindowEvent(WindowEventKind.Mouse);
        ref var mouse = ref localEvent.Cast<MouseEvent>();

        SetMouseButtonStates(wParam, ref mouse);

        var pixelPositionX = GET_X_LPARAM(lParam);
        var pixelPositionY = GET_Y_LPARAM(lParam);

        mouse.Position = WindowHelper.PixelToLogical(new Point(pixelPositionX, pixelPositionY), CurrentDpi);

        if ((message == WM_MOUSEMOVE) && !_mouseTracked)
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
                mouse.SubKind = MouseEventKind.Enter;
                OnWindowEvent(ref localEvent);
            }
            _mouseLastX = -1.0f;
            _mouseLastY = -1.0f;
            return 0;
        }

        if (message == WM_MOUSELEAVE)
        {
            _mouseTracked = false;
            // Send a mouse leave
            mouse.SubKind = MouseEventKind.Leave;
            mouse.Position.X = _mouseLastX;
            mouse.Position.Y = _mouseLastY;
            OnWindowEvent(ref localEvent);
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
                mouse.SubKind = MouseEventKind.Move;
                break;
            case WM_LBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = MouseButtonFlags.LeftButton;
                mouse.SubKind = MouseEventKind.ButtonDown;
                break;
            case WM_LBUTTONUP:
                EndCaptureMouse();
                mouse.Button = MouseButtonFlags.LeftButton;
                mouse.SubKind = MouseEventKind.ButtonUp;
                break;
            case WM_LBUTTONDBLCLK:
                mouse.Button = MouseButtonFlags.LeftButton;
                mouse.SubKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_RBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = MouseButtonFlags.RightButton;
                mouse.SubKind = MouseEventKind.ButtonDown;
                break;
            case WM_RBUTTONUP:
                EndCaptureMouse();
                mouse.Button = MouseButtonFlags.RightButton;
                mouse.SubKind = MouseEventKind.ButtonUp;
                break;
            case WM_RBUTTONDBLCLK:
                mouse.Button = MouseButtonFlags.RightButton;
                mouse.SubKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_MBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = MouseButtonFlags.MiddleButton;
                mouse.SubKind = MouseEventKind.ButtonDown;
                break;
            case WM_MBUTTONUP:
                EndCaptureMouse();
                mouse.Button = MouseButtonFlags.MiddleButton;
                mouse.SubKind = MouseEventKind.ButtonUp;
                break;
            case WM_MBUTTONDBLCLK:
                mouse.Button = MouseButtonFlags.MiddleButton;
                mouse.SubKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_XBUTTONDOWN:
                BeginCaptureMouse();
                mouse.Button = HIWORD(wParam) == XBUTTON1 ? MouseButtonFlags.Button1 : MouseButtonFlags.Button2;
                mouse.SubKind = MouseEventKind.ButtonDown;
                break;
            case WM_XBUTTONUP:
                EndCaptureMouse();
                mouse.Button = HIWORD(wParam) == XBUTTON1 ? MouseButtonFlags.Button1 : MouseButtonFlags.Button2;
                mouse.SubKind = MouseEventKind.ButtonUp;
                break;
            case WM_XBUTTONDBLCLK:
                mouse.Button = HIWORD(wParam) == XBUTTON1 ? MouseButtonFlags.Button1 : MouseButtonFlags.Button2;
                mouse.SubKind = MouseEventKind.ButtonDoubleClick;
                break;
            case WM_MOUSEWHEEL:
            case WM_MOUSEHWHEEL:
                mouse.SubKind = MouseEventKind.Wheel;

                // Mouse wheel mouse coords are relative to screen, so convert them back to the client area
                mouse.Position = ScreenToClient(new Point(GET_X_LPARAM(lParam), GET_Y_LPARAM(lParam)));
                mouse.WheelDelta = new Point(0, GET_WHEEL_DELTA_WPARAM(wParam) / WHEEL_DELTA);
                break;
        }

        // Notify the mouse event
        _mouseLastX = mouse.Position.X;
        _mouseLastY = mouse.Position.Y;
        OnWindowEvent(ref localEvent);

        return 0;

        static void SetMouseButtonStates(WPARAM wParam, ref MouseEvent evt)
        {
            // Gets the buttons clicked
            if ((wParam & MK_LBUTTON) != 0)
                evt.Pressed |= MouseButtonFlags.LeftButton;
            if ((wParam & MK_MBUTTON) != 0)
                evt.Pressed |= MouseButtonFlags.MiddleButton;
            if ((wParam & MK_RBUTTON) != 0)
                evt.Pressed |= MouseButtonFlags.RightButton;
            if ((wParam & MK_XBUTTON1) != 0)
                evt.Pressed |= MouseButtonFlags.Button1;
            if ((wParam & MK_XBUTTON2) != 0)
                evt.Pressed |= MouseButtonFlags.Button2;
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

    private void UpdateSize(SizeF value)
    {
        var screenSize = WindowHelper.LogicalToPixel(value, CurrentDpi);
        SetWindowPos(HWnd, HWND.NULL, 0, 0, screenSize.Width, screenSize.Height, SWP.SWP_NOREPOSITION | SWP.SWP_NOACTIVATE | SWP.SWP_NOZORDER);
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

    private void UpdateResizeable(bool value)
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
        InvalidateRect(HWnd, null, false);
        UpdateWindow(HWnd);

        _resizeable = value;
        OnFrameEvent(FrameEventKind.ResizeableChanged);
    }
    
    private void UpdateWindowState(WindowState state)
    {
        switch (state)
        {
            case WindowState.Normal:
                ShowWindow(HWnd, SW.SW_NORMAL);
                break;
            case WindowState.Minimized:
                ShowWindow(HWnd, SW.SW_MINIMIZE);
                break;
            case WindowState.Maximized:
                if (_maximumSize.IsEmpty)
                {
                    ShowWindow(HWnd, SW.SW_MAXIMIZE);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, "Invalid WindowState");
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
            exStyle |= ((uint)WS_EX_LAYERED);
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
            exStyle &= ~((uint)WS_EX_LAYERED);
            SetWindowLongPtr(HWnd, GWL.GWL_EXSTYLE, (nint)exStyle);
            InvalidateRect(HWnd, null, false);
            UpdateWindow(HWnd);
        }

        _opacity = value;
        OnFrameEvent(FrameEventKind.OpacityChanged);
    }

    private void UpdateTopMost(bool value)
    {
        SetWindowPos(HWnd, value ? HWND.HWND_TOPMOST : HWND.HWND_NOTOPMOST, 0, 0, 0, 0, SWP.SWP_NOMOVE | SWP.SWP_NOSIZE);
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

    private static (uint, uint) GetStyleAndStyleExFromOptions(in WindowCreateOptions options)
    {
        uint style = WS_CLIPSIBLINGS | WS_CLIPCHILDREN;
        //uint styleEx = WS_EX_WINDOWEDGE | WS_EX_ACCEPTFILES;
        // TODO: handle popup
        uint styleEx = WS_EX_ACCEPTFILES; // | WS_EX_LAYERED;

        if (options.Resizable)
        {
            style |= WS_SIZEBOX;
        }

        if (options.Kind == WindowKind.TopLevel)
        {
            styleEx |= WS_EX_APPWINDOW;
        }
        else if (options.Kind == WindowKind.Popup)
        {
            style |= WS_POPUP;
            //styleEx |= WS_EX_WINDOWEDGE;
        }

        style |= WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU;

        if (options.Maximizable && !options.MaximumSize.HasValue)
        {
            style |= WS_MAXIMIZEBOX;
        }

        if (options.Minimizable)
        {
            style |= WS_MINIMIZEBOX;
        }

        if (options.Maximized && !options.MaximumSize.HasValue)
        {
            style |= WS_MAXIMIZE;
        }
        else if (options.Minimized)
        {
            style |= WS_MINIMIZE;
        }

        if (options.Transparent)
        {
            styleEx |= WS_EX_TRANSPARENT;
        }

        return (style, styleEx);
    }
}
