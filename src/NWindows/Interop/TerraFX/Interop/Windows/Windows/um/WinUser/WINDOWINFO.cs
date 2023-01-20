// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/WinUser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal partial struct WINDOWINFO
{
    public uint cbSize;

    /// <include file='WINDOWINFO.xml' path='doc/member[@name="WINDOWINFO.rcWindow"]/*' />
    public RECT rcWindow;

    /// <include file='WINDOWINFO.xml' path='doc/member[@name="WINDOWINFO.rcClient"]/*' />
    public RECT rcClient;
    public uint dwStyle;
    public uint dwExStyle;
    public uint dwWindowStatus;

    /// <include file='WINDOWINFO.xml' path='doc/member[@name="WINDOWINFO.cxWindowBorders"]/*' />
    public uint cxWindowBorders;

    /// <include file='WINDOWINFO.xml' path='doc/member[@name="WINDOWINFO.cyWindowBorders"]/*' />
    public uint cyWindowBorders;
    public ushort atomWindowType;
    public ushort wCreatorVersion;
}
