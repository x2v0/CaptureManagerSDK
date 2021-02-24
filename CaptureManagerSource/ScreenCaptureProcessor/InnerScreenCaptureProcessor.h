#pragma once
#include "IInnerScreenCaptureProcessor.h"
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include <dxgi1_3.h>
#include <d3d11.h>

namespace CaptureManager
{
   namespace Sources
   {
      namespace ScreenCapture
      {
         class InnerScreenCaptureProcessor : public BaseUnknown<IInnerScreenCaptureProcessor>
         {
         public:
            InnerScreenCaptureProcessor();

            virtual ~InnerScreenCaptureProcessor();

            HRESULT init(IDXGIOutput1* aPtrIDXGIOutput1); // IInnerScreenCaptureProcessor
            HRESULT getSize(SIZE& aRefSize) override;

            HRESULT releaseResources() override;

            HRESULT initResources(IUnknown* aPtrUnkDevice, ID3D11DeviceContext* aPtrDeviceContext,
                                  UINT aVideoFrameDuration, int aStride) override;

            HRESULT getGDITexture(IUnknown** aPtrPtrUnk) override;

            HRESULT updateFrame() override;

            HRESULT updateFrame(ID3D11Texture2D* aPtrTexture2D) override;

            HRESULT readFromTexture(BYTE* aPtrData) override;

            HRESULT readFromTexture(ID3D11Texture2D* aPtrTexture2D) override;

            HRESULT getCursorInfo(RECT& aRefDesktopCoordinates, DXGI_MODE_ROTATION& aRefRotation) override;

         protected:
            SIZE mSize;
            bool mIsPortrait;
            int mStride;
            DXGI_MODE_ROTATION mRotation;
            DXGI_MODE_ROTATION mDisplayRotation;
            DXGI_OUTDUPL_DESC mDesc;
            DXGI_OUTPUT_DESC mOutputDesc;
            CComPtrCustom<ID3D11Device> mDevice;
            CComPtrCustom<ID3D11DeviceContext> mImmediateContext;
            CComPtrCustom<ID3D11VertexShader> mVertexShader;
            CComPtrCustom<ID3D11PixelShader> mPixelShader;
            CComPtrCustom<ID3D11InputLayout> mInputLayout;
            CComPtrCustom<ID3D11SamplerState> mSamplerState;
            CComPtrCustom<ID3D11Texture2D> mGDI_COMPATIBLEImage;
            CComPtrCustom<ID3D11Texture2D> mDestImage;
         private:
            UINT mVideoFrameDuration;
            CComPtrCustom<IDXGIOutput1> mOutput;
            CComPtrCustom<IDXGIOutputDuplication> mDeskDupl;
            CComPtrCustom<ID3D11Texture2D> mAcquiredDesktopImage;
            CComPtrCustom<ID3D11Texture2D> mTempDesktopImage;

            HRESULT acquireNextFrame(bool* aPtrTimeout);

            HRESULT redraw();
         };
      }
   }
}
