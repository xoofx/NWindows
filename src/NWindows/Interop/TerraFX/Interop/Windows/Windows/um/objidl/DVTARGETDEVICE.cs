// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/objidl.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

namespace TerraFX.Interop.Windows;
internal unsafe partial struct DVTARGETDEVICE
{
    public uint tdSize;
    public ushort tdDriverNameOffset;
    public ushort tdDeviceNameOffset;
    public ushort tdPortNameOffset;
    public ushort tdExtDevmodeOffset;
    public fixed byte tdData[1];
}
