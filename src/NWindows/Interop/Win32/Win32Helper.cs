// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System.Drawing;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Interop.Win32;


internal readonly record struct WndMsg(HWND HWnd, uint Message, WPARAM WParam, LPARAM LParam)
{
    public override string ToString()
    {
        return $"HWND: 0x{(nint)HWnd:X16} Message: {Win32Helper.GetMessageName(Message)} WPARAM: 0x{WParam.Value:x16} LPARAM: 0x{LParam.Value:x16}";
    }
}

internal static class Win32VirtualKeys
{
}

internal static class Win32Helper
{
    public static Color ToColor(COLORREF colorRef)
    {
        return Color.FromArgb(GetRValue(colorRef), GetGValue(colorRef), GetBValue(colorRef));
    }

    public static COLORREF FromColor(Color color)
    {
        return RGB(color.R, color.G, color.B);
    }
    
    public static unsafe int GetDpiForWindowSafe(HWND hWnd)
    {
        // Try to get the Dpi from the window, or from the monitor or from the system
        var dpiX = Windows.GetDpiForWindow(hWnd);
        if (dpiX == 0)
        {
            var monitor = MonitorFromWindow(hWnd, MONITOR.MONITOR_DEFAULTTONEAREST);
            if (monitor != 0)
            {
                if (GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, &dpiX, &dpiX).SUCCEEDED && dpiX != 0)
                {
                    return (int)dpiX;
                }
            }
            dpiX = GetDpiForSystem();
        }

