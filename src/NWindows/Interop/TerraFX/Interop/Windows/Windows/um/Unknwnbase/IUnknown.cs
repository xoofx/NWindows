// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/Unknwnbase.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;

/// <include file='IUnknown.xml' path='doc/member[@name="IUnknown"]/*' />
[Guid("00000000-0000-0000-C000-000000000046")]
internal unsafe partial struct IUnknown {

    public void** lpVtbl;
}
