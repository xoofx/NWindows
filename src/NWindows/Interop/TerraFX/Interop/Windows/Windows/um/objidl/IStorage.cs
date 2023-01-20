// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.

// Ported from um/objidl.h in the Windows SDK for Windows 10.0.22621.0
// Original source is Copyright © Microsoft. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TerraFX.Interop.Windows;

/// <include file='IStorage.xml' path='doc/member[@name="IStorage"]/*' />
[Guid("0000000B-0000-0000-C000-000000000046")]
internal unsafe partial struct IStorage {

    public void** lpVtbl;
}
