#pragma once
#include "../Common/IInnerCaptureProcessor.h"
//#include "BoxMode.h"
#include "../Common/BaseUnknown.h"
#include <thread>
#include "../Common/ComPtrCustom.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "ScreenCaptureProcessorFactory.h"
#include "../Common/ICheck.h"
#include "ScreenCaptureProcessor.h"
struct IMFMediaType;
struct IMFSample;
struct IMFMediaBuffer;

namespace CaptureManager
{
   namespace Sources
   {
      namespace ScreenCapture
      {
         class ScreenCaptureProcessorGDI : public ScreenCaptureProcessor //,protected ScreenCaptureProcessor
         {
         public:
            ScreenCaptureProcessorGDI();

            ScreenCaptureProcessorGDI(RECT aWorkArea);

            HRESULT init(const std::wstring& aRefDeviceName); // IScreenCaptureCheck interface
            HRESULT STDMETHODCALLTYPE check() override;       // IScreenCaptureProcessorEnum interface
            HRESULT STDMETHODCALLTYPE enumIInnerCaptureProcessor(UINT aIndex, UINT aOrientation,
                                                                 IInnerCaptureProcessor** aPtrPtrIInnerCaptureProcessor)
            override; // ScreenCaptureProcessor interface
            HRESULT initResources(ScreenCaptureConfig& aScreenCaptureConfig, IMFMediaType* aPtrOutputMediaType)
            override;

            HRESULT releaseResources() override;

            HRESULT grabImage(BYTE* aPtrData) override;

            std::wstring getSymbolicLink() override;

            std::wstring getFrendlyName() override; // ScreenCaptureProcessorGDI interface
            HWND mDesktopHWND;
            HDC mDesktopHDC;
            HDC mDesktopCompatibleHDC;
            BYTE* mPtrBits;
            HBITMAP mDesktopCompatibleHBitmap;
            HDC mSecondDesktopCompatibleHDC;
            HDC mDestCompatibleHDC;
            HBITMAP mSecondDesktopCompatibleHBitmap;
            HBITMAP mDestCompatibleHBitmap;
            BYTE* mSecondPtrBits;
            RECT mDesktopCoordinates;

            virtual ~ScreenCaptureProcessorGDI();

         protected:
            HRESULT fillVectorScreenCaptureConfigs(std::vector<ScreenCaptureConfig>& aRefVectorScreenCaptureConfigs)
            override;

         private:
            SIZE mDesktopSize;
            std::wstring mOutputName;
            std::wstring mDeviceName;
            RECT mWorkArea;
            POINT mMultiDisplayOffset;
            BOOL mIsMultiDisplay;
         };
      }
   }
}
