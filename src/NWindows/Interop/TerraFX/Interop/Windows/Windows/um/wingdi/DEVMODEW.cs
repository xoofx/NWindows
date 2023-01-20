// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/wingdi.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;
internal unsafe partial struct DEVMODEW
{
    public fixed ushort dmDeviceName[32];
    public ushort dmSpecVersion;
    public ushort dmDriverVersion;
    public ushort dmSize;
    public ushort dmDriverExtra;
    public uint dmFields;
    public _Anonymous1_e__Union Anonymous1;

    /// <include file='DEVMODEW.xml' path='doc/member[@name="DEVMODEW.dmColor"]/*' />
    public short dmColor;

    /// <include file='DEVMODEW.xml' path='doc/member[@name="DEVMODEW.dmDuplex"]/*' />
    public short dmDuplex;

    /// <include file='DEVMODEW.xml' path='doc/member[@name="DEVMODEW.dmYResolution"]/*' />
    public short dmYResolution;

    /// <include file='DEVMODEW.xml' path='doc/member[@name="DEVMODEW.dmTTOption"]/*' />
    public short dmTTOption;

    /// <include file='DEVMODEW.xml' path='doc/member[@name="DEVMODEW.dmCollate"]/*' />
    public short dmCollate;
    public fixed ushort dmFormName[32];
    public ushort dmLogPixels;
    public uint dmBitsPerPel;
    public uint dmPelsWidth;
    public uint dmPelsHeight;
    public _Anonymous2_e__Union Anonymous2;
    public uint dmDisplayFrequency;
    public uint dmICMMethod;
    public uint dmICMIntent;
    public uint dmMediaType;
    public uint dmDitherType;
    public uint dmReserved1;
    public uint dmReserved2;
    public uint dmPanningWidth;
    public uint dmPanningHeight;

    /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmOrientation"]/*' />
    [UnscopedRef]
    public ref short dmOrientation
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous1.Anonymous1.dmOrientation;
        }
    }

    /// <include file='_Anonymous1_e__Union.xml' path='doc/member[@name="_Anonymous1_e__Union"]/*' />
    [StructLayout(LayoutKind.Explicit)]
internal partial struct _Anonymous1_e__Union
    {
        /// <include file='_Anonymous1_e__Union.xml' path='doc/member[@name="_Anonymous1_e__Union.Anonymous1"]/*' />
        [FieldOffset(0)]
        public _Anonymous1_e__Struct Anonymous1;

        /// <include file='_Anonymous1_e__Union.xml' path='doc/member[@name="_Anonymous1_e__Union.Anonymous2"]/*' />
        [FieldOffset(0)]
        public _Anonymous2_e__Struct Anonymous2;

        /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct"]/*' />
        public partial struct _Anonymous1_e__Struct
        {
            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmOrientation"]/*' />
            public short dmOrientation;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmPaperSize"]/*' />
            public short dmPaperSize;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmPaperLength"]/*' />
            public short dmPaperLength;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmPaperWidth"]/*' />
            public short dmPaperWidth;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmScale"]/*' />
            public short dmScale;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmCopies"]/*' />
            public short dmCopies;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmDefaultSource"]/*' />
            public short dmDefaultSource;

            /// <include file='_Anonymous1_e__Struct.xml' path='doc/member[@name="_Anonymous1_e__Struct.dmPrintQuality"]/*' />
            public short dmPrintQuality;
        }
internal partial struct _Anonymous2_e__Struct
        {
            /// <include file='_Anonymous2_e__Struct.xml' path='doc/member[@name="_Anonymous2_e__Struct.dmPosition"]/*' />
            public POINTL dmPosition;
            public uint dmDisplayOrientation;
            public uint dmDisplayFixedOutput;
        }
    }

    /// <include file='_Anonymous2_e__Union.xml' path='doc/member[@name="_Anonymous2_e__Union"]/*' />
    [StructLayout(LayoutKind.Explicit)]
internal partial struct _Anonymous2_e__Union
    {
        /// <include file='_Anonymous2_e__Union.xml' path='doc/member[@name="_Anonymous2_e__Union.dmDisplayFlags"]/*' />
        [FieldOffset(0)]
        public uint dmDisplayFlags;

        /// <include file='_Anonymous2_e__Union.xml' path='doc/member[@name="_Anonymous2_e__Union.dmNup"]/*' />
        [FieldOffset(0)]
        public uint dmNup;
    }
}
