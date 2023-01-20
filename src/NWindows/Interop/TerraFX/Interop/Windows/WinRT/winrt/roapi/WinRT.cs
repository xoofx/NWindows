// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from winrt/roapi.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.InteropServices;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.WinRT.RO_INIT_TYPE;

namespace TerraFX.Interop.WinRT;
internal static unsafe partial class WinRT
{
    /// <include file='WinRT.xml' path='doc/member[@name="WinRT.RoInitialize"]/*' />
    [DllImport("combase", ExactSpelling = true)]
    public static extern HRESULT RoInitialize(RO_INIT_TYPE initType);

    /// <include file='WinRT.xml' path='doc/member[@name="WinRT.RoActivateInstance"]/*' />
    [DllImport("combase", ExactSpelling = true)]
    public static extern HRESULT RoActivateInstance(HSTRING activatableClassId, IInspectable** instance);

    /// <include file='WinRT.xml' path='doc/member[@name="WinRT.Initialize"]/*' />
    public static HRESULT Initialize(RO_INIT_TYPE initType = RO_INIT_SINGLETHREADED)
    {
        return RoInitialize(initType);
    }
}
