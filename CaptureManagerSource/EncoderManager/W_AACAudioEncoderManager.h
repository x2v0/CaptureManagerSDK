#pragma once
#include <vector>
#include "../Common/BaseUnknown.h"
#include "IEncoderManager.h"

namespace CaptureManager
{
   namespace Transform
   {
      namespace Encoder
      {
         class W_AACAudioEncoderManager : public BaseUnknown<IEncoderManager>
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
                               IMFTransform** aPtrPtrEncoderTransform) override; // W_H264VideoEncoderManager interface
            static GUID getMediaSubType();

         private:
            HRESULT checkAndFixInputMediatype(IMFMediaType** aPtrPtrInputMediaType);

            HRESULT createOutputMediaType(IMFMediaType* aPtrOriginalInputMediaType, IMFMediaType* aPtrOutputMediaType);
         };
      }
   }
}
