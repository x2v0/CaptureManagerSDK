#pragma once
#include <thread>
#include <memory>
#include <queue>
#include <mutex>
#include <condition_variable>
#include "../Common/BaseUnknown.h"
#include "../Common/IInnerCaptureProcessor.h"
#include "../Common/SourceState.h"
#include "../Common/ComPtrCustom.h"
#include "../CaptureInvoker/CaptureInvoker.h"
#include "CaptureManagerTypeInfo.h"
#include "InitilaizeCaptureSource.h"
struct IMFMediaType;
struct IMFSample;
struct IMMDevice;
struct IAudioClient;
struct IAudioCaptureClient;
struct IMFMediaBuffer;
typedef LONGLONG REFERENCE_TIME;
typedef LONGLONG MFTIME;

namespace CaptureManager
{
   namespace COMServer
   {
      class AudioCaptureProcessorProxy : public BaseUnknown<IInnerCaptureProcessor, ISourceRequestResult>,
                                         public Core::CaptureInvoker
      {
      public:
         AudioCaptureProcessorProxy(); // ISourceRequestResult
         /* [id][helpstring] */
         HRESULT STDMETHODCALLTYPE setData(/* [in] */ LPVOID aPtrData, /* [in] */ DWORD aByteSize, /* [in] */
                                                      BOOL aIsKeyFrame) override;

         /* [id][helpstring] */
         HRESULT STDMETHODCALLTYPE getStreamIndex(/* [out] */ DWORD* aPtrStreamIndex) override;

         // CaptureInvoker implementation
         HRESULT STDMETHODCALLTYPE invoke() override; //IInnerCaptureProcessor interface
         // get friendly name of Processor
         HRESULT getFrendlyName(BSTR* aPtrString) override;          // get symbolicLink of Processor
         HRESULT getSymbolicLink(BSTR* aPtrString) override;         // get supported amount of streams
         HRESULT getStreamAmount(UINT32* aPtrStreamAmount) override; // get supported media types of stream
         HRESULT getMediaTypes(UINT32 aStreamIndex, BSTR* aPtrStreamNameString, IUnknown*** aPtrPtrPtrMediaType,
                               UINT32* aPtrMediaTypeCount) override;

         // set supported media type to stream
         HRESULT setCurrentMediaType(UINT32 aStreamIndex, IUnknown* aPtrMediaType) override; // get new sample of stream
         HRESULT getNewSample(DWORD aStreamIdentifier, IUnknown** aPtrPtrSample) override;   // start processor
         HRESULT start() override;                                                           // stop processor
         HRESULT stop() override;                                                            // pause processor
         HRESULT pause() override;                                                           // restart processor
         HRESULT restart() override;                                                         // shutdown processor
         HRESULT shutdown() override;

         HRESULT init(ICaptureProcessor* aPtrICaptureProcessor, InitilaizeCaptureSource* aPtrInitilaizeCaptureSource);

      private:
         struct BufferContainer
         {
            IMFMediaBuffer* mPtrMediaBuffer;
         };

         virtual ~AudioCaptureProcessorProxy();

         HRESULT checkShutdown() const;

         bool isUninitialized();

         void clearMediaBuffer();

         HRESULT allocateBuffer();

         void writeAudioBuffer(BYTE* aPtrData, UINT32 aBufferByteLength);

         void pushMediaBuffer(IMFMediaBuffer* aPtrMediaBuffer);

         bool popMediaBuffer(IMFMediaBuffer** aPtrPtrMediaBuffer);

         CComPtrCustom<ICaptureProcessor> mICaptureProcessor;
         CComPtrCustom<InitilaizeCaptureSource> mInitilaizeCaptureSource;
         CComPtrCustom<ISourceRequestResult> mSourceRequestResult;
         Sources::SourceState mState;
         bool mReleaseAudioClientLock;
         bool mFirstInvoke;
         MFTIME mPrevTime;
         INT64 mCycleOfCapture;
         MFTIME mDeltaTimeDuration;
         MFTIME mCurrentSampleTime;
         UINT32 mBlockAlign;
         UINT32 mSamplesPerSec;
         UINT32 mExpectedBufferSize;
         DWORD mSleepDuration;
         DWORD mMillTickTime;
         LONGLONG mSampleDuration;
         UINT32 mBufferOffset;
         MFTIME mPrevSampleTime;
         std::mutex mAccessMutex;
         std::mutex mNewSampleMutex;
         std::condition_variable mNewSampleCondition;
         std::mutex mReleaseAudioClientMutex;
         std::condition_variable mReleaseAudioClientCondition;
         std::wstring mFriendlyName;
         std::wstring mSymbolicLink;
         DWORD mStreamIndex;
         std::queue<BufferContainer> mBufferQueue;
         std::mutex mAccessBufferQueueMutex;
         CComPtrCustom<IMFMediaBuffer> mCurrentMediaBuffer;
         BOOL mIsCallBack;
      };
   }
}
