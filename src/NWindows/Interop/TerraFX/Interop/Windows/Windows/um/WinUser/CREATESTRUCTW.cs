// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/WinUser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal unsafe partial struct CREATESTRUCTW
{
    public void* lpCreateParams;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.hInstance"]/*' />
    public HINSTANCE hInstance;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.hMenu"]/*' />
    public HMENU hMenu;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.hwndParent"]/*' />
    public HWND hwndParent;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.cy"]/*' />
    public int cy;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.cx"]/*' />
    public int cx;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.y"]/*' />
    public int y;

    /// <include file='CREATESTRUCTW.xml' path='doc/member[@name="CREATESTRUCTW.x"]/*' />
    public int x;
    public int style;
    public ushort* lpszName;
    public ushort* lpszClass;
    public uint dwExStyle;
}
