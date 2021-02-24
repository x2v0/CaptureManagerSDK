#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class CoLogPrintOut : public BaseDispatch<ILogPrintOutControl>
      {
      public:
         CoLogPrintOut();

         virtual ~CoLogPrintOut();

         STDMETHOD(setVerbose)(DWORD aLevelType, BSTR aFilePath, boolean aState) override;

         STDMETHOD(addPrintOutDestination)(DWORD aLevelType, BSTR aFilePath) override;

         STDMETHOD(removePrintOutDestination)(DWORD aLevelType, BSTR aFilePath) override; // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                                  /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames,      /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,       /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ _In_ DISPID dispIdMember,     /* [annotation][out][in] */
                                                      _In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                      VARIANT* pVarResult) override;

      private:
         HRESULT invokeSetVerbose(_In_ DISPPARAMS* pDispParams);

         HRESULT invokeAddPrintOutDestination(_In_ DISPPARAMS* pDispParams);

         HRESULT invokeRemovePrintOutDestination(_In_ DISPPARAMS* pDispParams);
      };
   }
}
