// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license.
// See license.txt file in the project root for full license information.

// Copyright © Tanner Gooding and Contributors. Licensed under the MIT License (MIT). See License.md in the repository root for more information.
// The following code is a simplified extract from:
// - https://github.com/terrafx/terrafx.interop.windows/blob/main/samples/DirectX/Shared/DXSample.cs
// - https://github.com/terrafx/terrafx.interop.windows/blob/main/samples/DirectX/D3D11/Shared/DX11Sample.cs
// - https://github.com/terrafx/terrafx.interop.windows/blob/main/samples/DirectX/D3D11/HelloTriangle11.cs

using System.Drawing;
using System.Numerics;
using NWindows;
using NWindows.Events;
using NWindows.Threading;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using static TerraFX.Interop.DirectX.D3D11;
using static TerraFX.Interop.DirectX.D3D_DRIVER_TYPE;
using static TerraFX.Interop.DirectX.D3D_FEATURE_LEVEL;
using static TerraFX.Interop.DirectX.DirectX;
using static TerraFX.Interop.DirectX.D3D_PRIMITIVE_TOPOLOGY;
using static TerraFX.Interop.DirectX.D3D11_BIND_FLAG;
using static TerraFX.Interop.DirectX.D3D11_INPUT_CLASSIFICATION;
using static TerraFX.Interop.DirectX.D3D11_USAGE;
using static TerraFX.Interop.DirectX.DXGI;
using static TerraFX.Interop.DirectX.DXGI_SWAP_EFFECT;
using static TerraFX.Interop.DirectX.DXGI_ADAPTER_FLAG;
using static TerraFX.Interop.DirectX.DXGI_FORMAT;
using static TerraFX.Interop.Windows.Windows;

namespace HelloDirect3D;

public unsafe class HelloTriangle11 : DispatcherObject
{
    private readonly Window _window;
    private ID3D11VertexShader* _vertexShader;
    private ID3D11InputLayout* _iputLayout;
    private ID3D11PixelShader* _pixelShader;
    private ID3D11Buffer* _vertexBuffer;
    private ID3D11Device* _d3DDevice;
    private IDXGIAdapter1* _dxgiAdapter;
    private IDXGIFactory2* _dxgiFactory;
    private ID3D11DeviceContext* _immediateContext;
    private ID3D11RenderTargetView* _renderTargetView;
    private IDXGISwapChain1* _swapChain;
    private RECT _scissorRect;
    private D3D11_VIEWPORT _viewport;

    private Size _currentSize;

    public HelloTriangle11(Window window)
    {
        _window = window;
    }

    public void Initialize()
    {
        VerifyAccess();
        CreateDeviceDependentResources();
        _window.Events.Paint += EventsOnPaint;
    }

    public void Draw()
    {
        if (_window.IsDisposed) return;

        VerifyAccess();
        CreateWindowSizeDependentResources();

        _immediateContext->ClearState();

        fixed (D3D11_VIEWPORT* viewport = &_viewport)
        {
            _immediateContext->RSSetViewports(1, viewport);
        }

        fixed (RECT* scissorRect = &_scissorRect)
        {
            _immediateContext->RSSetScissorRects(1, scissorRect);
        }

        var renderTargetView = _renderTargetView;
        var backgroundColor = new Vector4(0, 0, 0, 1.0f);
        _immediateContext->ClearRenderTargetView(renderTargetView, (float*)&backgroundColor);
        _immediateContext->OMSetRenderTargets(1, &renderTargetView, pDepthStencilView: null);

        _immediateContext->IASetInputLayout(_iputLayout);

        var stride = (uint)sizeof(Vertex);
        var offset = 0u;

        var vertexBuffer = _vertexBuffer;
        _immediateContext->IASetVertexBuffers(0, 1, &vertexBuffer, &stride, &offset);

        _immediateContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

        _immediateContext->VSSetShader(_vertexShader, null, 0);
        _immediateContext->PSSetShader(_pixelShader, null, 0);

        _immediateContext->Draw(3, 0);

        ThrowIfFailed(_swapChain->Present(SyncInterval: 1, Flags: 0));
    }

