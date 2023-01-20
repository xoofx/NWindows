// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using NWindows.Events;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.Windows.Windows;

namespace NWindows.Platforms.Win32;

internal sealed unsafe partial class Win32DropTarget : IDisposable
{
    private readonly Win32Window _window;
    private readonly IDropTarget* _dropTarget;
    private readonly DragDropEvent _dragDropEvent;
    private readonly List<string> _registeredFormats;
    private bool _registered;

    public Win32DropTarget(Win32Window window)
    {
        _window = window;
        _registeredFormats = new List<string>();
        _dragDropEvent = new DragDropEvent();
        _dropTarget = (IDropTarget*)NativeMemory.Alloc((nuint)sizeof(IDropTarget));
        _dropTarget->lpVtbl = IDropTargetVtbl.Instance;
        _dropTarget->RefCount = 1;
        _dropTarget->HandleToWin32DragAndDrop = GCHandle.ToIntPtr(GCHandle.Alloc(this));
    }

    public void Register()
    {
        if (_registered) return;

        var result = (HRESULT)Win32Ole.RegisterDragDrop(_window.HWnd, (nint)_dropTarget);
        if (result.SUCCEEDED)
        {
            _registered = true;
        }
    }

    public void UnRegister()
    {
        if (_registered) return;
        _ = Win32Ole.RevokeDragDrop(_window.HWnd);
    }

    internal HRESULT DragEnter(IDataObject* pDataObj, int grfKeyState, POINTL pt, int* pdwEffect)
    {
        RaiseEvent(DragDropKind.Enter, pDataObj, grfKeyState, pt, pdwEffect);
        return S.S_OK;
    }

    internal HRESULT DragOver(int grfKeyState, POINTL pt, int* pdwEffect)
    {
        RaiseEvent(DragDropKind.Over, null, grfKeyState, pt, pdwEffect);
        return S.S_OK;
    }

    private HRESULT DragLeave()
    {
        _dragDropEvent.DragDropKind = DragDropKind.Leave;
        _dragDropEvent.Effects = default;
        _dragDropEvent.Position = default;
        _dragDropEvent.KeyStates = default;
        _window.OnWindowEvent(_dragDropEvent);
        return S.S_OK;
    }

    private HRESULT Drop(IDataObject* pDataObj, int grfKeyState, POINTL pt, int* pdwEffect)
    {
        RaiseEvent(DragDropKind.Drop, pDataObj, grfKeyState, pt, pdwEffect);
        return S.S_OK;
    }

    private void RaiseEvent(DragDropKind kind, IDataObject* pDataObj, int grfKeyState, POINTL pt, int* pdwEffect)
    {
        _dragDropEvent.DragDropKind = DragDropKind.Enter;
        _dragDropEvent.Effects = FromEffects(*pdwEffect);
        _dragDropEvent.Position = _window.ScreenToClient(new Point(pt.x, pt.y));
        _dragDropEvent.KeyStates = GetKeyStates(grfKeyState);
        _dragDropEvent.Data = null;

        if (pDataObj != null)
        {
            _dragDropEvent.Data = Win32Ole.GetDropFiles(pDataObj);
            _dragDropEvent.Effects = _dragDropEvent.Effects;
        }

        try
        {
            _window.OnWindowEvent(_dragDropEvent);

            *pdwEffect = _dragDropEvent.Handled ? ToEffects(_dragDropEvent.Effects) : DROPEFFECT_NONE;
        }
        finally
        {
            // We never keep the data in memory
            _dragDropEvent.Data = null;
        }
    }

    private static DataTransferEffects FromEffects(int dwEffects)
    {
        var effect = DataTransferEffects.None;
        if ((dwEffects & DROPEFFECT_COPY) != 0) effect |= DataTransferEffects.Copy;
        if ((dwEffects & DROPEFFECT_MOVE) != 0) effect |= DataTransferEffects.Move;
        if ((dwEffects & DROPEFFECT_LINK) != 0) effect |= DataTransferEffects.Link;
        if ((dwEffects & unchecked((int)DROPEFFECT_SCROLL)) != 0) effect |= DataTransferEffects.Scroll;
        return effect;
    }

    private static int ToEffects(DataTransferEffects effects)
    {
        int dwEffects = 0;
        if ((effects & DataTransferEffects.Copy) != 0) dwEffects |= DROPEFFECT_COPY;
        if ((effects & DataTransferEffects.Move) != 0) dwEffects |= DROPEFFECT_MOVE;
        if ((effects & DataTransferEffects.Link) != 0) dwEffects |= DROPEFFECT_LINK;
        if ((effects & DataTransferEffects.Scroll) != 0) dwEffects |= unchecked((int)DROPEFFECT_SCROLL);
        return dwEffects;
    }

