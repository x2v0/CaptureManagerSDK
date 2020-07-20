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

#include "Direct3D11and3D9Presenter.h"
#include "../DirectXManager/Direct3D11Manager.h"
#include "../DirectXManager/Direct3D9Manager.h"
#include "../DirectXManager/Direct3D9ExManager.h"
#include "../MediaFoundationManager/MediaFoundationManager.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "../DirectXManager/DXGIManager.h"
#include "../Common/Common.h"
#include "../Common/Singleton.h"
#include "../Scheduler/SchedulerFactory.h"
#include "../Common/GUIDs.h"
#include <Windows.ui.xaml.media.dxinterop.h>


//#pragma comment(lib, "D2d1.lib")
//#pragma comment(lib, "Dwrite.lib")
//#pragma comment(lib, "Ddraw.lib")


namespace CaptureManager
{
	namespace Sinks
	{
		namespace EVR
		{
			namespace Direct3D11
			{
				using namespace Core;

				using namespace Core::Direct3D11;

				using namespace Core::Direct3D9;
				
				static const DWORD tri_fvf = D3DFVF_XYZRHW | D3DFVF_TEX1;

				struct textured_vertex{
					float x, y, z, rhw;  // The transformed(screen space) position for the vertex.
					float tu, tv;         // Texture coordinates
				};


				static int g_list_count;

				HRESULT Direct3D11and3D9Presenter::init_vb(void){
					textured_vertex data[] = {

						{ 0, mImageHeight, 1, 1, 0, 1 }, { 0, 0, 1, 1, 0, 0 }, { mImageWidth, 0, 1, 1, 1, 0 },
						{ 0, mImageHeight, 1, 1, 0, 1 }, { mImageWidth, 0, 1, 1, 1, 0 }, { mImageWidth, mImageHeight, 1, 1, 1, 1 }

					};
					int vert_count = sizeof(data) / sizeof(textured_vertex);
					int byte_count = vert_count*sizeof(textured_vertex);
					void *vb_vertices;
					HRESULT hr;

					g_list_count = vert_count / 3;

					g_list_vb.Release();

					hr = mDevice9->CreateVertexBuffer(byte_count,        //Length
						0,//Usage
						tri_fvf,           //FVF
						D3DPOOL_DEFAULT,   //Pool
						&g_list_vb,        //ppVertexBuffer
						NULL);             //Handle
					if (FAILED(hr)){

						return hr;
					}

					hr = g_list_vb->Lock(0, //Offset
						0, //SizeToLock
						&vb_vertices, //Vertices
						0);  //Flags
					if (FAILED(hr)){

						return hr;
					}

					memcpy(vb_vertices, data, byte_count);

					g_list_vb->Unlock();


					return D3D_OK;
				}


				void Direct3D11and3D9Presenter::set_device_states(void){


					mDevice9->SetTextureStageState(0, D3DTSS_COLOROP, D3DTOP_SELECTARG1);
					mDevice9->SetTextureStageState(0, D3DTSS_COLORARG1, D3DTA_TEXTURE);
					mDevice9->SetTextureStageState(0, D3DTSS_COLORARG2, D3DTA_DIFFUSE);   //Ignored

					mDevice9->SetTexture(0, mTexture9);
				}


				// Driver types supported
				static D3D_DRIVER_TYPE gDriverTypes[] =
				{
					D3D_DRIVER_TYPE_HARDWARE
				};
				static UINT gNumDriverTypes = ARRAYSIZE(gDriverTypes);

				// Feature levels supported
				static D3D_FEATURE_LEVEL gFeatureLevels[] =
				{
					D3D_FEATURE_LEVEL_11_1,
					D3D_FEATURE_LEVEL_11_0
				};

				static UINT gNumFeatureLevels = ARRAYSIZE(gFeatureLevels);


				UINT Direct3D11and3D9Presenter::mUseDebugLayer(D3D11_CREATE_DEVICE_VIDEO_SUPPORT | D3D11_CREATE_DEVICE_BGRA_SUPPORT);

				Direct3D11and3D9Presenter::Direct3D11and3D9Presenter()
				{
				}
				
				Direct3D11and3D9Presenter::~Direct3D11and3D9Presenter()
				{
					releaseResources();
				}


				// IPresenterInit methods

