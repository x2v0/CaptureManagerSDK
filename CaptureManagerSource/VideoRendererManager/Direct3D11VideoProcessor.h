#pragma once
#include <atomic>
#include <condition_variable>
#include <memory>
#include <vector>
#include <unordered_map>
#include <DirectXMath.h>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "IMixerStreamPositionControl.h"
#include "IStreamFilterControl.h"
#include "../PugiXML/pugixml.hpp"

namespace CaptureManager
{
   namespace Sinks
   {
      namespace EVR
      {
         namespace Mixer
         {
            class Direct3D11VideoProcessor : public BaseUnknown<
                  IMFTransform, IMixerStreamPositionControl, IStreamFilterControl>
            {
               struct StreamInfo
               {
                  CComPtrCustom<IMFMediaType> mInputMediaType;
                  CComPtrCustom<IMFSample> mSample;
                  MFVideoNormalizedRect mSrcVideoNormalizedRect;
                  MFVideoNormalizedRect mDestVideoNormalizedRect;
                  DXVA2_Fixed32 mDXVA2_Fixed32;

                  StreamInfo()
                  {
                     ZeroMemory(&mDestVideoNormalizedRect, sizeof(mDestVideoNormalizedRect));
                     mDestVideoNormalizedRect.bottom = 1.0f;
                     mDestVideoNormalizedRect.right = 1.0f;
                     ZeroMemory(&mSrcVideoNormalizedRect, sizeof(mSrcVideoNormalizedRect));
                     mSrcVideoNormalizedRect.bottom = 1.0f;
                     mSrcVideoNormalizedRect.right = 1.0f;
                     mDXVA2_Fixed32 = DXVA2FloatToFixed(1.0f);
                  }
               };

               std::unordered_map<DWORD, StreamInfo> m_InputStreams;
            public:
               Direct3D11VideoProcessor();

               static HRESULT createProcessor(IMFTransform** aPtrPtrTrsnaform, DWORD aMaxInputStreamCount,
                                              bool aIsFlipEnabled = false);

               static HRESULT convertDXGIFormatToSubType(const DXGI_FORMAT& aRefDXGIFormat, GUID* aPtrSubtype);

               static HRESULT convertSubTypeToDXGIFormat(const GUID& aRefSubtype, DXGI_FORMAT* aPtrDXGIFormat);

               // IMixerStreamPositionControl methods
               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE setPosition(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aLeft, /* [in] */
                                                                FLOAT aRight, /* [in] */ FLOAT aTop,          /* [in] */
                                                                FLOAT aBottom) override;

               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE setSrcPosition(/* [in] */ DWORD aStreamID, /* [in] */ FLOAT aLeft, /* [in] */
                                                                   FLOAT aRight, /* [in] */ FLOAT aTop,     /* [in] */
                                                                   FLOAT aBottom) override;

               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE setOpacity(/* [in] */ DWORD aInputStreamID, /* [in] */ FLOAT aOpacity)
               override; /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE setZOrder(/* [in] */ DWORD aInputStreamID, /* [in] */ DWORD aZOrder) override;

               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE getPosition(/* [in] */ DWORD aInputStreamID, /* [out] */ FLOAT* aPtrLeft,
                                                                /* [out] */ FLOAT* aPtrRight, /* [out] */
                                                                FLOAT* aPtrTop, /* [out] */ FLOAT* aPtrBottom) override;

               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE getSrcPosition(/* [in] */ DWORD aStreamID, /* [out] */ FLOAT* aPtrLeft,
                                                                   /* [out] */ FLOAT* aPtrRight, /* [out] */
                                                                   FLOAT* aPtrTop, /* [out] */ FLOAT* aPtrBottom)
               override; /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE getOpacity(/* [in] */ DWORD aInputStreamID, /* [out] */ FLOAT* aPtrOpacity)
               override; /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE getZOrder(/* [in] */ DWORD aInputStreamID, /* [out] */ DWORD* aPtrZOrder)
               override; /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE flush(/* [in] */ DWORD aInputStreamID) override;

               // IStreamFilterControl methods
               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE getCollectionOfFilters(
                  /* [in] */ DWORD aMixerStreamID, /* [out] */ BSTR* aPtrPtrXMLstring) override; /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE setFilterParametr(/* [in] */ DWORD aMixerStreamID,      /* [in] */
                                                                      DWORD aParametrIndex, /* [in] */ LONG aNewValue,
                                                                      /* [in] */ BOOL aIsEnabled) override;

               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE getCollectionOfOutputFeatures(/* [out] */ BSTR* aPtrPtrXMLstring) override;

               /* [id][helpstring] */
               HRESULT STDMETHODCALLTYPE setOutputFeatureParametr(
                  /* [in] */ DWORD aParametrIndex, /* [in] */ LONG aNewValue) override; // IMFTransform methods
               STDMETHODIMP GetStreamLimits(DWORD* aPtrInputMinimum, DWORD* aPtrInputMaximum, DWORD* aPtrOutputMinimum,
                                            DWORD* aPtrOutputMaximum) override;

               STDMETHODIMP GetStreamIDs(DWORD aInputIDArraySize, DWORD* aPtrInputIDs, DWORD aOutputIDArraySize,
                                         DWORD* aPtrOutputIDs) override;

               STDMETHODIMP GetStreamCount(DWORD* aPtrInputStreams, DWORD* aPtrOutputStreams) override;

               STDMETHODIMP GetInputStreamInfo(DWORD aInputStreamID, MFT_INPUT_STREAM_INFO* aPtrStreamInfo) override;

               STDMETHODIMP GetOutputStreamInfo(DWORD aOutputStreamID, MFT_OUTPUT_STREAM_INFO* aPtrStreamInfo) override;

               STDMETHODIMP GetInputStreamAttributes(DWORD aInputStreamID, IMFAttributes** aPtrPtrAttributes) override;

               STDMETHODIMP GetOutputStreamAttributes(DWORD aOutputStreamID, IMFAttributes** aPtrPtrAttributes)
               override;

               STDMETHODIMP DeleteInputStream(DWORD aInputStreamID) override;

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

            private:
               typedef struct _VERTEX
               {
                  DirectX::XMFLOAT3 Pos;
                  DirectX::XMFLOAT2 TexCoord;
               } VERTEX;

#define NUMVERTICES 6
               VERTEX mVertexes[NUMVERTICES];

               static const struct FormatEntry
               {
                  GUID Subtype;
                  DXGI_FORMAT DXGIFormat;
               } mDXGIFormatMapping[];

               std::condition_variable mConditionVariable;
               std::mutex mZOrderMutex;
               std::mutex mMutex;
               UINT mSubStreamCount;
               UINT64 mAverageTimePerFrame;
               bool mIsFlipEnabled;
               D3D11_VIDEO_PROCESSOR_COLOR_SPACE mColorSpace;
               D3D11_VIDEO_COLOR mBackgroundColor;
               std::vector<DWORD> m_dwInputIDs;
               std::vector<DWORD> m_dwOutputIDs;
               std::list<DWORD> mdwZOrders;
               RECT mOutputTargetRect;
               CComPtrCustom<IMFMediaType> mOutputMediaType;
               DXGI_FORMAT mVideoRenderTargetFormat;
               std::vector<D3D11_VIDEO_PROCESSOR_STREAM> mInputStreams;
               CComPtrCustom<IMFDXGIDeviceManager> mDeviceManager;
               CComPtrCustom<ID3D11VideoProcessorEnumerator> mVideoProcessorEnum;
               CComPtrCustom<ID3D11VideoProcessor> mVideoProcessor;
               CComPtrCustom<ID3D11Texture2D> mTempFlippedImage;
               CComPtrCustom<ID3D11VertexShader> mVertexShader;
               CComPtrCustom<ID3D11PixelShader> mPixelShader;
               CComPtrCustom<ID3D11InputLayout> mInputLayout;
               CComPtrCustom<ID3D11SamplerState> mSamplerState;

               virtual ~Direct3D11VideoProcessor();

               HRESULT getVideoProcessorService(IMFDXGIDeviceManager* pDeviceManager,
                                                ID3D11VideoDevice** aPtrPtrID3D11VideoDevice);

               HRESULT getVideoProcessorService(IMFDXGIDeviceManager* pDeviceManager,
                                                ID3D11Device** aPtrPtrID3D11VideoDevice);

               HRESULT blit(IMFSample* aPtrSample, REFERENCE_TIME aTargetFrame, REFERENCE_TIME aTargetEndFrame);

               HRESULT createVideoProcessor();

               HRESULT fillFilterNode(pugi::xml_node& aFilterNode, D3D11_VIDEO_PROCESSOR_FILTER aFilter,
                                      DWORD aInputStreamID);

               HRESULT fillColorFeatureNode(pugi::xml_node& aFilterNode, UINT32 aStartIndex, D3D11_VIDEO_COLOR aColor);

               HRESULT initFlipSupport(ID3D11Texture2D* aPtrSourceTexture);

               HRESULT flipTexture(ID3D11Texture2D* aPtrSourceTexture, ID3D11Texture2D* aPtrDestinationTexture);
            };
         }
      }
   }
}
