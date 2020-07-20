#pragma once

#include <VersionHelpers.h>
#include "../Common/ComPtrCustom.h"
#include "../Common/BaseMFAttributes.h"
#include "../Common/MFHeaders.h"
#include "../MediaFoundationManager/MediaFoundationManager.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "../Common/Common.h"
#include "../DirectXManager/DXGIManager.h"
#include "IEVRStreamControl.h"
#include "IMixerStreamPositionControl.h"
#include "IStreamFilterControl.h"
#include "IPresenter.h"

namespace EVRMultiSink
{
	namespace Sinks
	{
		namespace EVR
		{
			using namespace CaptureManager;

			using namespace CaptureManager::Core;

			class DECLSPEC_UUID("5A8E4C43-38F8-4193-B8B2-E7180D02E6B7")
				CLSID_SetDirectXDeviceProxy;

									
			template <typename MediaSinkFactory>
			class EVRActivate :
				public BaseMFAttributes<IMFActivate, IEVRStreamControl>
			{

				
			public:
			
				EVRActivate(IMFAttributes* aPtrIMFAttributes) :
					BaseMFAttributes<IMFActivate, IEVRStreamControl>(aPtrIMFAttributes)
				{}

				virtual ~EVRActivate() 
				{
					if (mUnkVideoRenderingClass)
					{
						mUnkVideoRenderingClass.Release();
					}

				}

				virtual ULONG STDMETHODCALLTYPE AddRef(void)
				{
					auto l = BaseMFAttributes::AddRef();

					return l;
				}

				virtual ULONG STDMETHODCALLTYPE Release(void)
				{
					auto l = BaseMFAttributes::Release();

					return l;
				}

				// IMFActivate implementation
				STDMETHODIMP ActivateObject(__RPC__in REFIID riid, __RPC__deref_out_opt void** ppvObject)
				{

					HRESULT lresult;

					do
					{
						if (!mUnkVideoRenderingClass)
						{

							HRESULT lhrCreateMediaSink(E_FAIL);

							CComPtrCustom<IUnknown> lUnkVideoRenderingClass;

							lhrCreateMediaSink = Singleton<MediaSinkFactory>::getInstance().createVideoRenderingClass(
								mPresenter,
								mMixer,
								mMixerStreamID,
								riid,
								&lUnkVideoRenderingClass
								);

							if (SUCCEEDED(lhrCreateMediaSink))
							{
								mUnkVideoRenderingClass = lUnkVideoRenderingClass;
							}
							else
							{
								LOG_CHECK_STATE_DESCR(FAILED(lhrCreateMediaSink), lhrCreateMediaSink);
							}
						}

						LOG_CHECK_PTR_MEMORY(mUnkVideoRenderingClass);

						LOG_INVOKE_WIDE_QUERY_INTERFACE_METHOD(mUnkVideoRenderingClass, riid, ppvObject);
						
					} while (false);

					return lresult;					
				}						

				STDMETHODIMP DetachObject()
				{
					return S_OK;
				}

				STDMETHODIMP ShutdownObject()
				{
					HRESULT lresult;
					
					if (mUnkVideoRenderingClass)
					{
						CComQIPtrCustom<IMFMediaSink> lMediaSink(mUnkVideoRenderingClass);

						if (lMediaSink)
						{
							do
							{
								LOG_INVOKE_MF_METHOD(Shutdown, lMediaSink);

							} while (true);
						}

						mUnkVideoRenderingClass.Release();
					}
					
					return S_OK;
				}		

				static HRESULT createInstance(
					IPresenter* aPtrPresenter,
					IMFTransform* aPtrMixer,
					DWORD aMixerStreamID,
					IMFActivate** aPtrPtrActivate)
				{
					using namespace Core;

					HRESULT lresult;

					do
					{

						LOG_CHECK_PTR_MEMORY(aPtrPtrActivate);

						LOG_CHECK_PTR_MEMORY(aPtrPresenter);

						LOG_CHECK_PTR_MEMORY(aPtrMixer);

						CComPtrCustom<IMFAttributes> lAttributes;

						LOG_INVOKE_MF_FUNCTION(MFCreateAttributes, &lAttributes, 0);

						LOG_CHECK_PTR_MEMORY(lAttributes);

						CComPtrCustom<EVRActivate> lActivate(new (std::nothrow) EVRActivate(lAttributes));

						LOG_CHECK_PTR_MEMORY(lActivate);

						lActivate->mMixer = aPtrMixer;

						lActivate->mPresenter = aPtrPresenter;

						lActivate->mMixerStreamID = aMixerStreamID;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(lActivate, aPtrPtrActivate);


					} while (false);
					
					return lresult;
				}






				// IEVRStreamControl methods


				HRESULT STDMETHODCALLTYPE setPosition(
					/* [in] */ FLOAT aLeft,
					/* [in] */ FLOAT aRight,
					/* [in] */ FLOAT aTop,
					/* [in] */ FLOAT aBottom) override
				{

					HRESULT lresult(E_NOTIMPL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, setPosition,
							mMixerStreamID,
							aLeft,
							aRight,
							aTop,
							aBottom);


						CComPtrCustom<IPresenter> lPresenter;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mPresenter, &lPresenter);

						if (lPresenter)
							lPresenter->ProcessFrame(FALSE);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE setSrcPosition(
					/* [in] */ FLOAT aLeft,
					/* [in] */ FLOAT aRight,
					/* [in] */ FLOAT aTop,
					/* [in] */ FLOAT aBottom) override
				{

					HRESULT lresult(E_NOTIMPL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, setSrcPosition,
							mMixerStreamID,
							aLeft,
							aRight,
							aTop,
							aBottom);

					} while (false);

					return lresult;
				}
				
