#pragma once
#include <vector>
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "IEncoderManager.h"
struct IPropertyStore;

namespace CaptureManager
{
   namespace Transform
   {
      namespace Encoder
      {
         class WMAudioEncoderManager : public BaseUnknown<IEncoderManager>
         {
         public: // IEncoderManager interface
            HRESULT enumEncoderMediaTypes(IMFMediaType* aPtrInputMediaType, EncodingSettings aEncodingSettings,
                                          REFGUID aRefEncoderCLSID,
                                          std::vector<CComPtrCustom<IUnknown>>& aRefListOfMediaTypes) override;

            HRESULT getCompressedMediaType(IMFMediaType* aPtrUncompressedMediaType, EncodingSettings aEncodingSettings,
                                           REFGUID aRefEncoderCLSID, DWORD lIndexCompressedMediaType,
                                           IMFMediaType** aPtrPtrCompressedMediaType) override;

            HRESULT getEncoder(IMFMediaType* aPtrUncompressedMediaType, EncodingSettings aEncodingSettings,
                               REFGUID aRefEncoderCLSID, DWORD lIndexCompressedMediaType,
                               IMFTransform** aPtrPtrEncoderTransform) override; // WMAudioEncoderManager interface
            static GUID getMediaSubType();

         private:
            HRESULT setEncodingProperties(IPropertyStore* aPtrPropertyStore, EncodingSettings aEncodingSettings);

            HRESULT copyAndModifyInputMediaType(IMFMediaType* aPtrOriginalInputMediaType,
                                                IMFMediaType** aPtrPtrCopyInputMediaType);

            HRESULT createStubUncompressedMediaType(UINT32 aSamplePerSecond, UINT32 aNumChannels, UINT32 aBitsPerSample,
                                                    IMFMediaType** aPtrPtrStubUncompressedMediaType);
         };
      }
   }
}
