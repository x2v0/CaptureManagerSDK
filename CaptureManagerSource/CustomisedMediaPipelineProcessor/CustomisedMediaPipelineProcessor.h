#pragma once
#include <mutex>
#include <condition_variable>
#include <vector>
#include <list>
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "../CustomisedMediaSession/IMediaPipeline.h"
#include "../TopologyResolver/CollectionOfIDs.h"
#include "IMediaPipelineProcessorControl.h"

namespace CaptureManager
{
   namespace MediaSession
   {
      namespace CustomisedMediaSession
      {
         struct ICustomisedRequest;
         class CustomisedMediaSession;

         class CustomisedMediaPipelineProcessor : public BaseUnknown<IMediaPipeline, IMediaPipelineProcessorControl>
         {
         public:
            CustomisedMediaPipelineProcessor(CustomisedMediaSession* aPtrCustomisedMediaSession);

            virtual ~CustomisedMediaPipelineProcessor();

            virtual HRESULT finishSourceStreamStarting(); // IMFAsyncCallback implementation
            HRESULT STDMETHODCALLTYPE GetParameters(DWORD* aPtrFlags, DWORD* aPtrQueue) override;

            HRESULT STDMETHODCALLTYPE Invoke(IMFAsyncResult* aPtrAsyncResult) override;

            HRESULT init(CollectionOfIDs& aRefCollectionOfIDs, IMFTopology* aPtrTopology,
                         IMFPresentationClock* aPtrPresentationClock); // IMediaPipeline interface
            HRESULT startSources(const GUID* aConstPtrGUIDTimeFormat,
                                 const PROPVARIANT* aConstPtrVarStartPosition) override;

            HRESULT pauseSources() override;

            HRESULT stopSources() override;

            virtual HRESULT finishSourceStreamStopping();

            virtual HRESULT finishSourceStreamPausing();

            HRESULT finishStreamSinkStopping() override;

            HRESULT finishStreamSinkPausing() override; // IMediaPipelineProcessorControl interface
            HRESULT pause() override;

            HRESULT restart() override;

            HRESULT removeStreamSink(IMFStreamSink* aPtrStreamSink) override;

            HRESULT checkSwitcherNode(const TOPOID& aRefSwitcherNodeID) override;

            HRESULT registerStreamSink(const TOPOID& aRefStreamSinkNodeID,
                                       IMFPresentationClock* aPtrPresentationClock) override;

         private:
            enum class State : byte
            {
               None = 0,
               Started = None + 1,
               Paused = Started + 1,
               Stopped = Paused + 1
            };

            std::atomic<LONG> mStreamSinkCount;
            std::atomic<LONG> mSourceStreamCount;
            std::atomic<LONG> mSourceStreamPausingCount;
            std::mutex mMediaPipelineMutex;
            std::mutex mSourceStartingPipelineMutex;
            int mStopWaitingTime;
            std::condition_variable mStreamSinkStoppingCondition;
            std::condition_variable mStreamSinkPausingCondition;
            std::condition_variable mSourceStreamStartingCondition;
            std::condition_variable mSourceStreamPausingCondition;
            std::condition_variable mSourceStreamStoppingCondition;
            DWORD mSyncWorkerQueue;
            GUID mGUIDMajorType;
            State mCurrentState;
            bool mIsDiscontinuity;
            bool m_processSampleAccess;
            CustomisedMediaSession* mPtrCustomisedMediaSession;
            std::vector<CComPtrCustom<IMFMediaStream>> mMediaStreams;
            std::list<CComPtrCustom<IMFStreamSink>> mStreamSinks;
            std::vector<LONGLONG> mSampleTimes;
            std::vector<LONGLONG> mPrevSampleTimes;
            CollectionOfIDs mCollectionOfIDs;
            CComPtrCustom<IMFTopology> mCurrentTopology; // CustomisedMediaSession implementation
            HRESULT handleSourceRequest(IMFAsyncResult* aPtrResult, ICustomisedRequest* aPtrICustomisedRequest);

            HRESULT handleSourceStreamRequest(IMFAsyncResult* aPtrResult, ICustomisedRequest* aPtrICustomisedRequest);

            HRESULT handleStreamSinkRequest(IMFAsyncResult* aPtrResult, ICustomisedRequest* aPtrICustomisedRequest);

            HRESULT handlePullNewSampleRequest(IMFAsyncResult* aPtrResult, ICustomisedRequest* aPtrICustomisedRequest);

            HRESULT handlePushNewSampleRequest(IMFAsyncResult* aPtrResult, ICustomisedRequest* aPtrICustomisedRequest);

            HRESULT pullNewSampleFromSourceStream(IMFTopologyNode* aPtrTopologyNode);

            HRESULT pullPushNewSampleFromToTransform(IMFSample* aPtrSample, IMFTopologyNode* aPtrTopologyNode);

            HRESULT pullPushNewSampleFromToTeeNode(IMFSample* aPtrSample, IMFTopologyNode* aPtrTopologyNode,
                                                   DWORD aIndexUpperStreamNode = 0);

            HRESULT pushNewSampleToOutputNode(IMFSample* aPtrSample, IMFTopologyNode* aPtrTopologyNode);

            HRESULT initOutputDataBuffer(IMFTransform* aPtrTransform, MFT_OUTPUT_DATA_BUFFER& aRefOutputBuffer);

            HRESULT findAndRegisterSwitcherNode(const TOPOID& aRefSwitcherNodeID, IMFTopologyNode* aPtrTopologyNode);

            MFTIME mCurrentTime;
         };
      }
   }
}