    public void Dispose()
    {
        VerifyAccess();

        _window.Events.Paint -= EventsOnPaint;

        _immediateContext->ClearState();
        Dispose(ref _swapChain);
        Dispose(ref _dxgiAdapter);
        Dispose(ref _dxgiFactory);
        Dispose(ref _immediateContext);
        Dispose(ref _d3DDevice);
        Dispose(ref _renderTargetView);
        Dispose(ref _vertexBuffer);
        Dispose(ref _iputLayout);
        Dispose(ref _pixelShader);
        Dispose(ref _vertexShader);
    }

    private void EventsOnPaint(Window window, PaintEvent evt)
    {
        Draw();
        evt.Handled = true;
    }

    private void CreateDeviceDependentResources()
    {
        IDXGIFactory2* dxgiFactory;
        ThrowIfFailed(CreateDXGIFactory2(0, __uuidof<IDXGIFactory2>(), (void**)&dxgiFactory));
        _dxgiFactory = dxgiFactory;
        _dxgiAdapter = GetDxgiAdapter(dxgiFactory);

        ID3D11Device* d3dDevice;
        ID3D11DeviceContext* immediateContext;

        var featureLevel = D3D_FEATURE_LEVEL_11_0;
        ThrowIfFailed(D3D11CreateDevice((IDXGIAdapter*)_dxgiAdapter, D3D_DRIVER_TYPE_HARDWARE, Software: HMODULE.NULL, Flags: 0, &featureLevel, FeatureLevels: 1, D3D11_SDK_VERSION, &d3dDevice, pFeatureLevel: null, &immediateContext));

        _d3DDevice = d3dDevice;
        _immediateContext = immediateContext;

        CreateAssets();

        static IDXGIAdapter1* GetDxgiAdapter(IDXGIFactory2* dxgiFactory)
        {
            IDXGIAdapter1* adapter;

            for (var adapterIndex = 0u; DXGI_ERROR_NOT_FOUND != dxgiFactory->EnumAdapters1(adapterIndex, &adapter); ++adapterIndex)
            {
                DXGI_ADAPTER_DESC1 desc;
                _ = adapter->GetDesc1(&desc);

                if ((desc.Flags & (uint)DXGI_ADAPTER_FLAG_SOFTWARE) != 0)
                {
                    // Don't select the Basic Render Driver adapter.
                    // If you want a software adapter, pass in "/warp" on the command line.
                    continue;
                }

                // Check to see if the adapter supports the required Direct3D version, but don't create the
                // actual device yet.

                var featureLevel = D3D_FEATURE_LEVEL_11_0;
                var supported = D3D11CreateDevice((IDXGIAdapter*)adapter, D3D_DRIVER_TYPE_HARDWARE, Software: HMODULE.NULL, Flags: 0, &featureLevel, FeatureLevels: 1, D3D11_SDK_VERSION, ppDevice: null, pFeatureLevel: null,
                    ppImmediateContext: null).SUCCEEDED;

                if (supported)
                {
                    break;
                }
            }

            return adapter;
        }
    }

