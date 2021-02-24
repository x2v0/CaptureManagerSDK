#pragma once
#include "ScreenCaptureProcessor.h"
#include <d3d9.h>

namespace CaptureManager
{
   namespace Sources
   {
      namespace ScreenCapture
      {
         class ScreenCaptureProcessorDirectX9 : public ScreenCaptureProcessor
         {
         public:
            ScreenCaptureProcessorDirectX9();           // IScreenCaptureCheck interface
            HRESULT STDMETHODCALLTYPE check() override; // IScreenCaptureProcessorEnum interface
            HRESULT STDMETHODCALLTYPE enumIInnerCaptureProcessor(UINT aIndex, UINT aOrientation,
                                                                 IInnerCaptureProcessor** aPtrPtrIInnerCaptureProcessor)
            override; // ScreenCaptureProcessor interface
            HRESULT initResources(ScreenCaptureConfig& aScreenCaptureConfig, IMFMediaType* aPtrOutputMediaType)
            override;

            HRESULT releaseResources() override;

            HRESULT grabImage(BYTE* aPtrData) override;

            CComPtrCustom<IDirect3D9Ex> mDirect3D;
            CComPtrCustom<IDirect3DDevice9Ex> mDirect3DDevice;
            CComPtrCustom<IDirect3DSurface9> mSurface;
         protected:
            HRESULT fillVectorScreenCaptureConfigs(std::vector<ScreenCaptureConfig>& aRefVectorScreenCaptureConfigs)
            override;

         private:
            SIZE mDesktopSize;
            RECT mDesktopCoordinates;

            virtual ~ScreenCaptureProcessorDirectX9();
         };
      }
   }
}
