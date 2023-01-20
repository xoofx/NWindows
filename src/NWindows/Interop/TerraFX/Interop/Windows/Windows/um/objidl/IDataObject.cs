// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/objidl.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;

/// <include file='IDataObject.xml' path='doc/member[@name="IDataObject"]/*' />
[Guid("0000010E-0000-0000-C000-000000000046")]
internal unsafe partial struct IDataObject {

    public void** lpVtbl;

    /// <inheritdoc cref="IUnknown.Release" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Release()
    {
        return ((delegate* unmanaged<IDataObject*, uint>)(lpVtbl[2]))((IDataObject*)Unsafe.AsPointer(ref this));
    }

    /// <include file='IDataObject.xml' path='doc/member[@name="IDataObject.GetData"]/*' />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT GetData(FORMATETC* pformatetcIn, STGMEDIUM* pmedium)
    {
        return ((delegate* unmanaged<IDataObject*, FORMATETC*, STGMEDIUM*, int>)(lpVtbl[3]))((IDataObject*)Unsafe.AsPointer(ref this), pformatetcIn, pmedium);
    }

    /// <include file='IDataObject.xml' path='doc/member[@name="IDataObject.SetData"]/*' />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT SetData(FORMATETC* pformatetc, STGMEDIUM* pmedium, BOOL fRelease)
    {
        return ((delegate* unmanaged<IDataObject*, FORMATETC*, STGMEDIUM*, BOOL, int>)(lpVtbl[7]))((IDataObject*)Unsafe.AsPointer(ref this), pformatetc, pmedium, fRelease);
    }
}