    private void CreateAssets()
    {
        _iputLayout = CreateInputLayout();
        CreateVertexBuffer();

        ID3D11InputLayout* CreateInputLayout()
        {
            using ComPtr<ID3DBlob> vertexShaderBlob = null;
            using ComPtr<ID3DBlob> pixelShaderBlob = null;

            fixed (char* fileName = Path.Combine(AppContext.BaseDirectory, "HelloTriangle.hlsl"))
            fixed (ID3D11VertexShader** vertexShader = &_vertexShader)
            fixed (ID3D11PixelShader** pixelShader = &_pixelShader)
            {
                var entryPoint = 0x00006E69614D5356;    // VSMain
                var target = 0x0000305F345F7376;        // vs_4_0
                ThrowIfFailed(D3DCompileFromFile((ushort*)fileName, null, null, (sbyte*)&entryPoint, (sbyte*)&target, 0, 0, vertexShaderBlob.GetAddressOf(), null));

                ThrowIfFailed(_d3DDevice->CreateVertexShader(vertexShaderBlob.Get()->GetBufferPointer(), vertexShaderBlob.Get()->GetBufferSize(), pClassLinkage: null, vertexShader));

                entryPoint = 0x00006E69614D5350;        // PSMain
                target = 0x0000305F345F7370;            // ps_4_0
                ThrowIfFailed(D3DCompileFromFile((ushort*)fileName, null, null, (sbyte*)&entryPoint, (sbyte*)&target, 0, 0, pixelShaderBlob.GetAddressOf(), null));

                ThrowIfFailed(_d3DDevice->CreatePixelShader(pixelShaderBlob.Get()->GetBufferPointer(), pixelShaderBlob.Get()->GetBufferSize(), pClassLinkage: null, pixelShader));
            }

            var inputElementDescs = stackalloc D3D11_INPUT_ELEMENT_DESC[2];
            {
                var semanticName0 = stackalloc sbyte[9];
                {
                    ((ulong*)semanticName0)[0] = 0x4E4F495449534F50;      // POSITION
                }
                inputElementDescs[0] = new D3D11_INPUT_ELEMENT_DESC
                {
                    SemanticName = semanticName0,
                    SemanticIndex = 0,
                    Format = DXGI_FORMAT_R32G32B32_FLOAT,
                    InputSlot = 0,
                    AlignedByteOffset = 0,
                    InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                };

                var semanticName1 = 0x000000524F4C4F43;                     // COLOR
                inputElementDescs[1] = new D3D11_INPUT_ELEMENT_DESC
                {
                    SemanticName = (sbyte*)&semanticName1,
                    SemanticIndex = 0,
                    Format = DXGI_FORMAT_R32G32B32A32_FLOAT,
                    InputSlot = 0,
                    AlignedByteOffset = 12,
                    InputSlotClass = D3D11_INPUT_PER_VERTEX_DATA,
                    InstanceDataStepRate = 0
                };
            }

            ID3D11InputLayout* inputLayout;
            ThrowIfFailed(_d3DDevice->CreateInputLayout(inputElementDescs, NumElements: 2, vertexShaderBlob.Get()->GetBufferPointer(), vertexShaderBlob.Get()->GetBufferSize(), &inputLayout));
            return inputLayout;
        }
    }