				HRESULT STDMETHODCALLTYPE getSrcPosition(
					/* [out] */ FLOAT *aPtrLeft,
					/* [out] */ FLOAT *aPtrRight,
					/* [out] */ FLOAT *aPtrTop,
					/* [out] */ FLOAT *aPtrBottom) override
				{

					HRESULT lresult(E_NOTIMPL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, getSrcPosition,
							mMixerStreamID,
							aPtrLeft,
							aPtrRight,
							aPtrTop,
							aPtrBottom);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE setOpacity(
					/* [in] */ FLOAT aOpacity) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, setOpacity,
							mMixerStreamID,
							aOpacity);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE setZOrder(
					/* [out] */ DWORD aZOrder) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, setZOrder,
							mMixerStreamID,
							aZOrder);		
						
						CComPtrCustom<IPresenter> lPresenter;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mPresenter, &lPresenter);

						if (lPresenter)
							lPresenter->ProcessFrame(FALSE);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE getPosition(
					/* [out] */ FLOAT *aPtrLeft,
					/* [out] */ FLOAT *aPtrRight,
					/* [out] */ FLOAT *aPtrTop,
					/* [out] */ FLOAT *aPtrBottom) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, getPosition,
							mMixerStreamID,
							aPtrLeft,
							aPtrRight,
							aPtrTop,
							aPtrBottom);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE getOpacity(
					/* [out] */ FLOAT *aPtrOpacity) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, getOpacity,
							mMixerStreamID,
							aPtrOpacity);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE getZOrder(
					/* [out] */ DWORD *aPtrZOrder) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, getZOrder,
							mMixerStreamID,
							aPtrZOrder);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE flush() override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IMixerStreamPositionControl> lIMixerStreamPositionControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIMixerStreamPositionControl);

						LOG_CHECK_PTR_MEMORY(lIMixerStreamPositionControl);

						LOG_INVOKE_POINTER_METHOD(lIMixerStreamPositionControl, flush,
							mMixerStreamID);

						CComPtrCustom<IPresenter> lPresenter;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mPresenter, &lPresenter);

						if (lPresenter)
							lPresenter->ProcessFrame(FALSE);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE getCollectionOfFilters(
					/* [out] */ BSTR *aPtrPtrXMLstring) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IStreamFilterControl> lIStreamFilterControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIStreamFilterControl);

						LOG_CHECK_PTR_MEMORY(lIStreamFilterControl);

						LOG_INVOKE_POINTER_METHOD(lIStreamFilterControl, getCollectionOfFilters,
							mMixerStreamID,
							aPtrPtrXMLstring);


					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE setFilterParametr(
					/* [in] */ DWORD aParametrIndex,
					/* [in] */ LONG aNewValue,
					/* [in] */ BOOL aIsEnabled) override
				{
					HRESULT lresult(E_FAIL);

					do
					{

						CComPtrCustom<IStreamFilterControl> lIStreamFilterControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIStreamFilterControl);

						LOG_CHECK_PTR_MEMORY(lIStreamFilterControl);

						LOG_INVOKE_POINTER_METHOD(lIStreamFilterControl, setFilterParametr,
							mMixerStreamID,
							aParametrIndex,
							aNewValue,
							aIsEnabled);

						CComPtrCustom<IPresenter> lPresenter;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mPresenter, &lPresenter);

						if (lPresenter)
							lPresenter->ProcessFrame(FALSE);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE getCollectionOfOutputFeatures(
					/* [out] */ BSTR *aPtrPtrXMLstring) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IStreamFilterControl> lIStreamFilterControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIStreamFilterControl);

						LOG_CHECK_PTR_MEMORY(lIStreamFilterControl);

						LOG_INVOKE_POINTER_METHOD(lIStreamFilterControl, getCollectionOfOutputFeatures,
							aPtrPtrXMLstring);

					} while (false);

					return lresult;
				}

				HRESULT STDMETHODCALLTYPE setOutputFeatureParametr(
					/* [in] */ DWORD aParametrIndex,
					/* [in] */ LONG aNewValue) override
				{
					HRESULT lresult(E_FAIL);

					do
					{
						CComPtrCustom<IStreamFilterControl> lIStreamFilterControl;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mMixer, &lIStreamFilterControl);

						LOG_CHECK_PTR_MEMORY(lIStreamFilterControl);

						LOG_INVOKE_POINTER_METHOD(lIStreamFilterControl, setOutputFeatureParametr,
							aParametrIndex,
							aNewValue);

						CComPtrCustom<IPresenter> lPresenter;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mPresenter, &lPresenter);

						if (lPresenter)
							lPresenter->ProcessFrame(FALSE);

					} while (false);

					return lresult;
				}
				
				
			private:

				CComPtrCustom<IPresenter> mPresenter;

				CComPtrCustom<IMFTransform> mMixer;

				//CComPtrCustom<IMFMediaSink> mMediaSink;

				//CComPtrCustom<IBaseFilter> mBaseFilter;

				CComPtrCustom<IUnknown> mUnkVideoRenderingClass;

				DWORD mMixerStreamID;
			};
		}
	}
}