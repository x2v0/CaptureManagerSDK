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

#include "RendererFactory.h"
#include "Direct3D9Renderer.h"
#include "Common\ComPtrCustom.h"


RendererFactory::RendererFactory()
{
}

RendererFactory::~RendererFactory()
{
}

HRESULT RendererFactory::createRenderer(RendererType aRendererType, HWND aWindowHandler, IRenderer** aPtrPtrIRenderer)
{
	HRESULT lresult(E_FAIL);

	do
	{
		if (aRendererType == RendererType::Directx3D9)
		{
			CComPtrCustom<Direct3D9Renderer> lDirect3D9Renderer = new Direct3D9Renderer();

			lDirect3D9Renderer->init(aWindowHandler);

			lDirect3D9Renderer->QueryInterface(IID_PPV_ARGS(aPtrPtrIRenderer));
		}

	} while (false);

	return lresult;
}