    private static DragDropKeyStates GetKeyStates(int grfKeyState)
    {
        var key = DragDropKeyStates.None;
        if ((grfKeyState & MK.MK_CONTROL) != 0) key |= DragDropKeyStates.ControlKey;
        if ((grfKeyState & MK.MK_SHIFT) != 0) key |= DragDropKeyStates.ShiftKey;
        if ((grfKeyState & MK.MK_ALT) != 0) key |= DragDropKeyStates.AltKey;
        if ((grfKeyState & MK.MK_LBUTTON) != 0) key |= DragDropKeyStates.LeftMouseButton;
        if ((grfKeyState & MK.MK_RBUTTON) != 0) key |= DragDropKeyStates.RightMouseButton;
        if ((grfKeyState & MK.MK_MBUTTON) != 0) key |= DragDropKeyStates.MiddleMouseButton;
        return key;
    }



    private unsafe struct IDropTarget
    {
        public IDropTargetVtbl* lpVtbl;
        public uint RefCount;
        public nint HandleToWin32DragAndDrop;

        public Win32DropTarget Get() => (Win32DropTarget)GCHandle.FromIntPtr(HandleToWin32DragAndDrop).Target!;
    }

    private unsafe struct IDropTargetVtbl
    {
        public static readonly IDropTargetVtbl* Instance;

        private static readonly Guid IID_IDropTarget = new("00000122-0000-0000-C000-000000000046");

        static IDropTargetVtbl()
        {
            Instance = (IDropTargetVtbl*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IDropTargetVtbl), 7 * sizeof(nint));
            *Instance = new IDropTargetVtbl();
        }

        public IDropTargetVtbl()
        {
        }

        public delegate* unmanaged[MemberFunction]<IDropTarget*, Guid*, void**, int> QueryInterface = &QueryInterfaceImpl;

        public delegate* unmanaged[MemberFunction]<IDropTarget*, uint> AddRef = &AddRefImpl;

        public delegate* unmanaged[MemberFunction]<IDropTarget*, uint> Release = &ReleaseImpl;

        public delegate* unmanaged[MemberFunction]<IDropTarget*, IDataObject*, int, POINTL, int*, int> DragEnter = &DragEnterImpl;

        public delegate* unmanaged[MemberFunction]<IDropTarget*, int, POINTL, int*, int> DragOver = &DragOverImpl;

        public delegate* unmanaged[MemberFunction]<IDropTarget*, int> DragLeave = &DragLeaveImpl;

        public delegate* unmanaged[MemberFunction]<IDropTarget*, IDataObject*, int, POINTL, int*, int> Drop = &DropImpl;

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static int QueryInterfaceImpl(IDropTarget* self, Guid* _iid, void** obj)
        {
            if (*_iid == IID_IDropTarget)
            {
                *obj = self;
                self->RefCount++;
                return S.S_OK;
            }
            *obj = null;
            return E.E_NOINTERFACE;
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static uint AddRefImpl(IDropTarget* self)
        {
            return Interlocked.Increment(ref self->RefCount);
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static uint ReleaseImpl(IDropTarget* self)
        {
            // We don't destroy anything here as we are disposing native memory once 
            return Interlocked.Decrement(ref self->RefCount);
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static int DragEnterImpl(IDropTarget* self, IDataObject* pDataObj, int grfKeyState, POINTL pt, int* pdwEffect)
        {
            if (pdwEffect == null) return E.E_INVALIDARG;
            try
            {
                return self->Get().DragEnter(pDataObj, grfKeyState, pt, pdwEffect);
            }
            catch
            {
                return E.E_UNEXPECTED;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static int DragOverImpl(IDropTarget* self, int grfKeyState, POINTL pt, int* pdwEffect)
        {
            if (pdwEffect == null) return E.E_INVALIDARG;
            try
            {
                return self->Get().DragOver(grfKeyState, pt, pdwEffect);
            }
            catch
            {
                return E.E_UNEXPECTED;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static int DragLeaveImpl(IDropTarget* self)
        {
            return self->Get().DragLeave();
        }

        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvMemberFunction) })]
        private static int DropImpl(IDropTarget* self, IDataObject* pDataObj, int grfKeyState, POINTL pt, int* pdwEffect)
        {
            if (pdwEffect == null) return E.E_INVALIDARG;
            try
            {
                return self->Get().Drop(pDataObj, grfKeyState, pt, pdwEffect);
            }
            catch
            {
                return E.E_UNEXPECTED;
            }
        }
    }


    private void ReleaseUnmanagedResources()
    {
        GCHandle.FromIntPtr(_dropTarget->HandleToWin32DragAndDrop).Free();
        NativeMemory.Free(_dropTarget);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Win32DropTarget()
    {
        ReleaseUnmanagedResources();
    }
}