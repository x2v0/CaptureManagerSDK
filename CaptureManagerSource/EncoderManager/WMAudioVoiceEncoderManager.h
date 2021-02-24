#pragma once
#include <vector>
#include "../Common/BaseUnknown.h"
#include "../Common/ComPtrCustom.h"
#include "IEncoderManager.h"
struct IPropertyStore;
struct IPropertyBag;

namespace CaptureManager
{
   namespace Transform
   {
      namespace Encoder
      {
         class WMAudioVoiceEncoderManager : public BaseUnknown<IEncoderManager>
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
                               IMFTransform** aPtrPtrEncoderTransform) override; // WMAudioVoiceEncoderManager interface
            static GUID getMediaSubType();

         private:
            HRESULT setEncodingProperties(IPropertyStore* aPtrPropertyStore, EncodingSettings aEncodingSettings);

            HRESULT setEncodingProperties(IPropertyBag* aPtrIPropertyBag, EncodingSettings aEncodingSettings);

            HRESULT copyAndModifyInputMediaType(IMFMediaType* aPtrOriginalInputMediaType,
                                                IMFMediaType** aPtrPtrCopyAndModifyInputMediaType);
         };
      }
   }
}
