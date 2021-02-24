#pragma once
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "ICustomisedRequest.h"

namespace CaptureManager
{
   namespace MediaSession
   {
      namespace CustomisedMediaSession
      {
         class CustomisedRequest : public BaseUnknown<ICustomisedRequest>
         {
         public:
            CustomisedRequest(CustomisedRequestType aCustomisedRequestType, IUnknown* aPtrUnkSender,
                              IUnknown* aPtrUnkSenderTopologyNode, DWORD aIndexOfStreamNode = 0);

            virtual ~CustomisedRequest(); // Interface for getting sent information.
            CustomisedRequestType getCustomisedRequestType() override;

            IUnknown* getPtrUnkSender() override;

            IUnknown* getPtrUnkSenderTopologyNode() override;

            DWORD getIndexOfStreamNode() override;

         private:
            CustomisedRequestType mCustomisedRequestType;
            CComPtrCustom<IUnknown> mUnkSender;
            CComPtrCustom<IUnknown> mUnkSenderTopologyNode;
            DWORD mIndexOfStreamNode;
         };
      }
   }
}
