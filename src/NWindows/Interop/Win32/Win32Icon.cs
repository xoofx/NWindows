// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using static TerraFX.Interop.Windows.IDI;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Interop.Win32;

using TerraFX.Interop.Windows;

internal unsafe class Win32Icon : IconImpl
{
    public override Icon GetApplicationIcon()
    {
        var icon = GetApplicationIcon(new Size(64, 64)) ?? (GetApplicationIcon(new Size(32, 32)) ?? GetApplicationIcon(new Size(16, 16)));
        if (icon == null)
        {
            throw new InvalidOperationException("Unable to retrieve application icon");
        }
        return icon;
    }

    public static HICON CreateIcon(Icon icon)
    {
        var largeBitmap = CreateNativeBitmap(icon);
        return CreateNativeIcon(largeBitmap, icon.Size);
    }
    
    private static HBITMAP CreateNativeBitmap(Icon image)
    {

        var sizeInBytes = image.Buffer.Length * sizeof(Icon.Rgba32);
        var ptr = NativeMemory.Alloc((nuint)sizeInBytes);
        if (ptr == null) throw new OutOfMemoryException($"Unable to allocate native memory for icon (requested size: {sizeInBytes} bytes)");

        var span = new Span<Icon.Rgba32>(ptr, image.Buffer.Length);
        image.Buffer.CopyTo(span);
        SwapBgra(span);

        fixed (void* pBuffer = span)
        {
            var bitmap = CreateBitmap(image.Width, image.Height, 1, 32, pBuffer);
            NativeMemory.Free(ptr);
            return bitmap;
        }
    }

    private static HICON CreateNativeIcon(HBITMAP image, Size size)
    {
        if (image == HBITMAP.NULL) return HICON.NULL;

        ICONINFO iconInfo;
        iconInfo.xHotspot = (uint)(size.Width / 2);
        iconInfo.yHotspot = (uint)(size.Height / 2);
        iconInfo.fIcon = true;

        iconInfo.hbmMask = CreateBitmap(size.Width, size.Height, 1, 1, null);
        var hIcon = HICON.NULL;
        if (iconInfo.hbmMask != HBITMAP.NULL)
        {
            iconInfo.hbmColor = image;
            hIcon = CreateIconIndirect(&iconInfo);
            DeleteBitmap(iconInfo.hbmMask);
        }
        DeleteBitmap(image);

        return hIcon;
    }
    
    private static Icon? GetApplicationIcon(Size size)
    {
        var icon = (HICON)LoadImageW(Win32Helper.ModuleHandle, IDI_APPLICATION, IMAGE.IMAGE_ICON, size.Width, size.Height, LR.LR_SHARED);
        if (icon == HICON.NULL) return null;

        ICONINFO iconInfo;
        if (!GetIconInfo(icon, &iconInfo)) return null;

        var hbitmap = iconInfo.hbmColor;
        if (hbitmap == HBITMAP.NULL) return null;

        BITMAP bitmap;
        if (GetObject(hbitmap, sizeof(BITMAP), &bitmap) == 0) return null;

        var width = bitmap.bmWidth;
        var height = bitmap.bmHeight;

        if (bitmap.bmBitsPixel != 32) return null;

        var image = new Icon(width, height);

        fixed (Icon.Rgba32* pBuffer = image.Buffer)
        {
            var sizeInBytes = width * height * sizeof(uint);
            var copied = GetBitmapBits(hbitmap, sizeInBytes, pBuffer);
            if (sizeInBytes != copied) return null;
        }

        // Convert from BGRA to RGBA
        SwapBgra(image.Buffer);
        return image;
    }
    
    private static void SwapBgra(Span<Icon.Rgba32> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            var color = buffer[i];
            // Swap R & B color
            buffer[i] = new(color.B, color.G, color.R, color.A);
        }
    }

    private static Size GetBigIconSize() => new Size(GetSystemMetrics(SM.SM_CXICON), GetSystemMetrics(SM.SM_CYICON));

    private static Size GetSmallIconSize() => new Size(GetSystemMetrics(SM.SM_CXSMICON), GetSystemMetrics(SM.SM_CYSMICON));
}
