#pragma once
#include <condition_variable>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"

namespace CaptureManager
{
   namespace Core
   {
      class MediaSinkFinalizeProcessor : public BaseUnknown<IMFAsyncCallback>
      {
      public:
         MediaSinkFinalizeProcessor();

         virtual ~MediaSinkFinalizeProcessor();

         HRESULT finalizeMediaSink(IMFMediaSink* aPtrMediaSink); // IMFAsyncCallback implements
         HRESULT STDMETHODCALLTYPE GetParameters(DWORD* aPtrFlags, DWORD* aPtrQueue) override;

         HRESULT STDMETHODCALLTYPE Invoke(IMFAsyncResult* aPtrAsyncResult) override;

      private:
         std::mutex mMutex;
         std::condition_variable mConditionVariable;
      };
   }
}