    private void CreateVertexBuffer()
    {
        const float AspectRatio = 1.0f;

        var triangleVertices = stackalloc Vertex[3];
        {
            triangleVertices[0] = new Vertex
            {
                Position = new Vector3(0.0f, 0.25f * AspectRatio, 0.0f),
                Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f)
            };
            triangleVertices[1] = new Vertex
            {
                Position = new Vector3(0.25f, -0.25f * AspectRatio, 0.0f),
                Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f)
            };
            triangleVertices[2] = new Vertex
            {
                Position = new Vector3(-0.25f, -0.25f * AspectRatio, 0.0f),
                Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
            };
        }

        var vertexBufferSize = (uint)sizeof(Vertex) * 3;

        var vertexBufferDesc = new D3D11_BUFFER_DESC
        {
            ByteWidth = vertexBufferSize,
            Usage = D3D11_USAGE_DEFAULT,
            BindFlags = (uint)D3D11_BIND_VERTEX_BUFFER
        };

        var vertexBufferData = new D3D11_SUBRESOURCE_DATA
        {
            pSysMem = triangleVertices
        };

        ID3D11Buffer* vertexBuffer;
        ThrowIfFailed(_d3DDevice->CreateBuffer(&vertexBufferDesc, &vertexBufferData, &vertexBuffer));
        _vertexBuffer = vertexBuffer;
    }

    private void CreateWindowSizeDependentResources()
    {
        var windowSize = _window.Dpi.LogicalToPixel(_window.ClientSize);

        if (windowSize == _currentSize) return;

        const uint backBufferCount = 2;
        var backBufferFormat = DXGI_FORMAT_R8G8B8A8_UNORM;

        _currentSize = windowSize;

        if (_swapChain != null)
        {
            _immediateContext->ClearState();
            _immediateContext->Flush();

            Dispose(ref _renderTargetView);

            ThrowIfFailed(_swapChain->ResizeBuffers(backBufferCount, (uint)windowSize.Width, (uint)windowSize.Height, backBufferFormat, 0));
        }
        else
        {
            _swapChain = CreateSwapChain();
        }

        _renderTargetView = CreateRenderTargetView();

        _viewport = new D3D11_VIEWPORT
        {
            TopLeftX = 0,
            TopLeftY = 0,
            Width = windowSize.Width,
            Height = windowSize.Height,
            MinDepth = D3D11_MIN_DEPTH,
            MaxDepth = D3D11_MAX_DEPTH
        };

        _scissorRect = new RECT
        {
            left = 0,
            top = 0,
            right = windowSize.Width,
            bottom = windowSize.Height
        };

        IDXGISwapChain1* CreateSwapChain()
        {
            var swapChainDesc = new DXGI_SWAP_CHAIN_DESC1
            {
                Width = (uint)windowSize.Width,
                Height = (uint)windowSize.Height,
                Format = backBufferFormat,
                SampleDesc = new DXGI_SAMPLE_DESC
                {
                    Count = 1
                },
                BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT,
                BufferCount = backBufferCount,
                SwapEffect = DXGI_SWAP_EFFECT_DISCARD,
                Flags = 0,
            };

            var fullScreenSwapChainDesc = new DXGI_SWAP_CHAIN_FULLSCREEN_DESC()
            {
                Windowed = true,
            };

            IDXGISwapChain1* swapChain;

            ThrowIfFailed(_dxgiFactory->CreateSwapChainForHwnd((IUnknown*)_d3DDevice, (HWND)_window.Handle, &swapChainDesc, &fullScreenSwapChainDesc, null, &swapChain));

            // WARNING: we need to call MakeWindowAssociation on the GetParent of the swapChain
            // not on the `_dxgiFactory` itself, otherwise DXGI_MWA_NO_ALT_ENTER won't be honored!!!
            IDXGIFactory* dxgiFactory;
            if (swapChain->GetParent(__uuidof<IDXGIFactory>(), (void**)&dxgiFactory).SUCCEEDED)
            {
                ThrowIfFailed(dxgiFactory->MakeWindowAssociation((HWND)_window.Handle, DXGI_MWA_NO_ALT_ENTER));
                dxgiFactory->Release();
            }

            return swapChain;
        }

        ID3D11RenderTargetView* CreateRenderTargetView()
        {
            using ComPtr<ID3D11Resource> backBuffer = null;
            ThrowIfFailed(_swapChain->GetBuffer(0, __uuidof<ID3D11Texture2D>(), (void**)backBuffer.GetAddressOf()));

            ID3D11RenderTargetView* renderTargetView;
            ThrowIfFailed(_d3DDevice->CreateRenderTargetView(backBuffer.Get(), null, &renderTargetView));
            return renderTargetView;
        }
    }

    private static void Dispose<T>(ref T* nativeReference) where T : unmanaged, IUnknown.Interface
    {
        if (nativeReference != null)
        {
            nativeReference->Release();
            nativeReference = null;
        }
    }

    private struct Vertex
    {
        public Vector3 Position;

        public Vector4 Color;
    }

    private static void ThrowIfFailed(HRESULT result)
    {
        if (!result.SUCCEEDED) throw new InvalidOperationException("An operation failed");
    }
}