#pragma once
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "ISourceOperation.h"

namespace CaptureManager
{
   namespace Sources
   {
      class SourceOperation : public BaseUnknown<ISourceOperation>
      {
      public:
         SourceOperation(SourceOperationType aSourceOperationType, IUnknown* aPtrObject = nullptr);

         SourceOperation(SourceOperationType aSourceOperationType, DWORD aStreamIdentifier, IUnknown* aPtrObject);

         virtual ~SourceOperation(); // ISourceOperation interface
         HRESULT getObject(IUnknown** aPtrPtrObject) override;

         HRESULT getSourceOperationType(SourceOperationType* aPtrSourceOperationType) override;

         HRESULT getStreamIdentifier(DWORD* aPtrStreamIdentifier) override;

      private:
         CComPtrCustom<IUnknown> mObject;
         SourceOperationType mSourceOperationType;
         DWORD mStreamIdentifier;
      };
   }
}
