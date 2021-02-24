#pragma once
#include <string>
#include <mutex>
#include <map>
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"
#include "../Common/ComPtrCustom.h"
#include "../Common/BaseUnknown.h"
#include "../CaptureManagerBroker/ISessionCallbackInner.h"
#include "../CaptureManagerBroker/SinkCommon.h"
#include "../LogPrintOut/LogPrintOut.h"
#include "../Common/Common.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class Session : public BaseDispatch<ISession>
      {
      private:
         struct GUIDComparer
         {
            bool operator()(const GUIDToNamePair& Left, const GUIDToNamePair& Right) const
            {
               return memcmp(&(Left.mGUID), &(Right.mGUID), sizeof(Right.mGUID)) < 0;
            }

            bool operator()(const GUID& Left, const GUID& Right) const
            {
               return memcmp(&Left, &Right, sizeof(Right)) < 0;
            }
         };

         struct SessionCallbackInner;

         struct EnumConnections : BaseUnknown<IEnumConnections>
         {
            EnumConnections(SessionCallbackInner* aSessionCallbackInner, ULONG aCurrentPosition = 0): mCurrentPosition(
               aCurrentPosition)
            {
               mSessionCallbackInner = aSessionCallbackInner;
            }

            STDMETHOD(Next)(ULONG cConnections, /* [length_is][size_is][out] */ LPCONNECTDATA rgcd,
                            ULONG* pcFetched) override;

            STDMETHOD(Skip)(ULONG cConnections) override;

            STDMETHOD(Reset)(void) override;

            STDMETHOD(Clone)(IEnumConnections** ppEnum) override;

         private:
            ULONG mCurrentPosition;
            CComPtrCustom<SessionCallbackInner> mSessionCallbackInner;
         };

         struct SessionCallbackInner : BaseUnknown<ISessionCallbackInner>
         {
            // ISessionCallbackInner interface
            void Invoke(CallbackEventCodeDescriptor aCallbackEventCode, SessionDescriptor aSessionDescriptor) override
            {
               std::lock_guard<std::mutex> lLock(mMutex);
               auto liter = mCallbackMassive.begin();
               for (; liter != mCallbackMassive.end(); ++liter) {
                  (*liter).second->invoke(aCallbackEventCode, aSessionDescriptor);
               }
            }

            STDMETHOD(Advise)(ISessionCallback* aPtrISessionCallback, /* [out] */DWORD* pdwCookie)
            {
               HRESULT lresult;
               do {
                  LOG_CHECK_PTR_MEMORY(aPtrISessionCallback);
                  LOG_CHECK_PTR_MEMORY(pdwCookie);
                  CComPtrCustom<ISessionCallback> lISessionCallback;
                  std::lock_guard<std::mutex> lLock(mMutex);
                  lISessionCallback = aPtrISessionCallback;
                  if (lLastCookie >= (2 << (sizeof(DWORD) * 7)) - 1) {
                     lresult = S_FALSE;
                     break;
                  }
                  *pdwCookie = lLastCookie;
                  mCallbackMassive[lLastCookie++] = lISessionCallback;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHOD(Unadvise)(DWORD dwCookie)
            {
               HRESULT lresult;
               do {
                  std::lock_guard<std::mutex> lLock(mMutex);
                  auto lfind = mCallbackMassive.find(dwCookie);
                  if (lfind == mCallbackMassive.end()) {
                     lresult = S_FALSE;
                     break;
                  }
                  mCallbackMassive.erase(lfind);
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHOD(getEnumConnections)(__RPC__deref_out_opt IEnumConnections** ppEnum)
            {
               HRESULT lresult;
               do {
                  CComPtrCustom<EnumConnections> lEnumConnections(new(std::nothrow)EnumConnections(this));
                  if (!lEnumConnections) {
                     lresult = E_OUTOFMEMORY;
                     break;
                  }
                  lresult = lEnumConnections->QueryInterface(IID_PPV_ARGS(ppEnum));
               } while (false);
               return lresult;
            }

            DWORD lLastCookie = 0;
            std::mutex mMutex;
            std::map<DWORD, CComPtrCustom<ISessionCallback>> mCallbackMassive;
         };

         struct ConnectionPoint : public BaseUnknown<IConnectionPoint>
         {
         private:
            IID mIID;
            CComPtrCustom<IConnectionPointContainer> mIConnectionPointContainer;
            CComPtrCustom<SessionCallbackInner> mSessionCallbackInner;
         public:
            ConnectionPoint(IID aIID, SessionCallbackInner* aPtrSessionCallbackInner) : mIID(aIID)
            {
               mSessionCallbackInner = aPtrSessionCallbackInner;
            }

            void registerContainer(IConnectionPointContainer* aPtrIConnectionPointContainer)
            {
               mIConnectionPointContainer = aPtrIConnectionPointContainer;
            }

            STDMETHOD(GetConnectionInterface)(__RPC__out IID* pIID) override
            {
               HRESULT lresult;
               do {
                  *pIID = mIID;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHOD(GetConnectionPointContainer)(__RPC__deref_out_opt IConnectionPointContainer** ppCPC) override
            {
               HRESULT lresult;
               do {
                  if (ppCPC == nullptr) {
                     lresult = E_INVALIDARG;
                     break;
                  }
                  if (!mIConnectionPointContainer) {
                     lresult = E_POINTER;
                     break;
                  }
                  lresult = mIConnectionPointContainer->QueryInterface(IID_PPV_ARGS(ppCPC));
               } while (false);
               return lresult;
            }

            STDMETHOD(Advise)(IUnknown* pUnkSink, __RPC__out DWORD* pdwCookie) override
            {
               HRESULT lresult;
               do {
                  if (!mSessionCallbackInner) {
                     lresult = E_POINTER;
                     break;
                  }
                  if (pUnkSink == nullptr) {
                     lresult = E_POINTER;
                     break;
                  }
                  CComPtrCustom<ISessionCallback> lSessionCallback;
                  lresult = pUnkSink->QueryInterface(IID_PPV_ARGS(&lSessionCallback));
                  if (FAILED(lresult)) {
                     break;
                  }
                  lresult = mSessionCallbackInner->Advise(lSessionCallback, pdwCookie);
                  if (FAILED(lresult)) {
                     break;
                  }
               } while (false);
               return lresult;
            }

            STDMETHOD(Unadvise)(DWORD dwCookie) override
            {
               HRESULT lresult;
               do {
                  if (!mSessionCallbackInner) {
                     lresult = E_POINTER;
                     break;
                  }
                  lresult = mSessionCallbackInner->Unadvise(dwCookie);
                  if (FAILED(lresult)) {
                     break;
                  }
               } while (false);
               return lresult;
            }

            STDMETHOD(EnumConnections)(__RPC__deref_out_opt IEnumConnections** ppEnum) override
            {
               HRESULT lresult;
               do {
                  lresult = mSessionCallbackInner->getEnumConnections(ppEnum);
               } while (false);
               return lresult;
            }
         };

         struct EnumSessionConnectionPoints : public BaseUnknown<IEnumConnectionPoints>
         {
         private:
            std::map<IID, CComPtrCustom<IUnknown>, GUIDComparer> mIConnectionPointCollection;
            ULONG mCurrentPosition;
         public:
            EnumSessionConnectionPoints() : mCurrentPosition(0) { }

            EnumSessionConnectionPoints(
               std::map<IID, CComPtrCustom<IUnknown>, GUIDComparer>& aRefIConnectionPointCollection,
               ULONG aCurrentPosition = 0) : mCurrentPosition(aCurrentPosition)
            {
               mIConnectionPointCollection = aRefIConnectionPointCollection;
            }

            HRESULT addConnectionPoint(CComPtrCustom<IConnectionPoint>& aRefConnectionPoint)
            {
               HRESULT lresult;
               do {
                  if (!aRefConnectionPoint) {
                     lresult = E_OUTOFMEMORY;
                     break;
                  }
                  IID lIID;
                  lresult = aRefConnectionPoint->GetConnectionInterface(&lIID);
                  if (FAILED(lresult)) {
                     break;
                  }
                  mIConnectionPointCollection[lIID] = aRefConnectionPoint;
               } while (false);
               return lresult;
            }

            STDMETHOD(Next)(ULONG cConnections, /* [length_is][size_is][out] */ LPCONNECTIONPOINT* ppCP,
                            ULONG* pcFetched) override
            {
               HRESULT lresult;
               do {
                  ULONG lFetched = 0;
                  auto lter = mIConnectionPointCollection.begin();
                  while (lFetched < cConnections && mCurrentPosition < mIConnectionPointCollection.size()) {
                     LPCONNECTIONPOINT* lPtrPtrCP = nullptr;
                     LOG_INVOKE_QUERY_INTERFACE_METHOD((*lter).second, lPtrPtrCP);
                     LOG_CHECK_PTR_MEMORY(lPtrPtrCP);
                     ppCP[lFetched] = *lPtrPtrCP;
                     lFetched++;
                     mCurrentPosition++;
                     ++lter;
                  }
                  *pcFetched = lFetched;
                  lresult = lFetched == cConnections ? S_OK : S_FALSE;
               } while (false);
               return lresult;
            }

            STDMETHOD(Skip)(ULONG cConnections) override
            {
               HRESULT lresult;
               do {
                  ULONG lSkipped = 0;
                  while (lSkipped < cConnections && mCurrentPosition < mIConnectionPointCollection.size()) {
                     lSkipped++;
                     mCurrentPosition++;
                  }
                  if (lSkipped == cConnections)
                     lresult = S_OK;
                  else
                     lresult = S_FALSE;
               } while (false);
               return lresult;
            }

            STDMETHOD(Reset)(void) override
            {
               HRESULT lresult;
               do {
                  mCurrentPosition = 0;
                  lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHOD(Clone)(IEnumConnectionPoints** ppEnum) override
            {
               HRESULT lresult(E_FAIL);
               do {
                  *ppEnum = new(std::nothrow) EnumSessionConnectionPoints(
                     this->mIConnectionPointCollection, mCurrentPosition);
                  if (*ppEnum != nullptr)
                     lresult = S_OK;
               } while (false);
               return lresult;
            }

            STDMETHOD(FindConnectionPoint)(__RPC__in REFIID riid, __RPC__deref_out_opt IConnectionPoint** ppCP)
            {
               HRESULT lresult;
               do {
                  if (ppCP == nullptr) {
                     lresult = E_POINTER;
                     break;
                  }
                  auto lfind = mIConnectionPointCollection.find(riid);
                  if (lfind == mIConnectionPointCollection.end()) {
                     lresult = E_BOUNDS;
                     break;
                  }
                  lresult = (*lfind).second->QueryInterface(IID_PPV_ARGS(ppCP));
               } while (false);
               return lresult;
            }
         };

         struct ConnectionPointContainer : public BaseUnknown<IConnectionPointContainer>
         {
         private:
            CComPtrCustom<EnumSessionConnectionPoints> mEnumConnectionPoints;
         public:
            ConnectionPointContainer(): mEnumConnectionPoints(new(std::nothrow) EnumSessionConnectionPoints()) { }

            HRESULT addConnectionPoint(IConnectionPoint* aPtrConnectionPoint)
            {
               HRESULT lresult;
               do {
                  if (aPtrConnectionPoint == nullptr) {
                     lresult = E_POINTER;
                     break;
                  }
                  if (!mEnumConnectionPoints) {
                     lresult = E_OUTOFMEMORY;
                     break;
                  }
                  CComPtrCustom<IConnectionPoint> lConnectionPoint = aPtrConnectionPoint;
                  lresult = mEnumConnectionPoints->addConnectionPoint(lConnectionPoint);
               } while (false);
               return lresult;
            }

            STDMETHOD(EnumConnectionPoints)(__RPC__deref_out_opt IEnumConnectionPoints** ppEnum) override
            {
               HRESULT lresult;
               do {
                  if (ppEnum == nullptr) {
                     lresult = E_POINTER;
                     break;
                  }
                  if (!mEnumConnectionPoints) {
                     lresult = E_OUTOFMEMORY;
                     break;
                  }
                  mEnumConnectionPoints->QueryInterface(IID_PPV_ARGS(ppEnum));
               } while (false);
               return lresult;
            }

            STDMETHOD(FindConnectionPoint)(__RPC__in REFIID riid, __RPC__deref_out_opt IConnectionPoint** ppCP) override
            {
               HRESULT lresult;
               do {
                  if (ppCP == nullptr) {
                     lresult = E_POINTER;
                     break;
                  }
                  if (!mEnumConnectionPoints) {
                     lresult = E_OUTOFMEMORY;
                     break;
                  }
                  lresult = mEnumConnectionPoints->FindConnectionPoint(riid, ppCP);
               } while (false);
               return lresult;
            }
         };

         SessionDescriptor mSessionDescriptor;
         CComPtrCustom<SessionCallbackInner> mSessionCallback;
         CComPtrCustom<ConnectionPointContainer> mConnectionPointContainer;
      public:
         HRESULT init(VARIANT aArrayPtrSourceNodesOfTopology); // ISession interface
         STDMETHOD(startSession)(LONGLONG aStartPositionInHundredNanosecondUnits, REFGUID aGUIDTimeFormat) override;

         STDMETHOD(stopSession)(void) override;

         STDMETHOD(pauseSession)(void) override;

         STDMETHOD(closeSession)(void) override;

         STDMETHOD(getSessionDescriptor)(DWORD* aPtrSessionDescriptor) override;

         STDMETHOD(getIConnectionPointContainer)(REFIID aREFIID, IUnknown** aPtrPtrControl) override;

         // IDispatch interface stub
         STDMETHOD(GetIDsOfNames)(__RPC__in REFIID riid,                             /* [size_is][in] */
                                  __RPC__in_ecount_full(cNames) LPOLESTR* rgszNames, /* [range][in] */
                                  __RPC__in_range(0, 16384) UINT cNames, LCID lcid,  /* [size_is][out] */
                                  __RPC__out_ecount_full(cNames) DISPID* rgDispId) override;

         HRESULT invokeMethod( /* [annotation][in] */ DISPID dispIdMember,     /* [annotation][out][in] */
                                                      DISPPARAMS* pDispParams, /* [annotation][out] */
                                                      VARIANT* pVarResult) override;
      };
   }
}
