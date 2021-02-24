#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"
#include "IContainer.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class EncoderNodeFactory : public BaseDispatch<IEncoderNodeFactory, IContainer>
      {
      public: // IEncoderNodeFactory interface
         STDMETHOD(createCompressedMediaType)(IUnknown* aPtrUncompressedMediaType, REFGUID aRefEncodingModeGUID,
                                              DWORD aEncodingModeValue, DWORD aIndexCompressedMediaType,
                                              IUnknown** aPtrPtrCompressedMediaType) override;

         STDMETHOD(createEncoderNode)(IUnknown* aPtrUncompressedMediaType, REFGUID aRefEncodingModeGUID,
                                      DWORD aEncodingModeValue, DWORD aIndexCompressedMediaType,
                                      IUnknown* aPtrDownStreamNode, IUnknown** aPtrPtrEncoderNode) override;

         // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                             /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,  /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ _In_ DISPID dispIdMember,
                                                      /* [annotation][out][in] */
                                                      _In_ DISPPARAMS* pDispParams,            /* [annotation][out] */
                                                      _Out_opt_ VARIANT* pVarResult) override; // IContainer interface
         STDMETHOD(setContainerFormat)(REFGUID aRefEncoderCLSID) override;

      private:
         GUID mEncoderCLSID;
      };
   }
}
