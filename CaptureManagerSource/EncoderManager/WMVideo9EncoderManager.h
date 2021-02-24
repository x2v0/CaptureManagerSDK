#pragma once
#include <vector>
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "BaseEncoderManager.h"
#include "IEncoderManager.h"
struct IPropertyStore;

namespace CaptureManager
{
   namespace Transform
   {
      namespace Encoder
      {
         class WMVideo9EncoderManager : public BaseUnknown<IEncoderManager>, public BaseEncoderManager
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
                               IMFTransform** aPtrPtrEncoderTransform) override; // WMVideo9EncoderManager interface
            static GUID getMediaSubType();

         private:
            HRESULT copyInputMediaType(IMFMediaType* aPtrOriginalInputMediaType, IMFMediaType* aPtrCopyInputMediaType);

            HRESULT setEncodingProperties(IPropertyStore* aPtrPropertyStore, EncodingSettings aEncodingSettings);

            HRESULT addPrivateData(IMFTransform* aPtrEncoderTransform, IMFMediaType* aPtrMediaType);
         };
      }
   }
}
