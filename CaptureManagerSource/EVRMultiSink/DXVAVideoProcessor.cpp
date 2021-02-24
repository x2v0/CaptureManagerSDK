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
#include "DXVAVideoProcessor.h"
#include "../MediaFoundationManager/MediaFoundationManager.h"
#include "../Common/MFHeaders.h"
#include "../Common/Common.h"
#include "../Common/Singleton.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "../DirectXManager/Direct3D9Manager.h"
#include "../ConfigManager/ConfigManager.h"
#include "../VideoSurfaceCopierManager/VideoSurfaceCopierManager.h"
#include <list>
#include <vector>
#define STREAM_STRETCH_ID 0
const D3DFORMAT VIDEO_MAIN_FORMAT = D3DFMT_YUY2;

namespace EVRMultiSink
{
   namespace Sinks
   {
      namespace EVR
      {
         namespace Mixer
         {
            using namespace Core;
            const UINT VIDEO_REQUIED_OP = DXVA2_VideoProcess_YUV2RGB | DXVA2_VideoProcess_StretchX |
                                          DXVA2_VideoProcess_StretchY | DXVA2_VideoProcess_SubRects |
                                          DXVA2_VideoProcess_SubStreams;
            const UINT EX_COLOR_INFO[][2] = {
               // SDTV ITU-R BT.601 YCbCr to driver's optimal RGB range
               {DXVA2_VideoTransferMatrix_BT601, DXVA2_NominalRange_Unknown},
               // SDTV ITU-R BT.601 YCbCr to studio RGB [16...235]
               {DXVA2_VideoTransferMatrix_BT601, DXVA2_NominalRange_16_235},
               // SDTV ITU-R BT.601 YCbCr to computer RGB [0...255]
               {DXVA2_VideoTransferMatrix_BT601, DXVA2_NominalRange_0_255},
               // HDTV ITU-R BT.709 YCbCr to driver's optimal RGB range
               {DXVA2_VideoTransferMatrix_BT709, DXVA2_NominalRange_Unknown},
               // HDTV ITU-R BT.709 YCbCr to studio RGB [16...235]
               {DXVA2_VideoTransferMatrix_BT709, DXVA2_NominalRange_16_235},
               // HDTV ITU-R BT.709 YCbCr to computer RGB [0...255]
               {DXVA2_VideoTransferMatrix_BT709, DXVA2_NominalRange_0_255}
            };
            INT g_ExColorInfo = 0; //
            // Helper inline functions.
            //
            inline BOOL operator !=(const DXVA2_ValueRange& x, const DXVA2_ValueRange& y)
            {
               return memcmp(&x, &y, sizeof(x)) ? TRUE : FALSE;
            }

            DXVA2_ValueRange g_ProcAmpRanges[4] = {0};
            DXVA2_Fixed32 g_ProcAmpValues[4] = {0};
            INT g_ProcAmpSteps[4] = {0};
            DXVA2_Fixed32 g_NFilterValues[6] = {0};
            DXVA2_ValueRange g_NFilterRanges[6] = {0};
            DXVA2_Fixed32 g_DFilterValues[6] = {0};
            DXVA2_ValueRange g_DFilterRanges[6] = {0};
            const DWORD g_dwOutputIDs[] = {0};

            HRESULT DXVAVideoProcessor::createDXVAVideoProcessor(IMFTransform** aPtrPtrTrsnaform)
            {
               HRESULT lresult = E_FAIL;
               do {
                  auto lMaxVideoRenderStreamCount = Singleton<ConfigManager>::getInstance().
                     getMaxVideoRenderStreamCount();
                  LOG_CHECK_PTR_MEMORY(aPtrPtrTrsnaform);
                  CComPtrCustom<DXVAVideoProcessor> lDXVAVideoProcessor(new(std::nothrow) DXVAVideoProcessor);
                  LOG_CHECK_PTR_MEMORY(lDXVAVideoProcessor);
                  for (size_t i = 0; i < lMaxVideoRenderStreamCount; i++) {
                     lDXVAVideoProcessor->m_dwInputIDs.push_back(i);
                     lDXVAVideoProcessor->m_dwZOrders.push_back(i);
                  }
                  DWORD lInputMin = 0;
                  DWORD lInputMax = 0;
                  DWORD lOutputMin = 0;
                  DWORD lOutputMax = 0;
                  LOG_INVOKE_POINTER_METHOD(lDXVAVideoProcessor, GetStreamLimits, &lInputMin, &lInputMax, &lOutputMin,
                                            &lOutputMax);
                  DWORD lInputIDStream;
                  DWORD lOutputIDStream;
                  LOG_INVOKE_POINTER_METHOD(lDXVAVideoProcessor, GetStreamIDs, 1, &lInputIDStream, 1, &lOutputIDStream);
                  LOG_INVOKE_POINTER_METHOD(lDXVAVideoProcessor, AddInputStreams, 1, &lInputIDStream);
                  LOG_INVOKE_QUERY_INTERFACE_METHOD(lDXVAVideoProcessor, aPtrPtrTrsnaform);
               } while (false);
               return lresult;
            }

            DXVAVideoProcessor::DXVAVideoProcessor() : mSubStreamCount(0), mAverageTimePerFrame(170000),
                                                       mBackgroundColor(D3DCOLOR_XRGB(0x10, 0x10, 0x10)),
                                                       mVideoRenderTargetFormat(D3DFMT_X8R8G8B8) { }

            DXVAVideoProcessor::~DXVAVideoProcessor() { }
            // IMixerStreamPositionControl methods
            HRESULT DXVAVideoProcessor::setPosition(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aLeft, /* [in] */
                                                               FLOAT aRight, /* [in] */ FLOAT aTop,          /* [in] */
                                                               FLOAT aBottom)
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

