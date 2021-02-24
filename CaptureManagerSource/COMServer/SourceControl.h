#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class SourceControl : public BaseDispatch<ISourceControl>
      {
      public: // ISourceControl DWORDerface
         STDMETHOD(getCollectionOfSources)(/* [out][in] */ BSTR* aPtrPtrXMLstring) override;

         STDMETHOD(getSourceOutputMediaType)(BSTR aSymbolicLink, DWORD aIndexStream, DWORD aIndexMediaType,
                                             IUnknown** aPtrPtrOutputMediaType) override;

         STDMETHOD(createSourceNode)(BSTR aSymbolicLink, DWORD aIndexStream, DWORD aIndexMediaType,
                                     IUnknown** aPtrPtrTopologyNode) override;

         STDMETHOD(createSourceNodeWithDownStreamConnection)(BSTR aSymbolicLink, DWORD aIndexStream,
                                                             DWORD aIndexMediaType,
                                                             IUnknown* aPtrDownStreamTopologyNode,
                                                             IUnknown** aPtrPtrTopologyNode) override;

         STDMETHOD(createSource)(BSTR aSymbolicLink, IUnknown** aPtrPtrMediaSource) override;

         STDMETHOD(createSourceFromCaptureProcessor)(IUnknown* aPtrCaptureProcessor, IUnknown** aPtrPtrMediaSource)
         override;

         STDMETHOD(createSourceNodeFromExternalSource)(IUnknown* aPtrMediaSource, DWORD aIndexStream,
                                                       DWORD aIndexMediaType, IUnknown** aPtrPtrTopologyNode) override;

         STDMETHOD(createSourceNodeFromExternalSourceWithDownStreamConnection)(
            IUnknown* aPtrMediaSource, DWORD aIndexStream, DWORD aIndexMediaType, IUnknown* aPtrDownStreamTopologyNode,
            IUnknown** aPtrPtrTopologyNode) override;

         STDMETHOD(createSourceControl)(BSTR aSymbolicLink, REFIID aREFIID, IUnknown** aPtrPtrSourceControl) override;

         STDMETHOD(getSourceOutputMediaTypeFromMediaSource)(IUnknown* aPtrMediaSource, DWORD aIndexStream,
                                                            DWORD aIndexMediaType, IUnknown** aPtrPtrOutputMediaType)
         override;                                                                   // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                             /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,  /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ _In_ DISPID dispIdMember,     /* [annotation][out][in] */
                                                      _In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                      _Out_opt_ VARIANT* pVarResult) override;

      private:
         HRESULT invokeGetSourceOutputMediaType(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSourceNode(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                        _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSourceNodeWithDownStreamConnection(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                                _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSource(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                    _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSourceFromCaptureProcessor(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                        _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSourceNodeFromExternalSource(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                          _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSourceNodeFromExternalSourceWithDownStreamConnection(
            _In_ DISPPARAMS* pDispParams, /* [annotation][out] */ _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeCreateSourceControl(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                           _Out_opt_ VARIANT* pVarResult);

         HRESULT invokeGetSourceOutputMediaTypeFromMediaSource(_In_ DISPPARAMS* pDispParams, /* [annotation][out] */
                                                               _Out_opt_ VARIANT* pVarResult);
      };
   }
}