				HRESULT Direct3D11and3D9Presenter::initializeSharedTarget(
					HANDLE aHandle,
					IUnknown* aPtrTarget)
				{
					HRESULT lresult(E_NOTIMPL);

					do
					{


						if (!mD3D9)
						{
							CComPtrCustom<IDirect3D9Ex> lD3D9;

							LOG_INVOKE_DX9EX_FUNCTION(Direct3DCreate9Ex, D3D_SDK_VERSION, &lD3D9);

							LOG_CHECK_PTR_MEMORY(lD3D9);

							LOG_INVOKE_QUERY_INTERFACE_METHOD(lD3D9, &mD3D9);

							LOG_CHECK_PTR_MEMORY(mD3D9);
						}



						LOG_CHECK_PTR_MEMORY(mDeviceManager);

						LOG_CHECK_PTR_MEMORY(mD3D11Device);

						if (aHandle != nullptr && aPtrTarget == nullptr)
						{							
							HANDLE sharedHandle = aHandle;
							
							CComPtrCustom<ID3D11Resource> lTempResource11;

							LOG_INVOKE_POINTER_METHOD(mD3D11Device, OpenSharedResource,
								sharedHandle, IID_PPV_ARGS(&lTempResource11));

							LOG_CHECK_PTR_MEMORY(lTempResource11);

							LOG_INVOKE_QUERY_INTERFACE_METHOD(lTempResource11, &mID3D11Texture2D);

							LOG_INVOKE_FUNCTION(createSample, Direct3D11and3D9Presenter::RenderTexture);

							mImmediateContext.Release();

							mD3D11Device->GetImmediateContext(&mImmediateContext);

							LOG_CHECK_PTR_MEMORY(mImmediateContext);

							CComPtrCustom<ID3D10Multithread> lMultiThread;

							// Need to explitly set the multithreaded mode for this device
							LOG_INVOKE_QUERY_INTERFACE_METHOD(mImmediateContext, &lMultiThread);

							LOG_CHECK_PTR_MEMORY(lMultiThread);

							BOOL lbRrsult = lMultiThread->SetMultithreadProtected(TRUE);
						}

						CComQIPtrCustom<ISwapChainPanelNative> lSwapChainPanelNative = aPtrTarget;

						if (lSwapChainPanelNative)
						{

							CComQIPtrCustom<IDXGIDevice> ldxgiDevice = mD3D11Device;

							LOG_CHECK_PTR_MEMORY(ldxgiDevice);

							CComQIPtrCustom<IDXGIAdapter> ldxgiAdapter;
							
							LOG_INVOKE_POINTER_METHOD(ldxgiDevice, GetAdapter,
								&ldxgiAdapter);

							CComQIPtrCustom<IDXGIFactory2> ldxgiFactory;

							LOG_INVOKE_POINTER_METHOD(ldxgiAdapter, GetParent,
								IID_PPV_ARGS(&ldxgiFactory));

							// Get the DXGISwapChain1
							DXGI_SWAP_CHAIN_DESC1 scd;
							ZeroMemory(&scd, sizeof(scd));
							scd.SampleDesc.Count = 1;
							scd.SampleDesc.Quality = 0;
							scd.SwapEffect = DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL;
							scd.Scaling = DXGI_SCALING_STRETCH;
							scd.Width = mImageWidth;
							scd.Height = mImageHeight;
							scd.Format = DXGI_FORMAT_B8G8R8A8_UNORM;
							scd.Stereo = FALSE;
							scd.BufferUsage = DXGI_USAGE_BACK_BUFFER | DXGI_USAGE_RENDER_TARGET_OUTPUT;
							scd.Flags = 0;// m_bStereoEnabled ? DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH : 0; //opt in to do direct flip;
							scd.BufferCount = 1;
							//scd.AlphaMode = DXGI_ALPHA_MODE::DXGI_ALPHA_MODE_STRAIGHT;

							mSwapChain1.Release();

							// create swap chain by calling CreateSwapChainForComposition
							ldxgiFactory->CreateSwapChainForComposition(
								mD3D11Device,
								&scd,
								nullptr,		// allow on any display 
								&mSwapChain1
								);

							LOG_CHECK_PTR_MEMORY(mSwapChain1);

							LOG_INVOKE_POINTER_METHOD(lSwapChainPanelNative, SetSwapChain,
								mSwapChain1);
														
							LOG_INVOKE_FUNCTION(createSample, SwapChain);

							break;
						}

						CComQIPtrCustom<IDXGISwapChain1> lIDXGISwapChain1 = aPtrTarget;

						if (lIDXGISwapChain1)
						{
							mSwapChain1.Release();

							mSwapChain1 = lIDXGISwapChain1;

							LOG_CHECK_PTR_MEMORY(mSwapChain1);

							mD3D11Device.Release();

							LOG_INVOKE_POINTER_METHOD(mSwapChain1, GetDevice, IID_PPV_ARGS(&mD3D11Device));

							CComPtrCustom<ID3D11VideoDevice> lDX11VideoDevice;

							LOG_INVOKE_QUERY_INTERFACE_METHOD(mD3D11Device, &lDX11VideoDevice);

							LOG_INVOKE_FUNCTION(createSample, SwapChain);

							LOG_CHECK_PTR_MEMORY(lDX11VideoDevice);

							LOG_CHECK_PTR_MEMORY(mDeviceManager);

							LOG_INVOKE_POINTER_METHOD(mDeviceManager, ResetDevice,
								mD3D11Device,
								mDeviceResetToken);

							mImmediateContext.Release();

							mD3D11Device->GetImmediateContext(&mImmediateContext);

							LOG_CHECK_PTR_MEMORY(mImmediateContext);

							do
							{
								CComPtrCustom<ID3D10Multithread> lMultiThread;

								// Need to explitly set the multithreaded mode for this device
								LOG_INVOKE_QUERY_INTERFACE_METHOD(mImmediateContext, &lMultiThread);

								LOG_CHECK_PTR_MEMORY(lMultiThread);

								BOOL lbRrsult = lMultiThread->SetMultithreadProtected(TRUE);

							} while (false);

							lresult = S_OK;

							break;
						}

						CComQIPtrCustom<ID3D11Texture2D> lID3D11Texture2D = aPtrTarget;
						
						if (lID3D11Texture2D)
						{
							mSwapChain1.Release();

							mID3D11Texture2D.Release();
							
							D3D11_TEXTURE2D_DESC lDestDesc;

							lID3D11Texture2D->GetDesc(&lDestDesc);

							CComPtrCustom<ID3D11Device> lD3D11Device;

							lID3D11Texture2D->GetDevice(&lD3D11Device);

							LOG_CHECK_PTR_MEMORY(lD3D11Device);

							auto lCreationFlags = lD3D11Device->GetCreationFlags();

							mSharedTexture = false;

							if ((lCreationFlags & D3D11_CREATE_DEVICE_SINGLETHREADED) != 0 ||
								(lCreationFlags & D3D11_CREATE_DEVICE_VIDEO_SUPPORT) == 0
								)
							{
								mSharedTexture = true;
							}

							if (mSharedTexture)
							{
								if (lDestDesc.Format == DXGI_FORMAT_R8G8B8A8_TYPELESS)
								{
									lDestDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
								}

								lDestDesc.BindFlags = D3D11_BIND_RENDER_TARGET;

								lDestDesc.MiscFlags = D3D11_RESOURCE_MISC_SHARED;

								LOG_INVOKE_POINTER_METHOD(lD3D11Device,CreateTexture2D,
									&lDestDesc, NULL, &mSharedID3D11Texture2D);

								LOG_CHECK_PTR_MEMORY(mSharedID3D11Texture2D);

								CComPtrCustom<IDXGIResource> lResource;

								LOG_INVOKE_QUERY_INTERFACE_METHOD(mSharedID3D11Texture2D, &lResource);

								HANDLE sharedHandle;

								LOG_INVOKE_POINTER_METHOD(lResource, GetSharedHandle, &sharedHandle);

								LOG_CHECK_PTR_MEMORY(sharedHandle);

								CComPtrCustom<ID3D11Resource> lTempResource11;

								LOG_INVOKE_POINTER_METHOD(mD3D11Device, OpenSharedResource,
									sharedHandle, IID_PPV_ARGS(&lTempResource11));

								LOG_CHECK_PTR_MEMORY(lTempResource11);

								LOG_INVOKE_QUERY_INTERFACE_METHOD(lTempResource11, &mID3D11Texture2D);

								LOG_INVOKE_FUNCTION(createSample, Direct3D11and3D9Presenter::RenderTexture);

								mImmediateContext.Release();

								mD3D11Device->GetImmediateContext(&mImmediateContext);

								LOG_CHECK_PTR_MEMORY(mImmediateContext);

								CComPtrCustom<ID3D10Multithread> lMultiThread;

								// Need to explitly set the multithreaded mode for this device
								LOG_INVOKE_QUERY_INTERFACE_METHOD(mImmediateContext, &lMultiThread);

								LOG_CHECK_PTR_MEMORY(lMultiThread);

								BOOL lbRrsult = lMultiThread->SetMultithreadProtected(TRUE);
							}
							else
							{
								mID3D11Texture2D = lID3D11Texture2D;
								
								LOG_CHECK_PTR_MEMORY(mID3D11Texture2D);

								mD3D11Device.Release();

								mID3D11Texture2D->GetDevice(&mD3D11Device);

								LOG_CHECK_PTR_MEMORY(mD3D11Device);

								CComPtrCustom<ID3D11VideoDevice> lDX11VideoDevice;

								LOG_INVOKE_QUERY_INTERFACE_METHOD(mD3D11Device, &lDX11VideoDevice);

								LOG_INVOKE_FUNCTION(createSample, Direct3D11and3D9Presenter::RenderTexture);

								LOG_CHECK_PTR_MEMORY(lDX11VideoDevice);

								LOG_CHECK_PTR_MEMORY(mDeviceManager);

								LOG_INVOKE_POINTER_METHOD(mDeviceManager, ResetDevice,
									mD3D11Device,
									mDeviceResetToken);

								mImmediateContext.Release();

								mD3D11Device->GetImmediateContext(&mImmediateContext);

								LOG_CHECK_PTR_MEMORY(mImmediateContext);

								do
								{
									CComPtrCustom<ID3D10Multithread> lMultiThread;

									// Need to explitly set the multithreaded mode for this device
									LOG_INVOKE_QUERY_INTERFACE_METHOD(mImmediateContext, &lMultiThread);

									LOG_CHECK_PTR_MEMORY(lMultiThread);

									BOOL lbRrsult = lMultiThread->SetMultithreadProtected(TRUE);

								} while (false);

								lresult = S_OK;
							}

							break;
						}

					} while (false);

					return lresult;
				}

