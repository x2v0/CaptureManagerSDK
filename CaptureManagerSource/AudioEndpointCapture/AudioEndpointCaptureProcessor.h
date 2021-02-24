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
   namespace Sources
   {
      namespace AudioEndpointCapture
      {
         class AudioEndpointCaptureProcessor : public BaseUnknown<IInnerCaptureProcessor>, public Core::CaptureInvoker
         {
         public:
            AudioEndpointCaptureProcessor();             // CaptureInvoker implementation
            HRESULT STDMETHODCALLTYPE invoke() override; //IInnerCaptureProcessor interface
            // get friendly name of Processor
            HRESULT getFrendlyName(BSTR* aPtrString) override;          // get symbolicLink of Processor
            HRESULT getSymbolicLink(BSTR* aPtrString) override;         // get supported amount of streams
            HRESULT getStreamAmount(UINT32* aPtrStreamAmount) override; // get supported media types of stream
            HRESULT getMediaTypes(UINT32 aStreamIndex, BSTR* aPtrStreamNameString, IUnknown*** aPtrPtrPtrMediaType,
                                  UINT32* aPtrMediaTypeCount) override; // set supported media type to stream
            HRESULT setCurrentMediaType(UINT32 aStreamIndex, IUnknown* aPtrMediaType) override;

            // get new sample of stream
            HRESULT getNewSample(DWORD aStreamIdentifier, IUnknown** aPtrPtrSample) override; // start processor
            HRESULT start() override;                                                         // stop processor
            HRESULT stop() override;                                                          // pause processor
            HRESULT pause() override;                                                         // restart processor
            HRESULT restart() override;                                                       // shutdown processor
            HRESULT shutdown() override;

            // AudioEndpointCaptureProcessor interface
            HRESULT init(IMMDevice* aPtrMMDevice, std::wstring aSymbolicLink); //CAPTUREMANAGER_BOX
            struct AudioCaptureConfig
            {
               PWAVEFORMATEX mPWAVEFORMATEX = nullptr;
               WAVEFORMATEX mWAVEFORMATEX;
               REFERENCE_TIME mDefaultDevicePeriod;
               BOOL mFloatToShort;

               AudioCaptureConfig()
               {
                  mPWAVEFORMATEX = nullptr;
               }
            };

            struct BufferContainer
            {
               IMFMediaBuffer* mPtrMediaBuffer;
            };

            IAudioClient* mAudioClient;
            IMMDevice* mPtrMMDevice;
            IAudioCaptureClient* mPtrAudioCaptureClient;
            bool mFirstInvoke;
            MFTIME mPrevTime;
            MFTIME mDeltaTimeDuration;
            MFTIME mCurrentSampleTime;
            MFTIME mPrevSampleTime;
            std::wstring mFriendlyName;
            std::wstring mSymbolicLink;
            std::vector<AudioCaptureConfig> mVectorAudioCaptureConfigs;
            AudioCaptureConfig mCurrentAudioCaptureConfig;
            SourceState mState;
            std::mutex mAccessMutex;
            std::mutex mNewSampleMutex;
            std::condition_variable mNewSampleCondition;
            std::mutex mReleaseAudioClientMutex;
            std::condition_variable mReleaseAudioClientCondition;
            bool mReleaseAudioClientLock;
            LONGLONG mSampleDuration;
            LONGLONG mCheckSampleDuration;
            INT64 mCycleOfCapture;
            INT64 mSleepDuration;
            UINT32 mExpectedBufferSize;
            UINT32 mBlockAlign;
            DWORD mMillTickTime;
            std::queue<BufferContainer> mBufferQueue;
            std::mutex mAccessBufferQueueMutex;
            CComPtrCustom<IMFMediaBuffer> mCurrentMediaBuffer;
            UINT32 mBufferOffset;
            BOOL mSilenceBlock;
            BOOL mIsSilenceBlock;

            HRESULT allocateBuffer();

            HRESULT createAudioMediaType(AudioCaptureConfig aAudioCaptureConfig, IMFMediaType** aPtrPtrMediaType);

            HRESULT fillVectorAudioCaptureConfigs(IMMDevice* aPtrMMDevice,
                                                  std::vector<AudioCaptureConfig>& aVectorAudioCaptureConfigs);

            HRESULT initializeAudioClient();

            HRESULT releaseAudioClient();

            void writeAudioBuffer(BYTE* aPtrData, UINT32 aBufferByteLength);

            bool isUninitialized();

            void pushMediaBuffer(IMFMediaBuffer* aPtrMediaBuffer);

            bool popMediaBuffer(IMFMediaBuffer** aPtrPtrMediaBuffer);

            void clearMediaBuffer();

            HRESULT STDMETHODCALLTYPE checkShutdown() const;

            virtual ~AudioEndpointCaptureProcessor();
         };
      }
   }
}
