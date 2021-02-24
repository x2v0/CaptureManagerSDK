#pragma once
#include <atomic>
#include <mutex>
#include "SampleGrabberPullStream.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "../Common/BaseUnknown.h"

namespace CaptureManager
{
   namespace Sinks
   {
      namespace SampleGrabberPull
      {
         class SampleGrabberPullSink : public BaseUnknown<IMFFinalizableMediaSink, IMFClockStateSink, IMFAsyncCallback>
         {
         public:
            SampleGrabberPullSink(); // IMFMediaSink interface
            STDMETHODIMP GetCharacteristics(DWORD* aPtrCharacteristics) override;

            STDMETHODIMP AddStreamSink(DWORD aStreamSinkIdentifier, IMFMediaType* aPtrMediaType,
                                       IMFStreamSink** aPtrPtrStreamSink) override;

            STDMETHODIMP RemoveStreamSink(DWORD aStreamSinkIdentifier) override;

            STDMETHODIMP GetStreamSinkCount(DWORD* aPtrStreamSinkCount) override;

            STDMETHODIMP GetStreamSinkByIndex(DWORD aIndex, IMFStreamSink** aPtrPtrStreamSink) override;

            STDMETHODIMP GetStreamSinkById(DWORD aStreamSinkIdentifier, IMFStreamSink** aPtrPtrStreamSink) override;

            STDMETHODIMP SetPresentationClock(IMFPresentationClock* aPtrPresentationClock) override;

            STDMETHODIMP GetPresentationClock(IMFPresentationClock** aPtrPtrPresentationClock) override;

            STDMETHODIMP Shutdown() override; // IMFClockStateSink interface
            STDMETHODIMP OnClockStart(MFTIME aSystemTime, LONGLONG aClockStartOffset) override;

            STDMETHODIMP OnClockStop(MFTIME aSystemTime) override;

            STDMETHODIMP OnClockPause(MFTIME aSystemTime) override;

            STDMETHODIMP OnClockRestart(MFTIME aSystemTime) override;

            STDMETHODIMP OnClockSetRate(MFTIME aSystemTime, float aRate) override; // IMFFinalizableMediaSink interface
            STDMETHODIMP BeginFinalize(IMFAsyncCallback* aPtrAsyncCallback, IUnknown* aPtrUnkState) override;

            STDMETHODIMP EndFinalize(IMFAsyncResult* aPtrAsyncResult) override;

            // IMFAsyncCallback interface implementation
            STDMETHODIMP GetParameters(DWORD* aPtrFlags, DWORD* aPtrQueue) override;

            STDMETHODIMP Invoke(IMFAsyncResult* aPtrAsyncResult) override;

            // SampleGrabberPullSink interface implementation
            STDMETHODIMP setStream(IUnknown* aPtrUnkSampleGrabberPullStream);

            virtual ~SampleGrabberPullSink();

         protected:
            bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
            {
               if (aRefIID == __uuidof(IMFMediaSink)) {
                  return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFMediaSink*>(this));
               }
               return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
            }

         private:
            std::mutex mMutex;
            CComPtrCustom<IMFStreamSink> mStreamSink;
            CComPtrCustom<IMFPresentationClock> mClock;
         };
      }
   }
}
