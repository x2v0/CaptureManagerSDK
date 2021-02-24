#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"
#include "SinkCollection.h"
#include "../CaptureManagerBroker/StreamControlCommon.h"
#include "../PugiXML/pugixml.hpp"

namespace CaptureManager
{
   namespace COMServer
   {
      class SampleAccumulatorNodeFactory : public BaseDispatch<ISpreaderNodeFactory>
      {
      public:
         SampleAccumulatorNodeFactory(REFIID aREFIID);

         static StreamControlInfo getStreamControlInfo(pugi::xml_node& aRefRootXMLElement,
                                                       std::vector<StreamControlInfo>& aCollection);

         // ISpreaderNodeFactory interface
         STDMETHOD(createSpreaderNode)(VARIANT aArrayPtrDownStreamTopologyNodes, IUnknown** aPtrPtrTopologySpreaderNode)
         override;                                                                   // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                             /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,  /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ _In_ DISPID dispIdMember,     /* [annotation][out][in] */
                                                      _In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                      _Out_opt_ VARIANT* pVarResult) override;

      private:
         IID mIID;
      };
   }
}
