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
#include "Direct3D11VideoProcessor.h"
#include "../MediaFoundationManager/MediaFoundationManager.h"
#include "../Common/MFHeaders.h"
#include "../Common/Common.h"
#include "../Common/GUIDs.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "../DirectXManager/Direct3D11Manager.h"
#include "../DirectXManager/DXGIManager.h"
#include "../Common/Singleton.h"
#include "../ConfigManager/ConfigManager.h"
#include <list>
#include <DXGI1_2.h>
#include <D3d11.h>

namespace EVRMultiSink
{
   namespace Sinks
   {
      namespace EVR
      {
         namespace Mixer
         {
            using namespace CaptureManager;
            using namespace Core;
            GUID const* const g_pVideoFormats[] = {
               &MFVideoFormat_NV12, &MFVideoFormat_IYUV, &MFVideoFormat_YUY2, &MFVideoFormat_YV12, &MFVideoFormat_RGB32,
               &MFVideoFormat_RGB32, &MFVideoFormat_RGB24, &MFVideoFormat_RGB555, &MFVideoFormat_RGB565,
               &MFVideoFormat_RGB8, &MFVideoFormat_AYUV, &MFVideoFormat_UYVY, &MFVideoFormat_YVYU, &MFVideoFormat_YVU9,
               &MEDIASUBTYPE_V216, &MFVideoFormat_v410, &MFVideoFormat_I420, &MFVideoFormat_NV11, &MFVideoFormat_420O
            };
            const Direct3D11VideoProcessor::FormatEntry Direct3D11VideoProcessor::mDXGIFormatMapping[] = {
               {MFVideoFormat_RGB32, DXGI_FORMAT_B8G8R8X8_UNORM}, {MFVideoFormat_ARGB32, DXGI_FORMAT_R8G8B8A8_UNORM},
               {MFVideoFormat_AYUV, DXGI_FORMAT_AYUV}, {MFVideoFormat_YUY2, DXGI_FORMAT_YUY2},
               {MFVideoFormat_NV12, DXGI_FORMAT_NV12}, {MFVideoFormat_NV11, DXGI_FORMAT_NV11},
               {MFVideoFormat_AI44, DXGI_FORMAT_AI44}, {MFVideoFormat_P010, DXGI_FORMAT_P010},
               {MFVideoFormat_P016, DXGI_FORMAT_P016}, {MFVideoFormat_Y210, DXGI_FORMAT_Y210},
               {MFVideoFormat_Y216, DXGI_FORMAT_Y216}, {MFVideoFormat_Y410, DXGI_FORMAT_Y410},
               {MFVideoFormat_Y416, DXGI_FORMAT_Y416}, {MFVideoFormat_420O, DXGI_FORMAT_420_OPAQUE}
            };

            HRESULT Direct3D11VideoProcessor::createProcessor(IMFTransform** aPtrPtrTrsnaform,
                                                              DWORD aMaxInputStreamCount)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrPtrTrsnaform);
                  CComPtrCustom<Direct3D11VideoProcessor> lProcessor(new(std::nothrow) Direct3D11VideoProcessor);
                  LOG_CHECK_PTR_MEMORY(lProcessor);
                  int lMaxVideoRenderStreamCount = Singleton<ConfigManager>::getInstance().
                     getMaxVideoRenderStreamCount();
                  lProcessor->m_dwOutputIDs.push_back(0);
                  for (size_t i = 0; i < aMaxInputStreamCount; i++) {
                     lProcessor->m_dwInputIDs.push_back(i);
                     lProcessor->mdwZOrders.push_back(i);
                     D3D11_VIDEO_PROCESSOR_STREAM lStream;
                     ZeroMemory(&lStream, sizeof(lStream));
                     lProcessor->mInputStreams.push_back(lStream);
                  }
                  DWORD lInputMin = 0;
                  DWORD lInputMax = 0;
                  DWORD lOutputMin = 0;
                  DWORD lOutputMax = 0;
                  LOG_INVOKE_POINTER_METHOD(lProcessor, GetStreamLimits, &lInputMin, &lInputMax, &lOutputMin,
                                            &lOutputMax);
                  DWORD lInputIDStream;
                  DWORD lOutputIDStream;
                  LOG_INVOKE_POINTER_METHOD(lProcessor, GetStreamIDs, 1, &lInputIDStream, 1, &lOutputIDStream);
                  LOG_INVOKE_POINTER_METHOD(lProcessor, AddInputStreams, 1, &lInputIDStream);
                  LOG_INVOKE_QUERY_INTERFACE_METHOD(lProcessor, aPtrPtrTrsnaform);
               } while (false);
               return lresult;
            }

            Direct3D11VideoProcessor::Direct3D11VideoProcessor()
            {
               mBackgroundColor.RGBA.A = 1.0F;
               mBackgroundColor.RGBA.R = 0.0F; // *static_cast<float>(GetRValue(0)) / 255.0F;
               mBackgroundColor.RGBA.G = 0.0F; // *static_cast<float>(GetGValue(0)) / 255.0F;
               mBackgroundColor.RGBA.B = 0.0F; // *static_cast<float>(GetBValue(0)) / 255.0F;
            }

