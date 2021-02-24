#pragma once
#include <atomic>
#include <condition_variable>
#include <memory>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "../SampleConvertor/SampleConvertor.h"

namespace CaptureManager
{
   namespace Transform
   {
      namespace VideoSampleAllocator
      {
         class DirectX11ConvertorTransform : public BaseUnknown<IMFTransform>
         {
         public:
            DirectX11ConvertorTransform();

            virtual ~DirectX11ConvertorTransform(); // IMFTransform interface
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

            //HRESULT init(
            //	IMFVideoSampleAllocator*  aPtrIMFVideoSampleAllocator,
            //	IMFMediaType* aPtrMediaType);
         private:
            std::condition_variable mConditionVariable;
            std::mutex mMutex;
            bool mIsReady;
            CComPtrCustom<IMFMediaType> mInputMediaType;
            CComPtrCustom<IMFDXGIDeviceManager> mInputStreamDeviceManager;
            CComPtrCustom<IMFMediaType> mOutputMediaType;
            CComPtrCustom<IMFDXGIDeviceManager> mOutputStreamDeviceManager;
            //CComPtrCustom<IMFVideoSampleAllocator>  mAllocator;
            CComPtrCustom<ISampleConvertor> mConvertor;
            CComPtrCustom<IMFSample> mSample;
         };
      }
   }
}