				HRESULT Direct3D11and3D9Presenter::createDevice()
				{
					HRESULT lresult(E_NOTIMPL);

					do
					{
						std::lock_guard<std::mutex> lLock(mAccessMutex);

						LOG_CHECK_STATE(!IsWindow(mHWNDVideo));

						LOG_INVOKE_FUNCTION(createManagerAndDevice);

					} while (false);

					return lresult;
				}
				
				HRESULT Direct3D11and3D9Presenter::setVideoWindowHandle(
					HWND aVideoWindowHandle)
				{
					HRESULT lresult(E_NOTIMPL);

					do
					{
						std::lock_guard<std::mutex> lLock(mAccessMutex);
						
						LOG_CHECK_STATE(!IsWindow(aVideoWindowHandle));

						if (mHWNDVideo != aVideoWindowHandle)
						{
							mHWNDVideo = aVideoWindowHandle;

							ShutdownHook();

							InitializeHook(mHWNDVideo);

							LOG_INVOKE_FUNCTION(createManagerAndDevice);
						}

					} while (false);

					return lresult;
				}



				// IMFGetService methods
				STDMETHODIMP Direct3D11and3D9Presenter::GetService(
					REFGUID aRefGUIDService,
					REFIID aRefIID,
					LPVOID* aPtrPtrObject)
				{
					HRESULT lresult(MF_E_UNSUPPORTED_SERVICE);

					do
					{
						LOG_CHECK_PTR_MEMORY(aPtrPtrObject);

						if (aRefGUIDService == MR_VIDEO_ACCELERATION_SERVICE)
						{
							if (aRefIID == __uuidof(IMFDXGIDeviceManager))
							{
								if (mDeviceManager)
								{
									LOG_INVOKE_WIDE_QUERY_INTERFACE_METHOD(mDeviceManager,
										__uuidof(IUnknown),
										aPtrPtrObject);
								}
								else
								{
									lresult = E_NOINTERFACE;
								}
							}
							else if (aRefIID == __uuidof(IMFVideoSampleAllocatorEx))
							{
								do
								{

									CComPtrCustom<IMFVideoSampleAllocatorEx> lVideoSampleAllocatorEx;

									LOG_INVOKE_MF_FUNCTION(MFCreateVideoSampleAllocatorEx,
										IID_PPV_ARGS(&lVideoSampleAllocatorEx));

									if (lVideoSampleAllocatorEx)
									{
										if (mDeviceManager)
											lVideoSampleAllocatorEx->SetDirectXManager(mDeviceManager);

										LOG_INVOKE_WIDE_QUERY_INTERFACE_METHOD(lVideoSampleAllocatorEx, aRefIID, aPtrPtrObject);
									}

								} while (false);
							}
						}

					} while (false);

					return lresult;
				}

