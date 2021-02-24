#pragma once
#include <atomic>
#include <mutex>
#include "../Common/MFHeaders.h"
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "ReadWriteBufferRegularSync.h"
#include "ISampleGrabberPullStream.h"

namespace CaptureManager
{
   namespace Sinks
   {
      namespace SampleGrabberPull
      {
         class SampleGrabberPullSink;

         class SampleGrabberPullStream : public BaseUnknown<
               IMFShutdown, IMFStreamSink, IMFMediaTypeHandler, ISampleGrabberPullStream, SampleGrabberCall::
               RegularSampleGrabberCall::ReadWriteBufferRegularSync>
         {
         public:
            SampleGrabberPullStream();

            virtual ~SampleGrabberPullStream(); // IMFShutdown interface implmenentation
            HRESULT STDMETHODCALLTYPE Shutdown() override;

            HRESULT STDMETHODCALLTYPE GetShutdownStatus(MFSHUTDOWN_STATUS* aPtrStatus) override;

            // IMFStreamSink interface implementation
            STDMETHODIMP GetMediaSink(IMFMediaSink** aPtrPtrMediaSink) override;

            STDMETHODIMP GetIdentifier(DWORD* aPtrIdentifier) override;

            STDMETHODIMP GetMediaTypeHandler(IMFMediaTypeHandler** aPtrPtrHandler) override;

            STDMETHODIMP ProcessSample(IMFSample* aPtrSample) override;

            STDMETHODIMP PlaceMarker(MFSTREAMSINK_MARKER_TYPE aMarkerType, const PROPVARIANT* aPtrVarMarkerValue,
                                     const PROPVARIANT* aPtrVarContextValue) override;

            STDMETHODIMP Flush() override; // IMFMediaEventGenerator interface implementation
            STDMETHODIMP BeginGetEvent(IMFAsyncCallback* aPtrAsyncCallback, IUnknown* aPtrUnkState) override;

            STDMETHODIMP EndGetEvent(IMFAsyncResult* aPtrAsyncResult, IMFMediaEvent** aPtrPtrEvent) override;

            STDMETHODIMP GetEvent(DWORD aFlags, IMFMediaEvent** aPtrPtrEvent) override;

            STDMETHODIMP QueueEvent(MediaEventType aMediaEventType, REFGUID aRefGUIDExtendedType, HRESULT aStatus,
                                    const PROPVARIANT* aPtrVarValue) override;

            // IMFMediaTypeHandler interface implementation
            STDMETHODIMP IsMediaTypeSupported(IMFMediaType* aPtrMediaType, IMFMediaType** aPtrPtrMediaType) override;

            STDMETHODIMP GetMediaTypeCount(DWORD* aPtrTypeCount) override;

            STDMETHODIMP GetMediaTypeByIndex(DWORD aIndex, IMFMediaType** aPtrPtrType) override;

            STDMETHODIMP SetCurrentMediaType(IMFMediaType* aPtrMediaType) override;

            STDMETHODIMP GetCurrentMediaType(IMFMediaType** aPtrPtrMediaType) override;

            STDMETHODIMP GetMajorType(GUID* aPtrGUIDMajorType) override;

            // ReadWriteBufferRegularSync interface implementation 
            HRESULT readData(unsigned char* aPtrData, DWORD* aPtrSampleSize) override;

            // SampleGrabberPullStream interface implementation
            virtual STDMETHODIMP init(SampleGrabberPullSink* aPtrSink, IMFMediaType* aPtrMediaType);

            // ISampleGrabberPullStream interface implementation
            STDMETHODIMP start() override;

            STDMETHODIMP stop() override;

         protected:
            bool findInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
            {
               if (aRefIID == __uuidof(IMFMediaEventGenerator)) {
                  return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IMFMediaEventGenerator*>(this));
               }
               if (aRefIID == __uuidof(IRead)) {
                  return castInterfaces(aRefIID, aPtrPtrVoidObject, static_cast<IRead*>(this));
               }
               return BaseUnknown::findInterface(aRefIID, aPtrPtrVoidObject);
            }

         private:
            std::mutex mMutex;
            SampleGrabberPullSink* mPtrSink;
            CComPtrCustom<IMFMediaType> mMediaType;
            CComPtrCustom<IMFMediaEventQueue> mEventQueue;
         };
      }
   }
}
