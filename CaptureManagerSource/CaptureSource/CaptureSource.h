#pragma once
#include <map>
#include <atomic>
#include <mutex>
#include <condition_variable>
#include "../Common/MFHeaders.h"
#include "../Common/SourceState.h"
#include "../Common/ComPtrCustom.h"
#include "../Common/IInnerCaptureProcessor.h"
#include "../Common/BaseUnknown.h"
#include "IStreamControl.h"

namespace CaptureManager
{
   namespace Sources
   {
      struct ISourceOperation;

      class CaptureSource : public BaseUnknown<IMFMediaSource, IMFGetService, IMFAsyncCallback>
      {
      public:
         CaptureSource(); // IMFMediaSource interface
         HRESULT STDMETHODCALLTYPE GetCharacteristics(__RPC__out DWORD* aPtrdwCharacteristics) override;

         HRESULT STDMETHODCALLTYPE CreatePresentationDescriptor(
            IMFPresentationDescriptor** aPtrPtrPresentationDescriptor) override;

         HRESULT STDMETHODCALLTYPE Start(IMFPresentationDescriptor* aPtrPresentationDescriptor,
                                         const GUID* aPtrGUIDTimeFormat,
                                         const PROPVARIANT* aPtrVarStartPosition) override;

         HRESULT STDMETHODCALLTYPE Stop() override;

         HRESULT STDMETHODCALLTYPE Pause() override;

         HRESULT STDMETHODCALLTYPE Shutdown() override;

         HRESULT STDMETHODCALLTYPE GetEvent(DWORD dwFlags, IMFMediaEvent** ppEvent) override;

         HRESULT STDMETHODCALLTYPE BeginGetEvent(IMFAsyncCallback* aPtrCallback, IUnknown* aPtrUnkState) override;

         HRESULT STDMETHODCALLTYPE EndGetEvent(IMFAsyncResult* aPtrResult, _Out_ IMFMediaEvent** aPtrPtrEvent) override;

         HRESULT STDMETHODCALLTYPE QueueEvent(MediaEventType met, __RPC__in REFGUID guidExtendedType, HRESULT hrStatus,
                                              const PROPVARIANT* pvValue) override; // IMFGetService
         HRESULT STDMETHODCALLTYPE GetService(REFGUID aRefGUIDService, REFIID aRefIID, LPVOID* aPtrPtrObject) override;

         // IMFAsyncCallback interface
         HRESULT STDMETHODCALLTYPE GetParameters(__RPC__out DWORD* pdwFlags, __RPC__out DWORD* pdwQueue) override;

         HRESULT STDMETHODCALLTYPE Invoke(IMFAsyncResult* pAsyncResult) override; // CaptureSource interface
         HRESULT init(CComPtrCustom<IInnerCaptureProcessor> aCaptureProcessor, BOOL aIsHardWare = FALSE);

         HRESULT createPresentationDescriptor(BOOL aIsHardWare);

         HRESULT requestNewSample(DWORD aStreamIdentifier, IUnknown* aPtrToken);

         HRESULT startCapture();

         HRESULT stopCapture();

         HRESULT shutdownCapture();

      protected:
         bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
         {
            if (aRefIID == __uuidof(IMFMediaEventGenerator)) {
               return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFMediaEventGenerator*>(this));
            }
            return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
         }

      private:
         DWORD mSyncWorkerQueue;
         std::condition_variable mConditionVariable;
         std::mutex mMutex;

         virtual ~CaptureSource();

         HRESULT STDMETHODCALLTYPE checkShutdown() const;

         HRESULT STDMETHODCALLTYPE isInitialized() const;

         HRESULT STDMETHODCALLTYPE initialized();

         HRESULT STDMETHODCALLTYPE isStarted() const;

         SourceState mState;
         CComPtrCustom<IInnerCaptureProcessor> mCaptureProcessor;
         std::map<DWORD, CComPtrCustom<IStreamControl>> mStreams;
         CComPtrCustom<IMFPresentationDescriptor> mPresentationDescriptor;
         CComPtrCustom<IMFMediaEventQueue> mEventQueue;

         HRESULT createStreamDescriptor(DWORD aStreamIdentifier, IMFStreamDescriptor** aPtrPtrStreamDescriptor);

         HRESULT STDMETHODCALLTYPE internalStart(ISourceOperation* aPtrCommand);

         HRESULT STDMETHODCALLTYPE internalPause();

         HRESULT STDMETHODCALLTYPE internalStop();

         HRESULT STDMETHODCALLTYPE requestSample(ISourceOperation* aPtrCommand);

         HRESULT STDMETHODCALLTYPE selectStream(IMFPresentationDescriptor* aPtrPresentationDescriptor);
      };
   }
}
