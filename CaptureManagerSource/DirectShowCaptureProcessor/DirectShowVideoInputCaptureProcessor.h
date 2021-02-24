#pragma once
#include <string>
#include <unordered_map>
#include <vector>
#include <thread>
#include <condition_variable>
#include <Strmif.h>
#include <control.h>
#include "../Common/ComPtrCustom.h"
#include "../Common/BoxMode.h"
#include "../Common/IInnerCaptureProcessor.h"
#include "../Common/BaseUnknown.h"
#include "../Common/SourceState.h"
#include "../Common/MFHeaders.h"
#include "DirectShowInterfaces.h"
typedef std::unordered_map<int, std::vector<long>> OUTPUT_INPUT_PIN_MAP;

namespace CaptureManager
{
   namespace Sources
   {
      namespace DirectShow
      {
         class DirectShowVideoInputCaptureProcessor : public BaseUnknown<IInnerCaptureProcessor, ISampleGrabberCB>
         {
         public:
            DirectShowVideoInputCaptureProcessor(IMoniker* aPtrIMoniker, OUTPUT_INPUT_PIN_MAP aOutputInputPinMap);

            ~DirectShowVideoInputCaptureProcessor(); //IInnerCaptureProcessor interface
            // get friendly name of Processor
            HRESULT getFrendlyName(BSTR* aPtrString) override;          // get synbolicLink of Processor
            HRESULT getSymbolicLink(BSTR* aPtrString) override;         // get supported amount of streams
            HRESULT getStreamAmount(UINT32* aPtrStreamAmount) override; // get supported media types of stream
            HRESULT getMediaTypes(UINT32 aStreamIndex, BSTR* aPtrStreamNameString, IUnknown*** aPtrPtrPtrMediaType,
                                  UINT32* aPtrMediaTypeCount) override; // set supported media type to stream
            HRESULT setCurrentMediaType(UINT32 aStreamIndex, IUnknown* aPtrMediaType) override;

            // get new sample of stream
            HRESULT getNewSample(DWORD aStreamIdentifier, IUnknown** ppSample) override; // start processor
            HRESULT start() override;                                                    // stop processor
            HRESULT stop() override;                                                     // pause processor
            HRESULT pause() override;                                                    // restart processor
            HRESULT restart() override;                                                  // shutdown processor
            HRESULT shutdown() override;                                                 //ISampleGrabberCB interface
            HRESULT STDMETHODCALLTYPE SampleCB(double aSampleTime, IMediaSample* aPtrSample) override;

            HRESULT STDMETHODCALLTYPE BufferCB(double aSampleTime, BYTE* aPtrBuffer, long aBufferLen) override;

         CAPTUREMANAGER_BOX
            struct ScreenCaptureConfig
            {
               BITMAPINFO mBitMapInfo;
               UINT32 mVideoFPS;
            };

         private:
            struct BufferContainer
            {
               IMFMediaBuffer* mPtrMediaBuffer;
               INT64 mCurrentTime;
               INT64 mCurrentDuration;
               std::mutex mAccessMutex;
               bool IsBufferReady;

               BufferContainer()
               {
                  mPtrMediaBuffer = nullptr;
                  IsBufferReady = false;
               }

               void setBuffer(IMFMediaBuffer* aPtrMediaBuffer, INT64 aCurrentTime, INT64 aCurrentDuration);

               bool getBuffer(IMFMediaBuffer** aPtrPtrMediaBuffer, INT64* aPtrCurrentTime, INT64* aPtrCurrentDuration);

               void clearBuffer();
            };

            CComPtrCustom<IMoniker> mMoniker;
            CComPtrCustom<ICaptureGraphBuilder2> mCaptureGraph;
            CComPtrCustom<IMediaControl> mMediaControl;
            SourceState mState;
            BufferContainer mBufferContainer;
            std::mutex mAccessBufferQueueMutex;
            std::mutex mAccessMutex;
            std::condition_variable mNewFrameCondition;
            OUTPUT_INPUT_PIN_MAP mOutputInputPinMap;
            UINT32 mDiscontinuity;
            UINT64 mDuration;
            const UINT32 mStreamAmount;
            CComPtrCustom<IMFMediaType> mCurrentMediaType;
            UINT32 mSelectedStreamIndex;

            HRESULT init();

            void clearMediaBuffer();

            HRESULT checkMediaType(IMFMediaType* aPtrMediaType, BOOL* aPtrBool,
                                   AM_MEDIA_TYPE** aPtrPtrAM_MEDIA_TYPE = nullptr);

            HRESULT route(IAMCrossbar* aPtrIAMCrossbar, UINT32 aPhysicalType);

            HRESULT getMediaTypes(IPin* aPtrPin, std::vector<CComPtrCustom<IMFMediaType>>& aMediaTypes);
         };
      }
   }
}