            HRESULT DXVAVideoProcessor::setSrcPosition(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aLeft,
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

            HRESULT DXVAVideoProcessor::setOpacity(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aOpacity)
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

            HRESULT DXVAVideoProcessor::setZOrder(/* [in] */ DWORD aInputStreamID, /* [in] */ DWORD aZOrder)
            {
               HRESULT lresult(E_FAIL);
               do {
                  std::lock_guard<std::mutex> lLock(mMutex);
                  LOG_CHECK_STATE(aZOrder >= m_dwZOrders.size());
                  m_dwZOrders.remove(aInputStreamID);
                  auto lbeginItr = m_dwZOrders.begin();
                  bool l_bAdded = false;
                  for (size_t i = 0; i < m_dwZOrders.size(); i++) {
                     if (i == aZOrder) {
                        m_dwZOrders.insert(lbeginItr, aInputStreamID);
                        l_bAdded = true;
                        break;
                     }
                     ++lbeginItr;
                  }
                  if (!l_bAdded)
                     m_dwZOrders.push_back(aInputStreamID);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::getPosition(/* [in] */ DWORD aInputStreamID, /* [out] */ FLOAT* aPtrLeft,
                                                               /* [out] */ FLOAT* aPtrRight, /* [out] */ FLOAT* aPtrTop,
                                                               /* [out] */ FLOAT* aPtrBottom)
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

            HRESULT DXVAVideoProcessor::getSrcPosition(/* [in] */ DWORD aInputStreamID, /* [out] */ FLOAT* aPtrLeft,
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

            HRESULT DXVAVideoProcessor::getOpacity(/* [in] */ DWORD aInputStreamID, /* [out] */ FLOAT* aPtrOpacity)
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

            HRESULT DXVAVideoProcessor::getZOrder(/* [in] */ DWORD aInputStreamID, /* [out] */ DWORD* aPtrZOrder)
            {
               HRESULT lresult(E_FAIL);
               do {
                  std::lock_guard<std::mutex> lLock(mMutex);
                  auto lbeginItr = m_dwZOrders.begin();
                  for (size_t i = 0; i < m_dwZOrders.size(); i++) {
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

            HRESULT DXVAVideoProcessor::flush(/* [in] */ DWORD aInputStreamID)
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
            HRESULT DXVAVideoProcessor::getCollectionOfFilters(
               /* [in] */ DWORD aInputStreamID, /* [out] */ BSTR* aPtrPtrXMLstring)
            {
               using namespace pugi;
               HRESULT lresult(E_FAIL);
               do {
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  xml_document lxmlDoc;
                  auto ldeclNode = lxmlDoc.append_child(node_declaration);
                  ldeclNode.append_attribute(L"version") = L"1.0";
                  xml_node lcommentNode = lxmlDoc.append_child(node_comment);
                  lcommentNode.set_value(L"XML Document of render stream filters");
                  auto lRootXMLElement = lxmlDoc.append_child(L"Filters");
                  {
                     auto lFilterXMLElement = lRootXMLElement.append_child(L"Filter");
                     lFilterXMLElement.append_attribute(L"Title").set_value(L"Stream Stretch");
                     auto lhr = getStreamStretch(aInputStreamID, STREAM_STRETCH_ID, lFilterXMLElement);
                     if (FAILED(lhr)) {
                        lRootXMLElement.remove_child(lFilterXMLElement);
                     }
                  }
                  std::wstringstream lwstringstream;
                  lxmlDoc.print(lwstringstream);
                  std::wstring lXMLDocumentString = lwstringstream.str();
                  *aPtrPtrXMLstring = SysAllocString(lXMLDocumentString.c_str());
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::setFilterParametr(/* [in] */ DWORD aInputStreamID, /* [in] */
                                                                     DWORD aParametrIndex, /* [in] */ LONG aNewValue,
                                                                     /* [in] */ BOOL aIsEnabled)
            {
               HRESULT lresult(E_FAIL);
               do {
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  if (aParametrIndex == STREAM_STRETCH_ID) {
                     auto& lItem = (*lIter).second;
                     if (aIsEnabled == TRUE)
                        lItem.mStretchState = static_cast<StretchState>(aNewValue);
                     else
                        lItem.mStretchState = SOURCE;
                  }
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::getCollectionOfOutputFeatures(/* [out] */ BSTR* aPtrPtrXMLstring)
            {
               using namespace pugi;
               HRESULT lresult(E_FAIL);
               do {
                  xml_document lxmlDoc;
                  auto ldeclNode = lxmlDoc.append_child(node_declaration);
                  ldeclNode.append_attribute(L"version") = L"1.0";
                  xml_node lcommentNode = lxmlDoc.append_child(node_comment);
                  lcommentNode.set_value(L"XML Document of render stream output features");
                  auto lRootXMLElement = lxmlDoc.append_child(L"Features");
                  {
                     auto lFeatureXMLElement = lRootXMLElement.append_child(L"Feature");
                     lFeatureXMLElement.append_attribute(L"Title").set_value(L"Background Color");
                     auto lhr = fillColorFeatureNode(lFeatureXMLElement, 0, mBackgroundColor);
                     if (FAILED(lhr)) {
                        lRootXMLElement.remove_child(lFeatureXMLElement);
                     }
                  }
                  std::wstringstream lwstringstream;
                  lxmlDoc.print(lwstringstream);
                  std::wstring lXMLDocumentString = lwstringstream.str();
                  *aPtrPtrXMLstring = SysAllocString(lXMLDocumentString.c_str());
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::fillColorFeatureNode(pugi::xml_node& aFeatureNode, UINT32 aStartIndex,
                                                             D3DCOLOR aColor)
            {
               HRESULT lresult(E_FAIL);
               do {
                  auto lColorXMLElement = aFeatureNode.append_child(L"Color"); //{
                  //	auto lChannelXMLElement = lColorXMLElement.append_child(L"Channel");
                  aStartIndex++; //	lChannelXMLElement.append_attribute(L"Index").set_value(aStartIndex++);
                  //	lChannelXMLElement.append_attribute(L"Title").set_value(L"Alpha");
                  //	lChannelXMLElement.append_attribute(L"Min").set_value(0);
                  //	lChannelXMLElement.append_attribute(L"Max").set_value(255);
                  //	lChannelXMLElement.append_attribute(L"CurrentValue").set_value((int)(aColor.A * 255.0f));
                  //	lChannelXMLElement.append_attribute(L"Step").set_value(1);
                  //}
                  {
                     auto lChannelXMLElement = lColorXMLElement.append_child(L"Channel");
                     lChannelXMLElement.append_attribute(L"Index").set_value(aStartIndex++);
                     lChannelXMLElement.append_attribute(L"Title").set_value(L"Red");
                     lChannelXMLElement.append_attribute(L"Min").set_value(0);
                     lChannelXMLElement.append_attribute(L"Max").set_value(255);
                     lChannelXMLElement.append_attribute(L"CurrentValue").set_value(GetRValue(aColor));
                     lChannelXMLElement.append_attribute(L"Step").set_value(1);
                  }
                  {
                     auto lChannelXMLElement = lColorXMLElement.append_child(L"Channel");
                     lChannelXMLElement.append_attribute(L"Index").set_value(aStartIndex++);
                     lChannelXMLElement.append_attribute(L"Title").set_value(L"Green");
                     lChannelXMLElement.append_attribute(L"Min").set_value(0);
                     lChannelXMLElement.append_attribute(L"Max").set_value(255);
                     lChannelXMLElement.append_attribute(L"CurrentValue").set_value(GetGValue(aColor));
                     lChannelXMLElement.append_attribute(L"Step").set_value(1);
                  }
                  {
                     auto lChannelXMLElement = lColorXMLElement.append_child(L"Channel");
                     lChannelXMLElement.append_attribute(L"Index").set_value(aStartIndex++);
                     lChannelXMLElement.append_attribute(L"Title").set_value(L"Blue");
                     lChannelXMLElement.append_attribute(L"Min").set_value(0);
                     lChannelXMLElement.append_attribute(L"Max").set_value(255);
                     lChannelXMLElement.append_attribute(L"CurrentValue").set_value(GetBValue(aColor));
                     lChannelXMLElement.append_attribute(L"Step").set_value(1);
                  }
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::setOutputFeatureParametr(
               /* [in] */ DWORD aParametrIndex, /* [in] */ LONG aNewValue)
            {
               using namespace Direct3D9;
               HRESULT lresult(E_FAIL);
               do {
                  LOG_CHECK_PTR_MEMORY(mDeviceManager);
                  lresult = S_OK;
                  if (aNewValue > 235)
                     aNewValue = 235;
                  if (aNewValue < 16)
                     aNewValue = 16; //if (aParametrIndex == 0)
                  //{
                  //	mBackgroundColor.RGBA.A = lValue;
                  //}
                  BYTE lR = ((mBackgroundColor) >> 16) & 0xff;
                  BYTE lG = ((mBackgroundColor) >> 8) & 0xff;
                  BYTE lB = mBackgroundColor & 0xff;
                  if (aParametrIndex == 1) {
                     mBackgroundColor = D3DCOLOR_XRGB(aNewValue, lG, lB);
                  }
                  if (aParametrIndex == 2) {
                     mBackgroundColor = D3DCOLOR_XRGB(lR, aNewValue, lB);
                  }
                  if (aParametrIndex == 3) {
                     mBackgroundColor = D3DCOLOR_XRGB(lR, lG, aNewValue);
                  }
                  CComPtrCustom<IDirect3DDevice9> lDirect3DDevice;
                  HANDLE hDevice;
                  LOG_INVOKE_POINTER_METHOD(mDeviceManager, OpenDeviceHandle, &hDevice);
                  // Get the video processor service 
                  HRESULT hr2 = mDeviceManager->LockDevice(hDevice, &lDirect3DDevice, TRUE);
                  do {
                     LOG_CHECK_PTR_MEMORY(lDirect3DDevice);
                     lresult = lDirect3DDevice->ColorFill(mBackSurface, nullptr, RGBtoYUV(mBackgroundColor));
                     if (FAILED(lresult)) {
                        break;
                     }
                  } while (false);
                  mDeviceManager->UnlockDevice(hDevice, FALSE); // Close the device handle.
                  lresult = mDeviceManager->CloseDeviceHandle(hDevice);
                  if (FAILED(hr2)) {
                     lresult = hr2;
                  }
               } while (false);
               return lresult;
            } // IMFTransform methods
            STDMETHODIMP DXVAVideoProcessor::GetStreamLimits(DWORD* aPtrInputMinimum, DWORD* aPtrInputMaximum,
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
                  *aPtrOutputMaximum = ARRAYSIZE(g_dwOutputIDs);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::GetStreamIDs(DWORD aInputIDArraySize, DWORD* aPtrInputIDs,
                                                          DWORD aOutputIDArraySize, DWORD* aPtrOutputIDs)
            {
               HRESULT lresult = E_FAIL;
               do {
                  LOG_CHECK_STATE_DESCR(aPtrInputIDs == nullptr || aPtrOutputIDs == nullptr, E_POINTER);
                  auto lMaxInputIDArraySize = m_dwInputIDs.size();
                  if (lMaxInputIDArraySize > aInputIDArraySize)
                     lMaxInputIDArraySize = aInputIDArraySize;
                  memcpy(aPtrInputIDs, m_dwInputIDs.data(), lMaxInputIDArraySize * sizeof(DWORD));
                  auto lMaxOutputIDArraySize = ARRAYSIZE(g_dwOutputIDs);
                  if (lMaxOutputIDArraySize > aOutputIDArraySize)
                     lMaxOutputIDArraySize = aOutputIDArraySize;
                  memcpy(aPtrOutputIDs, g_dwOutputIDs, lMaxOutputIDArraySize * sizeof(DWORD));
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::GetStreamCount(DWORD* aPtrInputStreams, DWORD* aPtrOutputStreams)
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

            STDMETHODIMP DXVAVideoProcessor::GetInputStreamInfo(DWORD aInputStreamID,
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

            STDMETHODIMP DXVAVideoProcessor::GetOutputStreamInfo(DWORD aOutputStreamID,
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

            STDMETHODIMP DXVAVideoProcessor::GetInputStreamAttributes(DWORD aInputStreamID,
                                                                      IMFAttributes** aPtrPtrAttributes)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP DXVAVideoProcessor::GetOutputStreamAttributes(DWORD aOutputStreamID,
                                                                       IMFAttributes** aPtrPtrAttributes)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP DXVAVideoProcessor::DeleteInputStream(DWORD aInputStreamID)
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

            STDMETHODIMP DXVAVideoProcessor::AddInputStreams(DWORD aStreams, DWORD* aPtrStreamIDs)
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

            STDMETHODIMP DXVAVideoProcessor::GetInputAvailableType(DWORD aInputStreamID, DWORD aTypeIndex,
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

            STDMETHODIMP DXVAVideoProcessor::GetOutputAvailableType(DWORD aOutputStreamID, DWORD aTypeIndex,
                                                                    IMFMediaType** aPtrPtrType)
            {
               HRESULT lresult = S_OK;
               CComPtrCustom<IMFMediaType> lMediaType;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrPtrType);
                  LOG_CHECK_STATE_DESCR(aOutputStreamID != 0, MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(aTypeIndex > 0, MF_E_NO_MORE_TYPES);
                  std::lock_guard<std::mutex> lock(mMutex);
                  auto lIter = m_InputStreams.find(0);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(!(*lIter).second.mInputMediaType, MF_E_TRANSFORM_TYPE_NOT_SET);
                  CComPtrCustom<IMFMediaType> lOutputMediaType;
                  LOG_INVOKE_FUNCTION(createMediaType, (*lIter).second.mInputMediaType, MFVideoFormat_RGB32,
                                      &lOutputMediaType);
                  LOG_CHECK_PTR_MEMORY(lOutputMediaType);
                  LOG_INVOKE_QUERY_INTERFACE_METHOD(lOutputMediaType, aPtrPtrType);
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::SetInputType(DWORD aInputStreamID, IMFMediaType* aPtrType, DWORD aFlags)
            {
               HRESULT lresult = S_OK;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrType);
                  GUID lGUID;
                  LOG_INVOKE_MF_METHOD(GetGUID, aPtrType, MF_MT_MAJOR_TYPE, &lGUID);
                  LOG_CHECK_STATE_DESCR(lGUID != MFMediaType_Video, MF_E_INVALIDMEDIATYPE);
                  std::lock_guard<std::mutex> lock(mMutex);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  DXVA2_VideoDesc lDesc;
                  lresult = ConvertMFTypeToDXVAType(aPtrType, &lDesc);
                  if (FAILED(lresult))
                     break;
                  lresult = checkSupportedSubFormat(&lDesc);
                  if (FAILED(lresult))
                     break;
                  if (aFlags != MFT_SET_TYPE_TEST_ONLY) {
                     (*lIter).second.mInputMediaType = aPtrType;
                     UINT32 lWidth = 0;
                     UINT32 lHeight = 0;
                     MFGetAttributeSize(aPtrType, MF_MT_FRAME_SIZE, &lWidth, &lHeight);
                     if (aInputStreamID == 0) {
                        MFRatio lframeRate;
                        LOG_INVOKE_FUNCTION(MFGetAttributeRatio, aPtrType, MF_MT_FRAME_RATE,
                                            (UINT32*)&lframeRate.Numerator, (UINT32*)&lframeRate.Denominator);
                        MFRatio lpixelAspectRate;
                        MFGetAttributeRatio(aPtrType, MF_MT_PIXEL_ASPECT_RATIO, (UINT32*)&lpixelAspectRate.Numerator,
                                            (UINT32*)&lpixelAspectRate.Denominator);
                        LOG_INVOKE_MF_FUNCTION(MFFrameRateToAverageTimePerFrame, lframeRate.Numerator,
                                               lframeRate.Denominator, &mAverageTimePerFrame);
                     } //using namespace pugi;
                     //xml_document lxmlDoc;
                     //auto ldeclNode = lxmlDoc.append_child(node_declaration);
                     //ldeclNode.append_attribute(L"version") = L"1.0";
                     //xml_node lcommentNode = lxmlDoc.append_child(node_comment);
                     //lcommentNode.set_value(L"XML Document of media type");
                     //auto lRootXMLElement = lxmlDoc.append_child(L"MediaType");
                     //DataParser::readMediaType(
                     //	aPtrType,
                     //	lRootXMLElement);
                     //std::wstringstream lwstringstream;
                     //lxmlDoc.print(lwstringstream);
                     //LogPrintOut::getInstance().printOutln(
                     //	LogPrintOut::ERROR_LEVEL,
                     //	lwstringstream.str().c_str());
                     if (mDXVAVPDService) {
                        (*lIter).second.mSubStreamSurface.Release(); //
                        // Create a sub stream surface.
                        //
                        lresult = mDXVAVPDService->CreateSurface(lWidth, lHeight, 0, lDesc.Format, mVPCaps.InputPool, 0,
                                                                 DXVA2_VideoSoftwareRenderTarget,
                                                                 &(*lIter).second.mSubStreamSurface, nullptr);
                        if (!(*lIter).second.mSubStreamSurface && mVPCaps.InputPool != D3DPOOL_DEFAULT) {
                           lresult = mDXVAVPDService->CreateSurface(lWidth, lHeight, 0, lDesc.Format, D3DPOOL_DEFAULT,
                                                                    0, DXVA2_VideoSoftwareRenderTarget,
                                                                    &(*lIter).second.mSubStreamSurface, nullptr);
                        }
                     }
                  }
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::SetOutputType(DWORD aOutputStreamID, IMFMediaType* aPtrType, DWORD aFlags)
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
                  LOG_CHECK_STATE_DESCR(lGUID != MFVideoFormat_RGB32 && lGUID != MFVideoFormat_ARGB32,
                                        MF_E_INVALIDMEDIATYPE);
                  if (aFlags != MFT_SET_TYPE_TEST_ONLY) {
                     mVideoRenderTargetFormat = static_cast<D3DFORMAT>(lGUID.Data1);
                     mOutputMediaType = aPtrType;
                  }
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::GetInputCurrentType(DWORD aInputStreamID, IMFMediaType** aPtrPtrType)
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

            STDMETHODIMP DXVAVideoProcessor::GetOutputCurrentType(DWORD aOutputStreamID, IMFMediaType** aPtrPtrType)
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

            STDMETHODIMP DXVAVideoProcessor::GetInputStatus(DWORD aInputStreamID, DWORD* aPtrFlags)
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

            STDMETHODIMP DXVAVideoProcessor::GetOutputStatus(DWORD* aPtrFlags)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP DXVAVideoProcessor::SetOutputBounds(LONGLONG aLowerBound, LONGLONG aUpperBound)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP DXVAVideoProcessor::ProcessEvent(DWORD aInputStreamID, IMFMediaEvent* aPtrEvent)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP DXVAVideoProcessor::GetAttributes(IMFAttributes** aPtrPtrAttributes)
            {
               return E_NOTIMPL;
            }

            STDMETHODIMP DXVAVideoProcessor::ProcessMessage(MFT_MESSAGE_TYPE aMessage, ULONG_PTR aParam)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  if (aMessage == MFT_MESSAGE_SET_D3D_MANAGER) {
                     if (aParam == 0) {
                        mDXVAVPD.Release();
                        mDeviceManager.Release();
                     } else {
                        IUnknown* lUnk = reinterpret_cast<IUnknown*>(aParam);
                        lresult = lUnk->QueryInterface(IID_PPV_ARGS(&mDeviceManager));
                     }
                  } else if (aMessage == MFT_MESSAGE_COMMAND_FLUSH) {
                     if (mVideoSurfaceCopier)
                        mVideoSurfaceCopier->ProcessMessage(aMessage, aParam);
                  } else if (aMessage == MFT_MESSAGE_COMMAND_DRAIN) { } else if (
                     aMessage == MFT_MESSAGE_NOTIFY_BEGIN_STREAMING) {
                     lresult = createVideoProcessor();
                     if (FAILED(lresult))
                        break;
                     lresult = createSurfaces(mDeviceManager);
                     if (FAILED(lresult))
                        break;
                     lresult = S_OK;
                  } else if (aMessage == MFT_MESSAGE_NOTIFY_END_STREAMING) { } else if (
                     aMessage == MFT_MESSAGE_NOTIFY_END_OF_STREAM) { } else if (
                     aMessage == MFT_MESSAGE_NOTIFY_START_OF_STREAM) { }
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::ProcessInput(DWORD aInputStreamID, IMFSample* aPtrSample, DWORD aFlags)
            {
               HRESULT lresult = S_OK;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrSample);
                  LOG_CHECK_STATE(aFlags != 0);
                  std::lock_guard<std::mutex> lock(mMutex);
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  LOG_CHECK_STATE_DESCR(!(*lIter).second.mInputMediaType, MF_E_NOTACCEPTING);
                  CComPtrCustom<IMFMediaBuffer> lBuffer;
                  aPtrSample->GetBufferByIndex(0, &lBuffer);
                  LOG_CHECK_PTR_MEMORY(lBuffer);
                  do {
                     CComPtrCustom<IDirect3DSurface9> lSurface; // Get the surface from the buffer.							
                     LOG_INVOKE_MF_FUNCTION(MFGetService, lBuffer, MR_BUFFER_SERVICE, IID_PPV_ARGS(&lSurface));
                     (*lIter).second.mSample = aPtrSample;
                  } while (false);
                  if (FAILED(lresult)) {
                     if (!mVideoSurfaceCopier) {
                        CComPtrCustom<IMFVideoSampleAllocator> lVideoSampleAllocator;
                        LOG_INVOKE_MF_FUNCTION(MFCreateVideoSampleAllocator, IID_PPV_ARGS(&lVideoSampleAllocator));
                        if (lVideoSampleAllocator) {
                           if (mDeviceManager)
                              lVideoSampleAllocator->SetDirectXManager(mDeviceManager);
                        }
                        CComPtrCustom<IUnknown> lUnknown;
                        LOG_INVOKE_FUNCTION(
                           Singleton<Transform::VideoSurfaceCopierManager>::getInstance().createVideoSurfaceCopier,
                           lVideoSampleAllocator, (*lIter).second.mInputMediaType, &lUnknown);
                        LOG_CHECK_PTR_MEMORY(lUnknown);
                        LOG_INVOKE_QUERY_INTERFACE_METHOD(lUnknown, &mVideoSurfaceCopier);
                        LOG_CHECK_PTR_MEMORY(mVideoSurfaceCopier);
                        LOG_INVOKE_POINTER_METHOD(lVideoSampleAllocator, InitializeSampleAllocator,
                                                  m_InputStreams.size() * 3, (*lIter).second.mInputMediaType);
                     }
                     if (mVideoSurfaceCopier) {
                        LOG_INVOKE_POINTER_METHOD(mVideoSurfaceCopier, ProcessInput, 0, aPtrSample, 0);
                        MFT_OUTPUT_DATA_BUFFER lOutputSamples{0};
                        DWORD lstatus;
                        LOG_INVOKE_MF_METHOD(ProcessOutput, mVideoSurfaceCopier, 0, 1, &lOutputSamples, &lstatus);
                        if (lOutputSamples.pSample) {
                           (*lIter).second.mSample = lOutputSamples.pSample;
                           lOutputSamples.pSample->Release();
                        }
                     }
                  }
               } while (false);
               return lresult;
            }

            STDMETHODIMP DXVAVideoProcessor::ProcessOutput(DWORD aFlags, DWORD aOutputBufferCount,
                                                           MFT_OUTPUT_DATA_BUFFER* aPtrOutputSamples, DWORD* aPtrStatus)
            {
               HRESULT lresult = S_OK;
               do {
                  std::lock_guard<std::mutex> lock(mMutex);
                  LOG_CHECK_PTR_MEMORY(aPtrOutputSamples);
                  LOG_CHECK_PTR_MEMORY(aPtrOutputSamples->pSample);
                  LOG_CHECK_PTR_MEMORY(aPtrStatus);
                  LOG_CHECK_STATE_DESCR(aOutputBufferCount != 1 || aFlags != 0, E_INVALIDARG);
                  LOG_CHECK_STATE_DESCR(m_InputStreams.empty(), MF_E_TRANSFORM_NEED_MORE_INPUT);
                  lresult = blit(aPtrOutputSamples->pSample, 333333, 333333 + 333333);
                  *aPtrStatus = 0;
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::GetVideoProcessorService(IDirect3DDeviceManager9* pDeviceManager,
                                                                 IDirectXVideoProcessorService** ppVPService)
            {
               *ppVPService = nullptr;
               HANDLE hDevice;
               HRESULT hr = pDeviceManager->OpenDeviceHandle(&hDevice);
               if (SUCCEEDED(hr)) {
                  // Get the video processor service 
                  HRESULT hr2 = pDeviceManager->GetVideoService(hDevice, IID_PPV_ARGS(ppVPService));
                  // Close the device handle.
                  hr = pDeviceManager->CloseDeviceHandle(hDevice);
                  if (FAILED(hr2)) {
                     hr = hr2;
                  }
               }
               if (FAILED(hr)) {
                  if ((*ppVPService) != nullptr)
                     (*ppVPService)->Release();
               }
               return hr;
            }

            HRESULT DXVAVideoProcessor::createSurfaces(IDirect3DDeviceManager9* pDeviceManager)
            {
               using namespace Direct3D9;
               HANDLE hDevice;
               HRESULT lresult = pDeviceManager->OpenDeviceHandle(&hDevice);
               if (SUCCEEDED(lresult)) {
                  CComPtrCustom<IDirect3DDevice9> lDirect3DDevice; // Get the video processor service 
                  HRESULT hr2 = pDeviceManager->LockDevice(hDevice, &lDirect3DDevice, TRUE);
                  do {
                     LOG_CHECK_PTR_MEMORY(lDirect3DDevice);
                     LOG_CHECK_PTR_MEMORY(mOutputMediaType);
                     mBackSurface.Release();
                     UINT32 lWidth(0);
                     UINT32 lHeight(0);
                     LOG_INVOKE_FUNCTION(MFGetAttributeSize, mOutputMediaType, MF_MT_FRAME_SIZE, &lWidth, &lHeight);
                     LOG_INVOKE_DX9_METHOD(CreateOffscreenPlainSurface, lDirect3DDevice, lWidth, lHeight,
                                           mMainVideoDesc.Format, D3DPOOL_DEFAULT, &mBackSurface, NULL);
                     lresult = lDirect3DDevice->ColorFill(mBackSurface, nullptr, RGBtoYUV(mBackgroundColor));
                     if (FAILED(lresult)) {
                        break;
                     }
                  } while (false);
                  pDeviceManager->UnlockDevice(hDevice, FALSE); // Close the device handle.
                  lresult = pDeviceManager->CloseDeviceHandle(hDevice);
                  if (FAILED(hr2)) {
                     lresult = hr2;
                  }
               }
               return lresult;
            }

            HRESULT DXVAVideoProcessor::createDXVA2VPDevice(REFGUID guid, IDirectXVideoProcessorService* aPtrVPService,
                                                            DXVA2_VideoDesc* pDesc)
            {
               HRESULT hr; //
               // Query the supported render target format.
               //
               UINT i, count;
               D3DFORMAT* formats = nullptr;
               hr = aPtrVPService->GetVideoProcessorRenderTargets(guid, pDesc, &count, &formats);
               if (FAILED(hr)) {
                  return FALSE;
               }
               for (i = 0; i < count; i++) {
                  if (formats[i] == mVideoRenderTargetFormat) {
                     break;
                  }
               }
               CoTaskMemFree(formats);
               if (i >= count) {
                  return FALSE;
               } //
               // Query the supported substream format.
               //
               formats = nullptr;
               DXVA2_VideoProcessorCaps lVPCaps = {0}; //
               // Query video processor capabilities.
               //
               hr = aPtrVPService->GetVideoProcessorCaps(guid, pDesc, mVideoRenderTargetFormat, &lVPCaps);
               if (FAILED(hr)) {
                  return FALSE;
               } //
               // Check to see if the device is software device.
               //
               if (lVPCaps.DeviceCaps & DXVA2_VPDev_SoftwareDevice) {
                  return FALSE;
               } //
               // This is a progressive device and we cannot provide any reference sample.
               //
               if (lVPCaps.NumForwardRefSamples > 0 || lVPCaps.NumBackwardRefSamples > 0) {
                  return FALSE;
               } //
               // Check to see if the device supports all the VP operations we want.
               //
               if ((lVPCaps.VideoProcessorOperations & VIDEO_REQUIED_OP) != VIDEO_REQUIED_OP) {
                  return FALSE;
               } //
               // Query ProcAmp ranges.
               //
               DXVA2_ValueRange range;
               for (i = 0; i < ARRAYSIZE(g_ProcAmpRanges); i++) {
                  if (lVPCaps.ProcAmpControlCaps & (1 << i)) {
                     hr = aPtrVPService->GetProcAmpRange(guid, pDesc, mVideoRenderTargetFormat, 1 << i, &range);
                     if (FAILED(hr)) {
                        return FALSE;
                     } //
                     // Reset to default value if the range is changed.
                     //
                     if (range != g_ProcAmpRanges[i]) {
                        g_ProcAmpRanges[i] = range;
                        g_ProcAmpValues[i] = range.DefaultValue;
                        g_ProcAmpSteps[i] = ComputeLongSteps(range);
                     }
                  }
               } //
               // Query Noise Filter ranges.
               //
               if (lVPCaps.VideoProcessorOperations & DXVA2_VideoProcess_NoiseFilter) {
                  for (i = 0; i < ARRAYSIZE(g_NFilterRanges); i++) {
                     hr = aPtrVPService->GetFilterPropertyRange(guid, pDesc, mVideoRenderTargetFormat,
                                                                DXVA2_NoiseFilterLumaLevel + i, &range);
                     if (FAILED(hr)) {
                        return FALSE;
                     } //
                     // Reset to default value if the range is changed.
                     //
                     if (range != g_NFilterRanges[i]) {
                        g_NFilterRanges[i] = range;
                        g_NFilterValues[i] = range.DefaultValue;
                     }
                  }
               } //
               // Query Detail Filter ranges.
               //
               if (lVPCaps.VideoProcessorOperations & DXVA2_VideoProcess_DetailFilter) {
                  for (i = 0; i < ARRAYSIZE(g_DFilterRanges); i++) {
                     hr = aPtrVPService->GetFilterPropertyRange(guid, pDesc, mVideoRenderTargetFormat,
                                                                DXVA2_DetailFilterLumaLevel + i, &range);
                     if (FAILED(hr)) {
                        return FALSE;
                     } //
                     // Reset to default value if the range is changed.
                     //
                     if (range != g_DFilterRanges[i]) {
                        g_DFilterRanges[i] = range;
                        g_DFilterValues[i] = range.DefaultValue;
                     }
                  }
               }
               mDXVAVPD.Release(); //
               // Finally create a video processor device.
               //
               hr = aPtrVPService->CreateVideoProcessor(guid, pDesc, mVideoRenderTargetFormat, mSubStreamCount + 1,
                                                        &mDXVAVPD);
               if (FAILED(hr)) {
                  return FALSE;
               }
               mProcessorGUID = guid;
               mVPCaps = lVPCaps; //WCHAR *lptrName = nullptr;
               //HRESULT lr = StringFromCLSID(guid, &lptrName);
               //if (SUCCEEDED(lr))
               //LogPrintOut::getInstance().printOutln(
               //	LogPrintOut::ERROR_LEVEL,
               //	L"VideoProcessor: ",
               //	lptrName);
               ////MessageBox(0, lptrName, lptrName, 0);
               //if (lptrName != nullptr)
               //	CoTaskMemFree(lptrName);
               return TRUE;
            }

            HRESULT DXVAVideoProcessor::createVideoProcessor()
            {
               HRESULT lresult(E_FAIL);
               do {
                  MFRatio lFrameRate;
                  lFrameRate.Numerator = 30000;
                  lFrameRate.Denominator = 1000;
                  MFRatio lPixelAperture;
                  lPixelAperture.Numerator = 1;
                  lPixelAperture.Denominator = 1;
                  CComPtrCustom<IMFMediaType> lInputMediaType;
                  createUncompressedVideoType( //D3DFMT_YUY2,
                     VIDEO_MAIN_FORMAT, 1920, 1080, MFVideoInterlace_Progressive, lFrameRate, lPixelAperture,
                     &lInputMediaType);
                  LOG_CHECK_PTR_MEMORY(lInputMediaType);
                  lresult = ConvertMFTypeToDXVAType(lInputMediaType, &mMainVideoDesc);
                  if (FAILED(lresult))
                     break; //
                  // Query the video processor GUID.
                  //
                  UINT count;
                  GUID* guids = nullptr;
                  CComPtrCustom<IDirectXVideoProcessorService> lVPService;
                  lresult = GetVideoProcessorService(mDeviceManager, &lVPService);
                  if (FAILED(lresult)) {
                     break;
                  }
                  mDXVAVPDService = lVPService;
                  lresult = lVPService->GetVideoProcessorDeviceGuids(&mMainVideoDesc, &count, &guids);
                  if (FAILED(lresult) || guids == nullptr) {
                     break;
                  }
                  for (UINT i = 0; i < count; i++) {
                     if (createDXVA2VPDevice(guids[i], lVPService, &mMainVideoDesc)) {
                        break;
                     }
                  }
                  CoTaskMemFree(guids);
               } while (false);
               return lresult;
            }

            BOOL DXVAVideoProcessor::checkVideoProcessorGUID(
               REFGUID aGUID, IDirectXVideoProcessorService* aPtrVPService, DXVA2_VideoDesc* pDesc)
            {
               UINT i, count;
               D3DFORMAT* formats = nullptr;
               HRESULT lresult = aPtrVPService->GetVideoProcessorRenderTargets(aGUID, pDesc, &count, &formats);
               if (FAILED(lresult) || formats == nullptr) {
                  return FALSE;
               }
               for (i = 0; i < count; i++) {
                  if (formats[i] == mVideoRenderTargetFormat) {
                     break;
                  }
               }
               CoTaskMemFree(formats);
               if (i >= count) {
                  return FALSE;
               }
               DXVA2_VideoProcessorCaps lVPCaps = {0}; //
               // Query video processor capabilities.
               //
               lresult = aPtrVPService->GetVideoProcessorCaps(aGUID, pDesc, mVideoRenderTargetFormat, &lVPCaps);
               if (FAILED(lresult)) {
                  return FALSE;
               } //
               // This is a progressive device and we cannot provide any reference sample.
               //
               if (lVPCaps.NumForwardRefSamples > 0 || lVPCaps.NumBackwardRefSamples > 0) {
                  return FALSE;
               } //
               // Check to see if the device supports all the VP operations we want.
               //
               if ((lVPCaps.VideoProcessorOperations & VIDEO_REQUIED_OP) != VIDEO_REQUIED_OP) {
                  return FALSE;
               }
               return TRUE;
            }

            HRESULT DXVAVideoProcessor::ConvertMFTypeToDXVAType(IMFMediaType* pType, DXVA2_VideoDesc* pDesc)
            {
               ZeroMemory(pDesc, sizeof(*pDesc));
               GUID subtype = GUID_NULL;
               UINT32 width = 0;
               UINT32 height = 0;
               UINT32 fpsNumerator = 0;
               UINT32 fpsDenominator = 0; // The D3D format is the first DWORD of the subtype GUID.
               HRESULT hr = pType->GetGUID(MF_MT_SUBTYPE, &subtype);
               if (FAILED(hr)) {
                  goto done;
               }
               pDesc->Format = static_cast<D3DFORMAT>(subtype.Data1); // Frame size.
               hr = MFGetAttributeSize(pType, MF_MT_FRAME_SIZE, &width, &height);
               if (FAILED(hr)) {
                  goto done;
               }
               pDesc->SampleWidth = width;
               pDesc->SampleHeight = height; // Frame rate.
               hr = MFGetAttributeRatio(pType, MF_MT_FRAME_RATE, &fpsNumerator, &fpsDenominator);
               if (FAILED(hr)) {
                  goto done;
               }
               pDesc->InputSampleFreq.Numerator = fpsNumerator;
               pDesc->InputSampleFreq.Denominator = fpsDenominator; // Extended format information.
               GetDXVA2ExtendedFormatFromMFMediaType(pType, &pDesc->SampleFormat);
               // For progressive or single-field types, the output frequency is the same as
               // the input frequency. For interleaved-field types, the output frequency is
               // twice the input frequency.  
               pDesc->OutputFrameFreq = pDesc->InputSampleFreq;
               if ((pDesc->SampleFormat.SampleFormat == DXVA2_SampleFieldInterleavedEvenFirst) || (
                      pDesc->SampleFormat.SampleFormat == DXVA2_SampleFieldInterleavedOddFirst)) {
                  pDesc->OutputFrameFreq.Numerator *= 2;
               }
               pDesc->SampleFormat.VideoChromaSubsampling = DXVA2_VideoChromaSubsampling_MPEG2;
               pDesc->SampleFormat.NominalRange = DXVA2_NominalRange_16_235;
               pDesc->SampleFormat.VideoTransferMatrix = EX_COLOR_INFO[g_ExColorInfo][0];
               pDesc->SampleFormat.VideoLighting = DXVA2_VideoLighting_dim;
               pDesc->SampleFormat.VideoPrimaries = DXVA2_VideoPrimaries_BT709;
               pDesc->SampleFormat.VideoTransferFunction = DXVA2_VideoTransFunc_709;
            done: return hr;
            }

            void DXVAVideoProcessor::GetDXVA2ExtendedFormatFromMFMediaType(
               IMFMediaType* pType, DXVA2_ExtendedFormat* pFormat)
            {
               // Get the interlace mode.
               MFVideoInterlaceMode interlace = static_cast<MFVideoInterlaceMode>(MFGetAttributeUINT32(
                  pType, MF_MT_INTERLACE_MODE, MFVideoInterlace_Unknown));
               // The values for interlace mode translate directly, except for mixed 
               // interlace or progressive mode.
               if (interlace == MFVideoInterlace_MixedInterlaceOrProgressive) {
                  // Default to interleaved fields.
                  pFormat->SampleFormat = DXVA2_SampleFieldInterleavedEvenFirst;
               } else {
                  pFormat->SampleFormat = static_cast<UINT>(interlace);
               } // The remaining values translate directly.
               // Use the "no-fail" attribute functions and default to "unknown."
               pFormat->VideoChromaSubsampling = MFGetAttributeUINT32(pType, MF_MT_VIDEO_CHROMA_SITING,
                                                                      MFVideoChromaSubsampling_Unknown);
               pFormat->NominalRange = MFGetAttributeUINT32(pType, MF_MT_VIDEO_NOMINAL_RANGE, MFNominalRange_Unknown);
               pFormat->VideoTransferMatrix = MFGetAttributeUINT32(pType, MF_MT_YUV_MATRIX,
                                                                   MFVideoTransferMatrix_Unknown);
               pFormat->VideoLighting = MFGetAttributeUINT32(pType, MF_MT_VIDEO_LIGHTING, MFVideoLighting_Unknown);
               pFormat->VideoPrimaries = MFGetAttributeUINT32(pType, MF_MT_VIDEO_PRIMARIES, MFVideoPrimaries_Unknown);
               pFormat->VideoTransferFunction = MFGetAttributeUINT32(pType, MF_MT_TRANSFER_FUNCTION,
                                                                     MFVideoTransFunc_Unknown);
            }

            HRESULT DXVAVideoProcessor::checkSupportedSubFormat(DXVA2_VideoDesc* pDesc)
            {
               HRESULT lresult = S_OK;
               BOOL lcheck(FALSE);
               do {
                  LOG_CHECK_PTR_MEMORY(pDesc); //
                  // Query the video processor GUID.
                  //
                  UINT count;
                  GUID* guids = nullptr;
                  CComPtrCustom<IDirectXVideoProcessorService> lVPService;
                  lresult = GetVideoProcessorService(mDeviceManager, &lVPService);
                  if (FAILED(lresult)) {
                     break;
                  }
                  lresult = lVPService->GetVideoProcessorDeviceGuids(pDesc, &count, &guids);
                  if (FAILED(lresult) || guids == nullptr) {
                     break;
                  } //
                  // Create a DXVA2 device.
                  //
                  for (UINT i = 0; i < count; i++) {
                     lcheck = checkVideoProcessorGUID(guids[i], lVPService, pDesc);
                     if (lcheck == TRUE) {
                        lresult = S_OK;
                        break;
                     }
                  }
                  CoTaskMemFree(guids);
               } while (false);
               return lresult;
            }

            DXVA2_AYUVSample16 DXVAVideoProcessor::GetBackgroundColor()
            {
               const D3DCOLOR yuv = RGBtoYUV(D3DCOLOR_XRGB(0x10, 0x10, 0x10));
               const BYTE Y = LOBYTE(HIWORD(yuv));
               const BYTE U = HIBYTE(LOWORD(yuv));
               const BYTE V = LOBYTE(LOWORD(yuv));
               DXVA2_AYUVSample16 color;
               color.Cr = V * 0x100;
               color.Cb = U * 0x100;
               color.Y = Y * 0x100;
               color.Alpha = 0xFFFF;
               return color;
            }

            DWORD DXVAVideoProcessor::RGBtoYUV(const D3DCOLOR rgb)
            {
               const INT A = HIBYTE(HIWORD(rgb));
               const INT R = LOBYTE(HIWORD(rgb)) - 16;
               const INT G = HIBYTE(LOWORD(rgb)) - 16;
               const INT B = LOBYTE(LOWORD(rgb)) - 16; //
               // studio RGB [16...235] to SDTV ITU-R BT.601 YCbCr
               //
               INT Y = (77 * R + 150 * G + 29 * B + 128) / 256 + 16;
               INT U = (-44 * R - 87 * G + 131 * B + 128) / 256 + 128;
               INT V = (131 * R - 110 * G - 21 * B + 128) / 256 + 128;
               return D3DCOLOR_AYUV(A, Y, U, V);
            }

            INT DXVAVideoProcessor::ComputeLongSteps(DXVA2_ValueRange& range)
            {
               float f_step = DXVA2FixedToFloat(range.StepSize);
               if (f_step == 0.0) {
                  return 0;
               }
               float f_max = DXVA2FixedToFloat(range.MaxValue);
               float f_min = DXVA2FixedToFloat(range.MinValue);
               INT steps = INT((f_max - f_min) / f_step / 32);
               return max(steps, 1);
            }

            RECT DXVAVideoProcessor::ScaleRectangle(const RECT& input, const RECT& src, const RECT& dst)
            {
               RECT rect;
               UINT src_dx = src.right - src.left;
               UINT src_dy = src.bottom - src.top;
               UINT dst_dx = dst.right - dst.left;
               UINT dst_dy = dst.bottom - dst.top; //
               // Scale input rectangle within src rectangle to dst rectangle.
               //
               rect.left = input.left * dst_dx / src_dx;
               rect.right = input.right * dst_dx / src_dx;
               rect.top = input.top * dst_dy / src_dy;
               rect.bottom = input.bottom * dst_dy / src_dy;
               return rect;
            }

            HRESULT DXVAVideoProcessor::blit(IMFSample* aPtrSample, REFERENCE_TIME aTargetFrame,
                                             REFERENCE_TIME aTargetEndFrame)
            {
               HRESULT lresult(E_FAIL);
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrSample);
                  LOG_CHECK_PTR_MEMORY(mDXVAVPD);
                  LOG_CHECK_PTR_MEMORY(mBackSurface);
                  CComPtrCustom<IMFMediaBuffer> lDestBuffer;
                  lresult = aPtrSample->GetBufferByIndex(0, &lDestBuffer);
                  if (FAILED(lresult)) {
                     break;
                  }
                  CComPtrCustom<IDirect3DSurface9> lDestSurface; // Get the surface from the buffer.
                  LOG_INVOKE_MF_FUNCTION(MFGetService, lDestBuffer, MR_BUFFER_SERVICE, IID_PPV_ARGS(&lDestSurface));
                  if (FAILED(lresult)) {
                     break;
                  }
                  DXVA2_VideoProcessBltParams blt = {0};
                  DXVA2_VideoSample samples[15] = {0};
                  LONGLONG start_100ns = aTargetFrame;
                  LONGLONG end_100ns = aTargetEndFrame;
                  D3DSURFACE_DESC lDestDesc;
                  lDestSurface->GetDesc(&lDestDesc);
                  mOutputTargetRect.left = 0;
                  mOutputTargetRect.right = lDestDesc.Width;
                  mOutputTargetRect.top = 0;
                  mOutputTargetRect.bottom = lDestDesc.Height;
                  float lPropDest = static_cast<float>(lDestDesc.Width) / static_cast<float>(lDestDesc.Height);
                  blt.TargetFrame = aTargetFrame;
                  blt.TargetRect = mOutputTargetRect; // DXVA2_VideoProcess_Constriction
                  blt.ConstrictionSize.cx = mOutputTargetRect.right - mOutputTargetRect.left;
                  blt.ConstrictionSize.cy = mOutputTargetRect.bottom - mOutputTargetRect.top;
                  blt.BackgroundColor = GetBackgroundColor(); // DXVA2_VideoProcess_YUV2RGBExtended
                  blt.DestFormat.VideoChromaSubsampling = DXVA2_VideoChromaSubsampling_Unknown;
                  blt.DestFormat.NominalRange = EX_COLOR_INFO[g_ExColorInfo][1];
                  blt.DestFormat.VideoTransferMatrix = DXVA2_VideoTransferMatrix_Unknown;
                  blt.DestFormat.VideoLighting = DXVA2_VideoLighting_dim;
                  blt.DestFormat.VideoPrimaries = DXVA2_VideoPrimaries_BT709;
                  blt.DestFormat.VideoTransferFunction = DXVA2_VideoTransFunc_709;
                  blt.DestFormat.SampleFormat = DXVA2_SampleProgressiveFrame; // DXVA2_ProcAmp_Brightness
                  blt.ProcAmpValues.Brightness = g_ProcAmpValues[0];          // DXVA2_ProcAmp_Contrast
                  blt.ProcAmpValues.Contrast = g_ProcAmpValues[1];            // DXVA2_ProcAmp_Hue
                  blt.ProcAmpValues.Hue = g_ProcAmpValues[2];                 // DXVA2_ProcAmp_Saturation
                  blt.ProcAmpValues.Saturation = g_ProcAmpValues[3];          // DXVA2_VideoProcess_AlphaBlend
                  blt.Alpha = DXVA2_Fixed32OpaqueAlpha();                     // DXVA2_VideoProcess_NoiseFilter
                  blt.NoiseFilterLuma.Level = g_NFilterValues[0];
                  blt.NoiseFilterLuma.Threshold = g_NFilterValues[1];
                  blt.NoiseFilterLuma.Radius = g_NFilterValues[2];
                  blt.NoiseFilterChroma.Level = g_NFilterValues[3];
                  blt.NoiseFilterChroma.Threshold = g_NFilterValues[4];
                  blt.NoiseFilterChroma.Radius = g_NFilterValues[5]; // DXVA2_VideoProcess_DetailFilter
                  blt.DetailFilterLuma.Level = g_DFilterValues[0];
                  blt.DetailFilterLuma.Threshold = g_DFilterValues[1];
                  blt.DetailFilterLuma.Radius = g_DFilterValues[2];
                  blt.DetailFilterChroma.Level = g_DFilterValues[3];
                  blt.DetailFilterChroma.Threshold = g_DFilterValues[4];
                  blt.DetailFilterChroma.Radius = g_DFilterValues[5]; //
                  // Initialize main stream video sample.
                  //
                  UINT lStreamIndex = 0;
                  {
                     samples[lStreamIndex].Start = start_100ns;
                     samples[lStreamIndex].End = end_100ns; // DXVA2_VideoProcess_YUV2RGBExtended
                     samples[lStreamIndex].SampleFormat.VideoChromaSubsampling = DXVA2_VideoChromaSubsampling_MPEG2;
                     samples[lStreamIndex].SampleFormat.NominalRange = DXVA2_NominalRange_16_235;
                     samples[lStreamIndex].SampleFormat.VideoTransferMatrix = EX_COLOR_INFO[g_ExColorInfo][0];
                     samples[lStreamIndex].SampleFormat.VideoLighting = DXVA2_VideoLighting_dim;
                     samples[lStreamIndex].SampleFormat.VideoPrimaries = DXVA2_VideoPrimaries_BT709;
                     samples[lStreamIndex].SampleFormat.VideoTransferFunction = DXVA2_VideoTransFunc_709;
                     samples[lStreamIndex].SampleFormat.SampleFormat = DXVA2_SampleProgressiveFrame;
                     D3DSURFACE_DESC lDesc1;
                     mBackSurface->GetDesc(&lDesc1);
                     RECT lSubStreamSrcRect;
                     lSubStreamSrcRect.left = 0;
                     lSubStreamSrcRect.right = lDesc1.Width;
                     lSubStreamSrcRect.top = 0;
                     lSubStreamSrcRect.bottom = lDesc1.Height;
                     float lPropSrc = static_cast<float>(lDesc1.Width) / static_cast<float>(lDesc1.Height);
                     if (lPropDest >= lPropSrc) {
                        UINT lidealWidth = (lDestDesc.Height * lDesc1.Width) / lDesc1.Height;
                        LONG lborder = static_cast<LONG>(lDestDesc.Width - lidealWidth) >> 1;
                        lSubStreamSrcRect.left = lborder;
                        lSubStreamSrcRect.right = lidealWidth + lborder;
                        lSubStreamSrcRect.top = 0;
                        lSubStreamSrcRect.bottom = lDestDesc.Height;
                     } else {
                        UINT lidealHeight = (lDestDesc.Width * lDesc1.Height) / lDesc1.Width;
                        LONG lborder = static_cast<LONG>(lDestDesc.Height - lidealHeight) >> 1;
                        lSubStreamSrcRect.top = lborder;
                        lSubStreamSrcRect.bottom = lidealHeight + lborder;
                        lSubStreamSrcRect.left = 0;
                        lSubStreamSrcRect.right = lDestDesc.Width;
                     }
                     samples[lStreamIndex].SrcSurface = mBackSurface;   // DXVA2_VideoProcess_SubRects
                     samples[lStreamIndex].SrcRect = lSubStreamSrcRect; // DXVA2_VideoProcess_StretchX, Y
                     samples[lStreamIndex].DstRect = mOutputTargetRect; // DXVA2_VideoProcess_PlanarAlpha
                     samples[lStreamIndex].PlanarAlpha = DXVA2FloatToFixed(float(0xFF) / 0xFF);
                     ++lStreamIndex;
                  }
                  for (auto& lIndexID : m_dwZOrders) {
                     auto lIter = m_InputStreams.find(lIndexID);
                     if (lIter == m_InputStreams.end())
                        continue;
                     auto& lItem = (*lIter).second;
                     if (!lItem.mSample)
                        continue;
                     auto lVideoNormalizedRect = lItem.mDestVideoNormalizedRect;
                     CComPtrCustom<IMFMediaBuffer> lBuffer;
                     lItem.mSample->GetBufferByIndex(0, &lBuffer);
                     CComPtrCustom<IDirect3DSurface9> lSurface; // Get the surface from the buffer.
                     LOG_INVOKE_MF_FUNCTION(MFGetService, lBuffer, MR_BUFFER_SERVICE, IID_PPV_ARGS(&lSurface));
                     if (FAILED(lresult)) {
                        break;
                     }
                     D3DSURFACE_DESC lDesc1;
                     lSurface->GetDesc(&lDesc1);
                     if (lDesc1.Pool != mVPCaps.InputPool) {
                        CComPtrCustom<IDirect3DSurface9> lSubSurface;
                        lSubSurface = lItem.mSubStreamSurface;
                        CComPtrCustom<IDirect3DDevice9> lDevice;
                        lSubSurface->GetDevice(&lDevice);
                        if (lDevice) {
                           lresult = lDevice->UpdateSurface(lSurface, nullptr, lSubSurface, nullptr);
                        }
                        if (SUCCEEDED(lresult))
                           lSurface = lSubSurface;
                     }
                     lSurface->GetDesc(&lDesc1);
                     RECT lSubStreamSrcRect;
                     lSubStreamSrcRect.left = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.left * static_cast<float>(lDesc1.Width));
                     lSubStreamSrcRect.right = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.right * static_cast<float>(lDesc1.Width));
                     lSubStreamSrcRect.top = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.top * static_cast<float>(lDesc1.Height));
                     lSubStreamSrcRect.bottom = static_cast<LONG>(
                        lItem.mSrcVideoNormalizedRect.bottom * static_cast<float>(lDesc1.Height));
                     auto lScaledWidthDest = static_cast<float>(lDestDesc.Width) * (
                                                lVideoNormalizedRect.right - lVideoNormalizedRect.left);
                     auto lScaledHeightDest = static_cast<float>(lDestDesc.Height) * (
                                                 lVideoNormalizedRect.bottom - lVideoNormalizedRect.top);
                     auto lLeftOffset = static_cast<float>(lDestDesc.Width) * lVideoNormalizedRect.left;
                     auto lTopOffset = static_cast<float>(lDestDesc.Height) * lVideoNormalizedRect.top;
                     RECT lSubStreamDestRect;
                     if (lItem.mStretchState == SOURCE) {
                        float lPropSrc = static_cast<float>(lDesc1.Width) / static_cast<float>(lDesc1.Height);
                        float llPropScaledDest = lPropSrc;
                        if (lScaledHeightDest > 0.0f)
                           llPropScaledDest = lScaledWidthDest / lScaledHeightDest;
                        if (llPropScaledDest >= lPropSrc) {
                           auto lprop = static_cast<float>(lScaledHeightDest) / static_cast<float>(lDesc1.Height);
                           auto lidealWidth = lprop * static_cast<float>(lDesc1.Width);
                           auto lborder = static_cast<float>(lScaledWidthDest - lidealWidth) * 0.5f;
                           lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset + lborder);
                           lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lidealWidth + lborder);
                           lSubStreamDestRect.top = static_cast<LONG>(lTopOffset);
                           lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lScaledHeightDest);
                        } else {
                           auto lprop = static_cast<float>(lScaledWidthDest) / static_cast<float>(lDesc1.Width);
                           auto lidealHeight = lprop * static_cast<float>(lDesc1.Height);
                           auto lborder = static_cast<float>(lScaledHeightDest - lidealHeight) * 0.5f;
                           lSubStreamDestRect.top = static_cast<LONG>(lTopOffset + lborder);
                           lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lidealHeight + lborder);
                           lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset);
                           lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lScaledWidthDest);
                        }
                     } else if (lItem.mStretchState == FILL) {
                        lSubStreamDestRect.left = static_cast<LONG>(lLeftOffset);
                        lSubStreamDestRect.right = static_cast<LONG>(lLeftOffset + lScaledWidthDest);
                        lSubStreamDestRect.top = static_cast<LONG>(lTopOffset);
                        lSubStreamDestRect.bottom = static_cast<LONG>(lTopOffset + lScaledHeightDest);
                     } //
                     // Initialize sub stream video sample.
                     //
                     samples[lStreamIndex] = samples[0];
                     // DXVA2_VideoProcess_SubStreamsExtended
                     samples[lStreamIndex].SampleFormat = samples[0].SampleFormat; // DXVA2_VideoProcess_SubStreams
                     samples[lStreamIndex].SampleFormat.SampleFormat = DXVA2_SampleSubStream;
                     samples[lStreamIndex].SrcSurface = lSurface;
                     samples[lStreamIndex].SrcRect = lSubStreamSrcRect;
                     samples[lStreamIndex].DstRect = lSubStreamDestRect;
                     samples[lStreamIndex].PlanarAlpha = DXVA2_Fixed32OpaqueAlpha();
                     ++lStreamIndex;
                  }
                  if (lStreamIndex > 0)
                     lresult = mDXVAVPD->VideoProcessBlt(lDestSurface, &blt, samples, lStreamIndex, nullptr);
                  if (FAILED(lresult)) {
                     break;
                  }
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::createMediaType(IMFMediaType* aPtrUpStreamMediaType, GUID aMFVideoFormat,
                                                        IMFMediaType** aPtrPtrMediaSinkMediaType)
            {
               HRESULT lresult;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrUpStreamMediaType);
                  LOG_CHECK_PTR_MEMORY(aPtrPtrMediaSinkMediaType);
                  CComPtrCustom<IMFMediaType> lnewMediaType;
                  LOG_INVOKE_MF_FUNCTION(MFCreateMediaType, &lnewMediaType);
                  LOG_CHECK_PTR_MEMORY(lnewMediaType);
                  LOG_INVOKE_MF_METHOD(SetGUID, lnewMediaType, MF_MT_MAJOR_TYPE, MFMediaType_Video);
                  LOG_INVOKE_MF_METHOD(SetGUID, lnewMediaType, MF_MT_SUBTYPE, aMFVideoFormat);
                  LOG_INVOKE_MF_METHOD(SetUINT32, lnewMediaType, MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
                  LOG_INVOKE_MF_METHOD(SetUINT32, lnewMediaType, MF_MT_ALL_SAMPLES_INDEPENDENT, TRUE);
                  PROPVARIANT lVarItem;
                  LOG_INVOKE_MF_METHOD(GetItem, aPtrUpStreamMediaType, MF_MT_FRAME_SIZE, &lVarItem);
                  LOG_INVOKE_MF_METHOD(SetItem, lnewMediaType, MF_MT_FRAME_SIZE, lVarItem);
                  LOG_INVOKE_MF_METHOD(GetItem, aPtrUpStreamMediaType, MF_MT_FRAME_RATE, &lVarItem);
                  LOG_INVOKE_MF_METHOD(SetItem, lnewMediaType, MF_MT_FRAME_RATE, lVarItem);
                  LOG_INVOKE_MF_METHOD(GetItem, aPtrUpStreamMediaType, MF_MT_PIXEL_ASPECT_RATIO, &lVarItem);
                  LOG_INVOKE_MF_METHOD(SetItem, lnewMediaType, MF_MT_PIXEL_ASPECT_RATIO, lVarItem);
                  UINT32 lWidthInPixels = 0;
                  UINT32 lHeightInPixels = 0;
                  LOG_INVOKE_FUNCTION(MediaFoundation::MediaFoundationManager::GetAttributeSize, lnewMediaType,
                                      MF_MT_FRAME_SIZE, lWidthInPixels, lHeightInPixels);
                  do {
                     LOG_INVOKE_MF_METHOD(GetItem, aPtrUpStreamMediaType, MF_MT_DEFAULT_STRIDE, &lVarItem);
                     LOG_INVOKE_MF_METHOD(SetItem, lnewMediaType, MF_MT_DEFAULT_STRIDE, lVarItem);
                  } while (false);
                  lresult = S_OK;
                  *aPtrPtrMediaSinkMediaType = lnewMediaType.detach();
               } while (false);
               return lresult;
            }

            HRESULT DXVAVideoProcessor::createUncompressedVideoType(DWORD fccFormat, // FOURCC or D3DFORMAT value.     
                                                                    UINT32 width, UINT32 height,
                                                                    MFVideoInterlaceMode interlaceMode,
                                                                    const MFRatio& frameRate, const MFRatio& par,
                                                                    IMFMediaType** ppType)
            {
               if (ppType == nullptr) {
                  return E_POINTER;
               }
               GUID subtype = MFVideoFormat_Base;
               LONG lStride = 0;
               UINT cbImage = 0;
               CComPtrCustom<IMFMediaType> pType; // Set the subtype GUID from the FOURCC or D3DFORMAT value.
               subtype.Data1 = fccFormat;
               HRESULT hr = MediaFoundation::MediaFoundationManager::MFCreateMediaType(&pType);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = pType->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = pType->SetGUID(MF_MT_SUBTYPE, subtype);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = pType->SetUINT32(MF_MT_INTERLACE_MODE, interlaceMode);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = MFSetAttributeSize(pType, MF_MT_FRAME_SIZE, width, height);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = MediaFoundation::MediaFoundationManager::MFGetStrideForBitmapInfoHeader(fccFormat, width, &lStride);
               if (FAILED(hr)) {
                  goto done;
               } // Calculate the default stride value.
               hr = pType->SetUINT32(MF_MT_DEFAULT_STRIDE, UINT32(lStride));
               if (FAILED(hr)) {
                  goto done;
               } // Calculate the image size in bytes.
               hr = MediaFoundation::MediaFoundationManager::MFCalculateImageSize(subtype, width, height, &cbImage);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = pType->SetUINT32(MF_MT_SAMPLE_SIZE, cbImage);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = pType->SetUINT32(MF_MT_FIXED_SIZE_SAMPLES, TRUE);
               if (FAILED(hr)) {
                  goto done;
               }
               hr = pType->SetUINT32(MF_MT_ALL_SAMPLES_INDEPENDENT, TRUE);
               if (FAILED(hr)) {
                  goto done;
               } // Frame rate
               hr = MFSetAttributeRatio(pType, MF_MT_FRAME_RATE, frameRate.Numerator, frameRate.Denominator);
               if (FAILED(hr)) {
                  goto done;
               } // Pixel aspect ratio
               hr = MFSetAttributeRatio(pType, MF_MT_PIXEL_ASPECT_RATIO, par.Numerator, par.Denominator);
               if (FAILED(hr)) {
                  goto done;
               } // Return the pointer to the caller.
               if (pType) {
                  hr = pType->QueryInterface(IID_PPV_ARGS(ppType));
               }
            done: return hr;
            }

            HRESULT DXVAVideoProcessor::getStreamStretch(DWORD aInputStreamID, int aIndex, pugi::xml_node& aFilterNode)
            {
               HRESULT lresult(E_FAIL);
               do {
                  auto lIter = m_InputStreams.find(aInputStreamID);
                  LOG_CHECK_STATE_DESCR(lIter == m_InputStreams.end(), MF_E_INVALIDSTREAMNUMBER);
                  auto& lItem = (*lIter).second;
                  aFilterNode.append_attribute(L"Index").set_value(aIndex);
                  aFilterNode.append_attribute(L"Min").set_value(SOURCE);
                  aFilterNode.append_attribute(L"Max").set_value(MAX - 1);
                  aFilterNode.append_attribute(L"CurrentValue").set_value(lItem.mStretchState);
                  aFilterNode.append_attribute(L"Step").set_value(1);
                  aFilterNode.append_attribute(L"Multiplier").set_value(1);
                  aFilterNode.append_attribute(L"Default").set_value(SOURCE);
                  aFilterNode.append_attribute(L"IsEnabled").set_value(TRUE);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }
         }
      }
   }
}
