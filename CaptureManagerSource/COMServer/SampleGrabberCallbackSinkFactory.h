#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"
#include "IContainer.h"
#include "../CaptureManagerBroker/SinkCommon.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class SampleGrabberCallbackSinkFactory : public BaseUnknown<ISampleGrabberCallbackSinkFactory, IContainer>
      {
      public:
         static GUIDToNamePair getGUIDToNamePair(); // ISampleGrabberCallbackSinkFactory interface
         STDMETHOD(createOutputNode)(REFGUID aRefMajorType, REFGUID aRefSubType, IUnknown* aPtrISampleGrabberCallback,
                                     IUnknown** aPtrPtrTopologyNode) override; // IContainer interface
         STDMETHOD(setContainerFormat)(REFGUID aRefContainerTypeGUID) override;

      private:
         static void getReadModes(std::vector<GUIDToNamePair>& aRefReadModes);
      };
   }
}
