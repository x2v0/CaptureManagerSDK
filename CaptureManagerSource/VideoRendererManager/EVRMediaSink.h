#pragma once
#include <mutex>
#include <atomic>
#include <unordered_map>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "../Common/IStreamSink.h"

//#include "IPresenter.h"
namespace CaptureManager
{
   namespace Sinks
   {
      namespace EVR
      {
         struct IPresenter;

         class EVRMediaSink : public BaseUnknown<IMFMediaSink, IMFGetService>
         {
         public:
            static HRESULT createEVRMediaSink(IPresenter* aPtrPresenter, IMFTransform* aPtrMixer, DWORD aMixerStreamID,
                                              bool aIsSingleStream, IMFMediaSink** aPtrPtrMediaSink);

            // IMFMediaSink methods
            STDMETHODIMP AddStreamSink(DWORD aStreamSinkIdentifier, IMFMediaType* aPtrMediaType,
                                       IMFStreamSink** aPtrPtrStreamSink) override;

            STDMETHODIMP GetCharacteristics(DWORD* aPtrCharacteristics) override;

            STDMETHODIMP GetPresentationClock(IMFPresentationClock** aPtrPtrPresentationClock) override;

            STDMETHODIMP GetStreamSinkById(DWORD aIdentifier, IMFStreamSink** aPtrPtrStreamSink) override;

            STDMETHODIMP GetStreamSinkByIndex(DWORD aIndex, IMFStreamSink** aPtrPtrStreamSink) override;

            STDMETHODIMP GetStreamSinkCount(DWORD* aPtrStreamSinkCount) override;

            STDMETHODIMP RemoveStreamSink(DWORD aStreamSinkIdentifier) override;

            STDMETHODIMP SetPresentationClock(IMFPresentationClock* aPtrPresentationClock) override;

            STDMETHODIMP Shutdown() override; // IMFGetService
            STDMETHODIMP GetService(REFGUID aRefGUIDService, REFIID aRefIID, LPVOID* aPtrPtrObject) override;

         private:
            std::mutex mAccessMutex;
            bool mIsShutdown;
            CComPtrCustom<IMFPresentationClock> mClock;
            const DWORD mStreamID;
            CComPtrCustom<IStreamSink> mStream;
            CComPtrCustom<IMFTransform> mMixer;

            EVRMediaSink();

            virtual ~EVRMediaSink();

            HRESULT checkShutdown() const;
         };
      }
   }
}
