#pragma once
#include <vector>
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "IEncoderManager.h"
#include "BaseEncoderManager.h"
struct IPropertyStore;

namespace CaptureManager
{
   namespace Transform
   {
      namespace Encoder
      {
         class WMVideo8EncoderManager : public BaseUnknown<IEncoderManager>, public BaseEncoderManager
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
                               IMFTransform** aPtrPtrEncoderTransform) override; // WMVideo8EncoderManager interface
            static GUID getFirstMediaSubType();

            static GUID getSecondMediaSubType();

         private:
            HRESULT setEncodingProperties(IPropertyStore* aPtrPropertyStore, EncodingSettings aEncodingSettings);

            HRESULT addPrivateData(IMFTransform* aPtrEncoderTransform, IMFMediaType* aPtrMediaType);

            HRESULT fixMediaType(IMFMediaType* aPtrUncompressedMediaType, IMFMediaType* aPtrFixedUncompressedMediaType);
         };
      }
   }
}
