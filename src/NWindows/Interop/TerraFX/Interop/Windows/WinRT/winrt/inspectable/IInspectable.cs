// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from winrt/inspectable.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;

namespace TerraFX.Interop.WinRT;

/// <include file='IInspectable.xml' path='doc/member[@name="IInspectable"]/*' />
[Guid("AF86E2E0-B12D-4C6A-9C5A-D7AA65101E90")]
internal unsafe partial struct IInspectable {

    public void** lpVtbl;

    /// <inheritdoc cref="IUnknown.QueryInterface" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public HRESULT QueryInterface(Guid* riid, void** ppvObject)
    {
        return ((delegate* unmanaged<IInspectable*, Guid*, void**, int>)(lpVtbl[0]))((IInspectable*)Unsafe.AsPointer(ref this), riid, ppvObject);
    }
}
