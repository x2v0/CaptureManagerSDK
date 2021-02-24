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

using System;
using System.Windows.Interop;
using WPFWebViewerEVRDisplay.Interop;

namespace WPFWebViewerEVRDisplay
{
   internal class D3D9Image : D3DImage
   {
      #region Constructors and destructors

      private D3D9Image()
      {
      }

      #endregion

      #region  Fields

      private Direct3DSurface9 surface;

      #endregion

      #region Public methods

      public static Tuple<D3DImage, Direct3DSurface9> createD3D9Image()
      {
         var lImageSource = new D3D9Image();

         return lImageSource.init() ? Tuple.Create<D3DImage, Direct3DSurface9>(lImageSource, lImageSource.surface) : null;
      }

      #endregion

      #region Private methods

      private static Direct3DDevice9Ex CreateDevice(IntPtr handle)
      {
         const int D3D_SDK_VERSION = 32;
         using (var d3d9 = Direct3D9Ex.Create(D3D_SDK_VERSION)) {
            var present = new NativeStructs.D3DPRESENT_PARAMETERS();
            present.Windowed = 1; // TRUE
            present.SwapEffect = 1; // D3DSWAPEFFECT_DISCARD
            present.hDeviceWindow = handle;
            present.PresentationInterval = unchecked((int) 0x80000000); // D3DPRESENT_INTERVAL_IMMEDIATE;

            return d3d9.CreateDeviceEx(0, // D3DADAPTER_DEFAULT
                                       1, // D3DDEVTYPE_HAL
                                       handle, 70, // D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_MULTITHREADED | D3DCREATE_FPU_PRESERVE
                                       present, null);
         }
      }

      private Direct3DSurface9 GetSharedSurface(Direct3DDevice9Ex device)
      {
         var handle = new IntPtr(0);

         return device.CreateRenderTarget(800, //1920,
                                          600, //1080,
                                          21, // D3DFMT_A8R8G8B8
                                          0, 0, 0, // UNLOCKABLE
                                          ref handle);
      }


      private bool init()
      {
         var lresult = false;

         do {
            // Free the old surface
            if (surface != null) {
               surface.Dispose();
               surface = null;
            }

            using (var device = CreateDevice(NativeMethods.GetDesktopWindow())) {
               surface = GetSharedSurface(device);

               Lock();

               SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativeInterface);

               Unlock();

               lresult = true;
            }
         } while (false);

         return lresult;
      }

      #endregion
   }
}
