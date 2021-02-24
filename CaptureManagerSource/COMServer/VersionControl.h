#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class VersionControl : public BaseDispatch<IVersionControl>
      {
         // IVersionControl interface
         STDMETHOD(getVersion)(DWORD* aPtrMAJOR, DWORD* aPtrMINOR, DWORD* aPtrPATCH, BSTR* aPtrPtrAdditionalLabel)
         override;

         STDMETHOD(getXMLStringVersion)(BSTR* aPtrPtrXMLstring) override;

         STDMETHOD(checkVersion)(DWORD aMAJOR, DWORD aMINOR, DWORD aPATCH, boolean* aPtrResult) override;

         // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                             /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,  /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ _In_ DISPID dispIdMember,     /* [annotation][out][in] */
                                                      _In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                      _Out_opt_ VARIANT* pVarResult) override;
      };
   }
}
