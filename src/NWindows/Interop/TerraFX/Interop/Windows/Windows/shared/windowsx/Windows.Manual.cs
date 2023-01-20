using static TerraFX.Interop.Windows.GWL;
using static TerraFX.Interop.Windows.GWLP;
using static TerraFX.Interop.Windows.SW;
using static TerraFX.Interop.Windows.WM;
using static TerraFX.Interop.Windows.WS;
using static TerraFX.Interop.Windows.VK;

namespace TerraFX.Interop.Windows;
internal static unsafe partial class Windows
{

    public static BOOL DeleteBitmap(HBITMAP hbm) => DeleteObject((HGDIOBJ)(HBITMAP)(hbm));

    public static uint GetWindowStyle(HWND hwnd) => ((uint)GetWindowLong(hwnd, GWL_STYLE));

    public static uint GetWindowExStyle(HWND hwnd) => ((uint)GetWindowLong(hwnd, GWL_EXSTYLE));

    public static BOOL IsMaximized(HWND hwnd) => IsZoomed(hwnd);

    public static int GET_X_LPARAM(LPARAM lp) => ((int)(short)LOWORD(lp));

    public static int GET_Y_LPARAM(LPARAM lp) => ((int)(short)HIWORD(lp));
}
