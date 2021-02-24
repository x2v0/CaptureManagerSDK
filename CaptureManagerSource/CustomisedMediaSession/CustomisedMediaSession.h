#pragma once
#include <mutex>
#include <condition_variable>
#include <list>
#include <map>
#include <queue>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/BoxMode.h"
#include "../Common/ComPtrCustom.h"
#include "IMediaPipeline.h"
#include "../TopologyResolver/ITopologyResolver.h"
#include "../TopologyResolver/CollectionOfIDs.h"
#include "ISessionSwitcherControl.h"

namespace CaptureManager
{
   namespace MediaSession
   {
      namespace CustomisedMediaSession
      {
         struct ICustomisedRequest;

         class CustomisedMediaSession : public BaseUnknown<IMFMediaSession, ISessionSwitcherControl>
         {
            enum class MediaSessionSate : int
            {
               None = 0,
               SessionStarted = None + 1,
               SessionPaused = SessionStarted + 1,
               SessionStopped = SessionPaused + 1,
               SessionClosed = SessionStopped + 1,
               SessionShutdowned = SessionClosed + 1
            };

         public:
            CustomisedMediaSession();

            virtual ~CustomisedMediaSession(); // ISessionSwitcherControl interface
            HRESULT STDMETHODCALLTYPE pauseSwitchers() override;

            HRESULT STDMETHODCALLTYPE resumeSwitchers() override;

            HRESULT STDMETHODCALLTYPE detachSwitchers() override;

            HRESULT STDMETHODCALLTYPE attachSwitcher(/* [in] */ IMFTopologyNode* aPtrSwitcherNode, /* [in] */
                                                                IMFTopologyNode* aPtrDownStreamNode) override;

            // IMFMediaEventGenerator implementation
            HRESULT STDMETHODCALLTYPE GetEvent(DWORD dwFlags, IMFMediaEvent** ppEvent) override;

            HRESULT STDMETHODCALLTYPE BeginGetEvent(IMFAsyncCallback* aPtrCallback, IUnknown* aPtrUnkState) override;

            HRESULT STDMETHODCALLTYPE EndGetEvent(IMFAsyncResult* aPtrResult, IMFMediaEvent** aPtrPtrEvent) override;

            HRESULT STDMETHODCALLTYPE QueueEvent(MediaEventType aMediaEventType, REFGUID aRefGUIDExtendedType,
                                                 HRESULT aHRESULTStatus, const PROPVARIANT* aPtrPropVariantValue)
            override; // IMFMediaSession implementation
            HRESULT STDMETHODCALLTYPE SetTopology(DWORD aSetTopologyFlags, IMFTopology* aPtrTopology) override;

            HRESULT STDMETHODCALLTYPE ClearTopologies() override;

            HRESULT STDMETHODCALLTYPE Start(const GUID* aConstPtrGUIDTimeFormat,
                                            const PROPVARIANT* aConstPtrVarStartPosition) override;

            HRESULT STDMETHODCALLTYPE Pause() override;

            HRESULT STDMETHODCALLTYPE Stop() override;

            HRESULT STDMETHODCALLTYPE Close() override;

            HRESULT STDMETHODCALLTYPE Shutdown() override;

            HRESULT STDMETHODCALLTYPE GetClock(IMFClock** ppClock) override;

            HRESULT STDMETHODCALLTYPE GetSessionCapabilities(DWORD* pdwCaps) override;

            HRESULT STDMETHODCALLTYPE GetFullTopology(DWORD dwGetFullTopologyFlags, TOPOID TopoId,
                                                      IMFTopology** ppFullTopology) override;

            virtual HRESULT checkInitBarier(IMFMediaStream* aPtrMediaStream);

            virtual HRESULT checkFinishBarier();

            HRESULT resetTargetNode(IMFTopologyNode* aPtrUpStreamTopologyNode, IMFMediaType* aPtrUpStreamMediaType,
                                    ITopologyResolver* aPtrTopologyResolver);

         protected:
            bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
            {
               if (aRefIID == __uuidof(IMFMediaEventGenerator)) {
                  return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFMediaEventGenerator*>(this));
               }
               return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
            }

         CAPTUREMANAGER_BOX
            std::mutex mMutex;
            std::mutex mAccesseMutex;
            bool mIsSwitchersDetached;
            std::map<TOPOID, CollectionOfIDs> mSwitcherCollection;
            std::list<CollectionOfIDs> mListOfStreamsCollectionOfID;
            CComPtrCustom<IMFMediaEventQueue> mEventQueue;
            CComPtrCustom<IMFPresentationClock> mClock;
            MediaSessionSate mMediaSessionSate;
            LONGLONG mClockStartOffset;
            std::list<CComPtrCustom<IMediaPipeline>> mListOfMediaPipelineProcessors;
            CComPtrCustom<ITopologyResolver> mVideoTopologyResolver;
            CComPtrCustom<ITopologyResolver> mAudioTopologyResolver;
            std::condition_variable mInitBarierCondition;
            std::mutex mInitBarierMutex;
            std::mutex mFinishBarierMutex;
            std::atomic<LONG> mInitBarierCount;
            std::list<IMFMediaStream*> mMediaStreamList;
            bool mFinishBarierState;
            bool mSessionIsInitialised;
            std::queue<CComPtrCustom<IMFTopology>> mTopologyQueue;
            CComPtrCustom<IMFTopology> mCurrentTopology;

            HRESULT resolveTopology(IMFTopology* aPtrTopology);

            HRESULT resolveStreamTopology(IMFTopology* aPtrTopology, std::list<TOPOID>& aRefListOfUsedSources,
                                          IMFTopologyNode* aPtrUpStreamTopologyNode,
                                          IMFMediaType* aPtrUpStreamMediaType);

            HRESULT queueTopologyReadyEvent(IMFTopology* aPtrTopology, HRESULT aResult);

            HRESULT notifyToBeginStreaming(IMFTopology* aPtrTopology);

            HRESULT notifyToEndStreaming(IMFTopology* aPtrTopology);

            HRESULT notifyToEndStreaming(IMFTopologyNode* aPtrTopologyNode);

            HRESULT findOutputNodes(std::vector<CComPtrCustom<IMFTopologyNode>>& aOutputTopologyNodes,
                                    IMFTopologyNode* aPtrDownStreamNode);

            HRESULT releaseNode(IMFTopologyNode* aPtrDownStreamNode);

            HRESULT enumAndAddOutputTopologyNode(IMFTopology* aPtrTopology, IMFTopologyNode* aPtrTopologyNode);

            HRESULT STDMETHODCALLTYPE notifyToBeginSwitcherStreaming(/* [in] */ IMFTopologyNode* aPtrTopologyNode);

            HRESULT getTargetNode(IMFTopologyNode* aPtrDownStreamNode, IMFTopologyNode** aPtrPtrTargetStreamNode);
         };
      }
   }
}