				HRESULT Direct3D11and3D9Presenter::createDevice(ID3D11Device** aPtrPtrDevice)
				{
					HRESULT lresult(E_FAIL);

					do
					{
						LOG_CHECK_PTR_MEMORY(aPtrPtrDevice);

						LOG_INVOKE_FUNCTION(Singleton<Direct3D11Manager>::getInstance().getState);
						
						D3D_FEATURE_LEVEL lfeatureLevel;

						LOG_INVOKE_DX11_FUNCTION(D3D11CreateDevice,
							NULL,
							D3D_DRIVER_TYPE_HARDWARE,
							NULL,
							mUseDebugLayer,
							gFeatureLevels,
							gNumFeatureLevels,
							D3D11_SDK_VERSION,
							aPtrPtrDevice,
							&lfeatureLevel,
							NULL);

						LOG_CHECK_PTR_MEMORY(aPtrPtrDevice);
						
					} while (false);

					return lresult;
				}

				HRESULT Direct3D11and3D9Presenter::createSample(TargetType aTargetType)
				{
					HRESULT lresult(E_NOTIMPL);

					do
					{

						mSample.Release();

						LOG_INVOKE_MF_FUNCTION(MFCreateSample,
							&mSample);

						LOG_CHECK_PTR_MEMORY(mSample);

						D3D11_TEXTURE2D_DESC lSurfaceDesc;

						ZeroMemory(&lSurfaceDesc, sizeof(lSurfaceDesc));

						switch (aTargetType)
						{
						case Handler:
						{

							CComPtrCustom<ID3D11Texture2D> lSurface;

							// Get Backbuffer
							LOG_INVOKE_POINTER_METHOD(mSwapChain1, GetBuffer,
								0, IID_PPV_ARGS(&lSurface));

							LOG_CHECK_PTR_MEMORY(lSurface);

							lSurface->GetDesc(
								&lSurfaceDesc);


							CComPtrCustom<IMFMediaBuffer> lBuffer;

							LOG_INVOKE_MF_FUNCTION(MFCreateDXGISurfaceBuffer,
								__uuidof(ID3D11Texture2D),
								lSurface,
								0,
								FALSE,
								&lBuffer);

							LOG_CHECK_PTR_MEMORY(lBuffer);

							LOG_INVOKE_MF_METHOD(AddBuffer, mSample,
								lBuffer);

						}
							break;
						case SwapChain:
						{

							LOG_CHECK_PTR_MEMORY(mSwapChain1);

							CComPtrCustom<ID3D11Texture2D> lSurface;

							// Get Backbuffer
							LOG_INVOKE_POINTER_METHOD(mSwapChain1, GetBuffer,
								0, IID_PPV_ARGS(&lSurface));

							LOG_CHECK_PTR_MEMORY(lSurface);

							lSurface->GetDesc(
								&lSurfaceDesc);

							LOG_INVOKE_MF_METHOD(SetUnknown, mSample,
								CM_SwapChain,
								mSwapChain1);
						}
							break;
						case RenderTexture:
						{
							LOG_CHECK_PTR_MEMORY(mID3D11Texture2D);

							mID3D11Texture2D->GetDesc(
								&lSurfaceDesc);

							LOG_INVOKE_MF_METHOD(SetUnknown, mSample,
								CM_RenderTexture,
								mID3D11Texture2D);
						}
							break;
						default:
							break;
						}


						CComPtrCustom<IMFMediaType> lCurrentMediaType;

						LOG_INVOKE_FUNCTION(createUncompressedVideoType,
							lSurfaceDesc.Format,
							lSurfaceDesc.Width,
							lSurfaceDesc.Height,
							MFVideoInterlaceMode::MFVideoInterlace_Progressive,
							mFrameRate,
							mPixelRate,
							&lCurrentMediaType);

						mCurrentMediaType = lCurrentMediaType;

						if (mMixer)
						{
							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								MFT_SET_TYPE_TEST_ONLY);

							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								0);
						}

					} while (false);

					return lresult;

				}

				HRESULT Direct3D11and3D9Presenter::getMaxInputStreamCount(DWORD* aPtrMaxInputStreamCount)
				{
					HRESULT lresult(E_FAIL);

					do
					{
						LOG_CHECK_PTR_MEMORY(aPtrMaxInputStreamCount);

						LOG_INVOKE_FUNCTION(Singleton<Direct3D11Manager>::getInstance().getState);

						CComPtrCustom<ID3D11Device> lD3D11Device;
						
						LOG_INVOKE_FUNCTION(createDevice, &lD3D11Device);
						
						LOG_CHECK_PTR_MEMORY(lD3D11Device);

						CComPtrCustom<ID3D11VideoDevice> lVideoDevice;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(lD3D11Device, &lVideoDevice);

						LOG_CHECK_PTR_MEMORY(lVideoDevice);

						CComPtrCustom<ID3D11VideoProcessorEnumerator> lVideoProcessorEnum;

						D3D11_VIDEO_PROCESSOR_CONTENT_DESC ContentDesc;
						ZeroMemory(&ContentDesc, sizeof(ContentDesc));
						ContentDesc.InputFrameFormat = D3D11_VIDEO_FRAME_FORMAT_PROGRESSIVE;// D3D11_VIDEO_FRAME_FORMAT_INTERLACED_TOP_FIELD_FIRST;
						ContentDesc.InputWidth = 1920;
						ContentDesc.InputHeight = 1080;
						ContentDesc.OutputWidth = 1920;
						ContentDesc.OutputHeight = 1080;
						ContentDesc.Usage = D3D11_VIDEO_USAGE_PLAYBACK_NORMAL;

						LOG_INVOKE_POINTER_METHOD(lVideoDevice, CreateVideoProcessorEnumerator,
							&ContentDesc,
							&lVideoProcessorEnum);

						LOG_CHECK_PTR_MEMORY(lVideoProcessorEnum);

						D3D11_VIDEO_PROCESSOR_CAPS lCAPS;

						LOG_INVOKE_POINTER_METHOD(lVideoProcessorEnum, GetVideoProcessorCaps,
							&lCAPS);

						*aPtrMaxInputStreamCount = lCAPS.MaxInputStreams;

					} while (false);

					return lresult;
				}