        return (int)dpiX;
    }

    public static unsafe bool TryGetPositionSizeDpiAndRECT(HWND hWnd, out (Point, SizeF, int, RECT) bounds)
    {
        RECT rect;
        if (GetWindowRect(hWnd, &rect))
        {
            var dpi = GetDpiForWindowSafe(hWnd);
            var position = new Point(rect.left, rect.top);
            var widthInPixel = rect.right - rect.left;
            var heightInPixel = rect.bottom - rect.top;
            if (widthInPixel != 0 || heightInPixel != 0)
            {
                var size = new SizeF(WindowHelper.PixelToLogical(widthInPixel, dpi), WindowHelper.PixelToLogical(heightInPixel, dpi));
                bounds = (position, size, dpi, rect);
                return true;
            }
        }

        bounds = default;
        return false;
    }

    public static string GetMessageName(uint message)
    {
        return message switch
        {
            WM_CTLCOLOR => nameof(WM_CTLCOLOR),
            WM_CPL_LAUNCH => nameof(WM_CPL_LAUNCH),
            WM_CPL_LAUNCHED => nameof(WM_CPL_LAUNCHED),
            WM_TABLET_ADDED => nameof(WM_TABLET_ADDED),
            WM_TABLET_DELETED => nameof(WM_TABLET_DELETED),
            WM_TABLET_FLICK => nameof(WM_TABLET_FLICK),
            WM_TABLET_QUERYSYSTEMGESTURESTATUS => nameof(WM_TABLET_QUERYSYSTEMGESTURESTATUS),
            WM_NULL => nameof(WM_NULL),
            WM_CREATE => nameof(WM_CREATE),
            WM_DESTROY => nameof(WM_DESTROY),
            WM_MOVE => nameof(WM_MOVE),
            WM_SIZE => nameof(WM_SIZE),
            WM_ACTIVATE => nameof(WM_ACTIVATE),
            WM_SETFOCUS => nameof(WM_SETFOCUS),
            WM_KILLFOCUS => nameof(WM_KILLFOCUS),
            WM_ENABLE => nameof(WM_ENABLE),
            WM_SETREDRAW => nameof(WM_SETREDRAW),
            WM_SETTEXT => nameof(WM_SETTEXT),
            WM_GETTEXT => nameof(WM_GETTEXT),
            WM_GETTEXTLENGTH => nameof(WM_GETTEXTLENGTH),
            WM_PAINT => nameof(WM_PAINT),
            WM_CLOSE => nameof(WM_CLOSE),
            WM_QUERYENDSESSION => nameof(WM_QUERYENDSESSION),
            WM_QUERYOPEN => nameof(WM_QUERYOPEN),
            WM_ENDSESSION => nameof(WM_ENDSESSION),
            WM_QUIT => nameof(WM_QUIT),
            WM_ERASEBKGND => nameof(WM_ERASEBKGND),
            WM_SYSCOLORCHANGE => nameof(WM_SYSCOLORCHANGE),
            WM_SHOWWINDOW => nameof(WM_SHOWWINDOW),
            WM_SETTINGCHANGE => nameof(WM_SETTINGCHANGE),
            WM_DEVMODECHANGE => nameof(WM_DEVMODECHANGE),
            WM_ACTIVATEAPP => nameof(WM_ACTIVATEAPP),
            WM_FONTCHANGE => nameof(WM_FONTCHANGE),
            WM_TIMECHANGE => nameof(WM_TIMECHANGE),
            WM_CANCELMODE => nameof(WM_CANCELMODE),
            WM_SETCURSOR => nameof(WM_SETCURSOR),
            WM_MOUSEACTIVATE => nameof(WM_MOUSEACTIVATE),
            WM_CHILDACTIVATE => nameof(WM_CHILDACTIVATE),
            WM_QUEUESYNC => nameof(WM_QUEUESYNC),
            WM_GETMINMAXINFO => nameof(WM_GETMINMAXINFO),
            WM_PAINTICON => nameof(WM_PAINTICON),
            WM_ICONERASEBKGND => nameof(WM_ICONERASEBKGND),
            WM_NEXTDLGCTL => nameof(WM_NEXTDLGCTL),
            WM_SPOOLERSTATUS => nameof(WM_SPOOLERSTATUS),
            WM_DRAWITEM => nameof(WM_DRAWITEM),
            WM_MEASUREITEM => nameof(WM_MEASUREITEM),
            WM_DELETEITEM => nameof(WM_DELETEITEM),
            WM_VKEYTOITEM => nameof(WM_VKEYTOITEM),
            WM_CHARTOITEM => nameof(WM_CHARTOITEM),
            WM_SETFONT => nameof(WM_SETFONT),
            WM_GETFONT => nameof(WM_GETFONT),
            WM_SETHOTKEY => nameof(WM_SETHOTKEY),
            WM_GETHOTKEY => nameof(WM_GETHOTKEY),
            WM_QUERYDRAGICON => nameof(WM_QUERYDRAGICON),
            WM_COMPAREITEM => nameof(WM_COMPAREITEM),
            WM_GETOBJECT => nameof(WM_GETOBJECT),
            WM_COMPACTING => nameof(WM_COMPACTING),
            WM_COMMNOTIFY => nameof(WM_COMMNOTIFY),
            WM_WINDOWPOSCHANGING => nameof(WM_WINDOWPOSCHANGING),
            WM_WINDOWPOSCHANGED => nameof(WM_WINDOWPOSCHANGED),
            WM_POWER => nameof(WM_POWER),
            WM_COPYDATA => nameof(WM_COPYDATA),
            WM_CANCELJOURNAL => nameof(WM_CANCELJOURNAL),
            WM_NOTIFY => nameof(WM_NOTIFY),
            WM_INPUTLANGCHANGEREQUEST => nameof(WM_INPUTLANGCHANGEREQUEST),
            WM_INPUTLANGCHANGE => nameof(WM_INPUTLANGCHANGE),
            WM_TCARD => nameof(WM_TCARD),
            WM_HELP => nameof(WM_HELP),
            WM_USERCHANGED => nameof(WM_USERCHANGED),
            WM_NOTIFYFORMAT => nameof(WM_NOTIFYFORMAT),
            WM_CONTEXTMENU => nameof(WM_CONTEXTMENU),
            WM_STYLECHANGING => nameof(WM_STYLECHANGING),
            WM_STYLECHANGED => nameof(WM_STYLECHANGED),
            WM_DISPLAYCHANGE => nameof(WM_DISPLAYCHANGE),
            WM_GETICON => nameof(WM_GETICON),
            WM_SETICON => nameof(WM_SETICON),
            WM_NCCREATE => nameof(WM_NCCREATE),
            WM_NCDESTROY => nameof(WM_NCDESTROY),
            WM_NCCALCSIZE => nameof(WM_NCCALCSIZE),
            WM_NCHITTEST => nameof(WM_NCHITTEST),
            WM_NCPAINT => nameof(WM_NCPAINT),
            WM_NCACTIVATE => nameof(WM_NCACTIVATE),
            WM_GETDLGCODE => nameof(WM_GETDLGCODE),
            WM_SYNCPAINT => nameof(WM_SYNCPAINT),
            WM_NCMOUSEMOVE => nameof(WM_NCMOUSEMOVE),
            WM_NCLBUTTONDOWN => nameof(WM_NCLBUTTONDOWN),
            WM_NCLBUTTONUP => nameof(WM_NCLBUTTONUP),
            WM_NCLBUTTONDBLCLK => nameof(WM_NCLBUTTONDBLCLK),
            WM_NCRBUTTONDOWN => nameof(WM_NCRBUTTONDOWN),
            WM_NCRBUTTONUP => nameof(WM_NCRBUTTONUP),
            WM_NCRBUTTONDBLCLK => nameof(WM_NCRBUTTONDBLCLK),
            WM_NCMBUTTONDOWN => nameof(WM_NCMBUTTONDOWN),
            WM_NCMBUTTONUP => nameof(WM_NCMBUTTONUP),
            WM_NCMBUTTONDBLCLK => nameof(WM_NCMBUTTONDBLCLK),
            WM_NCXBUTTONDOWN => nameof(WM_NCXBUTTONDOWN),
            WM_NCXBUTTONUP => nameof(WM_NCXBUTTONUP),
            WM_NCXBUTTONDBLCLK => nameof(WM_NCXBUTTONDBLCLK),
            WM_INPUT_DEVICE_CHANGE => nameof(WM_INPUT_DEVICE_CHANGE),
            WM_INPUT => nameof(WM_INPUT),
            WM_KEYDOWN => nameof(WM_KEYDOWN), // WM_KEYFIRST => nameof(WM_KEYFIRST),
            WM_KEYUP => nameof(WM_KEYUP),
            WM_CHAR => nameof(WM_CHAR),
            WM_DEADCHAR => nameof(WM_DEADCHAR),
            WM_SYSKEYDOWN => nameof(WM_SYSKEYDOWN),
            WM_SYSKEYUP => nameof(WM_SYSKEYUP),
            WM_SYSCHAR => nameof(WM_SYSCHAR),
            WM_SYSDEADCHAR => nameof(WM_SYSDEADCHAR),
            WM_UNICHAR => nameof(WM_UNICHAR), // WM_KEYLAST => nameof(WM_KEYLAST),
            WM_IME_STARTCOMPOSITION => nameof(WM_IME_STARTCOMPOSITION),
            WM_IME_ENDCOMPOSITION => nameof(WM_IME_ENDCOMPOSITION),
            WM_IME_COMPOSITION => nameof(WM_IME_COMPOSITION), // WM_IME_KEYLAST => nameof(WM_IME_KEYLAST),
            WM_INITDIALOG => nameof(WM_INITDIALOG),
            WM_COMMAND => nameof(WM_COMMAND),
            WM_SYSCOMMAND => nameof(WM_SYSCOMMAND),
            WM_TIMER => nameof(WM_TIMER),
            WM_HSCROLL => nameof(WM_HSCROLL),
            WM_VSCROLL => nameof(WM_VSCROLL),
            WM_INITMENU => nameof(WM_INITMENU),
            WM_INITMENUPOPUP => nameof(WM_INITMENUPOPUP),
            WM_GESTURE => nameof(WM_GESTURE),
            WM_GESTURENOTIFY => nameof(WM_GESTURENOTIFY),
            WM_MENUSELECT => nameof(WM_MENUSELECT),
            WM_MENUCHAR => nameof(WM_MENUCHAR),
            WM_ENTERIDLE => nameof(WM_ENTERIDLE),
            WM_MENURBUTTONUP => nameof(WM_MENURBUTTONUP),
            WM_MENUDRAG => nameof(WM_MENUDRAG),
            WM_MENUGETOBJECT => nameof(WM_MENUGETOBJECT),
            WM_UNINITMENUPOPUP => nameof(WM_UNINITMENUPOPUP),
            WM_MENUCOMMAND => nameof(WM_MENUCOMMAND),
            WM_CHANGEUISTATE => nameof(WM_CHANGEUISTATE),
            WM_UPDATEUISTATE => nameof(WM_UPDATEUISTATE),
            WM_QUERYUISTATE => nameof(WM_QUERYUISTATE),
            WM_CTLCOLORMSGBOX => nameof(WM_CTLCOLORMSGBOX),
            WM_CTLCOLOREDIT => nameof(WM_CTLCOLOREDIT),
            WM_CTLCOLORLISTBOX => nameof(WM_CTLCOLORLISTBOX),
            WM_CTLCOLORBTN => nameof(WM_CTLCOLORBTN),
            WM_CTLCOLORDLG => nameof(WM_CTLCOLORDLG),
            WM_CTLCOLORSCROLLBAR => nameof(WM_CTLCOLORSCROLLBAR),
            WM_CTLCOLORSTATIC => nameof(WM_CTLCOLORSTATIC),
            WM_MOUSEMOVE => nameof(WM_MOUSEMOVE), // WM_MOUSEFIRST => nameof(WM_MOUSEFIRST),
            WM_LBUTTONDOWN => nameof(WM_LBUTTONDOWN),
            WM_LBUTTONUP => nameof(WM_LBUTTONUP),
            WM_LBUTTONDBLCLK => nameof(WM_LBUTTONDBLCLK),
            WM_RBUTTONDOWN => nameof(WM_RBUTTONDOWN),
            WM_RBUTTONUP => nameof(WM_RBUTTONUP),
            WM_RBUTTONDBLCLK => nameof(WM_RBUTTONDBLCLK),
            WM_MBUTTONDOWN => nameof(WM_MBUTTONDOWN),
            WM_MBUTTONUP => nameof(WM_MBUTTONUP),
            WM_MBUTTONDBLCLK => nameof(WM_MBUTTONDBLCLK),
            WM_MOUSEWHEEL => nameof(WM_MOUSEWHEEL),
            WM_XBUTTONDOWN => nameof(WM_XBUTTONDOWN),
            WM_XBUTTONUP => nameof(WM_XBUTTONUP),
            WM_XBUTTONDBLCLK => nameof(WM_XBUTTONDBLCLK),
            WM_MOUSEHWHEEL => nameof(WM_MOUSEHWHEEL), // WM_MOUSELAST => nameof(WM_MOUSELAST),
            WM_PARENTNOTIFY => nameof(WM_PARENTNOTIFY),
            WM_ENTERMENULOOP => nameof(WM_ENTERMENULOOP),
            WM_EXITMENULOOP => nameof(WM_EXITMENULOOP),
            WM_NEXTMENU => nameof(WM_NEXTMENU),
            WM_SIZING => nameof(WM_SIZING),
            WM_CAPTURECHANGED => nameof(WM_CAPTURECHANGED),
            WM_MOVING => nameof(WM_MOVING),
            WM_POWERBROADCAST => nameof(WM_POWERBROADCAST),
            WM_DEVICECHANGE => nameof(WM_DEVICECHANGE),
            WM_MDICREATE => nameof(WM_MDICREATE),
            WM_MDIDESTROY => nameof(WM_MDIDESTROY),
            WM_MDIACTIVATE => nameof(WM_MDIACTIVATE),
            WM_MDIRESTORE => nameof(WM_MDIRESTORE),
            WM_MDINEXT => nameof(WM_MDINEXT),
            WM_MDIMAXIMIZE => nameof(WM_MDIMAXIMIZE),
            WM_MDITILE => nameof(WM_MDITILE),
            WM_MDICASCADE => nameof(WM_MDICASCADE),
            WM_MDIICONARRANGE => nameof(WM_MDIICONARRANGE),
            WM_MDIGETACTIVE => nameof(WM_MDIGETACTIVE),
            WM_MDISETMENU => nameof(WM_MDISETMENU),
            WM_ENTERSIZEMOVE => nameof(WM_ENTERSIZEMOVE),
            WM_EXITSIZEMOVE => nameof(WM_EXITSIZEMOVE),
            WM_DROPFILES => nameof(WM_DROPFILES),
            WM_MDIREFRESHMENU => nameof(WM_MDIREFRESHMENU),
            WM_POINTERDEVICECHANGE => nameof(WM_POINTERDEVICECHANGE),
            WM_POINTERDEVICEINRANGE => nameof(WM_POINTERDEVICEINRANGE),
            WM_POINTERDEVICEOUTOFRANGE => nameof(WM_POINTERDEVICEOUTOFRANGE),
            WM_TOUCH => nameof(WM_TOUCH),
            WM_NCPOINTERUPDATE => nameof(WM_NCPOINTERUPDATE),
            WM_NCPOINTERDOWN => nameof(WM_NCPOINTERDOWN),
            WM_NCPOINTERUP => nameof(WM_NCPOINTERUP),
            WM_POINTERUPDATE => nameof(WM_POINTERUPDATE),
            WM_POINTERDOWN => nameof(WM_POINTERDOWN),
            WM_POINTERUP => nameof(WM_POINTERUP),
            WM_POINTERENTER => nameof(WM_POINTERENTER),
            WM_POINTERLEAVE => nameof(WM_POINTERLEAVE),
            WM_POINTERACTIVATE => nameof(WM_POINTERACTIVATE),
            WM_POINTERCAPTURECHANGED => nameof(WM_POINTERCAPTURECHANGED),
            WM_TOUCHHITTESTING => nameof(WM_TOUCHHITTESTING),
            WM_POINTERWHEEL => nameof(WM_POINTERWHEEL),
            WM_POINTERHWHEEL => nameof(WM_POINTERHWHEEL),
            WM_POINTERROUTEDTO => nameof(WM_POINTERROUTEDTO),
            WM_POINTERROUTEDAWAY => nameof(WM_POINTERROUTEDAWAY),
            WM_POINTERROUTEDRELEASED => nameof(WM_POINTERROUTEDRELEASED),
            WM_IME_SETCONTEXT => nameof(WM_IME_SETCONTEXT),
            WM_IME_NOTIFY => nameof(WM_IME_NOTIFY),
            WM_IME_CONTROL => nameof(WM_IME_CONTROL),
            WM_IME_COMPOSITIONFULL => nameof(WM_IME_COMPOSITIONFULL),
            WM_IME_SELECT => nameof(WM_IME_SELECT),
            WM_IME_CHAR => nameof(WM_IME_CHAR),
            WM_IME_REQUEST => nameof(WM_IME_REQUEST),
            WM_IME_KEYDOWN => nameof(WM_IME_KEYDOWN),
            WM_IME_KEYUP => nameof(WM_IME_KEYUP),
            WM_MOUSEHOVER => nameof(WM_MOUSEHOVER),
            WM_MOUSELEAVE => nameof(WM_MOUSELEAVE),
            WM_NCMOUSEHOVER => nameof(WM_NCMOUSEHOVER),
            WM_NCMOUSELEAVE => nameof(WM_NCMOUSELEAVE),
            WM_WTSSESSION_CHANGE => nameof(WM_WTSSESSION_CHANGE),
            WM_TABLET_FIRST => nameof(WM_TABLET_FIRST),
            WM_TABLET_LAST => nameof(WM_TABLET_LAST),
            WM_DPICHANGED => nameof(WM_DPICHANGED),
            WM_DPICHANGED_BEFOREPARENT => nameof(WM_DPICHANGED_BEFOREPARENT),
            WM_DPICHANGED_AFTERPARENT => nameof(WM_DPICHANGED_AFTERPARENT),
            WM_GETDPISCALEDSIZE => nameof(WM_GETDPISCALEDSIZE),
            WM_CUT => nameof(WM_CUT),
            WM_COPY => nameof(WM_COPY),
            WM_PASTE => nameof(WM_PASTE),
            WM_CLEAR => nameof(WM_CLEAR),
            WM_UNDO => nameof(WM_UNDO),
            WM_RENDERFORMAT => nameof(WM_RENDERFORMAT),
            WM_RENDERALLFORMATS => nameof(WM_RENDERALLFORMATS),
            WM_DESTROYCLIPBOARD => nameof(WM_DESTROYCLIPBOARD),
            WM_DRAWCLIPBOARD => nameof(WM_DRAWCLIPBOARD),
            WM_PAINTCLIPBOARD => nameof(WM_PAINTCLIPBOARD),
            WM_VSCROLLCLIPBOARD => nameof(WM_VSCROLLCLIPBOARD),
            WM_SIZECLIPBOARD => nameof(WM_SIZECLIPBOARD),
            WM_ASKCBFORMATNAME => nameof(WM_ASKCBFORMATNAME),
            WM_CHANGECBCHAIN => nameof(WM_CHANGECBCHAIN),
            WM_HSCROLLCLIPBOARD => nameof(WM_HSCROLLCLIPBOARD),
            WM_QUERYNEWPALETTE => nameof(WM_QUERYNEWPALETTE),
            WM_PALETTEISCHANGING => nameof(WM_PALETTEISCHANGING),
            WM_PALETTECHANGED => nameof(WM_PALETTECHANGED),
            WM_HOTKEY => nameof(WM_HOTKEY),
            WM_PRINT => nameof(WM_PRINT),
            WM_PRINTCLIENT => nameof(WM_PRINTCLIENT),
            WM_APPCOMMAND => nameof(WM_APPCOMMAND),
            WM_THEMECHANGED => nameof(WM_THEMECHANGED),
            WM_CLIPBOARDUPDATE => nameof(WM_CLIPBOARDUPDATE),
            WM_DWMCOMPOSITIONCHANGED => nameof(WM_DWMCOMPOSITIONCHANGED),
            WM_DWMNCRENDERINGCHANGED => nameof(WM_DWMNCRENDERINGCHANGED),
            WM_DWMCOLORIZATIONCOLORCHANGED => nameof(WM_DWMCOLORIZATIONCOLORCHANGED),
            WM_DWMWINDOWMAXIMIZEDCHANGE => nameof(WM_DWMWINDOWMAXIMIZEDCHANGE),
            WM_DWMSENDICONICTHUMBNAIL => nameof(WM_DWMSENDICONICTHUMBNAIL),
            WM_DWMSENDICONICLIVEPREVIEWBITMAP => nameof(WM_DWMSENDICONICLIVEPREVIEWBITMAP),
            WM_GETTITLEBARINFOEX => nameof(WM_GETTITLEBARINFOEX),
            WM_HANDHELDFIRST => nameof(WM_HANDHELDFIRST),
            WM_HANDHELDLAST => nameof(WM_HANDHELDLAST),
            WM_AFXFIRST => nameof(WM_AFXFIRST),
            WM_AFXLAST => nameof(WM_AFXLAST),
            WM_PENWINFIRST => nameof(WM_PENWINFIRST),
            WM_PENWINLAST => nameof(WM_PENWINLAST),
            WM_APP => nameof(WM_APP),
            WM_USER => nameof(WM_USER),
            WM_TOOLTIPDISMISS => nameof(WM_TOOLTIPDISMISS),
            _ => GetMessageNameExtra(message)
        };
    }

    private static unsafe string GetMessageNameExtra(uint message)
    {
        const int capacity = 256;
        var localChars = stackalloc char[capacity];
        var length = GetClipboardFormatNameW(message, (ushort*)localChars, capacity);
        return length <= 0 ? message <= ushort.MaxValue ? $"0x{message:x8}" : $"0x{message:x16}" : new string(localChars, 0, length);
    }
}