            Direct3D11VideoProcessor::~Direct3D11VideoProcessor() { } // IMixerStreamPositionControl methods
            HRESULT Direct3D11VideoProcessor::setPosition(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aLeft,
                                                                     /* [in] */ FLOAT aRight, /* [in] */ FLOAT aTop,
                                                                     /* [in] */ FLOAT aBottom)
            {
               HRESULT lresult(E_FAIL);
               do {
                  MFVideoNormalizedRect lMFVideoNormalizedRect;
                  ZeroMemory(&lMFVideoNormalizedRect, sizeof(lMFVideoNormalizedRect));
                  lMFVideoNormalizedRect.left = aLeft;
                  lMFVideoNormalizedRect.right = aRight;
                  lMFVideoNormalizedRect.top = aTop;
                  lMFVideoNormalizedRect.bottom = aBottom;
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  (*lIter).second.mDestVideoNormalizedRect = lMFVideoNormalizedRect;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::setSrcPosition(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aLeft,
                                                                        /* [in] */ FLOAT aRight, /* [in] */ FLOAT aTop,
                                                                        /* [in] */ FLOAT aBottom)
            {
               HRESULT lresult(E_FAIL);
               do {
                  MFVideoNormalizedRect lMFVideoNormalizedRect;
                  ZeroMemory(&lMFVideoNormalizedRect, sizeof(lMFVideoNormalizedRect));
                  lMFVideoNormalizedRect.left = aLeft;
                  lMFVideoNormalizedRect.right = aRight;
                  lMFVideoNormalizedRect.top = aTop;
                  lMFVideoNormalizedRect.bottom = aBottom;
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  (*lIter).second.mSrcVideoNormalizedRect = lMFVideoNormalizedRect;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::setOpacity(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aOpacity)
            {
               HRESULT lresult(E_FAIL);
               do {
                  DXVA2_Fixed32 lDXVA2_Fixed32 = DXVA2FloatToFixed(aOpacity);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  (*lIter).second.mDXVA2_Fixed32 = lDXVA2_Fixed32;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::setZOrder(/* [in] */ DWORD aInputStreamID, /* [in] */ DWORD aZOrder)
            {
               HRESULT lresult(E_FAIL);
               do {
                  std::lock_guard<std::mutex> lLock(mMutex);
                  LOG_CHECK_STATE(aZOrder >= mdwZOrders.size());
                  mdwZOrders.remove(aInputStreamID);
                  auto lbeginItr = mdwZOrders.begin();
                  bool l_bAdded = false;
                  for (size_t i = 0; i < mdwZOrders.size(); i++) {
                     if (i == aZOrder) {
                        mdwZOrders.insert(lbeginItr, aInputStreamID);
                        l_bAdded = true;
                        break;
                     }
                     ++lbeginItr;
                  }
                  if (!l_bAdded)
                     mdwZOrders.push_back(aInputStreamID);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::getPosition(/* [in] */ DWORD aInputStreamID, /* [out] */ FLOAT* aPtrLeft,
                                                                     /* [out] */ FLOAT* aPtrRight, /* [out] */
                                                                     FLOAT* aPtrTop, /* [out] */ FLOAT* aPtrBottom)
            {
               HRESULT lresult(E_FAIL);
               do {
                  MFVideoNormalizedRect lMFVideoNormalizedRect;
                  ZeroMemory(&lMFVideoNormalizedRect, sizeof(lMFVideoNormalizedRect));
                  do {
                     std::lock_guard<std::mutex> lock(mMutex);
                     auto lIter = m_InputStreams.find(aInputStreamID);
                     LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                     lMFVideoNormalizedRect = (*lIter).second.mDestVideoNormalizedRect;
                     lresult = S_OK;
                  } while (false);
                  *aPtrLeft = lMFVideoNormalizedRect.left;
                  *aPtrRight = lMFVideoNormalizedRect.right;
                  *aPtrTop = lMFVideoNormalizedRect.top;
                  *aPtrBottom = lMFVideoNormalizedRect.bottom;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::getSrcPosition(/* [in] */ DWORD aInputStreamID, /* [out] */
                                                                        FLOAT* aPtrLeft, /* [out] */ FLOAT* aPtrRight,
                                                                        /* [out] */ FLOAT* aPtrTop, /* [out] */
                                                                        FLOAT* aPtrBottom)
            {
               HRESULT lresult(E_FAIL);
               do {
                  MFVideoNormalizedRect lMFVideoNormalizedRect;
                  ZeroMemory(&lMFVideoNormalizedRect, sizeof(lMFVideoNormalizedRect));
                  do {
                     std::lock_guard<std::mutex> lock(mMutex);
                     auto lIter = m_InputStreams.find(aInputStreamID);
                     LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                     lMFVideoNormalizedRect = (*lIter).second.mSrcVideoNormalizedRect;
                     lresult = S_OK;
                  } while (false);
                  *aPtrLeft = lMFVideoNormalizedRect.left;
                  *aPtrRight = lMFVideoNormalizedRect.right;
                  *aPtrTop = lMFVideoNormalizedRect.top;
                  *aPtrBottom = lMFVideoNormalizedRect.bottom;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::getOpacity(/* [in] */ DWORD aInputStreamID, /* [out] */
                                                                    FLOAT* aPtrOpacity)
            {
               HRESULT lresult(E_FAIL);
               do {
                  DXVA2_Fixed32 lDXVA2_Fixed32 = {0, 1};
                  do {
                     std::lock_guard<std::mutex> lock(mMutex);
                     auto lIter = m_InputStreams.find(aInputStreamID);
                     LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                     lDXVA2_Fixed32 = (*lIter).second.mDXVA2_Fixed32;
                     lresult = S_OK;
                  } while (false);
                  *aPtrOpacity = DXVA2FixedToFloat(lDXVA2_Fixed32);
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::getZOrder(/* [in] */ DWORD aInputStreamID, /* [out] */ DWORD* aPtrZOrder)
            {
               HRESULT lresult(E_FAIL);
               do {
                  std::lock_guard<std::mutex> lLock(mMutex);
                  auto lbeginItr = mdwZOrders.begin();
                  for (size_t i = 0; i < mdwZOrders.size(); i++) {
                     if ((*lbeginItr) == aInputStreamID) {
                        *aPtrZOrder = i;
                        lresult = S_OK;
                        break;
                     }
                     ++lbeginItr;
                  }
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::flush(/* [in] */ DWORD aInputStreamID)
            {
               HRESULT lresult(E_FAIL);
               do {
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  (*lIter).second.mSample.Release();
                  lresult = S_OK;
               } while (false);
               return lresult;
            } // IStreamFilterControl methods
            HRESULT Direct3D11VideoProcessor::getCollectionOfFilters(
               /* [in] */ DWORD aInputStreamID, /* [out] */ BSTR* aPtrPtrXMLstring)
            {
               HRESULT lresult(E_FAIL);
               do {
                  //LOG_CHECK_PTR_MEMORY(mVideoProcessorEnum);
                  //auto lIter = m_InputStreams.find(aInputStreamID);
                  //LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  //D3D11_VIDEO_PROCESSOR_CAPS lCAPS;
                  //LOG_INVOKE_POINTER_METHOD(mVideoProcessorEnum, GetVideoProcessorCaps,
                  //	&lCAPS);
                  //xml_document lxmlDoc;
                  //auto ldeclNode = lxmlDoc.append_child(node_declaration);
                  //ldeclNode.append_attribute(L"version") = L"1.0";
                  //xml_node lcommentNode = lxmlDoc.append_child(node_comment);
                  //lcommentNode.set_value(L"XML Document of render stream filters");
                  //auto lRootXMLElement = lxmlDoc.append_child(L"Filters");
                  //										
                  //if (lCAPS.FilterCaps & D3D11_VIDEO_PROCESSOR_FILTER_CAPS_BRIGHTNESS)
                  //{
                  //	auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                  //	lFilterXMLElement.append_attribute(L"Title").set_value(L"Brightness");
                  //	auto lhr = fillFilterNode(
                  //		lFilterXMLElement,
                  //		D3D11_VIDEO_PROCESSOR_FILTER_BRIGHTNESS,
                  //		aInputStreamID);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFilterXMLElement);
                  //	}
                  //}
                  //
                  //if (lCAPS.FilterCaps & D3D11_VIDEO_PROCESSOR_FILTER_CAPS_CONTRAST)
                  //{
                  //	auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                  //	lFilterXMLElement.append_attribute(L"Title").set_value(L"Contrast");
                  //	auto lhr = fillFilterNode(
                  //		lFilterXMLElement,
                  //		D3D11_VIDEO_PROCESSOR_FILTER_CONTRAST,
                  //		aInputStreamID);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFilterXMLElement);
                  //	}
                  //}
                  //
                  //if (lCAPS.FilterCaps & D3D11_VIDEO_PROCESSOR_FILTER_CAPS_HUE)
                  //{
                  //	auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                  //	lFilterXMLElement.append_attribute(L"Title").set_value(L"Hue");
                  //	auto lhr = fillFilterNode(
                  //		lFilterXMLElement,
                  //		D3D11_VIDEO_PROCESSOR_FILTER_HUE,
                  //		aInputStreamID);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFilterXMLElement);
                  //	}
                  //}
                  //
                  //if (lCAPS.FilterCaps & D3D11_VIDEO_PROCESSOR_FILTER_CAPS_SATURATION)
                  //{
                  //	auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                  //	lFilterXMLElement.append_attribute(L"Title").set_value(L"Saturation");
                  //	auto lhr = fillFilterNode(
                  //		lFilterXMLElement,
                  //		D3D11_VIDEO_PROCESSOR_FILTER_SATURATION,
                  //		aInputStreamID);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFilterXMLElement);
                  //	}
                  //}
                  //
                  //if (lCAPS.FilterCaps & D3D11_VIDEO_PROCESSOR_FILTER_CAPS_NOISE_REDUCTION)
                  //{
                  //	auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                  //	lFilterXMLElement.append_attribute(L"Title").set_value(L"Noise Reduction");
                  //	auto lhr = fillFilterNode(
                  //		lFilterXMLElement,
                  //		D3D11_VIDEO_PROCESSOR_FILTER_NOISE_REDUCTION,
                  //		aInputStreamID);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFilterXMLElement);
                  //	}
                  //}
                  //
                  //if (lCAPS.FilterCaps & D3D11_VIDEO_PROCESSOR_FILTER_CAPS_EDGE_ENHANCEMENT)
                  //{
                  //	auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                  //	lFilterXMLElement.append_attribute(L"Title").set_value(L"Edge Enhancement");
                  //	auto lhr = fillFilterNode(
                  //		lFilterXMLElement,
                  //		D3D11_VIDEO_PROCESSOR_FILTER_EDGE_ENHANCEMENT,
                  //		aInputStreamID);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFilterXMLElement);
                  //	}
                  //}
                  //
                  //std::wstringstream lwstringstream;
                  //lxmlDoc.print(lwstringstream);
                  //std::wstring lXMLDocumentString = lwstringstream.str();
                  //
                  //*aPtrPtrXMLstring = SysAllocString(lXMLDocumentString.c_str());
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::setFilterParametr(
               /* [in] */ DWORD aInputStreamID, /* [in] */ DWORD aParametrIndex, /* [in] */ LONG aNewValue, /* [in] */
                          BOOL aIsEnabled)
            {
               HRESULT lresult(E_FAIL);
               do {
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_PTR_MEMORY(mVideoProcessorEnum);
                  LOG_CHECK_PTR_MEMORY(mVideoProcessor);
                  CComPtrCustom<ID3D11Device> lDevice;
                  LOG_INVOKE_FUNCTION(getVideoProcessorService, mDeviceManager, &lDevice);
                  LOG_CHECK_PTR_MEMORY(lDevice);
                  CComPtrCustom<ID3D11DeviceContext> lDeviceContext;
                  lDevice->GetImmediateContext(&lDeviceContext);
                  LOG_CHECK_PTR_MEMORY(lDeviceContext);
                  CComPtrCustom<ID3D11VideoContext> lVideoContext;
                  LOG_INVOKE_QUERY_INTERFACE_METHOD(lDeviceContext, &lVideoContext);
                  LOG_CHECK_PTR_MEMORY(lVideoContext);
                  D3D11_VIDEO_PROCESSOR_FILTER lFilter = static_cast<D3D11_VIDEO_PROCESSOR_FILTER>(aParametrIndex);
                  lVideoContext->VideoProcessorSetStreamFilter(mVideoProcessor, aInputStreamID, lFilter, aIsEnabled,
                                                               aNewValue);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::getCollectionOfOutputFeatures(/* [out] */ BSTR* aPtrPtrXMLstring)
            {
               //using namespace pugi;
               HRESULT lresult(E_FAIL);
               do {
                  LOG_CHECK_PTR_MEMORY(mVideoProcessorEnum);
                  D3D11_VIDEO_PROCESSOR_CAPS lCAPS;
                  LOG_INVOKE_POINTER_METHOD(mVideoProcessorEnum, GetVideoProcessorCaps, &lCAPS); //xml_document lxmlDoc;
                  //auto ldeclNode = lxmlDoc.append_child(node_declaration);
                  //ldeclNode.append_attribute(L"version") = L"1.0";
                  //xml_node lcommentNode = lxmlDoc.append_child(node_comment);
                  //lcommentNode.set_value(L"XML Document of render stream output features");
                  //auto lRootXMLElement = lxmlDoc.append_child(L"Features");
                  //{
                  //	auto lFeatureXMLElement = lRootXMLElement.append_child(L"Feature");
                  //	
                  //	lFeatureXMLElement.append_attribute(L"Title").set_value(L"Background Color");
                  //	
                  //	auto lhr = fillColorFeatureNode(
                  //		lFeatureXMLElement,
                  //		0,
                  //		mBackgroundColor);
                  //	if (FAILED(lhr))
                  //	{
                  //		lRootXMLElement.remove_child(lFeatureXMLElement);
                  //	}
                  //}
                  //
                  //std::wstringstream lwstringstream;
                  //lxmlDoc.print(lwstringstream);
                  //std::wstring lXMLDocumentString = lwstringstream.str();
                  //*aPtrPtrXMLstring = SysAllocString(lXMLDocumentString.c_str());
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::setOutputFeatureParametr(
               /* [in] */ DWORD aParametrIndex, /* [in] */ LONG aNewValue)
            {
               HRESULT lresult(E_FAIL);
               do {
                  float lValue = 1.0F * static_cast<float>(aNewValue) / 255.0F;
                  if (aParametrIndex == 0) {
                     mBackgroundColor.RGBA.A = lValue;
                  }
                  if (aParametrIndex == 1) {
                     mBackgroundColor.RGBA.R = lValue;
                  }
                  if (aParametrIndex == 2) {
                     mBackgroundColor.RGBA.G = lValue;
                  }
                  if (aParametrIndex == 3) {
                     mBackgroundColor.RGBA.B = lValue;
                  }
                  lresult = S_OK;
               } while (false);
               return lresult;
            } // IMFTransform methods
            STDMETHODIMP Direct3D11VideoProcessor::GetStreamLimits(DWORD* aPtrInputMinimum, DWORD* aPtrInputMaximum,
                                                                   DWORD* aPtrOutputMinimum, DWORD* aPtrOutputMaximum)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_STATE_DESCR(
                     aPtrInputMinimum == NULL || aPtrInputMaximum == NULL || aPtrOutputMinimum == NULL ||
                     aPtrOutputMaximum == NULL, E_POINTER);
                  *aPtrInputMinimum = 1;
                  *aPtrInputMaximum = m_dwInputIDs.size();
                  *aPtrOutputMinimum = 1;
                  *aPtrOutputMaximum = m_dwOutputIDs.size();
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetStreamIDs(DWORD aInputIDArraySize, DWORD* aPtrInputIDs,
                                                                DWORD aOutputIDArraySize, DWORD* aPtrOutputIDs)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_STATE_DESCR(aPtrInputIDs == nullptr || aPtrOutputIDs == nullptr, E_POINTER);
                  auto lMaxInputIDArraySize = m_dwInputIDs.size();
                  if (lMaxInputIDArraySize > aInputIDArraySize)
                     lMaxInputIDArraySize = aInputIDArraySize;
                  for (size_t i = 0; i < lMaxInputIDArraySize; i++) {
                     aPtrInputIDs[i] = m_dwInputIDs[i];
                  }
                  auto lMaxOutputIDArraySize = m_dwOutputIDs.size();
                  if (lMaxOutputIDArraySize > aOutputIDArraySize)
                     lMaxOutputIDArraySize = aOutputIDArraySize;
                  for (size_t i = 0; i < lMaxOutputIDArraySize; i++) {
                     aPtrOutputIDs[i] = m_dwOutputIDs[i];
                  }
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetStreamCount(DWORD* aPtrInputStreams, DWORD* aPtrOutputStreams)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_STATE_DESCR(aPtrInputStreams == nullptr || aPtrOutputStreams == nullptr, E_POINTER);
                  *aPtrInputStreams = m_InputStreams.size();
                  *aPtrOutputStreams = 1;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetInputStreamInfo(DWORD aInputStreamID,
                                                                      MFT_INPUT_STREAM_INFO* aPtrStreamInfo)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrStreamInfo);
                  LOG_CHECK_STATE_DESCR(aInputStreamID != 0, MF_E_INVALIDSTREAMNUMBER);
                  aPtrStreamInfo->dwFlags = MFT_INPUT_STREAM_WHOLE_SAMPLES | MFT_INPUT_STREAM_SINGLE_SAMPLE_PER_BUFFER;
                  aPtrStreamInfo->cbMaxLookahead = 0;
                  aPtrStreamInfo->cbAlignment = 0;
                  aPtrStreamInfo->hnsMaxLatency = 0;
                  aPtrStreamInfo->cbSize = 0;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetOutputStreamInfo(DWORD aOutputStreamID,
                                                                       MFT_OUTPUT_STREAM_INFO* aPtrStreamInfo)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrStreamInfo);
                  LOG_CHECK_STATE_DESCR(aOutputStreamID != 0, MF_E_INVALIDSTREAMNUMBER);
                  aPtrStreamInfo->dwFlags = MFT_OUTPUT_STREAM_WHOLE_SAMPLES | MFT_OUTPUT_STREAM_SINGLE_SAMPLE_PER_BUFFER
                                            | MFT_OUTPUT_STREAM_FIXED_SAMPLE_SIZE | MFT_OUTPUT_STREAM_PROVIDES_SAMPLES;
                  aPtrStreamInfo->cbAlignment = 0;
                  aPtrStreamInfo->cbSize = 0;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetInputStreamAttributes(DWORD aInputStreamID,
                                                                            IMFAttributes** aPtrPtrAttributes)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetOutputStreamAttributes(DWORD aOutputStreamID,
                                                                             IMFAttributes** aPtrPtrAttributes)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP Direct3D11VideoProcessor::DeleteInputStream(DWORD aInputStreamID)
            {
               HRESULT lresult = E_FAIL;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  auto liter = m_InputStreams.find(aInputStreamID);
                  if (liter != m_InputStreams.end())
                     m_InputStreams.erase(liter);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::AddInputStreams(DWORD aStreams, DWORD* aPtrStreamIDs)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_STATE_DESCR(aPtrStreamIDs == nullptr, E_POINTER);
                  std::lock_guard<std::mutex> lock(mMutex);
                  for (DWORD i = 0; i < aStreams; i++) {
                     auto lID = aPtrStreamIDs[i];
                     StreamInfo lStreamInfo;
                     m_InputStreams[lID] = lStreamInfo;
                  }
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetInputAvailableType(DWORD aInputStreamID, DWORD aTypeIndex,
                                                                         IMFMediaType** aPtrPtrType)
            {
               HRESULT lresult = S_OK;
               CComPtrCustom<IMFMediaType> lMediaType;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrPtrType);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(!(*lIter).second.mInputMediaType, MF_E_TRANSFORM_TYPE_NOT_SET);
                  LOG_INVOKE_QUERY_INTERFACE_METHOD((*lIter).second.mInputMediaType, aPtrPtrType);
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetOutputAvailableType(DWORD aOutputStreamID, DWORD aTypeIndex,
                                                                          IMFMediaType** aPtrPtrType)
            {
               HRESULT lresult = E_NOTIMPL;
               do { } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::SetInputType(DWORD aInputStreamID, IMFMediaType* aPtrType,
                                                                DWORD aFlags)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrType);
                  LOG_CHECK_PTR_MEMORY(mOutputMediaType);
                  LOG_CHECK_PTR_MEMORY(mDeviceManager);
                  GUID lGUID;
                  LOG_INVOKE_MF_METHOD(GetGUID, aPtrType, MF_MT_MAJOR_TYPE, &lGUID);
                  LOG_CHECK_STATE_DESCR(lGUID != MFMediaType_Video, MF_E_INVALIDMEDIATYPE);
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_INVOKE_MF_METHOD(GetGUID, aPtrType, MF_MT_SUBTYPE, &lGUID);
                  lresult = MF_E_INVALIDMEDIATYPE;
                  for (DWORD i = 0; i < ARRAYSIZE(g_pVideoFormats); i++) {
                     if (lGUID == (*g_pVideoFormats[i])) {
                        lresult = S_OK;
                        break;
                     }
                  }
                  LOG_CHECK_STATE_DESCR(FAILED(lresult), lresult);
                  DXGI_FORMAT lDxgiFormat(DXGI_FORMAT_UNKNOWN);
                  for (DWORD i = 0; i < ARRAYSIZE(mDXGIFormatMapping); i++) {
                     const FormatEntry& e = mDXGIFormatMapping[i];
                     if (e.Subtype == lGUID) {
                        lDxgiFormat = e.DXGIFormat;
                        break;
                     }
                  }
                  LOG_CHECK_STATE_DESCR(lDxgiFormat == DXGI_FORMAT::DXGI_FORMAT_UNKNOWN, MF_E_INVALIDMEDIATYPE);
                  CComPtrCustom<ID3D11VideoDevice> lVideoDevice;
                  LOG_INVOKE_FUNCTION(getVideoProcessorService, mDeviceManager, &lVideoDevice);
                  UINT32 lInputWidth = 0;
                  UINT32 lInputHeight = 0;
                  LOG_INVOKE_FUNCTION(MFGetAttributeSize, aPtrType, MF_MT_FRAME_SIZE, &lInputWidth, &lInputHeight);
                  MFRatio lInputframeRate;
                  LOG_INVOKE_FUNCTION(MFGetAttributeRatio, aPtrType, MF_MT_FRAME_RATE,
                                      (UINT32*)&lInputframeRate.Numerator, (UINT32*)&lInputframeRate.Denominator);
                  UINT32 lOutputWidth = 0;
                  UINT32 lOutputHeight = 0;
                  LOG_INVOKE_FUNCTION(MFGetAttributeSize, mOutputMediaType, MF_MT_FRAME_SIZE, &lOutputWidth,
                                      &lOutputHeight);
                  MFRatio lOutputframeRate;
                  LOG_INVOKE_FUNCTION(MFGetAttributeRatio, mOutputMediaType, MF_MT_FRAME_RATE,
                                      (UINT32*)&lOutputframeRate.Numerator, (UINT32*)&lOutputframeRate.Denominator);
                  //Check if the format is supported
                  D3D11_VIDEO_PROCESSOR_CONTENT_DESC ContentDesc;
                  ZeroMemory(&ContentDesc, sizeof(ContentDesc));
                  ContentDesc.InputFrameFormat = D3D11_VIDEO_FRAME_FORMAT_INTERLACED_TOP_FIELD_FIRST;
                  ContentDesc.InputWidth = lInputWidth;
                  ContentDesc.InputHeight = lInputHeight;
                  ContentDesc.OutputWidth = lOutputWidth;
                  ContentDesc.OutputHeight = lOutputHeight;
                  ContentDesc.InputFrameRate.Numerator = lInputframeRate.Numerator;
                  ContentDesc.InputFrameRate.Denominator = lInputframeRate.Denominator;
                  ContentDesc.OutputFrameRate.Numerator = lOutputframeRate.Numerator;
                  ContentDesc.OutputFrameRate.Denominator = lOutputframeRate.Denominator;
                  ContentDesc.Usage = D3D11_VIDEO_USAGE_PLAYBACK_NORMAL;
                  CComPtrCustom<ID3D11VideoProcessorEnumerator> lVideoProcessorEnum;
                  LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessorEnumerator, &ContentDesc,
                                            &lVideoProcessorEnum);
                  LOG_CHECK_PTR_MEMORY(lVideoProcessorEnum);
                  UINT uiFlags = D3D11_VIDEO_PROCESSOR_FORMAT_SUPPORT_INPUT;
                  LOG_INVOKE_POINTER_METHOD(lVideoProcessorEnum, CheckVideoProcessorFormat, lDxgiFormat, &uiFlags);
                  if (SUCCEEDED(lresult) && uiFlags == 0) {
                     uiFlags = D3D11_VIDEO_PROCESSOR_FORMAT_SUPPORT_INPUT;
                  }
                  if (FAILED(lresult) || 0 == (uiFlags & D3D11_VIDEO_PROCESSOR_FORMAT_SUPPORT_INPUT)) {
                     lresult = MF_E_UNSUPPORTED_D3D_TYPE;
                     break;
                  }
                  if (aFlags != MFT_SET_TYPE_TEST_ONLY) {
                     auto lIter = m_InputStreams.find(aInputStreamID);
                     (*lIter).second.mInputMediaType = aPtrType;
                  }
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::SetOutputType(DWORD aOutputStreamID, IMFMediaType* aPtrType,
                                                                 DWORD aFlags)
            {
               HRESULT lresult = S_OK;
               do {
                  LOG_CHECK_STATE_DESCR(aOutputStreamID != 0, MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_PTR_MEMORY(aPtrType);
                  GUID lGUID;
                  LOG_INVOKE_MF_METHOD(GetGUID, aPtrType, MF_MT_MAJOR_TYPE, &lGUID);
                  LOG_CHECK_STATE_DESCR(lGUID != MFMediaType_Video, MF_E_INVALIDMEDIATYPE);
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_INVOKE_MF_METHOD(GetGUID, aPtrType, MF_MT_SUBTYPE, &lGUID);
                  DXGI_FORMAT lTargetFormat = static_cast<DXGI_FORMAT>(lGUID.Data1);
                  CComPtrCustom<ID3D11VideoDevice> lVideoDevice;
                  LOG_INVOKE_FUNCTION(getVideoProcessorService, mDeviceManager, &lVideoDevice);
                  LOG_CHECK_PTR_MEMORY(lVideoDevice);
                  UINT32 lWidth = 0;
                  UINT32 lHeight = 0;
                  LOG_INVOKE_FUNCTION(MFGetAttributeSize, aPtrType, MF_MT_FRAME_SIZE, &lWidth, &lHeight);
                  MFRatio lframeRate;
                  lframeRate.Numerator = 30;
                  lframeRate.Denominator = 1;
                  LOG_INVOKE_FUNCTION(MFGetAttributeRatio, aPtrType, MF_MT_FRAME_RATE, (UINT32*)&lframeRate.Numerator,
                                      (UINT32*)&lframeRate.Denominator);
                  LOG_INVOKE_MF_FUNCTION(MFFrameRateToAverageTimePerFrame, lframeRate.Numerator, lframeRate.Denominator,
                                         &mAverageTimePerFrame);
                  D3D11_VIDEO_PROCESSOR_CONTENT_DESC ContentDesc;
                  ZeroMemory(&ContentDesc, sizeof(ContentDesc));
                  ContentDesc.InputFrameFormat = D3D11_VIDEO_FRAME_FORMAT_PROGRESSIVE;
                  // D3D11_VIDEO_FRAME_FORMAT_INTERLACED_TOP_FIELD_FIRST;
                  ContentDesc.InputWidth = lWidth;
                  ContentDesc.InputHeight = lHeight;
                  ContentDesc.OutputWidth = lWidth;
                  ContentDesc.OutputHeight = lHeight;
                  ContentDesc.Usage = D3D11_VIDEO_USAGE_PLAYBACK_NORMAL;
                  CComPtrCustom<ID3D11VideoProcessorEnumerator> lVideoProcessorEnum;
                  LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessorEnumerator, &ContentDesc,
                                            &lVideoProcessorEnum);
                  LOG_CHECK_PTR_MEMORY(lVideoProcessorEnum);
                  UINT uiFlags;
                  LOG_INVOKE_POINTER_METHOD(lVideoProcessorEnum, CheckVideoProcessorFormat, lTargetFormat, &uiFlags);
                  if (FAILED(lresult) || 0 == (uiFlags & D3D11_VIDEO_PROCESSOR_FORMAT_SUPPORT_OUTPUT)) {
                     lresult = MF_E_UNSUPPORTED_D3D_TYPE;
                     break;
                  }
                  if (aFlags != MFT_SET_TYPE_TEST_ONLY) {
                     mVideoRenderTargetFormat = lTargetFormat;
                     mOutputMediaType = aPtrType;
                  }
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetInputCurrentType(DWORD aInputStreamID, IMFMediaType** aPtrPtrType)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrPtrType);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(!(*lIter).second.mInputMediaType, MF_E_TRANSFORM_TYPE_NOT_SET);
                  LOG_INVOKE_QUERY_INTERFACE_METHOD((*lIter).second.mInputMediaType, aPtrPtrType);
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetOutputCurrentType(
               DWORD aOutputStreamID, IMFMediaType** aPtrPtrType)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrPtrType);
                  LOG_CHECK_STATE_DESCR(aOutputStreamID != 0, MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(!mOutputMediaType, MF_E_TRANSFORM_TYPE_NOT_SET);
                  LOG_INVOKE_QUERY_INTERFACE_METHOD(mOutputMediaType, aPtrPtrType);
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetInputStatus(DWORD aInputStreamID, DWORD* aPtrFlags)
            {
               HRESULT lresult = S_OK;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrFlags);
                  std::lock_guard<std::mutex> lock(mMutex);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  if (!(*lIter).second.mSample) {
                     *aPtrFlags = MFT_INPUT_STATUS_ACCEPT_DATA;
                  } else {
                     *aPtrFlags = 0;
                  }
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetOutputStatus(DWORD* aPtrFlags)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP Direct3D11VideoProcessor::SetOutputBounds(LONGLONG aLowerBound, LONGLONG aUpperBound)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP Direct3D11VideoProcessor::ProcessEvent(DWORD aInputStreamID, IMFMediaEvent* aPtrEvent)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP Direct3D11VideoProcessor::GetAttributes(IMFAttributes** aPtrPtrAttributes)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP Direct3D11VideoProcessor::ProcessMessage(MFT_MESSAGE_TYPE aMessage, ULONG_PTR aParam)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  if (aMessage == MFT_MESSAGE_SET_D3D_MANAGER) {
                     if (aParam == 0) {
                        mDeviceManager.Release();
                     } else {
                        IUnknown* lUnk = reinterpret_cast<IUnknown*>(aParam);
                        lresult = lUnk->QueryInterface(IID_PPV_ARGS(&mDeviceManager));
                     }
                  } else if (aMessage == MFT_MESSAGE_COMMAND_FLUSH) { } else if (aMessage == MFT_MESSAGE_COMMAND_DRAIN
                  ) { } else if (aMessage == MFT_MESSAGE_NOTIFY_BEGIN_STREAMING) {
                     lresult = createVideoProcessor();
                     if (FAILED(lresult))
                        break;
                     lresult = S_OK;
                  } else if (aMessage == MFT_MESSAGE_NOTIFY_END_STREAMING) { } else if (
                     aMessage == MFT_MESSAGE_NOTIFY_END_OF_STREAM) { } else if (
                     aMessage == MFT_MESSAGE_NOTIFY_START_OF_STREAM) { }
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::ProcessInput(DWORD aInputStreamID, IMFSample* aPtrSample,
                                                                DWORD aFlags)
            {
               HRESULT lresult = S_OK;
               DWORD dwBufferCount = 0;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrSample);
                  LOG_CHECK_STATE(aFlags != 0);
                  std::lock_guard<std::mutex> lock(mMutex);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(!(*lIter).second.mInputMediaType, MF_E_NOTACCEPTING);
                  LOG_CHECK_STATE_DESCR(!mOutputMediaType, MF_E_NOTACCEPTING);
                  (*lIter).second.mSample = aPtrSample;
               } while (false);
               return lresult;
            }

            STDMETHODIMP Direct3D11VideoProcessor::ProcessOutput(DWORD aFlags, DWORD aOutputBufferCount,
                                                                 MFT_OUTPUT_DATA_BUFFER* aPtrOutputSamples,
                                                                 DWORD* aPtrStatus)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrOutputSamples);
                  LOG_CHECK_PTR_MEMORY(aPtrOutputSamples->pSample);
                  LOG_CHECK_PTR_MEMORY(aPtrStatus);
                  LOG_CHECK_STATE_DESCR(aOutputBufferCount != 1 || aFlags != 0, E_INVALIDARG);
                  LOG_CHECK_STATE_DESCR(m_InputStreams.empty(), MF_E_TRANSFORM_NEED_MORE_INPUT);
                  lresult = blit(aPtrOutputSamples->pSample, mAverageTimePerFrame,
                                 mAverageTimePerFrame + mAverageTimePerFrame); //try
                  //{
                  //	lresult = blit(aPtrOutputSamples->pSample, mAverageTimePerFrame, mAverageTimePerFrame + mAverageTimePerFrame);
                  //}
                  //catch (...)
                  //{
                  //	lresult = S_OK;
                  //}
                  *aPtrStatus = 0;
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::blit(IMFSample* aPtrSample, REFERENCE_TIME aTargetFrame,
                                                   REFERENCE_TIME aTargetEndFrame)
            {
               HRESULT lresult(E_FAIL);
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrSample);
                  LOG_CHECK_PTR_MEMORY(mVideoProcessor);
                  DWORD lBufferCount(0);
                  LOG_INVOKE_MF_METHOD(GetBufferCount, aPtrSample, &lBufferCount);
                  CComPtrCustom<ID3D11Texture2D> lDestSurface;
                  if (lBufferCount == 1) {
                     CComPtrCustom<IMFMediaBuffer> lDestBuffer;
                     LOG_INVOKE_MF_METHOD(GetBufferByIndex, aPtrSample, 0, &lDestBuffer);
                     // Get the surface from the buffer.
                     CComPtrCustom<IMFDXGIBuffer> lIMFDXGIBuffer;
                     LOG_INVOKE_QUERY_INTERFACE_METHOD(lDestBuffer, &lIMFDXGIBuffer);
                     LOG_CHECK_PTR_MEMORY(lIMFDXGIBuffer);
                     LOG_INVOKE_DXGI_METHOD(GetResource, lIMFDXGIBuffer, IID_PPV_ARGS(&lDestSurface));
                  } else {
                     CComPtrCustom<IDXGISwapChain1> lSwapChain1;
                     lresult = aPtrSample->GetUnknown(CM_SwapChain, IID_PPV_ARGS(&lSwapChain1));
                     if (SUCCEEDED(lresult)) {
                        LOG_INVOKE_POINTER_METHOD(lSwapChain1, GetBuffer, 0, IID_PPV_ARGS(&lDestSurface));
                     } else {
                        lresult = aPtrSample->GetUnknown(CM_RenderTexture, IID_PPV_ARGS(&lDestSurface));
                     }
                  }
                  LOG_CHECK_PTR_MEMORY(lDestSurface);
                  CComPtrCustom<ID3D11Device> lDevice;
                  LOG_INVOKE_FUNCTION(getVideoProcessorService, mDeviceManager, &lDevice);
                  LOG_CHECK_PTR_MEMORY(lDevice);
                  CComPtrCustom<ID3D11VideoDevice> lVideoDevice;
                  LOG_INVOKE_FUNCTION(getVideoProcessorService, mDeviceManager, &lVideoDevice);
                  LOG_CHECK_PTR_MEMORY(lVideoDevice);
                  CComPtrCustom<ID3D11DeviceContext> lDeviceContext;
                  lDevice->GetImmediateContext(&lDeviceContext);
                  LOG_CHECK_PTR_MEMORY(lDeviceContext);
                  CComPtrCustom<ID3D11VideoContext> lVideoContext;
                  LOG_INVOKE_QUERY_INTERFACE_METHOD(lDeviceContext, &lVideoContext);
                  LOG_CHECK_PTR_MEMORY(lVideoContext);
                  D3D11_TEXTURE2D_DESC lDestDesc;
                  lDestSurface->GetDesc(&lDestDesc);
                  CComPtrCustom<ID3D11VideoProcessorOutputView> lOutputView;
                  D3D11_VIDEO_PROCESSOR_OUTPUT_VIEW_DESC OutputViewDesc;
                  ZeroMemory(&OutputViewDesc, sizeof(OutputViewDesc));
                  OutputViewDesc.ViewDimension = D3D11_VPOV_DIMENSION_TEXTURE2D;
                  OutputViewDesc.Texture2D.MipSlice = 0;
                  OutputViewDesc.Texture2DArray.MipSlice = 0;
                  OutputViewDesc.Texture2DArray.FirstArraySlice = 0;
                  LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessorOutputView, lDestSurface,
                                            mVideoProcessorEnum, &OutputViewDesc, &lOutputView);
                  RECT lOutputTargetRect;
                  lOutputTargetRect.left = 0;
                  lOutputTargetRect.right = lDestDesc.Width;
                  lOutputTargetRect.top = 0;
                  lOutputTargetRect.bottom = lDestDesc.Height;
                  float lPropDest = static_cast<float>(lDestDesc.Width) / static_cast<float>(lDestDesc.Height);
                  float lNativeClientProp = lPropDest;
                  UINT32 lNativeClientWidth = 0;
                  UINT32 lNativeClientHeight = 0;
                  lresult = MFGetAttributeSize(aPtrSample, MF_MT_FRAME_SIZE, &lNativeClientWidth, &lNativeClientHeight);
                  BOOL lisNative(FALSE);
                  if (SUCCEEDED(lresult) && lNativeClientWidth != 0 && lNativeClientHeight != 0) {
                     lNativeClientProp = static_cast<float>(lNativeClientWidth) / static_cast<float>(lNativeClientHeight
                                         );
                     lisNative = TRUE;
                  }
                  RECT lOutputRect;
                  float lWidthScale = 1.0f;
                  float lHeightScale = 1.0f;
                  ZeroMemory(&lOutputRect, sizeof(lOutputRect));
                  if (lPropDest == lNativeClientProp) {
                     lOutputRect.left = 0;
                     lOutputRect.right = lDestDesc.Width;
                     lOutputRect.top = 0;
                     lOutputRect.bottom = lDestDesc.Height;
                  } else {
                     if (lPropDest > lNativeClientProp) {
                        UINT lidealHeight = static_cast<UINT>(
                           static_cast<float>(static_cast<float>(lNativeClientWidth) * static_cast<float>(lDestDesc.
                                                 Height) * static_cast<float>(lDestDesc.Height)) / static_cast<float>(
                              static_cast<float>(lNativeClientHeight) * static_cast<float>(lDestDesc.Width)));
                        UINT lborder = (lDestDesc.Height - lidealHeight) >> 1;
                        lOutputRect.left = 0;
                        lOutputRect.right = lDestDesc.Width;
                        lOutputRect.top = lborder;
                        lOutputRect.bottom = lDestDesc.Height - lborder;
                     } else {
                        UINT lidealWidth = static_cast<UINT>(
                           static_cast<float>(static_cast<float>(lDestDesc.Width) * static_cast<float>(lDestDesc.Width)
                                              * static_cast<float>(lNativeClientHeight)) / static_cast<float>(
                              static_cast<float>(lDestDesc.Height) * static_cast<float>(lNativeClientWidth)));
                        UINT lborder = (lDestDesc.Width - lidealWidth) >> 1;
                        lOutputRect.left = lborder;
                        lOutputRect.right = lDestDesc.Width - lborder;
                        lOutputRect.top = 0;
                        lOutputRect.bottom = lDestDesc.Height;
                     }
                  } // Stream color space
                  D3D11_VIDEO_PROCESSOR_COLOR_SPACE colorSpace = {};
                  colorSpace.YCbCr_xvYCC = 1; // Output color space
                  lVideoContext->VideoProcessorSetOutputColorSpace(mVideoProcessor, &colorSpace);
                  lVideoContext->VideoProcessorSetOutputBackgroundColor(mVideoProcessor, FALSE, &mBackgroundColor);
                  UINT lStreamIndex = 0;
                  float lPropStretchDest = static_cast<float>(lOutputRect.right - lOutputRect.left) / static_cast<float>
                                           (lOutputRect.bottom - lOutputRect.top);
                  for (auto& lIndexID : mdwZOrders) {
                     auto& lInputStream = mInputStreams[lStreamIndex];
                     UINT lCurrentStreamIndex = lStreamIndex;
                     lVideoContext->VideoProcessorSetStreamColorSpace(mVideoProcessor, lStreamIndex, &colorSpace);
                     ++lStreamIndex;
                     ZeroMemory(&lInputStream, sizeof(lInputStream));
                     auto lIter = m_InputStreams.find(lIndexID);
                     if (lIter == m_InputStreams.end())
                        continue;
                     auto& lItem = (*lIter).second;
                     if (!lItem.mSample)
                        continue;
                     auto lVideoNormalizedRect = lItem.mDestVideoNormalizedRect;
                     CComPtrCustom<IMFMediaBuffer> lBuffer;
                     LOG_INVOKE_MF_METHOD(GetBufferByIndex, lItem.mSample, 0, &lBuffer);
                     CComPtrCustom<ID3D11Texture2D> lSurface; // Get the surface from the buffer.
                     CComPtrCustom<IMFDXGIBuffer> lIMFDXGIBuffer;
                     LOG_INVOKE_QUERY_INTERFACE_METHOD(lBuffer, &lIMFDXGIBuffer);
                     LOG_CHECK_PTR_MEMORY(lIMFDXGIBuffer);
                     LOG_INVOKE_DXGI_METHOD(GetResource, lIMFDXGIBuffer, IID_PPV_ARGS(&lSurface));
                     LOG_CHECK_PTR_MEMORY(lSurface);
                     D3D11_VIDEO_PROCESSOR_INPUT_VIEW_DESC InputLeftViewDesc;
                     ZeroMemory(&InputLeftViewDesc, sizeof(InputLeftViewDesc));
                     InputLeftViewDesc.FourCC = 0;
                     InputLeftViewDesc.ViewDimension = D3D11_VPIV_DIMENSION_TEXTURE2D;
                     InputLeftViewDesc.Texture2D.MipSlice = 0;
                     InputLeftViewDesc.Texture2D.ArraySlice = 0;
                     CComPtrCustom<ID3D11VideoProcessorInputView> lInputView;
                     LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessorInputView, lSurface,
                                               mVideoProcessorEnum, &InputLeftViewDesc, &lInputView);
                     lInputStream.Enable = TRUE;
                     lInputStream.OutputIndex = 0;
                     lInputStream.InputFrameOrField = 0;
                     lInputStream.PastFrames = 0;
                     lInputStream.FutureFrames = 0;
                     lInputStream.ppPastSurfaces = nullptr;
                     lInputStream.ppFutureSurfaces = nullptr;
                     lInputStream.pInputSurface = lInputView.detach();
                     lInputStream.ppPastSurfacesRight = nullptr;
                     lInputStream.ppFutureSurfacesRight = nullptr;
                     D3D11_TEXTURE2D_DESC lInputDesc;
                     lSurface->GetDesc(&lInputDesc);
                     RECT lSubStreamSrcRect;
                     lSubStreamSrcRect.left = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.left * static_cast<float>(lInputDesc.Width));
                     lSubStreamSrcRect.right = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.right * static_cast<float>(lInputDesc.Width));
                     lSubStreamSrcRect.top = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.top * static_cast<float>(lInputDesc.Height));
                     lSubStreamSrcRect.bottom = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.bottom * static_cast<float>(lInputDesc.Height));
                     auto lScaledWidthDest = static_cast<float>(lOutputRect.right - lOutputRect.left) * (
                                                lVideoNormalizedRect.right - lVideoNormalizedRect.left);
                     auto lScaledHeightDest = static_cast<float>(lOutputRect.bottom - lOutputRect.top) * (
                                                 lVideoNormalizedRect.bottom - lVideoNormalizedRect.top);
                     auto lLeftOffset = static_cast<float>(lOutputRect.left) + static_cast<float>(
                                           lOutputRect.right - lOutputRect.left) * lVideoNormalizedRect.left;
                     auto lTopOffset = static_cast<float>(lOutputRect.top) + static_cast<float>(
                                          lOutputRect.bottom - lOutputRect.top) * lVideoNormalizedRect.top;
                     RECT lSubStreamDestRect;
                     float lPropScaled = lPropDest;
                     if (lScaledHeightDest > 0.0f)
                        lPropScaled = lScaledWidthDest / lScaledHeightDest;
                     float lPropSrc = static_cast<float>(lInputDesc.Width) / static_cast<float>(lInputDesc.Height);
                     if (lisNative == FALSE) {
                        if (lPropScaled >= lPropSrc) {
                           auto lprop = lScaledHeightDest / static_cast<float>(lInputDesc.Height);
                           //float lw = 1.0f;// lScaledWidthDest / (float)lDestDesc.Width;
                           auto lidealWidth = lprop * static_cast<float>(lInputDesc.Width);
                           auto lborder = static_cast<float>(lScaledWidthDest - lidealWidth) * 0.5f;
                           lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset + lborder);
                           lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lidealWidth + lborder);
                           lSubStreamDestRect.top = static_cast<LONG>(lTopOffset);
                           lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lScaledHeightDest);
                        } else {
                           auto lprop = lScaledWidthDest / static_cast<float>(lInputDesc.Width);
                           //float lh = 1.0f;// lScaledHeightDest / (float)lDestDesc.Height;
                           auto lidealHeight = lprop * static_cast<float>(lInputDesc.Height);
                           auto lborder = static_cast<float>(lScaledHeightDest - lidealHeight) * 0.5f;
                           lSubStreamDestRect.top = static_cast<LONG>(lTopOffset + lborder);
                           lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lidealHeight + lborder);
                           lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset);
                           lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lScaledWidthDest);
                        }
                     } else {
                        if (lPropDest >= lPropSrc) {
                           auto lprop = static_cast<float>(lDestDesc.Height) / static_cast<float>(lInputDesc.Height);
                           float lw = lScaledWidthDest / static_cast<float>(lDestDesc.Width);
                           auto lidealWidth = lprop * static_cast<float>(lInputDesc.Width);
                           auto lborder = static_cast<float>(lDestDesc.Width - lidealWidth) * 0.5f;
                           lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset + lborder * lw);
                           lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lidealWidth * lw + lborder * lw);
                           lSubStreamDestRect.top = static_cast<LONG>(lTopOffset);
                           lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lScaledHeightDest);
                        } else {
                           auto lprop = static_cast<float>(lScaledWidthDest) / static_cast<float>(lInputDesc.Width);
                           float lh = 1.0f; // lInputDesc.Height / (float)lDestDesc.Height;
                           auto lidealHeight = lprop * static_cast<float>(lInputDesc.Height);
                           auto lborder = static_cast<float>(lDestDesc.Height - lidealHeight) * 0.5f;
                           lSubStreamDestRect.top = static_cast<LONG>(lTopOffset + lborder * lh);
                           lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lidealHeight * lh + lborder * lh);
                           lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset);
                           lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lScaledWidthDest);
                        }
                     }
                     lVideoContext->VideoProcessorSetStreamSourceRect(mVideoProcessor, lCurrentStreamIndex, TRUE,
                                                                      &lSubStreamSrcRect);
                     lVideoContext->VideoProcessorSetStreamDestRect(mVideoProcessor, lCurrentStreamIndex, TRUE,
                                                                    &lSubStreamDestRect);
                  }
                  lresult = lVideoContext->VideoProcessorBlt(mVideoProcessor, lOutputView, 0, mInputStreams.size(),
                                                             mInputStreams.data());
                  for (auto& lInputStream : mInputStreams) {
                     if (lInputStream.pInputSurface != nullptr)
                        lInputStream.pInputSurface->Release();
                  }
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::createVideoProcessor()
            {
               HRESULT lresult(E_FAIL);
               do {
                  LOG_CHECK_PTR_MEMORY(mOutputMediaType);
                  GUID lGUID;
                  LOG_INVOKE_MF_METHOD(GetGUID, mOutputMediaType, MF_MT_MAJOR_TYPE, &lGUID);
                  LOG_CHECK_STATE_DESCR(lGUID != MFMediaType_Video, MF_E_INVALIDMEDIATYPE);
                  LOG_INVOKE_MF_METHOD(GetGUID, mOutputMediaType, MF_MT_SUBTYPE, &lGUID);
                  DXGI_FORMAT lTargetFormat = static_cast<DXGI_FORMAT>(lGUID.Data1);
                  CComPtrCustom<ID3D11VideoDevice> lVideoDevice;
                  LOG_INVOKE_FUNCTION(getVideoProcessorService, mDeviceManager, &lVideoDevice);
                  LOG_CHECK_PTR_MEMORY(lVideoDevice);
                  UINT32 lWidth = 0;
                  UINT32 lHeight = 0;
                  LOG_INVOKE_FUNCTION(MFGetAttributeSize, mOutputMediaType, MF_MT_FRAME_SIZE, &lWidth, &lHeight);
                  MFRatio lframeRate;
                  LOG_INVOKE_FUNCTION(MFGetAttributeRatio, mOutputMediaType, MF_MT_FRAME_RATE,
                                      (UINT32*)&lframeRate.Numerator, (UINT32*)&lframeRate.Denominator);
                  LOG_INVOKE_MF_FUNCTION(MFFrameRateToAverageTimePerFrame, lframeRate.Numerator, lframeRate.Denominator,
                                         &mAverageTimePerFrame);
                  D3D11_VIDEO_PROCESSOR_CONTENT_DESC ContentDesc;
                  ZeroMemory(&ContentDesc, sizeof(ContentDesc));
                  ContentDesc.InputFrameFormat = D3D11_VIDEO_FRAME_FORMAT_PROGRESSIVE;
                  // D3D11_VIDEO_FRAME_FORMAT_INTERLACED_TOP_FIELD_FIRST;
                  ContentDesc.InputWidth = lWidth;
                  ContentDesc.InputHeight = lHeight;
                  ContentDesc.OutputWidth = lWidth;
                  ContentDesc.OutputHeight = lHeight;
                  ContentDesc.Usage = D3D11_VIDEO_USAGE_PLAYBACK_NORMAL;
                  mVideoProcessorEnum.Release();
                  LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessorEnumerator, &ContentDesc,
                                            &mVideoProcessorEnum);
                  LOG_CHECK_PTR_MEMORY(mVideoProcessorEnum);
                  UINT uiFlags;
                  LOG_INVOKE_POINTER_METHOD(mVideoProcessorEnum, CheckVideoProcessorFormat, lTargetFormat, &uiFlags);
                  if (FAILED(lresult) || 0 == (uiFlags & D3D11_VIDEO_PROCESSOR_FORMAT_SUPPORT_OUTPUT)) {
                     lresult = MF_E_UNSUPPORTED_D3D_TYPE;
                     break;
                  }
                  mVideoProcessor.Release();
                  LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessor, mVideoProcessorEnum, 0,
                                            &mVideoProcessor); // Main video stream
                  //auto lIter = m_InputStreams.find(0);
                  //LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  //LOG_CHECK_PTR_MEMORY((*lIter).second.mInputMediaType);
                  //MFRatio lFrameRate;
                  //lFrameRate.Numerator = 30000;
                  //lFrameRate.Denominator = 1000;
                  //MFRatio lPixelAperture;
                  //lPixelAperture.Numerator = 1;
                  //lPixelAperture.Denominator = 1;
                  //CComPtrCustom<IMFMediaType> lInputMediaType;
                  //createUncompressedVideoType(
                  //	D3DFMT_X8R8G8B8,
                  //	1920,
                  //	1080,
                  //	MFVideoInterlaceMode::MFVideoInterlace_Progressive,
                  //	lFrameRate,
                  //	lPixelAperture,
                  //	&lInputMediaType);
                  //LOG_CHECK_PTR_MEMORY(lInputMediaType);
                  //DXVA2_VideoDesc lDesc;
                  //lresult = ConvertMFTypeToDXVAType(lInputMediaType, &lDesc);
                  //if (FAILED(lresult))
                  //	break;
                  ////
                  //// Query the video processor GUID.
                  //// //UINT count;
                  //GUID* guids = NULL;
                  //CComPtrCustom<IDirectXVideoProcessorService> lVPService;
                  //lresult = GetVideoProcessorService(
                  //	mDeviceManager,
                  //	&lVPService);
                  //if (FAILED(lresult))
                  //{
                  //	break;
                  //}
                  //lresult = lVPService->GetVideoProcessorDeviceGuids(&lDesc, &count, &guids);
                  //if (FAILED(lresult))
                  //{
                  //	break;
                  //}
                  //lresult = E_FAIL;
                  ////
                  //// Create a DXVA2 device.
                  //// //UINT lGUIDIndex;
                  //for (lGUIDIndex = 0; lGUIDIndex < count; lGUIDIndex++)
                  //{
                  //	BOOL lcheck = checkVideoProcessorGUID(
                  //		guids[lGUIDIndex],
                  //		lVPService,
                  //		&lDesc);
                  //	if (lcheck == TRUE)
                  //	{
                  //		lresult = S_OK;
                  //		break;
                  //	}
                  //}
                  //GUID lSelectedGUID = guids[lGUIDIndex];
                  //CoTaskMemFree(guids);
                  //DXVA2_VideoProcessorCaps l_VPCaps = { 0 };
                  //if (SUCCEEDED(lresult))
                  //{
                  //	lresult = lVPService->GetVideoProcessorCaps(
                  //		lSelectedGUID,
                  //		&lDesc,
                  //		mVideoRenderTargetFormat,
                  //		&l_VPCaps);
                  //}
                  //if (SUCCEEDED(lresult))
                  //{
                  //	mDXVAVPD.Release();
                  //	UINT i(0);
                  //	//
                  //	// Query ProcAmp ranges.
                  //	//
                  //	DXVA2_ValueRange range;
                  //	for (i = 0; i < ARRAYSIZE(g_ProcAmpRanges); i++)
                  //	{
                  //		if (l_VPCaps.ProcAmpControlCaps & (1 << i))
                  //		{
                  //			lresult = lVPService->GetProcAmpRange(
                  //				lSelectedGUID,
                  //				&lDesc,
                  //				mVideoRenderTargetFormat,
                  //				1 << i,
                  //				&range);
                  //			if (FAILED(lresult))
                  //			{
                  //				return FALSE;
                  //			}
                  //			//
                  //			// Reset to default value if the range is changed.
                  //			//
                  //			if (range != g_ProcAmpRanges[i])
                  //			{
                  //				g_ProcAmpRanges[i] = range;
                  //				g_ProcAmpValues[i] = range.DefaultValue;
                  //				g_ProcAmpSteps[i] = ComputeLongSteps(range);
                  //			}
                  //		}
                  //	}
                  //	//
                  //	// Query Noise Filter ranges.
                  //	//
                  //	if (l_VPCaps.VideoProcessorOperations & DXVA2_VideoProcess_NoiseFilter)
                  //	{
                  //		for (i = 0; i < ARRAYSIZE(g_NFilterRanges); i++)
                  //		{
                  //			lresult = lVPService->GetFilterPropertyRange(
                  //				lSelectedGUID,
                  //				&lDesc,
                  //				mVideoRenderTargetFormat,
                  //				DXVA2_NoiseFilterLumaLevel + i,
                  //				&range);
                  //			if (FAILED(lresult))
                  //			{
                  //				return FALSE;
                  //			}
                  //			//
                  //			// Reset to default value if the range is changed.
                  //			//
                  //			if (range != g_NFilterRanges[i])
                  //			{
                  //				g_NFilterRanges[i] = range;
                  //				g_NFilterValues[i] = range.DefaultValue;
                  //			}
                  //		}
                  //	}
                  //	//
                  //	// Query Detail Filter ranges.
                  //	//
                  //	if (l_VPCaps.VideoProcessorOperations & DXVA2_VideoProcess_DetailFilter)
                  //	{
                  //		for (i = 0; i < ARRAYSIZE(g_DFilterRanges); i++)
                  //		{
                  //			lresult = lVPService->GetFilterPropertyRange(
                  //				lSelectedGUID,
                  //				&lDesc,
                  //				mVideoRenderTargetFormat,
                  //				DXVA2_DetailFilterLumaLevel + i,
                  //				&range);
                  //			if (FAILED(lresult))
                  //			{
                  //				return FALSE;
                  //			}
                  //			//
                  //			// Reset to default value if the range is changed.
                  //			//
                  //			if (range != g_DFilterRanges[i])
                  //			{
                  //				g_DFilterRanges[i] = range;
                  //				g_DFilterValues[i] = range.DefaultValue;
                  //			}
                  //		}
                  //	}
                  //	//
                  //	// Finally create a video processor device.
                  //	//
                  //	lresult = lVPService->CreateVideoProcessor(
                  //		lSelectedGUID,
                  //		&lDesc,
                  //		mVideoRenderTargetFormat,
                  //		mSubStreamCount + 1,
                  //		&mDXVAVPD);
                  //}
               } while (false);
               return lresult;
            }

            HRESULT Direct3D11VideoProcessor::getVideoProcessorService(IMFDXGIDeviceManager* pDeviceManager,
                                                                       ID3D11VideoDevice** aPtrPtrID3D11VideoDevice)
            {
               *aPtrPtrID3D11VideoDevice = nullptr;
               HANDLE hDevice;
               HRESULT hr = pDeviceManager->OpenDeviceHandle(&hDevice);
               if (SUCCEEDED(hr)) {
                  // Get the video processor service 
                  HRESULT hr2 = pDeviceManager->GetVideoService(hDevice, IID_PPV_ARGS(aPtrPtrID3D11VideoDevice));
                  // Close the device handle.
                  hr = pDeviceManager->CloseDeviceHandle(hDevice);
                  if (FAILED(hr2)) {
                     hr = hr2;
                  }
               }
               if (FAILED(hr)) {
                  if ((*aPtrPtrID3D11VideoDevice) != nullptr)
                     (*aPtrPtrID3D11VideoDevice)->Release();
               }
               return hr;
            }

            HRESULT Direct3D11VideoProcessor::getVideoProcessorService(IMFDXGIDeviceManager* pDeviceManager,
                                                                       ID3D11Device** aPtrPtrID3D11VideoDevice)
            {
               *aPtrPtrID3D11VideoDevice = nullptr;
               HANDLE hDevice;
               HRESULT hr = pDeviceManager->OpenDeviceHandle(&hDevice);
               if (SUCCEEDED(hr)) {
                  // Get the video processor service 
                  HRESULT hr2 = pDeviceManager->GetVideoService(hDevice, IID_PPV_ARGS(aPtrPtrID3D11VideoDevice));
                  // Close the device handle.
                  hr = pDeviceManager->CloseDeviceHandle(hDevice);
                  if (FAILED(hr2)) {
                     hr = hr2;
                  }
               }
               if (FAILED(hr)) {
                  if ((*aPtrPtrID3D11VideoDevice) != nullptr)
                     (*aPtrPtrID3D11VideoDevice)->Release();
               }
               return hr;
            }
         }
      }
   }
}
