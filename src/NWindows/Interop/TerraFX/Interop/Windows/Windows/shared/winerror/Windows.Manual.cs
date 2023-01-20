
namespace TerraFX.Interop.Windows;
internal static partial class Windows
{
    public static bool SUCCEEDED(HRESULT hr)
    {
        return hr >= 0;
    }

    public static bool FAILED(HRESULT hr)
    {
        return hr < 0;
    }
}
