// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/WinUser.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal partial struct MONITORINFO
{
    public uint cbSize;

    /// <include file='MONITORINFO.xml' path='doc/member[@name="MONITORINFO.rcMonitor"]/*' />
    public RECT rcMonitor;

    /// <include file='MONITORINFO.xml' path='doc/member[@name="MONITORINFO.rcWork"]/*' />
    public RECT rcWork;
    public uint dwFlags;
}
