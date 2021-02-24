#pragma once
#include <list>
#include <dxgi1_2.h>
#include <d3d11.h>
#include <DirectXMath.h>
#include "IInnerScreenCaptureProcessor.h"
#include "ScreenCaptureProcessor.h"
#include "../Common/ComPtrCustom.h"
#include "../Common/MFHeaders.h"
#include "../MemoryManager/IMemorySampleManager.h"

namespace CaptureManager
{
   namespace Sources
   {
      namespace ScreenCapture
      {
         typedef struct _PTR_INFO PTR_INFO;

         class ScreenCaptureProcessorIDXGIOutputDuplication : public ScreenCaptureProcessor
         {
         public:
            ScreenCaptureProcessorIDXGIOutputDuplication();

            virtual ~ScreenCaptureProcessorIDXGIOutputDuplication(); // IInnerGetService
            HRESULT STDMETHODCALLTYPE GetService(REFGUID aRefGUIDService, REFIID aRefIID, LPVOID* aPtrPtrObject)
            override;                                   // IScreenCaptureCheck interface
            HRESULT STDMETHODCALLTYPE check() override; // IScreenCaptureProcessorEnum interface
            HRESULT STDMETHODCALLTYPE enumIInnerCaptureProcessor(UINT aIndex, UINT aOrientation,
                                                                 IInnerCaptureProcessor** aPtrPtrIInnerCaptureProcessor)
            override;

            HRESULT releaseResources() override;

            HRESULT initResources(ScreenCaptureConfig& aScreenCaptureConfig, IMFMediaType* aPtrOutputMediaType)
            override;

            HRESULT grabImage(BYTE* aPtrData) override;

            virtual HRESULT init(std::wstring aDeviceName, UINT aOrientation, IDXGIOutput1* aPtrOutput);

            std::wstring getSymbolicLink() override;

            std::wstring getFrendlyName() override;

         protected:
            HRESULT fillVectorScreenCaptureConfigs(std::vector<ScreenCaptureConfig>& aRefVectorScreenCaptureConfigs)
            override;

            HRESULT grabImage(IUnknown** aPtrPtrUnkSample) override;

            HRESULT execute() override;

            HRESULT createVideoMediaType(ScreenCaptureConfig aScreenCaptureConfig, IMFMediaType** aPtrPtrMediaType)
            override;

         private:
            typedef struct _VERTEX
            {
               DirectX::XMFLOAT3 Pos;
               DirectX::XMFLOAT2 TexCoord;
            } VERTEX;

#define NUMVERTICES 6
            VERTEX mVertexes[NUMVERTICES];
            std::wstring mOutputName;
            std::wstring mDeviceName;
            UINT mVideoFrameDuration;
            SIZE mDesktopSize;
            UINT mDeviceResetToken;
            CComPtrCustom<IUnknown> mUnkDeviceManager;
            CComPtrCustom<IUnknown> mUnkVideoSampleAllocator;
            CComPtrCustom<ID3D11Device> mDevice;
            CComPtrCustom<ID3D11DeviceContext> mImmediateContext;
            CComPtrCustom<ID3D11VertexShader> mVertexShader;
            CComPtrCustom<ID3D11PixelShader> mPixelShader;
            CComPtrCustom<ID3D11InputLayout> mInputLayout;
            CComPtrCustom<ID3D11SamplerState> mSamplerState;
            CComPtrCustom<ID3D11Texture2D> mClipImage;
            CComPtrCustom<ID3D11Texture2D> mDestImage;
            CComPtrCustom<ID3D11Texture2D> mTempImage;
            CComPtrCustom<IInnerScreenCaptureProcessor> mIInnerScreenCaptureProcessor;

            HRESULT getOutputs(std::vector<CComPtrCustom<IDXGIOutput1>>& Outputs);

            HRESULT drawClip(ID3D11Texture2D* aPtrID3D11Texture2D, BYTE* aPtrData);

            HRESULT drawClip(ID3D11Texture2D* aPtrID3D11Texture2D, ID3D11Texture2D* aPtrDestID3D11Texture2D);

            HRESULT isDefaultAdapter(LUID aAdapterLuid);

            HRESULT createOrGetDeviceManager(REFIID aRefIID, LPVOID* aPtrPtrObject);
         };
      }
   }
}
