#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"
#include "IContainer.h"
#include "../CaptureManagerBroker/SinkCommon.h"

namespace CaptureManager
{
   namespace COMServer
   {
      MIDL_INTERFACE("A2224D8D-C3C1-4593-8AC9-C0FCF318FF05") CMVRMultiSinkFactoryProxy : public IUnknown { };

      class MFEVRMultiSinkFactory : public BaseDispatch<IEVRMultiSinkFactory, IContainer, CMVRMultiSinkFactoryProxy>
      {
      public:
         MFEVRMultiSinkFactory();

         virtual ~MFEVRMultiSinkFactory();

         static GUIDToNamePair getGUIDToNamePair();

         // IEVRMultiSinkFactory interface
         STDMETHOD(createOutputNodes)(/* [in] */ LPVOID aHandle, /* [in] */ IUnknown* aPtrUnkTarget, /* [in] */
                                                 DWORD aOutputNodeAmount,                            /* [out] */
                                                 VARIANT* aPtrArrayPtrTopologyOutputNodes) override;

         // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                             /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,  /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ _In_ DISPID dispIdMember,
                                                      /* [annotation][out][in] */
                                                      _In_ DISPPARAMS* pDispParams,            /* [annotation][out] */
                                                      _Out_opt_ VARIANT* pVarResult) override; // IContainer interface
         STDMETHOD(setContainerFormat)(REFGUID aRefContainerTypeGUID) override;

      private:
         static void getReadModes(std::vector<GUIDToNamePair>& aRefReadModes);
      };
   }
}
