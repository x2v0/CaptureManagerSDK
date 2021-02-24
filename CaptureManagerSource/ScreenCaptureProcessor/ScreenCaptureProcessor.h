#pragma once
#include <Unknwnbase.h>
#include <queue>
#include <vector>
#include <memory>
#include <thread>
#include <condition_variable>
#include <dxgitype.h>
#include "../Common/BoxMode.h"
#include "../Common/IInnerCaptureProcessor.h"
#include "../Common/BaseUnknown.h"
#include "../Common/ICheck.h"
#include "../Common/IOptions.h"
#include "../Common/SourceState.h"
#include "../Common/ComPtrCustom.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "../Common/Common.h"
#include "../Scheduler/IScheduler.h"
#include  "../Scheduler/SchedulerFactory.h"
#include "IScreenCaptureProcessorEnum.h"
#include "../MemoryManager/IMemoryBufferManager.h"
#include "../CaptureInvoker/CaptureInvoker.h"
#include "../Common/IInnerGetService.h"
struct IMFMediaType;
struct IMFSample;
struct IMFMediaBuffer;
typedef LONGLONG MFTIME;

namespace CaptureManager
{
   using namespace Core;

   namespace Sources
   {
      namespace ScreenCapture
      {
         class ScreenCaptureProcessor : public BaseUnknown<
                                           IInnerCaptureProcessor, IInnerGetService, ICheck, IOptions,
                                           IScreenCaptureProcessorEnum>, public CaptureInvoker
         {
         public:
            ScreenCaptureProcessor(); // IInnerGetService
            HRESULT STDMETHODCALLTYPE GetService(REFGUID aRefGUIDService, REFIID aRefIID, LPVOID* aPtrPtrObject)
            override;                                    // CaptureInvoker implementation
            HRESULT STDMETHODCALLTYPE invoke() override; //IInnerCaptureProcessor interface
            // get friendly name of Processor
            HRESULT getFrendlyName(BSTR* aPtrString) override;          // get synbolicLink of Processor
            HRESULT getSymbolicLink(BSTR* aPtrString) override;         // get supported amount of streams
            HRESULT getStreamAmount(UINT32* aPtrStreamAmount) override; // get supported media types of stream
            HRESULT getMediaTypes(UINT32 aStreamIndex, BSTR* aPtrStreamNameString, IUnknown*** aPtrPtrPtrMediaType,
                                  UINT32* aPtrMediaTypeCount) override; // set supported media type to stream
            HRESULT setCurrentMediaType(UINT32 aStreamIndex, IUnknown* aPtrMediaType) override;

            // get new sample of stream
            HRESULT getNewSample(DWORD aStreamIdentifier, IUnknown** ppSample) override; // start processor
            HRESULT start() override;                                                    // stop processor
            HRESULT stop() override;                                                     // pause processor
            HRESULT pause() override;                                                    // restart processor
            HRESULT restart() override;                                                  // shutdown processor
            HRESULT shutdown() override;                                                 // IOptions interface
            HRESULT STDMETHODCALLTYPE setOptions(const OLECHAR* aOptionsString) override;

            virtual std::wstring getSymbolicLink();

         CAPTUREMANAGER_BOX
            struct ScreenCaptureConfig
            {
               BITMAPINFO mBitMapInfo;
               UINT32 mVideoFPS;
            };

            void update();

         protected:
            struct ClipResource
            {
               int mLeft = 0;
               int mTop = 0;
               int mHeight = 0;
               int mWidth = 0;
               bool misEnable = false;
            };

            struct HWNDHandler
            {
               HWND mHandler = nullptr;
               bool misEnable = false;
               int mTopOffset = 0;
               int mBorderWidth = 0;
               bool mIsWindow = false;
            };

            ClipResource mClipResource;
            HWNDHandler mHWNDHandler;
            ClipResource mResizeResource;
            int mWidth;
            int mHeight;
            int mStride;
            RECT mScreenRect;
            HRESULT mCurrentGrabResult;
            std::vector<ScreenCaptureConfig> mVectorScreenCaptureConfigs;

            void drawOn(RECT aDesktopCoordinates, HDC aHDC, DXGI_MODE_ROTATION aRotation = DXGI_MODE_ROTATION_IDENTITY);

            HRESULT initResources(HDC aHDC);

            virtual std::wstring getFrendlyName();

            virtual HRESULT fillVectorScreenCaptureConfigs(
               std::vector<ScreenCaptureConfig>& aRefVectorScreenCaptureConfigs);

            virtual HRESULT releaseResources() = 0;

            virtual HRESULT initResources(ScreenCaptureConfig& aScreenCaptureConfig,
                                          IMFMediaType* aPtrOutputMediaType) = 0;

            virtual HRESULT grabImage(BYTE* aPtrData) = 0;

            virtual HRESULT grabImage(IUnknown** aPtrPtrUnkSample);

            virtual HRESULT execute();

            virtual HRESULT createVideoMediaType(ScreenCaptureConfig aScreenCaptureConfig,
                                                 IMFMediaType** aPtrPtrMediaType);

            virtual ~ScreenCaptureProcessor();

            bool mIsBlocked;
            bool mIsDirectX11;
         private:
            struct ImageResource
            {
               enum ImageShape
               {
                  Rectangle,
                  Ellipse
               };

               ImageShape mImageShape;
               ULONG mBitmapWidth;
               ULONG mBitmapHeight;
               HBITMAP mBitmap;
               HDC mHDC;
               BOOL mIsEnabled;
               BOOL mIsFill;
               unsigned int mFill;
            };

            struct BufferContainer
            {
               IMFMediaBuffer* mPtrMediaBuffer;
               INT64 mCurrentTime;
               INT64 mCurrentDuration;
               std::mutex mAccessMutex;
               bool IsBufferReady;

               BufferContainer()
               {
                  mPtrMediaBuffer = nullptr;
                  IsBufferReady = false;
               }

               void setBuffer(IMFMediaBuffer* aPtrMediaBuffer, INT64 aCurrentTime, INT64 aCurrentDuration);

               bool getBuffer(IMFMediaBuffer** aPtrPtrMediaBuffer, INT64* aPtrCurrentTime, INT64* aPtrCurrentDuration);

               void clearBuffer();
            };

            enum OptionType
            {
               None,
               Cursor,
               Clip,
               Resize
            };

            ImageResource mBackImageResource;
            ImageResource mFrontImageResource;
            BOOL mVisibility;
            BLENDFUNCTION mBlendFunction;
            SourceState mState;
            UINT32 mSizeImage;
            UINT32 mVideoFPS;
            INT64 mVideoFrameDuration;
            std::mutex mAccessBufferQueueMutex;
            std::mutex mAccessMutex;
            std::mutex mPauseMutex;
            std::mutex mFinishPauseMutex;
            std::mutex mReadyPauseMutex;
            MFTIME mPrevCount;
            MFTIME mSampleTime;
            MFTIME mPrevTime;
            MFTIME mCycleOfCapture;
            MFTIME mSleepDuration;
            MFTIME mDeltaTimeDuration;
            bool mFirstInvoke;
            BufferContainer mBufferContainer;
            CComPtrCustom<IMemoryBufferManager> mMemoryBufferManager;
            CComPtrCustom<IUnknown> mUnkSample;
            std::condition_variable mNewFrameCondition;
            UINT32 mDiscontinuity;
            std::unique_ptr<std::thread> mGrabFrameThread;

            HRESULT setCurrentScreenCaptureConfig(ScreenCaptureConfig& aScreenCaptureConfig,
                                                  IMFMediaType* aPtrMediaType);

            void clearMediaBuffer();

            HRESULT checkInitialisation();

            HRESULT createSample(INT64 aCurrentTime, INT64 aCurrentDuration);

            HRESULT createMediaBuffer(INT64 aCurrentTime, INT64 aCurrentDuration);
         };
      }
   }
}
