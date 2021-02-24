#pragma once
#include "../Common/BaseUnknown.h"
#include "../CaptureManagerBroker/ISampleGrabberCallInner.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include "IRead.h"

namespace CaptureManager
{
   namespace Sinks
   {
      namespace SampleGrabberCall
      {
         class SampleGrabberGemstone : public BaseUnknown<ISampleGrabberCallInner>
         {
         public:
            SampleGrabberGemstone(CComPtrCustom<IMFTopologyNode>& aRefOutputNode, CComPtrCustom<IRead>& aReadBuffer);

            virtual ~SampleGrabberGemstone();

            HRESULT readData(unsigned char* aPtrData, DWORD& aRefByteSize) override;

         protected:
            bool findIncapsulatedInterface(REFIID aRefIID, void** aPtrPtrVoidObject) override
            {
               if (aRefIID == __uuidof(IMFTopologyNode) && mOutputNode) {
                  auto lr = mOutputNode->QueryInterface(aRefIID, aPtrPtrVoidObject);
                  return SUCCEEDED(lr);
               }
               return false;
            }

         private:
            CComPtrCustom<IMFTopologyNode> mOutputNode;
            CComPtrCustom<IRead> mReadBuffer;
         };
      }
   }
}
