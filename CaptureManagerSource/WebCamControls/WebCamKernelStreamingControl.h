#pragma once
#include <string>
#include <mutex>
#include <condition_variable>
#include "../CaptureManagerBroker/IWebCamKernelStreamingControl.h"
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"

namespace CaptureManager
{
   namespace Controls
   {
      namespace WebCamControls
      {
         namespace CustomisedWebCamControl
         {
            class WebCamKernelStreamingControl : public BaseUnknown<IWebCamKernelStreamingControl>
            {
            public:
               static HRESULT createIWebCamKernelStreamingControl(std::wstring aSymbolicLink,
                                                                  IWebCamKernelStreamingControl**
                                                                  aPtrPtrIWebCamKernelStreamingControl);

               // IWebCamKernelStreamingControl interface implemnetation
               virtual ~WebCamKernelStreamingControl();

               HRESULT STDMETHODCALLTYPE getCamParametrs(BSTR* aXMLstring) override;

               HRESULT STDMETHODCALLTYPE getCamParametr(DWORD aParametrIndex, LONG* aCurrentValue, LONG* aMin,
                                                        LONG* aMax, LONG* aStep, LONG* aDefault, LONG* aFlag) override;

               HRESULT STDMETHODCALLTYPE setCamParametr(DWORD aParametrIndex, LONG aNewValue, LONG aFlag) override;

            private:
               HANDLE mDevice;

               WebCamKernelStreamingControl(HANDLE aDevice);
            };
         }
      }
   }
}