				// IPresenter methods
				HRESULT Direct3D11and3D9Presenter::initialize(
					UINT32 aImageWidth,
					UINT32 aImageHeight,
					DWORD aNumerator,
					DWORD aDenominator,
					IMFTransform* aPtrMixer)
				{
					HRESULT lresult(E_NOTIMPL);

					do
					{
						LOG_CHECK_PTR_MEMORY(aPtrMixer);

						LOG_INVOKE_FUNCTION(Singleton<Direct3D11Manager>::getInstance().getState);

						LOG_INVOKE_FUNCTION(Singleton<Direct3D9ExManager>::getInstance().getState);

						LOG_INVOKE_FUNCTION(Singleton<Direct3D9Manager>::getInstance().getState);





						CComPtrCustom<IDirect3D9Ex> lD3D9;

						LOG_INVOKE_DX9EX_FUNCTION(Direct3DCreate9Ex, D3D_SDK_VERSION, &lD3D9);

						LOG_CHECK_PTR_MEMORY(lD3D9);

						if (mD3D9 == nullptr)
							LOG_INVOKE_QUERY_INTERFACE_METHOD(lD3D9, &mD3D9);

						LOG_CHECK_PTR_MEMORY(mD3D9);

						
						if (!mD3D11Device)
						LOG_INVOKE_FUNCTION(createDevice, &mD3D11Device);

						LOG_CHECK_PTR_MEMORY(mD3D11Device);

						CComPtrCustom<ID3D11VideoDevice> lDX11VideoDevice;

						LOG_INVOKE_QUERY_INTERFACE_METHOD(mD3D11Device, &lDX11VideoDevice);
						
						LOG_CHECK_PTR_MEMORY(lDX11VideoDevice);

												
						if (mDeviceManager == nullptr)
							LOG_INVOKE_MF_FUNCTION(MFCreateDXGIDeviceManager,
								&mDeviceResetToken,
								&mDeviceManager);
						
						LOG_CHECK_PTR_MEMORY(mDeviceManager);
						
						LOG_INVOKE_POINTER_METHOD(mDeviceManager, ResetDevice,
							mD3D11Device,
							mDeviceResetToken);

						MFRatio lFrameRate;

						lFrameRate.Numerator = aNumerator;

						lFrameRate.Denominator = aDenominator;

						LOG_INVOKE_FUNCTION(init,
							aImageWidth,
							aImageHeight,
							lFrameRate);

						mMixer = aPtrMixer;

						LOG_CHECK_PTR_MEMORY(mMixer);
												
						CComPtrCustom<IUnknown> lDeviceManager;
						
						LOG_INVOKE_POINTER_METHOD(this, GetService,
							MR_VIDEO_ACCELERATION_SERVICE,
							__uuidof(IMFDXGIDeviceManager),
							(void**)&lDeviceManager);

						LOG_CHECK_PTR_MEMORY(lDeviceManager);

						LOG_INVOKE_MF_METHOD(ProcessMessage, mMixer, MFT_MESSAGE_SET_D3D_MANAGER, 0);

						LOG_INVOKE_MF_METHOD(ProcessMessage, mMixer, MFT_MESSAGE_SET_D3D_MANAGER, (ULONG_PTR)lDeviceManager.get());
						
						if (mCurrentMediaType)
						{
							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								MFT_SET_TYPE_TEST_ONLY);

							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								0);
						}

						if (mSample)
							resize();

					} while (false);

