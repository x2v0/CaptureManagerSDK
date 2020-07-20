/*
MIT License

Copyright(c) 2020 Evgeny Pereguda

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files(the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions :

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#include <Unknwnbase.h>

#include "CaptureManagerVideoRendererFactory.h"

#include "Common/ComPtrCustom.h"
#include "Common/CaptureManagerTypeInfo.h"
#include "CaptureManagerLoader.h"
#include "CMVRMultiSinkFactoryLoader.h"

CaptureManagerVideoRendererFactory::CaptureManagerVideoRendererFactory()
{
}

CaptureManagerVideoRendererFactory::~CaptureManagerVideoRendererFactory()
{	
}

CaptureManagerVideoRendererFactory* CaptureManagerVideoRendererFactory::getInstance()
{
	static CaptureManagerVideoRendererFactory mInsance;
	
	return &mInsance;
}

void CaptureManagerVideoRendererFactory::selectLoader(int aloaderIndex)
{
	mloaderIndex = aloaderIndex;
}

long CaptureManagerVideoRendererFactory::getMaxVideoRenderStreamCount(unsigned long* aPtrCount)
{
	if (aPtrCount == nullptr)
		return E_POINTER;
	
	HRESULT lhresult(E_FAIL);

	do
	{
		if (mloaderIndex == 0)
		{
			CaptureManagerLoader::getInstance().getMaxVideoRenderStreamCount(aPtrCount);
		}
		else if (mloaderIndex == 1)
		{
			CCMVRMultiSinkFactoryLoader::getInstance().getMaxVideoRenderStreamCount(aPtrCount);
		}


	} while (false);

	return lhresult;
}

long CaptureManagerVideoRendererFactory::getRenderer(void* aPtrHandler, IUnknown** aPtrPtrUnknown)
{
	HRESULT lhresult(E_FAIL);

	do
	{

		if (aPtrPtrUnknown == nullptr)
			return E_POINTER;

		if (aPtrHandler == nullptr)
			return E_POINTER;

		std::vector<CComPtrCustom<IUnknown>> lOutputNodes;

		if (mloaderIndex == 0)
		{
			CaptureManagerLoader::getInstance().createRendererOutputNodes(
				aPtrHandler,
				nullptr,
				1,
				lOutputNodes);

		}
		else if (mloaderIndex == 1)
		{

			CCMVRMultiSinkFactoryLoader::getInstance().createRendererOutputNodes(
				aPtrHandler,
				nullptr,
				1,
				lOutputNodes);			
		}
		
		if (!lOutputNodes.empty())
		{
			lOutputNodes[0]->QueryInterface(IID_PPV_ARGS(aPtrPtrUnknown));
		}

	} while (false);

	return lhresult;

}

long CaptureManagerVideoRendererFactory::getRenderer(IUnknown* aPtrRenderTarget, IUnknown** aPtrPtrUnknown)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		std::vector<CComPtrCustom<IUnknown>> lOutputNodes;

		if (aPtrPtrUnknown == nullptr)
			return E_POINTER;

		if (aPtrRenderTarget == nullptr)
			return E_POINTER;

		if (mloaderIndex == 0)
		{
			CaptureManagerLoader::getInstance().createRendererOutputNodes(
				nullptr,
				aPtrRenderTarget,
				1,
				lOutputNodes);
		}
		else if (mloaderIndex == 1)
		{
			CCMVRMultiSinkFactoryLoader::getInstance().createRendererOutputNodes(
				nullptr,
				aPtrRenderTarget,
				1,
				lOutputNodes);
		}

		if (!lOutputNodes.empty())
		{
			lOutputNodes[0]->QueryInterface(IID_PPV_ARGS(aPtrPtrUnknown));
		}

	} while (false);

	return lhresult;

}

long CaptureManagerVideoRendererFactory::getEVRControl(IUnknown** aPtrPtrUnknown)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (aPtrPtrUnknown == nullptr)
			return E_POINTER;

		if (mloaderIndex == 0)
		{
			CaptureManagerLoader::getInstance().createEVRStreamControl(
				aPtrPtrUnknown);
		}
		else if (mloaderIndex == 1)
		{
			CCMVRMultiSinkFactoryLoader::getInstance().createEVRStreamControl(
				aPtrPtrUnknown);
		}

	} while (false);
}