#pragma once
#include <mutex>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "../Common/IStreamSink.h"
#include "IPresenter.h"

namespace CaptureManager
{
   namespace Sinks
   {
      namespace EVR
      {
         class EVRStreamSink : public BaseUnknown<IMFStreamSink, IStreamSink, IMFMediaTypeHandler, IMFGetService,
                                                  IMFAsyncCallback, IMFClockStateSink>
         {
            enum StreamEnum
            {
               Uninitialized,
               Ready,
               Started,
               Paused,
               Stoped
            };

         public:
            static HRESULT createEVRStreamSink(DWORD aStreamID, IMFMediaSink* aPtrMediaSink, IPresenter* aPtrPresenter,
                                               DWORD aMixerStreamID, bool aIsSingleStream,
                                               IStreamSink** aPtrPtrStreamSink);

            HRESULT initialize(IMFMediaSink* aPtrMediaSink, bool aIsSingleStream, IPresenter* aPtrPresenter);

            // IMFClockStateSink methods
            HRESULT STDMETHODCALLTYPE OnClockStart(MFTIME aHNSSystemTime, LONGLONG aClockStartOffset) override;

            HRESULT STDMETHODCALLTYPE OnClockStop(MFTIME aHNSSystemTime) override;

            HRESULT STDMETHODCALLTYPE OnClockPause(MFTIME aHNSSystemTime) override;

            HRESULT STDMETHODCALLTYPE OnClockRestart(MFTIME aHNSSystemTime) override;

            HRESULT STDMETHODCALLTYPE OnClockSetRate(MFTIME aHNSSystemTime, float aRate) override;

            // IMFMediaTypeHandler implementation
            HRESULT STDMETHODCALLTYPE IsMediaTypeSupported(IMFMediaType* aPtrMediaType, IMFMediaType** aPtrPtrMediaType)
            override;

            HRESULT STDMETHODCALLTYPE GetMediaTypeCount(DWORD* aPtrTypeCount) override;

            HRESULT STDMETHODCALLTYPE GetMediaTypeByIndex(DWORD aIndex, IMFMediaType** aPtrPtrType) override;

            HRESULT STDMETHODCALLTYPE SetCurrentMediaType(IMFMediaType* aPtrMediaType) override;

            HRESULT STDMETHODCALLTYPE GetCurrentMediaType(IMFMediaType** aPtrPtrMediaType) override;

            HRESULT STDMETHODCALLTYPE GetMajorType(GUID* PtrGUIDMajorType) override; // IMFStreamSink implementation
            HRESULT STDMETHODCALLTYPE Flush() override;

            HRESULT STDMETHODCALLTYPE GetIdentifier(DWORD* aPtrIdentifier) override;

            HRESULT STDMETHODCALLTYPE GetMediaSink(IMFMediaSink** aPtrPtrMediaSink) override;

            HRESULT STDMETHODCALLTYPE GetMediaTypeHandler(IMFMediaTypeHandler** aPtrPtrHandler) override;

            HRESULT STDMETHODCALLTYPE PlaceMarker(MFSTREAMSINK_MARKER_TYPE aMarkerType,
                                                  const PROPVARIANT* aPtrVarMarkerValue,
                                                  const PROPVARIANT* aPtrVarContextValue) override;

            HRESULT STDMETHODCALLTYPE ProcessSample(IMFSample* aPtrSample) override;

            // IMFMediaEventGenerator implementation
            HRESULT STDMETHODCALLTYPE BeginGetEvent(IMFAsyncCallback* aPtrCallback, IUnknown* aPtrUnkState) override;

            HRESULT STDMETHODCALLTYPE EndGetEvent(IMFAsyncResult* aPtrResult, IMFMediaEvent** aPtrPtrEvent) override;

            HRESULT STDMETHODCALLTYPE GetEvent(DWORD aFlags, IMFMediaEvent** aPtrPtrEvent) override;

            HRESULT STDMETHODCALLTYPE QueueEvent(MediaEventType aMediaEventType, REFGUID aRefGUIDExtendedType,
                                                 HRESULT aHRStatus, const PROPVARIANT* aPtrValue) override;

            // IStreamSink implementation
            HRESULT STDMETHODCALLTYPE getMaxRate(BOOL aThin, float* aPtrRate) override;

            HRESULT STDMETHODCALLTYPE preroll() override;

            HRESULT STDMETHODCALLTYPE shutdown() override; // IMFGetService
            STDMETHODIMP GetService(REFGUID guidService, REFIID riid, LPVOID* ppvObject) override;

            // IMFAsyncCallback methods
            HRESULT STDMETHODCALLTYPE GetParameters(DWORD* aPtrFlags, DWORD* aPtrQueue) override;

            HRESULT STDMETHODCALLTYPE Invoke(IMFAsyncResult* aPtrAsyncResult) override;

         protected:
            bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
            {
               auto lresult = castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFMediaEventGenerator*>(this));
               if (!lresult) {
                  return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
               }
               return lresult;
            }

         private:
            const DWORD mStreamID;
            const DWORD mMixerStreamID;
            DWORD mWorkQueueId;
            bool mIsShutdown;
            bool mIsSingleStream;
            CComPtrCustom<IMFMediaType> mCurrentMediaType;
            CComPtrCustom<IMFMediaSink> mMediaSink;
            CComPtrCustom<IMFMediaEventQueue> mEventQueue;
            CComPtrCustom<IPresenter> mPresenter;
            CComPtrCustom<IMFMediaType> mCurrentType;
            CComPtrCustom<IMFTransform> mMixer;
            std::mutex mAccessMutex;
            std::mutex mClockStateAccessMutex;
            std::mutex mProcessMutex;
            StreamEnum mStreamState;

            EVRStreamSink(DWORD aStreamID, DWORD aMixerStreamID);

            virtual ~EVRStreamSink();

            HRESULT checkShutdown() const;

            HRESULT createVideoAllocator(IMFVideoSampleAllocator** aPtrPtrVideoSampleAllocator);
         };
      }
   }
}