					return lresult;
				}					
				
				HRESULT Direct3D11and3D9Presenter::processFrameInner()
				{
					HRESULT lresult(E_FAIL);

					do
					{
						std::lock_guard<std::mutex> lLock(mAccessMutex);

						if (mHWNDVideo != nullptr)
						{
							RECT lWindowRect;

							GetClientRect(mHWNDVideo, &lWindowRect);

							if (mPrevWindowRect != lWindowRect)
							{
								RECT mInvalidFirstRect;

								RECT mInvalidSecondRect;

								auto lNativeClientWidth = lWindowRect.right - lWindowRect.left;

								auto lNativeClientHeight = lWindowRect.bottom - lWindowRect.top;

								auto lnativeProportion = (float)lNativeClientHeight / (float)lNativeClientWidth;

								auto limageProportion = (float)mImageHeight / (float)mImageWidth;

								if (lnativeProportion >= limageProportion)
								{
									LONG lidealHeight = lNativeClientWidth * mImageHeight / mImageWidth;

									LONG lborder = (lNativeClientHeight - lidealHeight) >> 1;

									mDestRect.left = 0;

									mDestRect.right = lNativeClientWidth;

									mDestRect.top = lborder;

									mDestRect.bottom = lborder + lidealHeight;



									mInvalidFirstRect.left = 0;

									mInvalidFirstRect.right = lNativeClientWidth;

									mInvalidFirstRect.top = 0;

									mInvalidFirstRect.bottom = mDestRect.bottom;




									mInvalidSecondRect.left = 0;

									mInvalidSecondRect.right = lNativeClientWidth;

									mInvalidSecondRect.top = mDestRect.top;

									mInvalidSecondRect.bottom = lNativeClientHeight;
								}
								else
								{
									LONG lidealWidth = lNativeClientHeight * mImageWidth / mImageHeight;

									LONG lborder = (lNativeClientWidth - lidealWidth) >> 1;

									mDestRect.left = lborder;

									mDestRect.right = lborder + lidealWidth;

									mDestRect.top = 0;

									mDestRect.bottom = lNativeClientHeight;




									mInvalidFirstRect.left = 0;

									mInvalidFirstRect.right = mDestRect.left;

									mInvalidFirstRect.top = 0;

									mInvalidFirstRect.bottom = lNativeClientHeight;





									mInvalidSecondRect.left = mDestRect.right;

									mInvalidSecondRect.right = lNativeClientWidth;

									mInvalidSecondRect.top = 0;

									mInvalidSecondRect.bottom = lNativeClientHeight;
								}


								mPrevWindowRect = lWindowRect;




								InvalidateRect(mHWNDVideo, &mInvalidFirstRect, TRUE);

								InvalidateRect(mHWNDVideo, &mInvalidSecondRect, TRUE);
							}							
							
							MFT_OUTPUT_DATA_BUFFER lBuffer;

							ZeroMemory(&lBuffer, sizeof(lBuffer));

							lBuffer.dwStreamID = 0;

							lBuffer.pSample = mSample;

							DWORD lState(0);

							LOG_INVOKE_MF_METHOD(ProcessOutput, mMixer,
								0,
								1,
								&lBuffer,
								&lState);

							LOG_CHECK_PTR_MEMORY(mDevice9);



							//Notify the device that we're ready to render
							auto hr = mDevice9->BeginScene();
							if (FAILED(hr)){
								return hr;
							}

							//Render from our Vertex Buffer
							mDevice9->DrawPrimitive(D3DPT_TRIANGLELIST, //PrimitiveType
								0,                  //StartVertex
								g_list_count);      //PrimitiveCount
							
							//Notify the device that we're finished rendering for this frame
							mDevice9->EndScene();
							
							mDevice9->WaitForVBlank(0);

							mDevice9->WaitForVBlank(0);
													   
							lresult = mDevice9->Present(nullptr, &mDestRect, mHWNDVideo, nullptr);
						}
						else
						{
							MFT_OUTPUT_DATA_BUFFER lBuffer;

							ZeroMemory(&lBuffer, sizeof(lBuffer));

							lBuffer.dwStreamID = 0;

							lBuffer.pSample = mSample;

							DWORD lState(0);

							LOG_INVOKE_MF_METHOD(ProcessOutput, mMixer,
								0,
								1,
								&lBuffer,
								&lState);

							LOG_CHECK_PTR_MEMORY(mSwapChain1);

							lresult = mSwapChain1->Present(0, 0);
						}

					} while (false);

					return lresult;
				}
								
				void Direct3D11and3D9Presenter::releaseResources()
				{

					mImmediateContext.Release();

					mDXGIFactory2.Release();

					mDXGIOutput1.Release();

					mSwapChain1.Release();
					
					mSample.Release();
				}	

				HRESULT Direct3D11and3D9Presenter::createManagerAndDevice()
				{
					HRESULT lresult(E_FAIL);
										
					do
					{

						UINT lAdapterID = D3DADAPTER_DEFAULT;

						D3DDISPLAYMODE dm;

						D3DPRESENT_PARAMETERS pp;

						LOG_CHECK_PTR_MEMORY(mD3D9);

						LOG_CHECK_PTR_MEMORY(mDeviceManager);

						LOG_CHECK_PTR_MEMORY(mD3D11Device);

						LOG_CHECK_STATE_DESCR(mHWNDVideo == nullptr, E_UNEXPECTED);

						releaseResources();

						mImmediateContext.Release();

						mD3D11Device->GetImmediateContext(&mImmediateContext);

						LOG_CHECK_PTR_MEMORY(mImmediateContext);

						CComPtrCustom<ID3D10Multithread> lMultiThread;

						// Need to explitly set the multithreaded mode for this device
						LOG_INVOKE_QUERY_INTERFACE_METHOD(mImmediateContext, &lMultiThread);

						LOG_CHECK_PTR_MEMORY(lMultiThread);
												
						BOOL lbRrsult = lMultiThread->SetMultithreadProtected(TRUE);
						
						LOG_CHECK_STATE_DESCR(mHWNDVideo == nullptr, E_UNEXPECTED);
						
						LOG_INVOKE_DX9_METHOD(GetAdapterDisplayMode,
							mD3D9,
							lAdapterID,
							&dm);

						ZeroMemory(&pp, sizeof(pp));

						pp.Windowed = TRUE;
						pp.hDeviceWindow = mHWNDVideo;
						pp.SwapEffect = D3DSWAPEFFECT_COPY;
						pp.BackBufferFormat = dm.Format;
						pp.BackBufferWidth = mImageWidth;
						pp.BackBufferHeight = mImageHeight;
						pp.Flags = 0;
						pp.PresentationInterval = D3DPRESENT_INTERVAL_ONE;
						pp.BackBufferCount = 1;

						mDevice9.Release();


						LOG_INVOKE_DX9EX_METHOD(CreateDeviceEx,
							mD3D9,
							lAdapterID,
							D3DDEVTYPE_HAL,
							mHWNDVideo,
							D3DCREATE_HARDWARE_VERTEXPROCESSING |
							D3DCREATE_MULTITHREADED |
							D3DCREATE_FPU_PRESERVE,
							&pp,
							NULL,
							&mDevice9);


						LOG_CHECK_PTR_MEMORY(mDevice9);

						HANDLE* lSharedHandle = NULL;

						HANDLE* lPtrSharedHandle = (HANDLE*)&lSharedHandle;

						mTexture9.Release();

						mDevice9->CreateTexture(
							mImageWidth,
							mImageHeight,
							1,
							1,  //D3DUSAGE_RENDERTARGET
							D3DFMT_X8R8G8B8,
							D3DPOOL_DEFAULT,  //D3DPOOL_DEFAULT
							&mTexture9,
							lPtrSharedHandle);



						// Create shared texture

						CComPtrCustom<ID3D11Resource> l_Resource;

						LOG_INVOKE_POINTER_METHOD(mD3D11Device, OpenSharedResource, lSharedHandle, IID_PPV_ARGS(&l_Resource));
							


						CComPtrCustom<ID3D11Texture2D> lSurface;

						LOG_INVOKE_POINTER_METHOD(l_Resource, QueryInterface, IID_PPV_ARGS(&lSurface));

						LOG_CHECK_PTR_MEMORY(lSurface);

						D3D11_TEXTURE2D_DESC lSurfaceDesc;

						ZeroMemory(&lSurfaceDesc, sizeof(lSurfaceDesc));

						lSurface->GetDesc(
							&lSurfaceDesc);


						CComPtrCustom<IMFMediaBuffer> lBuffer;

						LOG_INVOKE_MF_FUNCTION(MFCreateDXGISurfaceBuffer,
							__uuidof(ID3D11Texture2D),
							lSurface,
							0,
							FALSE,
							&lBuffer);

						LOG_CHECK_PTR_MEMORY(lBuffer);

						mSample.Release();

						LOG_INVOKE_MF_FUNCTION(MFCreateSample,
							&mSample);

						LOG_CHECK_PTR_MEMORY(mSample);

						LOG_INVOKE_MF_METHOD(AddBuffer, mSample,
							lBuffer);	

						init_vb();

						set_device_states();

						mDevice9->SetFVF(tri_fvf);

						//Bind our Vertex Buffer
						mDevice9->SetStreamSource(0,                   //StreamNumber
							g_list_vb,           //StreamData
							0,                   //OffsetInBytes
							sizeof(textured_vertex)); //Stride

						CComPtrCustom<IMFMediaType> lCurrentMediaType;

						LOG_INVOKE_FUNCTION(createUncompressedVideoType,
							lSurfaceDesc.Format,
							lSurfaceDesc.Width,
							lSurfaceDesc.Height,
							MFVideoInterlaceMode::MFVideoInterlace_Progressive,
							mFrameRate,
							mPixelRate,
							&lCurrentMediaType);

						mCurrentMediaType = lCurrentMediaType;

						if (mMixer)
						{
							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								MFT_SET_TYPE_TEST_ONLY);

							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								0);
						}			

					} while (false);

					return lresult;
				}

				HRESULT Direct3D11and3D9Presenter::resize()
				{
					HRESULT lresult(E_FAIL);

					do
					{
						ZeroMemory(&mPrevWindowRect, sizeof(mPrevWindowRect));

						LOG_CHECK_PTR_MEMORY(mD3D9);
						
						LOG_CHECK_PTR_MEMORY(mDeviceManager);

						LOG_CHECK_PTR_MEMORY(mD3D11Device);
							
						LOG_CHECK_PTR_MEMORY(mDevice9);

						UINT lAdapterID = D3DADAPTER_DEFAULT;

						D3DDISPLAYMODE dm;

						LOG_INVOKE_DX9_METHOD(GetAdapterDisplayMode,
							mD3D9,
							lAdapterID,
							&dm);

						D3DPRESENT_PARAMETERS pp;


						ZeroMemory(&pp, sizeof(pp));

						pp.Windowed = TRUE;
						pp.hDeviceWindow = mHWNDVideo;
						pp.SwapEffect = D3DSWAPEFFECT_COPY;
						pp.BackBufferFormat = dm.Format;
						pp.BackBufferWidth = mImageWidth;
						pp.BackBufferHeight = mImageHeight;
						pp.Flags = 0;
						pp.PresentationInterval = D3DPRESENT_INTERVAL_ONE;
						pp.BackBufferCount = 1;

						mDevice9->Reset(&pp);


						HANDLE* lSharedHandle = NULL;

						HANDLE* lPtrSharedHandle = (HANDLE*)&lSharedHandle;

						mTexture9.Release();

						mDevice9->CreateTexture(
							mImageWidth,
							mImageHeight,
							1,
							1,  //D3DUSAGE_RENDERTARGET
							D3DFMT_X8R8G8B8,
							D3DPOOL_DEFAULT,  //D3DPOOL_DEFAULT
							&mTexture9,
							lPtrSharedHandle);



						// Create shared texture

						CComPtrCustom<ID3D11Resource> l_Resource;

						LOG_INVOKE_POINTER_METHOD(mD3D11Device, OpenSharedResource, lSharedHandle, IID_PPV_ARGS(&l_Resource));



						CComPtrCustom<ID3D11Texture2D> lSurface;

						LOG_INVOKE_POINTER_METHOD(l_Resource, QueryInterface, IID_PPV_ARGS(&lSurface));

						LOG_CHECK_PTR_MEMORY(lSurface);

						D3D11_TEXTURE2D_DESC lSurfaceDesc;

						ZeroMemory(&lSurfaceDesc, sizeof(lSurfaceDesc));

						lSurface->GetDesc(
							&lSurfaceDesc);


						CComPtrCustom<IMFMediaBuffer> lBuffer;

						LOG_INVOKE_MF_FUNCTION(MFCreateDXGISurfaceBuffer,
							__uuidof(ID3D11Texture2D),
							lSurface,
							0,
							FALSE,
							&lBuffer);

						LOG_CHECK_PTR_MEMORY(lBuffer);

						mSample.Release();

						LOG_INVOKE_MF_FUNCTION(MFCreateSample,
							&mSample);

						LOG_CHECK_PTR_MEMORY(mSample);

						LOG_INVOKE_MF_METHOD(AddBuffer, mSample,
							lBuffer);

						init_vb();

						CComPtrCustom<IMFMediaType> lCurrentMediaType;

						LOG_INVOKE_FUNCTION(createUncompressedVideoType,
							lSurfaceDesc.Format,
							lSurfaceDesc.Width,
							lSurfaceDesc.Height,
							MFVideoInterlaceMode::MFVideoInterlace_Progressive,
							mFrameRate,
							mPixelRate,
							&lCurrentMediaType);

						mCurrentMediaType = lCurrentMediaType;

						if (mMixer)
						{
							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								MFT_SET_TYPE_TEST_ONLY);

							LOG_INVOKE_MF_METHOD(SetOutputType, mMixer,
								0,
								mCurrentMediaType,
								0);
						}
					

					} while (false);

					return lresult;
				}



				// IRenderingControl implements

				HRESULT Direct3D11and3D9Presenter::renderToTarget(
					IUnknown* aPtrRenderTarget,
					BOOL aCopyMode)
				{
					HRESULT lresult(E_FAIL);

					do
					{

						std::lock_guard<std::mutex> lLock(mAccessMutex);

						if (aCopyMode == TRUE)
						{
							MFT_OUTPUT_DATA_BUFFER lBuffer;

							ZeroMemory(&lBuffer, sizeof(lBuffer));

							lBuffer.dwStreamID = 0;

							lBuffer.pSample = mSample;

							DWORD lState(0);

							LOG_INVOKE_MF_METHOD(ProcessOutput, mMixer,
								0,
								1,
								&lBuffer,
								&lState);
							
							LOG_CHECK_PTR_MEMORY(aPtrRenderTarget);

							CComPtrCustom<ID3D11Texture2D> lSurface;

							aPtrRenderTarget->QueryInterface(IID_PPV_ARGS(&lSurface));

							LOG_CHECK_PTR_MEMORY(lSurface);



							CComPtrCustom<ID3D11Device> lDevice;

							lSurface->GetDevice(&lDevice);
							
							LOG_CHECK_PTR_MEMORY(lDevice);

							CComPtrCustom<ID3D11DeviceContext> lDeviceContext;

							lDevice->GetImmediateContext(&lDeviceContext);

							LOG_CHECK_PTR_MEMORY(lDeviceContext);
							


							DWORD lBufferCount(0);

							LOG_INVOKE_MF_METHOD(GetBufferCount, mSample, &lBufferCount);

							CComPtrCustom<ID3D11Texture2D> lDestSurface;

							if (lBufferCount == 1)
							{
								CComPtrCustom<IMFMediaBuffer> lDestBuffer;

								LOG_INVOKE_MF_METHOD(GetBufferByIndex, mSample, 0, &lDestBuffer);

								// Get the surface from the buffer.

								CComPtrCustom<IMFDXGIBuffer> lIMFDXGIBuffer;

								LOG_INVOKE_QUERY_INTERFACE_METHOD(lDestBuffer, &lIMFDXGIBuffer);

								LOG_CHECK_PTR_MEMORY(lIMFDXGIBuffer);

								LOG_INVOKE_DXGI_METHOD(GetResource, lIMFDXGIBuffer,
									IID_PPV_ARGS(&lDestSurface));
							}
							else
							{
								CComPtrCustom<IDXGISwapChain1> lSwapChain1;

								do
								{
									LOG_INVOKE_MF_METHOD(GetUnknown, mSample, CM_SwapChain, IID_PPV_ARGS(&lSwapChain1));

									LOG_INVOKE_POINTER_METHOD(lSwapChain1, GetBuffer, 0, IID_PPV_ARGS(&lDestSurface));

								} while (false);

								if (!lDestSurface)
								{
									CComPtrCustom<IUnknown> lUnkRenderTexture;

									LOG_INVOKE_MF_METHOD(GetUnknown, mSample, CM_RenderTexture, IID_PPV_ARGS(&lUnkRenderTexture));

									LOG_INVOKE_QUERY_INTERFACE_METHOD(lUnkRenderTexture, &lDestSurface);
								}
							}

							LOG_CHECK_PTR_MEMORY(lDestSurface);

							if (mSharedTexture)
							{								
								lDeviceContext->CopyResource(lSurface, mSharedID3D11Texture2D);
							}
							else
							{
								lDeviceContext->CopyResource(lSurface, lDestSurface);
							}

						}
						else
						{
							LOG_CHECK_PTR_MEMORY(aPtrRenderTarget);

							CComQIPtrCustom<ID3D11Texture2D> lSurface(aPtrRenderTarget);

							LOG_CHECK_PTR_MEMORY(lSurface);

							CComPtrCustom<IMFMediaBuffer> lTargetBuffer;

							LOG_INVOKE_MF_FUNCTION(MFCreateDXGISurfaceBuffer,
								__uuidof(ID3D11Texture2D),
								lSurface,
								0,
								FALSE,
								&lTargetBuffer);

							LOG_CHECK_PTR_MEMORY(lTargetBuffer);

							DWORD lBufferCount(0);

							LOG_INVOKE_MF_METHOD(GetBufferCount, mSample,
								&lBufferCount);

							CComPtrCustom<IMFMediaBuffer> lInitBuffer;

							if (lBufferCount == 1)
							{
								mSample->GetBufferByIndex(0, &lInitBuffer);

								mSample->RemoveBufferByIndex(0);
							}

							LOG_INVOKE_MF_METHOD(AddBuffer, mSample,
								lTargetBuffer);

							MFT_OUTPUT_DATA_BUFFER lBuffer;

							ZeroMemory(&lBuffer, sizeof(lBuffer));

							lBuffer.dwStreamID = 0;

							lBuffer.pSample = mSample;

							DWORD lState(0);

							LOG_INVOKE_MF_METHOD(ProcessOutput, mMixer,
								0,
								1,
								&lBuffer,
								&lState);

							if (lInitBuffer)
							{
								lBufferCount = 0;

								LOG_INVOKE_MF_METHOD(GetBufferCount, mSample,
									&lBufferCount);

								if (lBufferCount == 1)
								{
									mSample->RemoveBufferByIndex(0);
								}

								LOG_INVOKE_MF_METHOD(AddBuffer, mSample,
									lInitBuffer);
							}

						}

					} while (false);

					return lresult;
				}
			}
		}
	}
}