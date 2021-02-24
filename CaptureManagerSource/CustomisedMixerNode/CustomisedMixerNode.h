#pragma once
#include <atomic>
#include <condition_variable>
#include <memory>
#include <vector>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "IMixerWrapper.h"
#include "IVideoMixerControl.h"
#include "IAudioMixerControl.h"

namespace CaptureManager
{
   namespace MediaSession
   {
      namespace CustomisedMediaSession
      {
         class CustomisedMixerNode : public BaseUnknown<IMFTransform, IVideoMixerControl, IAudioMixerControl>
         {
         public:
            CustomisedMixerNode(UINT32 aIndex, IMixerWrapper* aPtrIMixerWrapper);

            virtual ~CustomisedMixerNode();

            static HRESULT create(DWORD aInputNodeAmount, std::vector<CComPtrCustom<IUnknown>>& aRefOutputNodes);

            // IMFTransform implements
            STDMETHODIMP GetStreamLimits(DWORD* aPtrInputMinimum, DWORD* aPtrInputMaximum, DWORD* aPtrOutputMinimum,
                                         DWORD* aPtrOutputMaximum) override;

            STDMETHODIMP GetStreamIDs(DWORD aInputIDArraySize, DWORD* aPtrInputIDs, DWORD aOutputIDArraySize,
                                      DWORD* aPtrOutputIDs) override;

            STDMETHODIMP GetStreamCount(DWORD* aPtrInputStreams, DWORD* aPtrOutputStreams) override;

            STDMETHODIMP GetInputStreamInfo(DWORD aInputStreamID, MFT_INPUT_STREAM_INFO* aPtrStreamInfo) override;

            STDMETHODIMP GetOutputStreamInfo(DWORD aOutputStreamID, MFT_OUTPUT_STREAM_INFO* aPtrStreamInfo) override;

            STDMETHODIMP GetInputStreamAttributes(DWORD aInputStreamID, IMFAttributes** aPtrPtrAttributes) override;

            STDMETHODIMP GetOutputStreamAttributes(DWORD aOutputStreamID, IMFAttributes** aPtrPtrAttributes) override;

            STDMETHODIMP DeleteInputStream(DWORD aStreamID) override;

            STDMETHODIMP AddInputStreams(DWORD aStreams, DWORD* aPtrStreamIDs) override;

            STDMETHODIMP GetInputAvailableType(DWORD aInputStreamID, DWORD aTypeIndex, IMFMediaType** aPtrPtrType)
            override;

            STDMETHODIMP GetOutputAvailableType(DWORD aOutputStreamID, DWORD aTypeIndex, IMFMediaType** aPtrPtrType)
            override;

            STDMETHODIMP SetInputType(DWORD aInputStreamID, IMFMediaType* aPtrType, DWORD aFlags) override;

            STDMETHODIMP SetOutputType(DWORD aOutputStreamID, IMFMediaType* aPtrType, DWORD aFlags) override;

            STDMETHODIMP GetInputCurrentType(DWORD aInputStreamID, IMFMediaType** aPtrPtrType) override;

            STDMETHODIMP GetOutputCurrentType(DWORD aOutputStreamID, IMFMediaType** aPtrPtrType) override;

            STDMETHODIMP GetInputStatus(DWORD aInputStreamID, DWORD* aPtrFlags) override;

            STDMETHODIMP GetOutputStatus(DWORD* aPtrFlags) override;

            STDMETHODIMP SetOutputBounds(LONGLONG aLowerBound, LONGLONG aUpperBound) override;

            STDMETHODIMP ProcessEvent(DWORD aInputStreamID, IMFMediaEvent* aPtrEvent) override;

            STDMETHODIMP GetAttributes(IMFAttributes** aPtrPtrAttributes) override;

            STDMETHODIMP ProcessMessage(MFT_MESSAGE_TYPE aMessage, ULONG_PTR aParam) override;

            STDMETHODIMP ProcessInput(DWORD aInputStreamID, IMFSample* aPtrSample, DWORD aFlags) override;

            STDMETHODIMP ProcessOutput(DWORD aFlags, DWORD aOutputBufferCount,
                                       MFT_OUTPUT_DATA_BUFFER* aPtrOutputSamples, DWORD* aPtrStatus) override;

            // IVideoMixerControl implements
            STDMETHODIMP setPosition(/* [in] */ FLOAT aLeft, /* [in] */ FLOAT aRight, /* [in] */ FLOAT aTop, /* [in] */
                                                FLOAT aBottom) override;

            STDMETHODIMP setSrcPosition(/* [in] */ FLOAT aLeft, /* [in] */ FLOAT aRight, /* [in] */ FLOAT aTop,
                                                   /* [in] */ FLOAT aBottom) override;

            STDMETHODIMP setZOrder(/* [in] */ DWORD aZOrder) override;

            STDMETHODIMP setOpacity(/* [in] */ FLOAT aOpacity) override;

            STDMETHODIMP flush() override; // IAudioMixerControl implements
            STDMETHODIMP setRelativeVolume(/* [in] */ FLOAT aRelativeVolume) override;

         private:
            std::condition_variable mConditionVariable;
            std::mutex mMutex;
            const UINT32 mIndex;
            CComPtrCustom<IMFMediaType> mInputMediaType;
            CComPtrCustom<IMFMediaType> mOutputMediaType;
            CComPtrCustom<IMFTransform> mMixerTransform;
            CComPtrCustom<IMixerWrapper> mMixerWrapper;
            CComPtrCustom<IMFSample> mSample;
         };
      }
   }
}
