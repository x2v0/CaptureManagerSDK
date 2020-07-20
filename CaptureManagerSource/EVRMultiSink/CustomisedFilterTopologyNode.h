#pragma once

#include "../Common/BaseMFAttributes.h"
#include "../Common/MFHeaders.h"
#include "../Common/ComPtrCustom.h"
#include <strmif.h>

namespace EVRMultiSink
{
	using namespace CaptureManager;

	class CustomisedFilterTopologyNode :
		public BaseUnknown<IUnknown>
	{
	public:
		CustomisedFilterTopologyNode(
			IMFTopologyNode* aPtrIMFTopologyNode,
			IMFActivate* aPtrIMFActivate);
		virtual ~CustomisedFilterTopologyNode();
				
	protected:

		virtual bool findIncapsulatedInterface(
				REFIID aRefIID,
				void** aPtrPtrVoidObject);

	private:

		CComPtrCustom<IMFTopologyNode> mTopologyNode;

		CComPtrCustom<IBaseFilter> mBaseFilter;

		CComPtrCustom<IMFActivate> mActivate;


		HRESULT createBaseFilter();
	};
}
