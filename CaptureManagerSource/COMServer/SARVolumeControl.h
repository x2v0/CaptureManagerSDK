#pragma once
#include "CaptureManagerTypeInfo.h"
#include "BaseDispatch.h"

namespace CaptureManager
{
   namespace COMServer
   {
      class SARVolumeControl : public BaseDispatch<ISARVolumeControl>
      {
      public:
         SARVolumeControl();

         virtual ~SARVolumeControl(); //	ISARVolumeControl implements
         /* [id][helpstring] */
         HRESULT STDMETHODCALLTYPE getChannelCount(/* [in] */ IUnknown* aPtrSARNode, /* [out] */ UINT32* aPtrCount)
         override; /* [id][helpstring] */
         HRESULT STDMETHODCALLTYPE setChannelVolume(/* [in] */ IUnknown* aPtrSARNode, /* [in] */ UINT32 aIndex,
                                                               /* [in] */ float aLevel) override; /* [id][helpstring] */
         HRESULT STDMETHODCALLTYPE getChannelVolume(/* [in] */ IUnknown* aPtrSARNode, /* [in] */ UINT32 aIndex,
                                                               /* [out] */ float* aPtrLevel) override;
      };
   }
}
