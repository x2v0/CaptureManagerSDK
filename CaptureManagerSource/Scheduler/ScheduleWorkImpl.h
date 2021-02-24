#pragma once
#include <mutex>
#include <condition_variable>
#include "IScheduler.h"
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"

namespace CaptureManager
{
   namespace Core
   {
      namespace Scheduler
      {
         class ScheduleWorkImpl : public IScheduler, public BaseUnknown<IMFAsyncCallback>
         {
         private:
            ISchedulerCallback* mPtrCallback;
            CComPtrCustom<IMFAsyncResult> mAsyncResult;
            MFWORKITEM_KEY mCancelKey;
            INT64 mPartFrameDurationMillSecTimeout;
            INT64 mPartFrameDuration100nsecondsTimeout;
            INT64 mFrameDuration100nseconds;
            MFTIME mLastTme;
            MFTIME mInitTime;
            MFTIME mShiftTime;
            INT64 lCycleCount;
            INT64 lCycleMax;
            bool mStopFlag;
            std::mutex mStopMutex;
            std::condition_variable mStopCondition;
         public:
            ScheduleWorkImpl(ISchedulerCallback* aPtrCallback);

            virtual ~ScheduleWorkImpl();

            HRESULT init(INT64 aFrameDuration100nseconds);

            HRESULT start() override;

            HRESULT stop() override;

            STDMETHODIMP GetParameters(DWORD*, DWORD*) override;

            STDMETHODIMP Invoke(IMFAsyncResult* pAsyncResult) override;
         };
      }
   }
}
