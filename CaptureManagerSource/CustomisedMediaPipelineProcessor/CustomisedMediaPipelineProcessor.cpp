/*
MIT License

Copyright(c) 2020 Evgeny Pereguda

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#include "CustomisedMediaPipelineProcessor.h"
#include "../MediaFoundationManager/MediaFoundationManager.h"
#include "../Common/MFHeaders.h"
#include "../Common/Common.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "CustomisedRequest.h"
#include "../CustomisedMediaSession/CustomisedMediaSession.h"
#include "../CustomisedTeeNode/ITeeNode.h"
#include "../CustomisedPresentationClock/IPresentationClock.h"
#include "../Common/GUIDs.h"
#include "../CaptureManagerBroker/CaptureManagerConstants.h"
#include "../TopologyResolver/VideoTopologyResolver.h"
#include "../TopologyResolver/AudioTopologyResolver.h"
#include <propvarutil.h>
#include <typeinfo>


namespace CaptureManager
{
	namespace MediaSession
	{
		namespace CustomisedMediaSession
		{
			using namespace Core;

			// {64ABBDD2-B84C-4C39-9F19-04BE6AF125F9}
			static const GUID MediaStreamID =
			{ 0x64abbdd2, 0xb84c, 0x4c39, { 0x9f, 0x19, 0x4, 0xbe, 0x6a, 0xf1, 0x25, 0xf9 } };
			
			CustomisedMediaPipelineProcessor::CustomisedMediaPipelineProcessor(
				CustomisedMediaSession* aPtrCustomisedMediaSession) :
				mStreamSinkCount(0),
				mSourceStreamCount(0),
				mSyncWorkerQueue(MFASYNC_CALLBACK_QUEUE_LONG_FUNCTION),
				mPtrCustomisedMediaSession(aPtrCustomisedMediaSession),
				mCurrentState(State::None),
				mStopWaitingTime(5),
				mCurrentTime(0),
				m_processSampleAccess(false)
			{
				HRESULT lresult;

				do
				{
					LOG_INVOKE_FUNCTION(MediaFoundation::MediaFoundationManager::GetLockWorkQueue,
						&mSyncWorkerQueue);
						
				} while (false);

				if (FAILED(lresult))
				{
					mSyncWorkerQueue = MFASYNC_CALLBACK_QUEUE_LONG_FUNCTION;
				}
			}

			CustomisedMediaPipelineProcessor::~CustomisedMediaPipelineProcessor()
			{
				if (mSyncWorkerQueue != MFASYNC_CALLBACK_QUEUE_LONG_FUNCTION)
				{
					HRESULT lresult;

					do
					{
						LOG_INVOKE_MF_FUNCTION(MFUnlockWorkQueue,
							mSyncWorkerQueue);
						
					} while (false);
				}


			}

			// IMFAsyncCallback implementation

			HRESULT STDMETHODCALLTYPE CustomisedMediaPipelineProcessor::GetParameters(
				DWORD *pdwFlags,
				DWORD *pdwQueue)
			{
				HRESULT lresult;

				do
				{
					lresult = E_NOTIMPL;

				} while (false);

				return lresult;
			}

			HRESULT STDMETHODCALLTYPE CustomisedMediaPipelineProcessor::Invoke(
				IMFAsyncResult* aPtrAsyncResult)
			{
				HRESULT lresult;

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrAsyncResult);
					
					CComQIPtrCustom<ICustomisedRequest> lICustomisedRequest;
					
					CComPtrCustom<IUnknown> lUnkState;

					LOG_INVOKE_MF_METHOD(GetStatus, aPtrAsyncResult);

					LOG_INVOKE_MF_METHOD(GetState, aPtrAsyncResult, &lUnkState);

					lICustomisedRequest = lUnkState;

					LOG_CHECK_PTR_MEMORY(lICustomisedRequest);
					
					switch (lICustomisedRequest->getCustomisedRequestType())
					{
					case ICustomisedRequest::SourceRequest:
						lresult = handleSourceRequest(aPtrAsyncResult, lICustomisedRequest);
						break;
					case ICustomisedRequest::SourceStreamRequest:
						lresult = handleSourceStreamRequest(aPtrAsyncResult, lICustomisedRequest);
						break;
					case ICustomisedRequest::StreamSinkRequest:
						lresult = handleStreamSinkRequest(aPtrAsyncResult, lICustomisedRequest);
						break;
					case ICustomisedRequest::PullNewSampleRequest:
						lresult = handlePullNewSampleRequest(aPtrAsyncResult, lICustomisedRequest);
						break;
					case ICustomisedRequest::PushNewSampleRequest:
						lresult = handlePushNewSampleRequest(aPtrAsyncResult, lICustomisedRequest);
						break;
					default:
						break;
					}
					
				} while (false);

				if (FAILED(lresult))
				{
					do
					{

						LOG_INVOKE_MF_METHOD(QueueEvent,
							mPtrCustomisedMediaSession,
							MEError,
							GUID_NULL,
							lresult,
							nullptr);

					} while (false);
				}

				return lresult;
			}

			// CustomisedMediaSession implementation

			HRESULT CustomisedMediaPipelineProcessor::handleSourceRequest(
				IMFAsyncResult* aPtrResult,
				ICustomisedRequest* aPtrICustomisedRequest)
			{
				HRESULT lresult = S_OK;

				PROPVARIANT leventVariant;

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrResult);

					LOG_CHECK_PTR_MEMORY(aPtrICustomisedRequest);
					
					PropVariantInit(&leventVariant);

					CComPtrCustom<IMFMediaEvent> lEvent;

					MediaEventType leventType;

					CComQIPtrCustom<IMFMediaSource> lSource = aPtrICustomisedRequest->getPtrUnkSender();

					LOG_CHECK_PTR_MEMORY(lSource);
					
					CComQIPtrCustom<IMFTopologyNode> lSourceTopologyNode = aPtrICustomisedRequest->getPtrUnkSenderTopologyNode();

					LOG_CHECK_PTR_MEMORY(lSourceTopologyNode);

					LOG_INVOKE_MF_METHOD(EndGetEvent,
						lSource,
						aPtrResult,
						&lEvent);
					
					LOG_INVOKE_MF_METHOD(GetType,
						lEvent,
						&leventType);
										
					if (leventType == MEError)
						leventType = MEVideoCaptureDeviceRemoved;

					LOG_INVOKE_MF_METHOD(QueueEvent,
						mPtrCustomisedMediaSession,
						leventType,
						GUID_NULL,
						lresult,
						nullptr);
					
					if (leventType == MENewStream)
					{
						LOG_INVOKE_MF_METHOD(GetValue, lEvent, &leventVariant);
						
						CComQIPtrCustom<IMFMediaStream> lSourceStream = leventVariant.punkVal;

						LOG_CHECK_PTR_MEMORY(lSourceStream);

						LOG_INVOKE_MF_METHOD(SetUINT32,
							lSourceTopologyNode,
							MediaStreamID,
							mMediaStreams.size());

						CComPtrCustom<ICustomisedRequest> lSourceStreamRequest = new (std::nothrow) CustomisedRequest(
							ICustomisedRequest::SourceStreamRequest,
							lSourceStream,
							aPtrICustomisedRequest->getPtrUnkSenderTopologyNode());

						LOG_CHECK_PTR_MEMORY(lSourceStreamRequest);

						LOG_INVOKE_MF_METHOD(BeginGetEvent,
							lSourceStream,
							this,
							lSourceStreamRequest);
						
						mMediaStreams.push_back(lSourceStream);

						mSampleTimes.push_back(0);

						mPrevSampleTimes.push_back(0);

						lSourceStream->RequestSample(nullptr);
					}
					else if (leventType == MEUpdatedStream)
					{
					}
					else if (leventType == MESourceStarted)
					{
					}
					else if (leventType == MESourceSeeked)
					{
					}
					else if (leventType == MESourceStopped)
					{
						break;
					}
					else if (leventType == MESourcePaused)
					{
					}
					else if (leventType == MESourceRateChangeRequested)
					{
					}
					else if (leventType == MEEndOfPresentation)
					{
					}

					CComPtrCustom<ICustomisedRequest> lRequest = new (std::nothrow) CustomisedRequest(
						ICustomisedRequest::SourceRequest,
						lSource,
						aPtrICustomisedRequest->getPtrUnkSenderTopologyNode());

					LOG_CHECK_PTR_MEMORY(lRequest);

					LOG_INVOKE_MF_METHOD(BeginGetEvent,
						lSource,
						this,
						lRequest);
					
				} while (false);

				PropVariantClear(&leventVariant);
				
				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::handleSourceStreamRequest(
				IMFAsyncResult* aPtrResult,
				ICustomisedRequest* aPtrICustomisedRequest)
			{
				HRESULT lresult;
				CComPtrCustom<IMFMediaEvent> lEvent;

				MediaEventType leventType;

				PROPVARIANT leventVariant;

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrResult);

					LOG_CHECK_PTR_MEMORY(aPtrICustomisedRequest);
										
					PropVariantInit(&leventVariant);

					CComQIPtrCustom<IMFMediaStream> lSourceStream = aPtrICustomisedRequest->getPtrUnkSender();

					LOG_CHECK_PTR_MEMORY(lSourceStream);

					LOG_INVOKE_MF_METHOD(EndGetEvent,
						lSourceStream,
						aPtrResult,
						&lEvent);
					
					LOG_INVOKE_MF_METHOD(GetType,
						lEvent,
						&leventType);
					
					CComQIPtrCustom<IMFTopologyNode> lTopologyNode = aPtrICustomisedRequest->getPtrUnkSenderTopologyNode();

					LOG_CHECK_PTR_MEMORY(lTopologyNode);

					CComPtrCustom<ICustomisedRequest> lRequest = new (std::nothrow) CustomisedRequest(
						ICustomisedRequest::SourceStreamRequest,
						lSourceStream,
						lTopologyNode);

					LOG_CHECK_PTR_MEMORY(lRequest);

					LOG_INVOKE_MF_METHOD(BeginGetEvent,
						lSourceStream,
						this,
						lRequest);


					if (mPtrCustomisedMediaSession)
					{
						mPtrCustomisedMediaSession->QueueEvent(
							leventType,
							GUID_NULL,
							lresult,
							nullptr);
					}
					
					if (leventType == MEStreamStarted)
					{
						std::lock_guard<std::mutex> lLock(mSourceStartingPipelineMutex);

						--mSourceStreamCount;
							
						mSourceStreamStartingCondition.notify_one();

						break;
					}
					else if (leventType == MEMediaSample)
					{
						LOG_INVOKE_MF_METHOD(GetValue, lEvent, &leventVariant);

						CComQIPtrCustom<IMFSample> lReadySourceSample = leventVariant.punkVal;

						LOG_CHECK_PTR_MEMORY(lReadySourceSample);

						CComPtrCustom<IMFTopologyNode> lDownStreamTopologyNode;

						auto lOutputStreamIndex = aPtrICustomisedRequest->getIndexOfStreamNode();

						DWORD lInputIndex;

						LOG_INVOKE_MF_METHOD(GetOutput,
							lTopologyNode,
							lOutputStreamIndex,
							&lDownStreamTopologyNode,
							&lInputIndex);
						
						CComPtrCustom<ICustomisedRequest> lPushNewSampleRequest = new (std::nothrow) CustomisedRequest(
							ICustomisedRequest::PushNewSampleRequest,
							lReadySourceSample,
							lDownStreamTopologyNode);

						LOG_CHECK_PTR_MEMORY(lPushNewSampleRequest);

						lresult = mPtrCustomisedMediaSession->checkInitBarier(lSourceStream);

						if (FAILED(lresult))
						{
							lSourceStream->RequestSample(nullptr);

							lresult = S_OK;

							break;
						}
						
						UINT32 lMediaStreamIndex;

						LOG_INVOKE_MF_METHOD(GetUINT32,
							lTopologyNode,
							MediaStreamID,
							&lMediaStreamIndex);
						
						auto lSampleTime = mSampleTimes[lMediaStreamIndex];

						auto lPrevSampleTimes = mPrevSampleTimes[lMediaStreamIndex];

						LONGLONG lSampleDuration;

						LOG_INVOKE_MF_METHOD(GetSampleDuration, lReadySourceSample, &lSampleDuration);

						LONGLONG lSampleTimeDevice;

						LOG_INVOKE_MF_METHOD(GetSampleTime, lReadySourceSample, &lSampleTimeDevice);

						UINT32 Discontinuity = FALSE;

						do
						{
							LOG_INVOKE_MF_METHOD(GetUINT32,
								lReadySourceSample,
								MFSampleExtension_Discontinuity,
								&Discontinuity);

						} while (false);


						if (mGUIDMajorType == MFMediaType_Video)
						if (Discontinuity == FALSE && lPrevSampleTimes > 0)
							lSampleDuration = lSampleTimeDevice - lPrevSampleTimes;

						mPrevSampleTimes[lMediaStreamIndex] = lSampleTimeDevice;

						LOG_INVOKE_MF_METHOD(SetSampleDuration, lReadySourceSample, lSampleDuration);

						LOG_INVOKE_MF_METHOD(SetSampleTime, lReadySourceSample, lSampleTime);

						lSampleTime += lSampleDuration;

						mSampleTimes[lMediaStreamIndex] = lSampleTime;

						mCurrentTime = lSampleTime;

											


						LOG_INVOKE_MF_FUNCTION(MFPutWorkItem,
							mSyncWorkerQueue,
							this,
							lPushNewSampleRequest);						
					}
					else if (leventType == MEStreamSeeked)
					{
						break;
					}
					else if (leventType == MEStreamStopped)
					{
						std::lock_guard<std::mutex> lLock(mMediaPipelineMutex);

						--mSourceStreamCount;

						mSourceStreamStoppingCondition.notify_one();

						break;
					}
					else if (leventType == MEStreamPaused)
					{
						std::lock_guard<std::mutex> lLock(mMediaPipelineMutex);

						--mSourceStreamPausingCount;

						mSourceStreamPausingCondition.notify_one();

						break;
					}
					else if (leventType == MEStreamTick)
					{
						break;
					}
					else if (leventType == MEStreamThinMode)
					{
						break;
					}
					else if (leventType == MEStreamFormatChanged)
					{
						break;
					}

				} while (false);

				PropVariantClear(&leventVariant);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::handleStreamSinkRequest(
				IMFAsyncResult* aPtrResult,
				ICustomisedRequest* aPtrICustomisedRequest)
			{
				HRESULT lresult;
				CComPtrCustom<IMFMediaEvent> lEvent;
				MediaEventType leventType;

				do
				{

					LOG_CHECK_PTR_MEMORY(aPtrResult);

					LOG_CHECK_PTR_MEMORY(aPtrICustomisedRequest);

					CComQIPtrCustom<IMFStreamSink> lStreamSink = aPtrICustomisedRequest->getPtrUnkSender();

					LOG_CHECK_PTR_MEMORY(lStreamSink);
					
					LOG_INVOKE_MF_METHOD(EndGetEvent,
						lStreamSink,
						aPtrResult,
						&lEvent);
					
					LOG_INVOKE_MF_METHOD(GetType,
						lEvent,
						&leventType);
					
					CComQIPtrCustom<IMFTopologyNode> lTopologyNode = aPtrICustomisedRequest->getPtrUnkSenderTopologyNode();

					LOG_CHECK_PTR_MEMORY(lTopologyNode);
					
					if (leventType == MEStreamSinkStarted)
					{
						leventType = MEStreamSinkRequestSample;
					}
					else if (leventType == MEStreamSinkPaused)
					{
						std::lock_guard<std::mutex> lLock(mMediaPipelineMutex);

						--mStreamSinkCount;

						mStreamSinkPausingCondition.notify_one();
					}
										
					if (leventType == MEStreamSinkRequestSample)
					{
						CComPtrCustom<IMFTopologyNode> lNextTopologyNode;

						DWORD lOutputIndexOnUpstreamNode;

						LOG_INVOKE_MF_METHOD(GetInput,
							lTopologyNode,
							0,
							&lNextTopologyNode,
							&lOutputIndexOnUpstreamNode);
						
						CComPtrCustom<ICustomisedRequest> lPullNewSampleRequest = new (std::nothrow) CustomisedRequest(
							ICustomisedRequest::PullNewSampleRequest,
							lStreamSink,
							lNextTopologyNode,
							lOutputIndexOnUpstreamNode);

						LOG_CHECK_PTR_MEMORY(lPullNewSampleRequest);

						LOG_INVOKE_MF_FUNCTION(MFPutWorkItem,
							mSyncWorkerQueue,
							this,
							lPullNewSampleRequest);
					}
					else if (leventType == MEStreamSinkStopped)
					{
						std::lock_guard<std::mutex> lLock(mMediaPipelineMutex);

						--mStreamSinkCount;

						mStreamSinkStoppingCondition.notify_one();
					}

					CComPtrCustom<ICustomisedRequest> lRequest = new (std::nothrow) CustomisedRequest(
						ICustomisedRequest::StreamSinkRequest,
						lStreamSink,
						aPtrICustomisedRequest->getPtrUnkSenderTopologyNode());

					LOG_CHECK_PTR_MEMORY(lRequest);

					LOG_INVOKE_MF_METHOD(BeginGetEvent,
						lStreamSink,
						this,
						lRequest);
					
				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::handlePullNewSampleRequest(
				IMFAsyncResult* aPtrResult,
				ICustomisedRequest* aPtrICustomisedRequest)
			{
				HRESULT lresult;
				CComPtrCustom<IMFMediaEvent> lEvent;
				CComPtrCustom<ICustomisedRequest> lRequest;

				do
				{

					LOG_CHECK_PTR_MEMORY(aPtrResult);
					
					CComQIPtrCustom<IMFTopologyNode> lTopologyNode = aPtrICustomisedRequest->getPtrUnkSenderTopologyNode();

					LOG_CHECK_PTR_MEMORY(lTopologyNode);
					
					DWORD lIndexUpperStreamNode = aPtrICustomisedRequest->getIndexOfStreamNode();

					MF_TOPOLOGY_TYPE lTopologyType;

					LOG_INVOKE_MF_METHOD(GetNodeType,
						lTopologyNode,
						&lTopologyType);
					
					switch (lTopologyType)
					{
					case MF_TOPOLOGY_OUTPUT_NODE:
						break;
					case MF_TOPOLOGY_SOURCESTREAM_NODE:
						LOG_INVOKE_FUNCTION(pullNewSampleFromSourceStream,
							lTopologyNode);
						break;
					case MF_TOPOLOGY_TRANSFORM_NODE:
						LOG_INVOKE_FUNCTION(pullPushNewSampleFromToTransform,
							nullptr,
							lTopologyNode);
						break;
					case MF_TOPOLOGY_TEE_NODE:
						LOG_INVOKE_FUNCTION(pullPushNewSampleFromToTeeNode,
							nullptr,
							lTopologyNode,
							lIndexUpperStreamNode);
						break;
					case MF_TOPOLOGY_MAX:
						break;
					default:
						break;
					}

				} while (false);
				
				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::handlePushNewSampleRequest(
				IMFAsyncResult* aPtrResult,
				ICustomisedRequest* aPtrICustomisedRequest)
			{
				HRESULT lresult;

				do
				{

					LOG_CHECK_PTR_MEMORY(aPtrResult);
					
					LOG_CHECK_PTR_MEMORY(aPtrICustomisedRequest);
					
					CComQIPtrCustom<IMFSample> lReadySourceSample = aPtrICustomisedRequest->getPtrUnkSender();

					CComQIPtrCustom<IMFTopologyNode> lTopologyNode = aPtrICustomisedRequest->getPtrUnkSenderTopologyNode();

					LOG_CHECK_PTR_MEMORY(lTopologyNode);

					MF_TOPOLOGY_TYPE lTopologyType;

					LOG_INVOKE_MF_METHOD(GetNodeType,
						lTopologyNode,
						&lTopologyType);
					
					switch (lTopologyType)
					{
					case MF_TOPOLOGY_OUTPUT_NODE:
						LOG_INVOKE_FUNCTION(pushNewSampleToOutputNode,
							lReadySourceSample,
							lTopologyNode);
						break;
					case MF_TOPOLOGY_SOURCESTREAM_NODE:
						break;
					case MF_TOPOLOGY_TRANSFORM_NODE:
						LOG_INVOKE_FUNCTION(pullPushNewSampleFromToTransform,
							lReadySourceSample,
							lTopologyNode);
						break;
					case MF_TOPOLOGY_TEE_NODE:
						LOG_INVOKE_FUNCTION(pullPushNewSampleFromToTeeNode,
							lReadySourceSample,
							lTopologyNode);
						break;
					case MF_TOPOLOGY_MAX:
						break;
					default:
						break;
					}

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::pullNewSampleFromSourceStream(
				IMFTopologyNode* aPtrTopologyNode)
			{
				HRESULT lresult;
				CComPtrCustom<IMFMediaEvent> lEvent;
				CComPtrCustom<ICustomisedRequest> lRequest;
				do
				{

					LOG_CHECK_PTR_MEMORY(aPtrTopologyNode);

					UINT32 lMediaStreamIndex;

					LOG_INVOKE_MF_METHOD(GetUINT32,
						aPtrTopologyNode,
						MediaStreamID,
						&lMediaStreamIndex);
					
					auto lMediaStream = mMediaStreams[lMediaStreamIndex];
					
					LOG_INVOKE_POINTER_METHOD(lMediaStream, RequestSample, nullptr);
					
				} while (false);

				return lresult;
			}

			//INT g_count = 40;

			HRESULT CustomisedMediaPipelineProcessor::pullPushNewSampleFromToTransform(
				IMFSample* aPtrSample,
				IMFTopologyNode* aPtrTopologyNode)
			{
				HRESULT lresult;

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrTopologyNode);
					
					CComQIPtrCustom<IMFTransform> lTransform;

					CComPtrCustom<IUnknown> lUnkTransform;

					LOG_INVOKE_MF_METHOD(GetObject,
						aPtrTopologyNode,
						&lUnkTransform);
					
					lTransform = lUnkTransform;

					LOG_CHECK_PTR_MEMORY(lTransform);
						
					MFT_INPUT_STREAM_INFO loutputStreamInfo;

					ZeroMemory(&loutputStreamInfo, sizeof(loutputStreamInfo));

					LOG_INVOKE_MF_METHOD(GetInputStreamInfo, lTransform, 0, &loutputStreamInfo);

					if (aPtrSample != nullptr)
					{
						LOG_INVOKE_MF_METHOD(ProcessInput, lTransform, 0, aPtrSample, 0);
					}

					CComPtrCustom<IMFTopologyNode> lDownStreamTopologyNode;

					CComPtrCustom<IMFSample> lSample;

					if (lresult == S_FALSE)
					{
						lresult = MF_E_TRANSFORM_NEED_MORE_INPUT;
					}
					else
					{

						MFT_OUTPUT_DATA_BUFFER loutputDataBuffer;

						LOG_INVOKE_FUNCTION(initOutputDataBuffer,
							lTransform,
							loutputDataBuffer);

						DWORD lprocessOutputStatus = 0;

						lresult = lTransform->ProcessOutput(
							0,
							1,
							&loutputDataBuffer,
							&lprocessOutputStatus);

						CComPtrCustom<IMFSample> lInnerSample(loutputDataBuffer.pSample);

						lSample = lInnerSample;

						DWORD lOutputIndex;

						aPtrTopologyNode->GetOutput(
							0,
							&lDownStreamTopologyNode,
							&lOutputIndex);

						if (!lDownStreamTopologyNode)
						{
							if (mGUIDMajorType == MFMediaType_Audio)
							{
								lresult = MF_E_TRANSFORM_NEED_MORE_INPUT;
							}
						}
					}


					if (lresult == MF_E_TRANSFORM_STREAM_CHANGE)
					{

						CComPtrCustom<IMFMediaType> lOutputMediaType;

						lOutputMediaType.Release();

						DWORD lOutputIndex = 0;

						LOG_INVOKE_MF_METHOD(GetOutputAvailableType,
							lTransform,
							0,
							lOutputIndex,
							&lOutputMediaType);

						//using namespace pugi;

						//xml_document lxmlDoc;

						//auto ldeclNode = lxmlDoc.append_child(node_declaration);

						//ldeclNode.append_attribute(L"version") = L"1.0";

						//xml_node lcommentNode = lxmlDoc.append_child(node_comment);

						//lcommentNode.set_value(L"XML Document of sources");

						//auto lRootXMLElement = lxmlDoc.append_child(L"Sources");
						//DataParser::readMediaType(
						//	lOutputMediaType,
						//	lRootXMLElement);

						//std::wstringstream lwstringstream;

						//lxmlDoc.print(lwstringstream);

						//std::wstring lXMLDocumentString;

						//lXMLDocumentString = lwstringstream.str();

						LOG_INVOKE_MF_METHOD(SetOutputType, lTransform, 0, lOutputMediaType, 0);

						CComPtrCustom<IMFMediaType> lMediaType;

						LOG_INVOKE_MF_METHOD(GetOutputCurrentType, lTransform, 0, &lMediaType);

						LOG_CHECK_PTR_MEMORY(lMediaType);

						if (mGUIDMajorType == MFMediaType_Video)
						{
							CComPtrCustom<ITopologyResolver> lVideoTopologyResolver(new (std::nothrow)CaptureManager::MediaSession::CustomisedMediaSession::VideoTopologyResolver());

							LOG_CHECK_PTR_MEMORY(lVideoTopologyResolver);

							do
							{
								LOG_INVOKE_POINTER_METHOD(mPtrCustomisedMediaSession, resetTargetNode,
									aPtrTopologyNode,
									lMediaType,
									lVideoTopologyResolver);

							} while (false);




							MFT_OUTPUT_DATA_BUFFER loutputDataBuffer;

							LOG_INVOKE_FUNCTION(initOutputDataBuffer,
								lTransform,
								loutputDataBuffer);

							DWORD lprocessOutputStatus = 0;

							lresult = lTransform->ProcessOutput(
								0,
								1,
								&loutputDataBuffer,
								&lprocessOutputStatus);

							CComPtrCustom<IMFSample> lInnerSample(loutputDataBuffer.pSample);

							lSample = lInnerSample;



							lDownStreamTopologyNode.Release();

							DWORD lOutputIndex;

							aPtrTopologyNode->GetOutput(
								0,
								&lDownStreamTopologyNode,
								&lOutputIndex);

							if (!lDownStreamTopologyNode)
							{
								if (mGUIDMajorType == MFMediaType_Audio)
								{
									lresult = MF_E_TRANSFORM_NEED_MORE_INPUT;
								}
							}

						}
						else if (mGUIDMajorType == MFMediaType_Audio)
						{

						}
					}



					if (lresult == MF_E_TRANSFORM_TYPE_NOT_SET)
					{
					}
					else	
					if (lresult == MF_E_TRANSFORM_NEED_MORE_INPUT)
					{
						DWORD lOutputNodeCount;

						LOG_INVOKE_MF_METHOD(GetOutputCount,
							aPtrTopologyNode,
							&lOutputNodeCount);
							
						CComPtrCustom<IMFTopologyNode> lUpStreamTopologyNode;

						DWORD lOutputIndex;

						LOG_INVOKE_MF_METHOD(GetInput,
							aPtrTopologyNode,
							0,
							&lUpStreamTopologyNode,
							&lOutputIndex);
							
						CComPtrCustom<ICustomisedRequest> lPullNewSampleRequest = new (std::nothrow) CustomisedRequest(
							ICustomisedRequest::PullNewSampleRequest,
							nullptr,
							lUpStreamTopologyNode,
							lOutputIndex);

						LOG_CHECK_PTR_MEMORY(lPullNewSampleRequest);
							
						LOG_INVOKE_MF_FUNCTION(MFPutWorkItem,
							mSyncWorkerQueue,
							this,
							lPullNewSampleRequest);
					}
					else if (lresult == S_FALSE)
					{
						lresult = S_OK;
					}
					else if (SUCCEEDED(lresult))
					{

						if (!lDownStreamTopologyNode)
						{
							lresult = S_OK;
						}
						else
						{
							UINT32 lIncludeHeader = FALSE;

							lresult = aPtrTopologyNode->GetUINT32(CM_HEADER, &lIncludeHeader);

							if (SUCCEEDED(lresult) && mGUIDMajorType == MFMediaType_Video)
							{
								UINT32 lvalue;

								auto ltempresult = lSample->GetUINT32(MFSampleExtension_CleanPoint, &lvalue);

								if (SUCCEEDED(ltempresult) && lvalue != FALSE)
								{
									lIncludeHeader = TRUE;
								}
							}

							if (SUCCEEDED(lresult) && lIncludeHeader != FALSE)
							{								
								CComPtrCustom<IMFMediaType> lOutputMediaType;

								LOG_INVOKE_MF_METHOD(GetOutputCurrentType, lTransform, 0, &lOutputMediaType);

								if (lOutputMediaType)
								{

									UINT32 lBlobSize = 0;

									UINT8 * lPtrBlob = nullptr;

									lresult = lOutputMediaType->GetAllocatedBlob(MF_MT_MPEG_SEQUENCE_HEADER, &lPtrBlob, &lBlobSize);

									if (SUCCEEDED(lresult))
									{
										lSample->SetBlob(CM_HEADER, lPtrBlob, lBlobSize);
									}

									GUID lSubType;

									lresult = lOutputMediaType->GetGUID(MF_MT_SUBTYPE, &lSubType);
									
									if (mGUIDMajorType == MFMediaType_Audio &&
										SUCCEEDED(lresult)&&
										lSubType == MFAudioFormat_AAC)
									{
										UINT32 sampleRate = 48000;

										UINT32 channels = 2;

										lresult = lOutputMediaType->GetUINT32(MF_MT_AUDIO_SAMPLES_PER_SECOND, &sampleRate);

										lresult = lOutputMediaType->GetUINT32(MF_MT_AUDIO_NUM_CHANNELS, &channels);


										UINT8 extraData[3];

										UINT16 *extraData16 = (UINT16 *)extraData;
										UINT16 profile = 2; //Low Complexity
#define SWAPU16(x) (x>>8) | (x<<8)
										// Profile
										// XXXX X... .... ....
										*extraData16 = profile << 11;
										// Sample Index (3=48, 4=44.1)
										// .... .XXX X... ....
										

										*extraData16 |= (sampleRate == 48000 ? 3 : 4) << 7;
										// Channels
										// .... .... .XXX X...


										*extraData16 |= channels << 3;
										*extraData16 = SWAPU16(*extraData16);

										extraData[2] = 0;
#undef SWAPU16

										lSample->SetBlob(CM_HEADER, extraData, sizeof(extraData));
									}									

									CoTaskMemFree(lPtrBlob);
								}
								
								lresult = aPtrTopologyNode->SetUINT32(CM_HEADER, FALSE);
							}



							CComPtrCustom<ICustomisedRequest> lPullNewSampleRequest = new (std::nothrow) CustomisedRequest(
								ICustomisedRequest::PushNewSampleRequest,
								lSample,
								lDownStreamTopologyNode);

							LOG_CHECK_PTR_MEMORY(lPullNewSampleRequest);

							LOG_INVOKE_MF_FUNCTION(MFPutWorkItem,
								mSyncWorkerQueue,
								this,
								lPullNewSampleRequest);
						}

					}
					else if (FAILED(lresult))
					{
						break;
					}
			

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::pullPushNewSampleFromToTeeNode(
				IMFSample* aPtrSample,
				IMFTopologyNode* aPtrTopologyNode,
				DWORD aIndexStreamNode)
			{
				HRESULT lresult;

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrTopologyNode);
					
					CComQIPtrCustom<ITeeNode> lTeeNode;

					CComPtrCustom<IUnknown> lUnkTransform;

					LOG_INVOKE_MF_METHOD(GetObject,
						aPtrTopologyNode,
						&lUnkTransform);
					
					lTeeNode = lUnkTransform;

					LOG_CHECK_PTR_MEMORY(lTeeNode);

					if (aPtrSample != nullptr)
					{
						std::vector<DWORD> lVectorOfOutputNodesIndexes;

						LOG_INVOKE_POINTER_METHOD(lTeeNode, processInput,
							aPtrSample,
							lVectorOfOutputNodesIndexes);
						
						DWORD lOutputNodeCount;

						LOG_INVOKE_MF_METHOD(GetOutputCount,
							aPtrTopologyNode,
							&lOutputNodeCount);
						
						for (DWORD lIndexOfRegisteredOutputNode = 0;
							lIndexOfRegisteredOutputNode < lOutputNodeCount;
							lIndexOfRegisteredOutputNode++)
						{

							DWORD lIndex = lIndexOfRegisteredOutputNode;

							CComPtrCustom<IMFTopologyNode> lDownStreamTopologyNode;

							DWORD lOutputIndex;

							LOG_INVOKE_MF_METHOD(GetOutput,
								aPtrTopologyNode,
								lIndex,
								&lDownStreamTopologyNode,
								&lOutputIndex);
							
							CComPtrCustom<ICustomisedRequest> lPushNewSampleRequest = new (std::nothrow) CustomisedRequest(
								ICustomisedRequest::PushNewSampleRequest,
								aPtrSample,
								lDownStreamTopologyNode);

							LOG_CHECK_PTR_MEMORY(lPushNewSampleRequest);
							
							LOG_INVOKE_MF_FUNCTION(MFPutWorkItem,
								mSyncWorkerQueue,
								this,
								lPushNewSampleRequest);
						}
					}
					else
					{
						LOG_INVOKE_POINTER_METHOD(lTeeNode, registerOutputNodeRequest, aIndexStreamNode);
						
						if (lresult == S_OK)
						{
							CComPtrCustom<IMFTopologyNode> lUpStreamTopologyNode;

							DWORD lOutputIndex;

							LOG_INVOKE_MF_METHOD(GetInput,
								aPtrTopologyNode,
								0,
								&lUpStreamTopologyNode,
								&lOutputIndex);
							
							CComPtrCustom<ICustomisedRequest> lPullNewSampleRequest = new (std::nothrow) CustomisedRequest(
								ICustomisedRequest::PullNewSampleRequest,
								nullptr,
								lUpStreamTopologyNode,
								lOutputIndex);

							LOG_CHECK_PTR_MEMORY(lPullNewSampleRequest);
							
							LOG_INVOKE_MF_FUNCTION(MFPutWorkItem,
								mSyncWorkerQueue,
								this,
								lPullNewSampleRequest);
						}
					}
				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::pushNewSampleToOutputNode(
				IMFSample* aPtrSample,
				IMFTopologyNode* aPtrTopologyNode)
			{
				HRESULT lresult;
				CComPtrCustom<IMFMediaEvent> lEvent;

				do
				{

					lresult = mPtrCustomisedMediaSession->checkFinishBarier();

					if (FAILED(lresult))
					{
						lresult = S_OK;

						break;
					}

					LOG_CHECK_PTR_MEMORY(aPtrSample);
					
					LOG_CHECK_PTR_MEMORY(aPtrTopologyNode);
					

					CComPtrCustom<IUnknown> lActivateUnknown;

					LOG_INVOKE_MF_METHOD(GetObject,
						aPtrTopologyNode,
						&lActivateUnknown);

					LOG_CHECK_PTR_MEMORY(lActivateUnknown);

					CComQIPtrCustom<IMFActivate> lSinkActivate = lActivateUnknown;

					LOG_CHECK_PTR_MEMORY(lSinkActivate);

					UINT32 lStreamID;

					LOG_INVOKE_MF_METHOD(GetUINT32,
						aPtrTopologyNode,
						MF_TOPONODE_STREAMID,
						&lStreamID);

					CComPtrCustom<IMFMediaSink> lMediaSink;

					LOG_INVOKE_MF_METHOD(ActivateObject,
						lSinkActivate,
						IID_PPV_ARGS(&lMediaSink));

					CComPtrCustom<IMFStreamSink> lStreamSink;

					LOG_INVOKE_MF_METHOD(GetStreamSinkById,
						lMediaSink,
						lStreamID,
						&lStreamSink);

					DWORD lflag = 0;
						
					UINT32 lvalue;

					lresult = aPtrSample->GetUINT32(MFSampleExtension_CleanPoint, &lvalue);

					if (SUCCEEDED(lresult) && lvalue != FALSE)
					{
						lflag |= CleanPoint;
					}

					aPtrSample->SetSampleFlags(lflag);


					//LONGLONG lSampleDuration;

					//LOG_INVOKE_MF_METHOD(GetSampleDuration, aPtrSample, &lSampleDuration);

					//LONGLONG lSampleTimeDevice;

					//LOG_INVOKE_MF_METHOD(GetSampleTime, aPtrSample, &lSampleTimeDevice);

					//LogPrintOut::getInstance().printOutln(
					//	LogPrintOut::ERROR_LEVEL,
					//	L"PipeLine - ",
					//	L"lSampleDuration: ",
					//	lSampleDuration,
					//	L" lSampleTimeDevice: ",
					//	lSampleTimeDevice);
										
					if (m_processSampleAccess)
						LOG_INVOKE_MF_METHOD(ProcessSample, lStreamSink, aPtrSample);
					
				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::startSources(
				const GUID* aConstPtrGUIDTimeFormat,
				const PROPVARIANT* aConstPtrVarStartPosition)
			{
				HRESULT lresult(E_FAIL);

				do
				{
					LOG_CHECK_PTR_MEMORY(mCurrentTopology);

					m_processSampleAccess = true;
					
					if (mCurrentState == State::Paused ||
						mCurrentState == State::Stopped)
					{
						for (auto& litem : mPrevSampleTimes)
						{
							litem = 0;
						}
					}

					mSourceStreamCount = mCollectionOfIDs.mListOfSourceIDs.size();

					for (auto lItem : mCollectionOfIDs.mListOfSourceIDs)
					{
						CComQIPtrCustom<IMFTopologyNode> lSourceTopologyNode;

						LOG_INVOKE_MF_METHOD(GetNodeByID,
							mCurrentTopology,
							lItem,
							&lSourceTopologyNode);
						
						LOG_CHECK_PTR_MEMORY(lSourceTopologyNode);
						
						CComPtrCustom<IUnknown> lUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_STREAM_DESCRIPTOR,
							__uuidof(IMFStreamDescriptor),
							(void**)&lUnknown);
						
						LOG_CHECK_PTR_MEMORY(lUnknown);
						
						CComQIPtrCustom<IMFStreamDescriptor> lStreamDescriptor = lUnknown;

						LOG_CHECK_PTR_MEMORY(lStreamDescriptor);
						
						CComPtrCustom<IMFMediaTypeHandler> lMediaTypeHandler;

						LOG_INVOKE_MF_METHOD(GetMediaTypeHandler,
							lStreamDescriptor,
							&lMediaTypeHandler);
						
						LOG_CHECK_PTR_MEMORY(lMediaTypeHandler);
						
						LOG_INVOKE_MF_METHOD(GetMajorType,
							lMediaTypeHandler,
							&mGUIDMajorType);
						
						CComPtrCustom<IUnknown> lMediaSourceUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_SOURCE,
							__uuidof(IMFMediaSource),
							(void**)&lMediaSourceUnknown);
						
						LOG_CHECK_PTR_MEMORY(lMediaSourceUnknown);
						
						CComQIPtrCustom<IMFMediaSource> lMediaSource = lMediaSourceUnknown;

						LOG_CHECK_PTR_MEMORY(lMediaSource);

						DWORD lCharacteristics;

						LOG_INVOKE_MF_METHOD(GetCharacteristics, lMediaSource, &lCharacteristics);
						
						CComPtrCustom<IUnknown> lPresentationDescriptorUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_PRESENTATION_DESCRIPTOR,
							__uuidof(IMFPresentationDescriptor),
							(void**)&lPresentationDescriptorUnknown);
						
						LOG_CHECK_PTR_MEMORY(lPresentationDescriptorUnknown);

						CComQIPtrCustom<IMFPresentationDescriptor> lPresentationDescriptor = lPresentationDescriptorUnknown;

						LOG_CHECK_PTR_MEMORY(lPresentationDescriptor);
						
						do
						{
							//PROPVARIANT ltime;

							//if (mCurrentTime != 0)
							//	mCurrentTime = MediaFoundation::MediaFoundationManager::MFGetSystemTime();

							//InitPropVariantFromInt64(mCurrentTime, &ltime);												

							LOG_INVOKE_MF_METHOD(Start, lMediaSource,
								lPresentationDescriptor,
								aConstPtrGUIDTimeFormat,
								//&ltime
								aConstPtrVarStartPosition
								);

						} while (false);

						if (lresult == MF_E_HW_MFT_FAILED_START_STREAMING)
						{
							mStopWaitingTime = 1;

							break;
						}

					}

					if (FAILED(lresult))
					{
						break;
					}

					do
					{
						LOG_INVOKE_FUNCTION(finishSourceStreamStarting);

						mCurrentState = State::Started;
						
						WORD lNodeCount = 0;

						mCurrentTopology->GetNodeCount(&lNodeCount);

						for (WORD i = 0; i < lNodeCount; i++)
						{
							CComQIPtrCustom<IMFTopologyNode> lTopologyNode;

							mCurrentTopology->GetNode(
								i,
								&lTopologyNode);

							if (lTopologyNode)
							{
								CComQIPtrCustom<ITeeNode> lTeeNode;

								CComPtrCustom<IUnknown> lUnkTransform;

								lTopologyNode->GetObject(
									&lUnkTransform);

								lTeeNode = lUnkTransform;

								if (lTeeNode)
								{
									lTeeNode->reset();
								}
							}
						}

					} while (false);

					if (FAILED(lresult))
					{
						lresult = MF_E_HW_MFT_FAILED_START_STREAMING;

						mStopWaitingTime = 1;

						break;
					}

				} while (false);
				
				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::pauseSources()
			{
				HRESULT lresult(E_FAIL);

				do
				{
					LOG_CHECK_PTR_MEMORY(mCurrentTopology);

					m_processSampleAccess = false;

					mSourceStreamPausingCount = mCollectionOfIDs.mListOfSourceIDs.size();

					for (auto lItem : mCollectionOfIDs.mListOfSourceIDs)
					{
						CComQIPtrCustom<IMFTopologyNode> lSourceTopologyNode;

						LOG_INVOKE_MF_METHOD(GetNodeByID,
							mCurrentTopology,
							lItem,
							&lSourceTopologyNode);
						
						LOG_CHECK_PTR_MEMORY(lSourceTopologyNode);
						
						CComPtrCustom<IUnknown> lMediaSourceUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_SOURCE,
							__uuidof(IMFMediaSource),
							(void**)&lMediaSourceUnknown);
						
						LOG_CHECK_PTR_MEMORY(lMediaSourceUnknown);
						
						CComQIPtrCustom<IMFMediaSource> lMediaSource = lMediaSourceUnknown;

						LOG_CHECK_PTR_MEMORY(lMediaSource);
						
						LOG_INVOKE_MF_METHOD(Pause, lMediaSource);
					}

					if (FAILED(lresult))
					{
						break;
					}

					LOG_INVOKE_FUNCTION(finishSourceStreamPausing);
					
					mCurrentState = State::Paused;
															
				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::stopSources()
			{
				HRESULT lresult(E_FAIL);

				do
				{
					LOG_CHECK_PTR_MEMORY(mCurrentTopology);

					m_processSampleAccess = false;

					mSourceStreamPausingCount = mCollectionOfIDs.mListOfSourceIDs.size();

					for (auto lItem : mCollectionOfIDs.mListOfSourceIDs)
					{
						CComQIPtrCustom<IMFTopologyNode> lSourceTopologyNode;

						LOG_INVOKE_MF_METHOD(GetNodeByID,
							mCurrentTopology,
							lItem,
							&lSourceTopologyNode);
						
						LOG_CHECK_PTR_MEMORY(lSourceTopologyNode);
						
						CComPtrCustom<IUnknown> lMediaSourceUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_SOURCE,
							__uuidof(IMFMediaSource),
							(void**)&lMediaSourceUnknown);
						
						LOG_CHECK_PTR_MEMORY(lMediaSourceUnknown);
						
						CComQIPtrCustom<IMFMediaSource> lMediaSource = lMediaSourceUnknown;

						LOG_CHECK_PTR_MEMORY(lMediaSource);
						
						LOG_INVOKE_MF_METHOD(Stop, lMediaSource);
					}

					if (FAILED(lresult))
					{
						break;
					}

					LOG_INVOKE_FUNCTION(finishSourceStreamStopping);
					
					mCurrentState = State::Stopped;

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::findAndRegisterSwitcherNode(const TOPOID& aRefSwitcherNodeID, IMFTopologyNode* aPtrTopologyNode)
			{
				HRESULT lresult(E_FAIL);

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrTopologyNode);

					MF_TOPOLOGY_TYPE lTopoNodeType;

					LOG_INVOKE_MF_METHOD(GetNodeType,
						aPtrTopologyNode,
						&lTopoNodeType);

					switch (lTopoNodeType)
					{
					case MF_TOPOLOGY_OUTPUT_NODE:
						break;
					case MF_TOPOLOGY_SOURCESTREAM_NODE:
						break;
					case MF_TOPOLOGY_TRANSFORM_NODE:
					{
						UINT32 lState(0);

						do
						{
							LOG_INVOKE_MF_METHOD(GetUINT32, aPtrTopologyNode, CM_SwitcherNode, &lState);

							if (lState != TRUE)
								break;

							TOPOID lTOPOID(0);

							aPtrTopologyNode->GetTopoNodeID(&lTOPOID);

							if (aRefSwitcherNodeID == lTOPOID)
								return S_OK;

						} while (false);

					}
					break;
					case MF_TOPOLOGY_TEE_NODE:
						break;
					case MF_TOPOLOGY_MAX:
						break;
					default:
						break;
					}

					lresult = E_FAIL;
					
					DWORD lOutputCount(0);

					LOG_INVOKE_MF_METHOD(GetOutputCount, aPtrTopologyNode, &lOutputCount);

					if (lOutputCount == 0)
					{
						lresult = E_FAIL;

						break;
					}

					for (size_t i = 0; i < lOutputCount; i++)
					{
						CComPtrCustom<IMFTopologyNode> lDownStreamNode;

						DWORD lInputIndexOnDownStreamNode(0);

						LOG_INVOKE_MF_METHOD(GetOutput, aPtrTopologyNode, i, &lDownStreamNode, &lInputIndexOnDownStreamNode);

						lresult = findAndRegisterSwitcherNode(
							aRefSwitcherNodeID,
							lDownStreamNode);

						if (SUCCEEDED(lresult))
							break;
					}

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::init(
				CollectionOfIDs& aRefCollectionOfIDs,
				IMFTopology* aPtrTopology,
				IMFPresentationClock* aPtrPresentationClock)
			{
				HRESULT lresult;

				do
				{

					LOG_CHECK_PTR_MEMORY(aPtrTopology);

					LOG_CHECK_PTR_MEMORY(aPtrPresentationClock);

					LOG_INVOKE_FUNCTION(MediaFoundation::MediaFoundationManager::GetLockWorkQueue,
						&mSyncWorkerQueue);

					CComQIPtrCustom<IPresentationClock> l_PresentationClock;
					
					l_PresentationClock = aPtrPresentationClock;

					if (l_PresentationClock)
						l_PresentationClock->addIMediaPipelineProcessorControl(this);
					
					mCollectionOfIDs = aRefCollectionOfIDs;

					mCurrentTopology = aPtrTopology;

					for (auto lItem : aRefCollectionOfIDs.mListOfSourceIDs)
					{
						CComQIPtrCustom<IMFTopologyNode> lSourceTopologyNode;

						LOG_INVOKE_MF_METHOD(GetNodeByID,
							aPtrTopology,
							lItem,
							&lSourceTopologyNode);
						
						LOG_CHECK_PTR_MEMORY(lSourceTopologyNode);
						
						CComPtrCustom<IUnknown> lUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_STREAM_DESCRIPTOR,
							__uuidof(IMFStreamDescriptor),
							(void**)&lUnknown);
						
						LOG_CHECK_PTR_MEMORY(lUnknown);
						
						CComQIPtrCustom<IMFStreamDescriptor> lStreamDescriptor = lUnknown;

						LOG_CHECK_PTR_MEMORY(lStreamDescriptor);
						
						CComPtrCustom<IMFMediaTypeHandler> lMediaTypeHandler;

						LOG_INVOKE_MF_METHOD(GetMediaTypeHandler,
							lStreamDescriptor,
							&lMediaTypeHandler);
						
						LOG_CHECK_PTR_MEMORY(lMediaTypeHandler)
						
						LOG_INVOKE_MF_METHOD(GetMajorType,
							lMediaTypeHandler,
							&mGUIDMajorType);
						
						CComPtrCustom<IUnknown> lMediaSourceUnknown;

						LOG_INVOKE_MF_METHOD(GetUnknown,
							lSourceTopologyNode,
							MF_TOPONODE_SOURCE,
							__uuidof(IMFMediaSource),
							(void**)&lMediaSourceUnknown);
						
						LOG_CHECK_PTR_MEMORY(lMediaSourceUnknown);
						
						CComQIPtrCustom<IMFMediaSource> lMediaSource = lMediaSourceUnknown;

						LOG_CHECK_PTR_MEMORY(lMediaSource);
						
						CComPtrCustom<IUnknown> lRequest = new (std::nothrow) CustomisedRequest(
							ICustomisedRequest::SourceRequest,
							lMediaSource,
							lSourceTopologyNode);

						LOG_CHECK_PTR_MEMORY(lRequest);
						
						LOG_INVOKE_MF_METHOD(BeginGetEvent,
							lMediaSource,
							this,
							lRequest);
					}

					LOG_CHECK_STATE_DESCR(FAILED(lresult), lresult);
					
					mStreamSinks.clear();

					mSampleTimes.clear();

					mPrevSampleTimes.clear();

					for (auto lItem : aRefCollectionOfIDs.mListOfSinkIDs)
					{
						CComPtrCustom<IMFTopologyNode> lOutputTopologyNode;

						LOG_INVOKE_MF_METHOD(GetNodeByID,
							aPtrTopology,
							lItem,
							&lOutputTopologyNode);
						
						LOG_CHECK_PTR_MEMORY(lOutputTopologyNode);
						
						CComPtrCustom<IUnknown> lActivateUnknown;

						LOG_INVOKE_MF_METHOD(GetObject,
							lOutputTopologyNode,
							&lActivateUnknown);
						
						LOG_CHECK_PTR_MEMORY(lActivateUnknown);
						
						CComQIPtrCustom<IMFActivate> lSinkActivate = lActivateUnknown;

						LOG_CHECK_PTR_MEMORY(lSinkActivate);
						
						UINT32 lStreamID;

						LOG_INVOKE_MF_METHOD(GetUINT32,
							lOutputTopologyNode,
							MF_TOPONODE_STREAMID,
							&lStreamID);
						
						CComPtrCustom<IMFMediaSink> lMediaSink;

						LOG_INVOKE_MF_METHOD(ActivateObject,
							lSinkActivate,
							IID_PPV_ARGS(&lMediaSink));
						
						CComPtrCustom<IMFStreamSink> lStreamSink;

						LOG_INVOKE_MF_METHOD(GetStreamSinkById,
							lMediaSink,
							lStreamID,
							&lStreamSink);
						
						
						mStreamSinks.push_back(lStreamSink);

						CComPtrCustom<IUnknown> lRequest = new (std::nothrow) CustomisedRequest(
							ICustomisedRequest::StreamSinkRequest,
							lStreamSink,
							lOutputTopologyNode);

						LOG_CHECK_PTR_MEMORY(lRequest);
						
						LOG_INVOKE_MF_METHOD(BeginGetEvent,
							lStreamSink,
							this,
							lRequest);

						UINT32 lIsSampleGRabber = FALSE;

						do
						{

							UINT32 lValue = FALSE;

							LOG_INVOKE_MF_METHOD(GetUINT32,
								lOutputTopologyNode,
								MF_MT_SAMPLEGRABBER,
								&lValue);

							lIsSampleGRabber = lValue;

						} while (false);
						
						if (lIsSampleGRabber == TRUE)
						{
							CComPtrCustom<IMFPresentationClock> lOriginalPresentationClock;

							l_PresentationClock->getPresentationClock(&lOriginalPresentationClock);

							if (lOriginalPresentationClock)
							{
								LOG_INVOKE_MF_METHOD(SetPresentationClock,
									lMediaSink,
									lOriginalPresentationClock);
							}
						}
						else
							LOG_INVOKE_MF_METHOD(SetPresentationClock,
								lMediaSink,
								aPtrPresentationClock);
					}

					mStreamSinkCount = mStreamSinks.size();
					
				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::initOutputDataBuffer(IMFTransform* aPtrTransform,
				MFT_OUTPUT_DATA_BUFFER& aRefOutputBuffer)
			{
				HRESULT lresult;

				MFT_OUTPUT_STREAM_INFO loutputStreamInfo;

				DWORD loutputStreamId = 0;

				CComPtrCustom<IMFSample> lOutputSample;

				CComPtrCustom<IMFMediaBuffer> lMediaBuffer;

				do
				{
					LOG_CHECK_PTR_MEMORY(aPtrTransform);
					
					ZeroMemory(&loutputStreamInfo, sizeof(loutputStreamInfo));

					ZeroMemory(&aRefOutputBuffer, sizeof(aRefOutputBuffer));

					LOG_INVOKE_MF_METHOD(GetOutputStreamInfo, aPtrTransform, loutputStreamId, &loutputStreamInfo);
					
					if ((loutputStreamInfo.dwFlags & MFT_OUTPUT_STREAM_PROVIDES_SAMPLES) == 0 &&
						(loutputStreamInfo.dwFlags & MFT_OUTPUT_STREAM_CAN_PROVIDE_SAMPLES) == 0)
					{
						LOG_INVOKE_MF_FUNCTION(MFCreateSample,
							&lOutputSample);
						
						LOG_INVOKE_MF_FUNCTION(MFCreateMemoryBuffer, loutputStreamInfo.cbSize, &lMediaBuffer);
						
						LOG_INVOKE_MF_METHOD(AddBuffer, lOutputSample, lMediaBuffer);
						
						aRefOutputBuffer.pSample = lOutputSample.Detach();
					}
					else
					{
						lresult = S_OK;
					}

					aRefOutputBuffer.dwStreamID = loutputStreamId;
				} while (false);

				return lresult;
			}
			

			// IMediaPipeline interface

			HRESULT CustomisedMediaPipelineProcessor::finishSourceStreamPausing()
			{
				HRESULT lresult;

				do
				{
					std::unique_lock<std::mutex> lLock(mMediaPipelineMutex);

					mSourceStreamPausingCount = mCollectionOfIDs.mListOfSourceIDs.size();

					auto lconditionResult = mSourceStreamPausingCondition.wait_for(
						lLock,
						std::chrono::seconds(5),
						[this]{return mSourceStreamPausingCount == 0; });

					if (lconditionResult == false)
					{

						lresult = CONTEXT_E_SYNCH_TIMEOUT;

						LogPrintOut::getInstance().printOutln(
							LogPrintOut::ERROR_LEVEL,
							__FUNCTIONW__,
							L" Error code: ",
							lresult);
					}
					else
					{
						lresult = S_OK;
					}

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::finishStreamSinkStopping()
			{
				HRESULT lresult;

				do
				{
					std::unique_lock<std::mutex> lLock(mMediaPipelineMutex);

					auto lconditionResult = mStreamSinkStoppingCondition.wait_for(
						lLock,
						std::chrono::seconds(5),
						[this]{return mStreamSinkCount == 0; });

					if (lconditionResult == false)
					{

						lresult = CONTEXT_E_SYNCH_TIMEOUT;

						LogPrintOut::getInstance().printOutln(
							LogPrintOut::ERROR_LEVEL,
							__FUNCTIONW__,
							L" Error code: ",
							lresult);
					}
					else
					{
						lresult = S_OK;
					}

					mStreamSinkCount = mStreamSinks.size();

					for (auto& litem : mSampleTimes)
					{
						litem = 0;
					}

					for (auto& litem : mPrevSampleTimes)
					{
						litem = 0;
					}

					lresult = S_OK;

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::finishStreamSinkPausing()
			{
				HRESULT lresult;

				do
				{
					std::unique_lock<std::mutex> lLock(mMediaPipelineMutex);

					auto lconditionResult = mStreamSinkPausingCondition.wait_for(
						lLock,
						std::chrono::seconds(5),
						[this]{return mStreamSinkCount == 0; });

					if (lconditionResult == false)
					{

						lresult = CONTEXT_E_SYNCH_TIMEOUT;

						LogPrintOut::getInstance().printOutln(
							LogPrintOut::ERROR_LEVEL,
							__FUNCTIONW__,
							L" Error code: ",
							lresult);
					}
					else
					{
						lresult = S_OK;
					}

					mStreamSinkCount = mStreamSinks.size();

					lresult = S_OK;

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::finishSourceStreamStopping()
			{
				HRESULT lresult;

				do
				{

					std::unique_lock<std::mutex> lLock(mMediaPipelineMutex);

					auto lconditionResult = mSourceStreamStoppingCondition.wait_for(
						lLock,
						std::chrono::seconds(mStopWaitingTime),
						[this]{return mSourceStreamCount == 0; });

					if (lconditionResult == false)
					{

						lresult = CONTEXT_E_SYNCH_TIMEOUT;

						LogPrintOut::getInstance().printOutln(
							LogPrintOut::ERROR_LEVEL,
							__FUNCTIONW__,
							L" Error code: ",
							lresult);
					}
					else
					{
						lresult = S_OK;
					}

					mStopWaitingTime = 5;

					mSourceStreamCount = mMediaStreams.size();

					lresult = S_OK;

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::finishSourceStreamStarting()
			{
				HRESULT lresult;

				do
				{
					std::unique_lock<std::mutex> lLock(mSourceStartingPipelineMutex);

					auto lconditionResult = mSourceStreamStartingCondition.wait_for(
						lLock,
						std::chrono::seconds(8),
						[this]{return mSourceStreamCount == 0; });
										
					if (lconditionResult == false)
					{

						lresult = CONTEXT_E_SYNCH_TIMEOUT;

						LogPrintOut::getInstance().printOutln(
							LogPrintOut::ERROR_LEVEL,
							__FUNCTIONW__,
							L" Error code: ",
							lresult);
					}
					else
					{
						lresult = S_OK;
					}

					mSourceStreamCount = mMediaStreams.size();

				} while (false);

				return lresult;
			}



			// IMediaPipelineProcessorControl interface

			HRESULT CustomisedMediaPipelineProcessor::pause()
			{
				HRESULT lresult(E_FAIL);

				do
				{

					std::lock_guard<std::mutex> lLock(mMediaPipelineMutex);

					do
					{
						--mStreamSinkCount;

						mStreamSinkPausingCondition.notify_one();

					} while (mStreamSinkCount > 0);

					m_processSampleAccess = false;

					lresult = S_OK;

				} while (false);

				return lresult;
			}

			HRESULT CustomisedMediaPipelineProcessor::restart()
			{
				HRESULT lresult;
				CComPtrCustom<IMFMediaEvent> lEvent;
				CComPtrCustom<ICustomisedRequest> lRequest;
				do
				{
					m_processSampleAccess = true;

					for (auto& l_item : mMediaStreams)
					{
						auto lMediaStream = l_item.get();

						LOG_INVOKE_POINTER_METHOD(lMediaStream, RequestSample, nullptr);
					}

				} while (false);

				return lresult;
			}
			
			HRESULT CustomisedMediaPipelineProcessor::removeStreamSink(IMFStreamSink* aPtrStreamSink)
			{
				HRESULT lresult = S_OK;

				do
				{
					auto lfineIter = std::find_if(mStreamSinks.begin(),
						mStreamSinks.end(),
						[=](const CComPtrCustom<IMFStreamSink>& a) {
						return a.get() >= aPtrStreamSink;
					});
					
					if (lfineIter != mStreamSinks.end())
					{
						mStreamSinks.remove(*(lfineIter));
					}
																				
					mStreamSinkCount = mStreamSinks.size();

				} while (false);

				return lresult;
			}
			
			HRESULT CustomisedMediaPipelineProcessor::checkSwitcherNode(const TOPOID& aRefSwitcherNodeID)
			{
				HRESULT lresult = S_OK;

				do
				{
					for (auto lItem : mCollectionOfIDs.mListOfSourceIDs)
					{
						CComQIPtrCustom<IMFTopologyNode> lSourceTopologyNode;

						LOG_INVOKE_MF_METHOD(GetNodeByID,
							mCurrentTopology,
							lItem,
							&lSourceTopologyNode);

						if (lSourceTopologyNode)
						{
							lresult = findAndRegisterSwitcherNode(aRefSwitcherNodeID, lSourceTopologyNode);
						}

						if (SUCCEEDED(lresult))
							break;
					}									

				} while (false);

				return lresult;
			}
			
			HRESULT CustomisedMediaPipelineProcessor::registerStreamSink(const TOPOID& aRefStreamSinkNodeID,
				IMFPresentationClock* aPtrPresentationClock)
			{
				HRESULT lresult = S_OK;

				do
				{

					CComQIPtrCustom<IMFTopologyNode> lOutputTopologyNode;

					LOG_INVOKE_MF_METHOD(GetNodeByID,
						mCurrentTopology,
						aRefStreamSinkNodeID,
						&lOutputTopologyNode);

					LOG_CHECK_PTR_MEMORY(lOutputTopologyNode);

					CComPtrCustom<IUnknown> lActivateUnknown;

					LOG_INVOKE_MF_METHOD(GetObject,
						lOutputTopologyNode,
						&lActivateUnknown);

					LOG_CHECK_PTR_MEMORY(lActivateUnknown);

					CComQIPtrCustom<IMFActivate> lSinkActivate = lActivateUnknown;

					LOG_CHECK_PTR_MEMORY(lSinkActivate);

					UINT32 lStreamID;

					LOG_INVOKE_MF_METHOD(GetUINT32,
						lOutputTopologyNode,
						MF_TOPONODE_STREAMID,
						&lStreamID);

					CComPtrCustom<IMFMediaSink> lMediaSink;

					LOG_INVOKE_MF_METHOD(ActivateObject,
						lSinkActivate,
						IID_PPV_ARGS(&lMediaSink));

					CComPtrCustom<IMFStreamSink> lStreamSink;

					LOG_INVOKE_MF_METHOD(GetStreamSinkById,
						lMediaSink,
						lStreamID,
						&lStreamSink);
					
					mStreamSinks.push_back(lStreamSink);
					
					CComPtrCustom<IUnknown> lRequest = new (std::nothrow) CustomisedRequest(
						ICustomisedRequest::StreamSinkRequest,
						lStreamSink,
						lOutputTopologyNode);

					LOG_CHECK_PTR_MEMORY(lRequest);

					LOG_INVOKE_MF_METHOD(BeginGetEvent,
						lStreamSink,
						this,
						lRequest);

					LOG_INVOKE_MF_METHOD(SetPresentationClock,
						lMediaSink,
						aPtrPresentationClock);

					mStreamSinkCount = mStreamSinks.size();

				} while (false);

				return lresult;
			}
		}
	}
}