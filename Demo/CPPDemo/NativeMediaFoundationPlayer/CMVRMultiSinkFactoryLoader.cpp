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

#include "CMVRMultiSinkFactoryLoader.h"
#include <Unknwnbase.h>
#include "CMVRMultiSinkFactory.h"


CCMVRMultiSinkFactoryLoader::CCMVRMultiSinkFactoryLoader()
{
}


CCMVRMultiSinkFactoryLoader::~CCMVRMultiSinkFactoryLoader()
{
}


CCMVRMultiSinkFactoryLoader& CCMVRMultiSinkFactoryLoader::getInstance()
{
	static CCMVRMultiSinkFactoryLoader mInsance;

	return mInsance;
}

long CCMVRMultiSinkFactoryLoader::createRendererOutputNodes(
	void * aHandle,
	void *aPtrUnkTarget,
	unsigned long aOutputNodeAmount,
	std::vector<CComPtrCustom<IUnknown>>& aOutputNodes)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (mHCMVRMultiSinkFactoryLibrary == nullptr)
		{
			lhresult = init();

			if (FAILED(lhresult))
				break;
		}

		aOutputNodes.clear();

		auto l_createOutputNodesFunc = (decltype(&::createOutputNodes))GetProcAddress((HMODULE)mHCMVRMultiSinkFactoryLibrary, "createOutputNodes");

		if (l_createOutputNodesFunc == nullptr)
		{
			lhresult = E_POINTER;

			break;
		}

		void* lPtrOutputNodes = nullptr;

		lhresult = l_createOutputNodesFunc(
			aHandle,
			aPtrUnkTarget,
			aOutputNodeAmount,
			&lPtrOutputNodes);

		if (FAILED(lhresult))
			break;

		if (lPtrOutputNodes == nullptr)
		{
			lhresult = E_POINTER;

			break;
		}

		IUnknown ** lPtrPtrUnkOutputNodes = (IUnknown**)(lPtrOutputNodes);

		for (size_t i = 0; i < aOutputNodeAmount; i++)
		{
			CComPtrCustom<IUnknown> lUnkOutputNode;

			lUnkOutputNode = lPtrPtrUnkOutputNodes[i];

			aOutputNodes.push_back(lUnkOutputNode);
		}

		CoTaskMemFree(lPtrOutputNodes);
		
	} while (false);

	return lhresult;
}

long CCMVRMultiSinkFactoryLoader::getMaxVideoRenderStreamCount(
	unsigned long* aPtrOutputNodeAmount)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (mHCMVRMultiSinkFactoryLibrary == nullptr)
		{
			lhresult = init();

			if (FAILED(lhresult))
				break;
		}
		
		auto l_getMaxOutputNodeCountFunc = (decltype(&::getMaxOutputNodeCount))GetProcAddress((HMODULE)mHCMVRMultiSinkFactoryLibrary, "getMaxOutputNodeCount");
		
		if (l_getMaxOutputNodeCountFunc == nullptr)
		{
			lhresult = E_POINTER;

			break;
		}

		lhresult = l_getMaxOutputNodeCountFunc(aPtrOutputNodeAmount);

	} while (false);

	return lhresult;
}

long CCMVRMultiSinkFactoryLoader::createEVRStreamControl(
	IUnknown** aPtrPtrUnkIEVRStreamControl)
{
	HRESULT lhresult(E_FAIL);

	do
	{
		if (mHCMVRMultiSinkFactoryLibrary == nullptr)
		{
			lhresult = init();

			if (FAILED(lhresult))
				break;
		}

		auto l_createEVRStreamControlFunc = (decltype(&::createEVRStreamControl))GetProcAddress((HMODULE)mHCMVRMultiSinkFactoryLibrary, "createEVRStreamControl");

		if (l_createEVRStreamControlFunc == nullptr)
		{
			lhresult = E_POINTER;

			break;
		}

		lhresult = l_createEVRStreamControlFunc((void**)aPtrPtrUnkIEVRStreamControl);
		
	} while (false);

	return lhresult;
}

HRESULT CCMVRMultiSinkFactoryLoader::init()
{
	HRESULT lhresult(E_FAIL);

	do
	{
		mHCMVRMultiSinkFactoryLibrary = LoadLibraryEx(L"CMVRMultiSinkFactory.dll", NULL, 0);

		if (mHCMVRMultiSinkFactoryLibrary == nullptr)
		{
			return -1;
		}
		
		lhresult = S_OK;

	} while (false);

	return lhresult;
}
