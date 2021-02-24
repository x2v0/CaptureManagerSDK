#pragma once
#include <map>
#include "../Common/BaseUnknown.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "IPresentationClock.h"

namespace CaptureManager
{
   namespace MediaSession
   {
      namespace CustomisedMediaSession
      {
         class CustomisedPresentationClock : public BaseUnknown<
               IMFPresentationClock, IMFClockStateSink, IPresentationClock>
         {
            CustomisedPresentationClock(IMFPresentationClock* a_InnerPresentationClock);

            virtual ~CustomisedPresentationClock();

            CComPtrCustom<IMFPresentationClock> m_InnerPresentationClock;
            std::map<IMFClockStateSink*, IMFClockStateSink*> m_IMFClockStateSinks;
            std::map<IMediaPipelineProcessorControl*, IMediaPipelineProcessorControl*>
            m_IMediaPipelineProcessorControls;
         protected:
            bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
            {
               if (aRefIID == __uuidof(IMFPresentationClock)) {
                  return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFPresentationClock*>(this));
               }
               if (aRefIID == __uuidof(IMFClock)) {
                  return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFClock*>(this));
               }
               return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
            }

         public:
            static HRESULT create(IMFPresentationClock* a_InnerPresentationClock,
                                  IMFPresentationClock** aPtrPtrIMFPresentationClock); // IMFPresentationClock interface
            HRESULT STDMETHODCALLTYPE SetTimeSource(/* [in] */ __RPC__in_opt IMFPresentationTimeSource* pTimeSource)
            override;

            HRESULT STDMETHODCALLTYPE GetTimeSource(
               /* [out] */ __RPC__deref_out_opt IMFPresentationTimeSource** ppTimeSource) override;

            HRESULT STDMETHODCALLTYPE GetTime(/* [out] */ __RPC__out MFTIME* phnsClockTime) override;

            HRESULT STDMETHODCALLTYPE AddClockStateSink(/* [in] */ __RPC__in_opt IMFClockStateSink* pStateSink)
            override;

            HRESULT STDMETHODCALLTYPE RemoveClockStateSink(/* [in] */ __RPC__in_opt IMFClockStateSink* pStateSink)
            override;

            HRESULT STDMETHODCALLTYPE Start(/* [in] */ LONGLONG llClockStartOffset) override;

            HRESULT STDMETHODCALLTYPE Stop(void) override;

            HRESULT STDMETHODCALLTYPE Pause(void) override; // IMFClock interface
            HRESULT STDMETHODCALLTYPE GetClockCharacteristics(/* [out] */ __RPC__out DWORD* pdwCharacteristics)
            override;

            HRESULT STDMETHODCALLTYPE GetCorrelatedTime(/* [in] */ DWORD dwReserved,                  /* [out] */
                                                                   __RPC__out LONGLONG* pllClockTime, /* [out] */
                                                                   __RPC__out MFTIME* phnsSystemTime) override;

            HRESULT STDMETHODCALLTYPE GetContinuityKey(/* [out] */ __RPC__out DWORD* pdwContinuityKey) override;

            HRESULT STDMETHODCALLTYPE GetState(/* [in] */ DWORD dwReserved, /* [out] */
                                                          __RPC__out MFCLOCK_STATE* peClockState) override;

            HRESULT STDMETHODCALLTYPE GetProperties(/* [out] */ __RPC__out MFCLOCK_PROPERTIES* pClockProperties)
            override;                                                               // IMFClockStateSink interface
            HRESULT STDMETHODCALLTYPE OnClockStart(/* [in] */ MFTIME hnsSystemTime, /* [in] */
                                                              LONGLONG llClockStartOffset) override;

            HRESULT STDMETHODCALLTYPE OnClockStop(/* [in] */ MFTIME hnsSystemTime) override;

            HRESULT STDMETHODCALLTYPE OnClockPause(/* [in] */ MFTIME hnsSystemTime) override;

            HRESULT STDMETHODCALLTYPE OnClockRestart(/* [in] */ MFTIME hnsSystemTime) override;

            HRESULT STDMETHODCALLTYPE OnClockSetRate(/* [in] */ MFTIME hnsSystemTime, /* [in] */ float flRate) override;

            // IPresentationClock interface
            HRESULT addIMediaPipelineProcessorControl(
               IMediaPipelineProcessorControl* aPtrIMediaPipelineProcessorControl) override;

            HRESULT removeIMediaPipelineProcessorControl(
               IMediaPipelineProcessorControl* aPtrIMediaPipelineProcessorControl) override;

            HRESULT getPresentationClock(IMFPresentationClock** aPtrPtrIMFPresentationClock) override;

            HRESULT shutdown() override;
         };
      }
   }
}
