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

#include "WriteSurface.h"
#include <Wingdi.h>


CWriteSurface::CWriteSurface() :
    m_pWriteSurface(NULL),
    m_width(100),
    m_height(100)
{
	Create();
}


CWriteSurface::~CWriteSurface(void)
{
	if (mBitmapHDC)
		DeleteDC(mBitmapHDC);

	if (mBitmap)
		DeleteObject(mBitmap);

    ClearData();
}


void CWriteSurface::ClearData(void)
{
    DWORD height = m_height;

    m_width = 0;
    m_height = 0;

    if(m_pWriteSurface != NULL && height > 0)
    {
        delete m_pWriteSurface;
        m_pWriteSurface = NULL;
    }
}


HRESULT CWriteSurface::Create()
{
	HRESULT hr(E_FAIL);

	HWND lDesktopHWND = GetDesktopWindow();

	HDC lDesktopHDC = GetDC(lDesktopHWND);

	mBitmapHDC = CreateCompatibleDC(lDesktopHDC);

	BITMAPINFO bmi;        // bitmap header


	// setup bitmap info  
	// set the bitmap width and height to 60% of the width and height of each of the three horizontal areas. Later on, the blending will occur in the center of each of the three areas. 
	bmi.bmiHeader.biSize = sizeof(BITMAPINFOHEADER);
	bmi.bmiHeader.biWidth = m_width;
	bmi.bmiHeader.biHeight = m_height;
	bmi.bmiHeader.biPlanes = 1;
	bmi.bmiHeader.biBitCount = 32;         // four 8-bit components 
	bmi.bmiHeader.biCompression = BI_RGB;
	bmi.bmiHeader.biSizeImage = m_height * m_width * 4;

	void* pvBits;

	// create our DIB section and select the bitmap into the dc 
	mBitmap = CreateDIBSection(mBitmapHDC, &bmi, DIB_RGB_COLORS, &pvBits, NULL, 0x0);
	SelectObject(mBitmapHDC, mBitmap);

	m_pWriteSurface = new (std::nothrow) RGBQUAD*[m_height];

	if (m_pWriteSurface == nullptr)
		return hr;

	for (int y = 0; y < m_height; y++)
	{
		m_pWriteSurface[y] = (RGBQUAD*)((BYTE*)pvBits + y * m_width * 4);
	}

	mClientArea.left = 0;

	mClientArea.top = 0;

	mClientArea.right = m_width;

	mClientArea.bottom = m_height;
	
    return S_OK;
}

bool CWriteSurface::writeText(LPCWSTR aText)
{
	int old = 0;
	old = SetBkMode(mBitmapHDC, TRANSPARENT); //default to transparent mode

	FillRect(mBitmapHDC, &mClientArea, (HBRUSH)GetStockObject(BLACK_BRUSH));

	HFONT hf = mFontMgr.GetFontHandle(mBitmapHDC);
	HFONT defaultfont = (HFONT)::SelectObject(mBitmapHDC, hf);
	SetTextColor(mBitmapHDC, mFontMgr.GetFontColor());
		
	TextOut(mBitmapHDC, 0, 0, aText, wcslen(aText));

	SelectObject(mBitmapHDC, defaultfont);

	SetBkMode(mBitmapHDC, old);

	return true;
}