#pragma once
#include <atomic>
#include <mutex>
#include <condition_variable>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/SourceState.h"
#include "../Common/ComPtrCustom.h"
#include "IStreamControl.h"

namespace CaptureManager
{
   namespace Sources
   {
      class CaptureSource;

      class CaptureStream : public BaseUnknown<IMFMediaStream, IStreamControl>
      {
         class RequestContainer : public BaseUnknown<IUnknown>
         {
         public:
            RequestContainer(IUnknown* aPtrToken)
            {
               mToken = aPtrToken;
            }

            HRESULT getToken(IUnknown** aPtrPtrToken)
            {
               HRESULT lresult(E_FAIL);
               if (mToken)
                  lresult = mToken->QueryInterface(IID_PPV_ARGS(aPtrPtrToken));
               return lresult;
            }

         private:
            CComPtrCustom<IUnknown> mToken;
         };

      public:
         CaptureStream(CaptureSource* aPtrCaptureSource, IMFStreamDescriptor* aStreamDescriptor);

         virtual ~CaptureStream();

         STDMETHODIMP BeginGetEvent(IMFAsyncCallback* aPtrCallback, IUnknown* aPtrUnkState) override;

         STDMETHODIMP EndGetEvent(IMFAsyncResult* aPtrResult, IMFMediaEvent** aPtrPtrEvent) override;

         STDMETHODIMP GetEvent(DWORD aFlags, IMFMediaEvent** aPtrPtrEvent) override;

         STDMETHODIMP QueueEvent(MediaEventType aMediaEventType, REFGUID aGUIDExtendedType, HRESULT aHRStatus,
                                 const PROPVARIANT* aPtrvValue = nullptr) override;

         STDMETHODIMP GetMediaSource(IMFMediaSource** aPtrPtrMediaSource) override;

         STDMETHODIMP GetStreamDescriptor(IMFStreamDescriptor** aPtrPtrStreamDescriptor) override;

         STDMETHODIMP RequestSample(IUnknown* aPtrToken) override;

         HRESULT start() override;

         HRESULT pause() override;

         HRESULT stop() override;

         HRESULT shutdown() override;

         HRESULT isActive(BOOL* aPtrState) override;

         HRESULT activate(BOOL aActivate) override;

         virtual HRESULT checkShutdown();

         HRESULT QueueEvent(MediaEventType aMediaEventType, REFGUID aGUIDExtendedType, HRESULT aHRStatus,
                            IUnknown* aPtrUnk) override;

         HRESULT queueRequestToken(IUnknown* aPtrToken) override;

      protected:
         bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
         {
            if (aRefIID == __uuidof(IMFMediaEventGenerator)) {
               return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFMediaEventGenerator*>(this));
            }
            return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
         }

      private:
         std::mutex mMutex;
         std::mutex mOperationMutex;
         CaptureSource* mPtrCaptureSource;
         CComPtrCustom<IMFStreamDescriptor> mStreamDescriptor;
         CComPtrCustom<IMFMediaEventQueue> mEventQueue;
         CComPtrCustom<RequestContainer> mPostponedRequestContainer;
         bool mIsActive;
         DWORD mStreamIdentifier;
         SourceState mState;
      };
   }
}
