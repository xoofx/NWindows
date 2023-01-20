// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/objidl.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal unsafe partial struct FORMATETC
{
    public ushort cfFormat;

    /// <include file='FORMATETC.xml' path='doc/member[@name="FORMATETC.ptd"]/*' />
    public DVTARGETDEVICE* ptd;
    public uint dwAspect;
    public int lindex;
    public uint tymed;
}
