// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/objidl.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;
internal unsafe partial struct STGMEDIUM
{
    public uint tymed;
    public _Anonymous_e__Union Anonymous;

    /// <include file='STGMEDIUM.xml' path='doc/member[@name="STGMEDIUM.pUnkForRelease"]/*' />
    public IUnknown* pUnkForRelease;

    /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.hGlobal"]/*' />
    [UnscopedRef]
    public ref HGLOBAL hGlobal
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return ref Anonymous.hGlobal;
        }
    }

    /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union"]/*' />
    [StructLayout(LayoutKind.Explicit)]
internal unsafe partial struct _Anonymous_e__Union
    {
        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.hBitmap"]/*' />
        [FieldOffset(0)]
        public HBITMAP hBitmap;

        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.hMetaFilePict"]/*' />
        [FieldOffset(0)]
        public HMETAFILEPICT hMetaFilePict;

        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.hEnhMetaFile"]/*' />
        [FieldOffset(0)]
        public HENHMETAFILE hEnhMetaFile;

        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.hGlobal"]/*' />
        [FieldOffset(0)]
        public HGLOBAL hGlobal;

        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.lpszFileName"]/*' />
        [FieldOffset(0)]
        public ushort* lpszFileName;

        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.pstm"]/*' />
        [FieldOffset(0)]
        public IStream* pstm;

        /// <include file='_Anonymous_e__Union.xml' path='doc/member[@name="_Anonymous_e__Union.pstg"]/*' />
        [FieldOffset(0)]
        public IStorage* pstg;
    }
}
