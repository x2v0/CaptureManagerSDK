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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFRtspServer.Interop;

namespace WPFRtspServer
{
   public class VideoPanel : ContentControl
   {
      #region Constants

      private const int Format = (int) D3DFMT.D3DFMT_X8R8G8B8;

      #endregion

      #region Static fields

      private static readonly uint m_height = 720; // 600;

      private static readonly uint m_width = 1280; // 800; 

      #endregion

      #region Constructors and destructors

      public VideoPanel()
      {
         var lTuple = D3D9Image.createD3D9Image();

         if (lTuple != null) {
            imageSource = lTuple.Item1;

            SharedResource = lTuple.Item2;

            Handle = SharedResource.SharedHandle;
         }

         if (imageSource != null) {
            var image = new Image();
            image.Stretch = Stretch.Uniform;
            image.Source = imageSource;
            AddChild(image);

            // To greatly reduce flickering we're only going to AddDirtyRect
            // when WPF is rendering.
            CompositionTarget.Rendering += CompositionTargetRendering;
         }
      }

      #endregion

      #region  Fields

      private readonly D3DImage imageSource;

      private bool mSkip;

      #endregion

      #region Enums

      private enum D3DFMT
      {
         D3DFMT_A8R8G8B8 = 21,
         D3DFMT_X8R8G8B8 = 22
      }

      #endregion

      #region Public properties

      public IntPtr Handle
      {
         get;
      } = IntPtr.Zero;

      #endregion

      #region  Other properties

      internal Direct3DSurface9 SharedResource
      {
         get;
      }

      #endregion

      #region Public methods

      public byte[] takeScreenshot()
      {
         byte[] l_result = null;

         var l_D3D9Image = imageSource as D3D9Image;

         if (l_D3D9Image != null) {
            var l_bitmap = l_D3D9Image.getBackBuffer();

            var l_encoder = new JpegBitmapEncoder();

            l_encoder.QualityLevel = 75;

            l_encoder.Frames.Add(BitmapFrame.Create(l_bitmap));

            using (var outputStream = new MemoryStream()) {
               l_encoder.Save(outputStream);

               l_result = outputStream.ToArray();
            }
         }

         return l_result;
      }

      #endregion

      #region Private methods

      private void CompositionTargetRendering(object sender, EventArgs e)
      {
         mSkip = !mSkip;

         if (mSkip) {
            return;
         }

         if ((imageSource != null) &&
             imageSource.IsFrontBufferAvailable) {
            imageSource.Lock();
            imageSource.AddDirtyRect(new Int32Rect(0, 0, imageSource.PixelWidth, imageSource.PixelHeight));
            imageSource.Unlock();
         }
      }

      #endregion

      #region Nested classes

      private class D3D9Image : D3DImage
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

         public BitmapSource getBackBuffer()
         {
            return CopyBackBuffer();
         }

         #endregion

         #region Private methods

         //private Direct3DTexture9 GetSharedSurface(Direct3DDevice9 device)
         //{

         //    return device.CreateTexture(
         //        m_width,
         //        m_height,
         //        1,
         //        1,  //D3DUSAGE_RENDERTARGET
         //        Format, 
         //        0  //D3DPOOL_DEFAULT
         //        );                
         //}


         private static Direct3DDevice9Ex CreateDevice(IntPtr handle)
         {
            const int D3D_SDK_VERSION = 32;
            using (var d3d9 = Direct3D9Ex.Create(D3D_SDK_VERSION)) {
               var present = new NativeStructs.D3DPRESENT_PARAMETERS();

               try {
                  var wih = new WindowInteropHelper(Application.Current.MainWindow);

                  if (wih.Handle != IntPtr.Zero) {
                     handle = wih.Handle;
                  }
               } catch (Exception) {
               }

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
            return device.CreateRenderTarget(m_width, m_height, Format, // D3DFMT_A8R8G8B8
                                             0, 0, 0 // UNLOCKABLE
                                            );
         }

         private bool init()
         {
            var lresult = false;

            do {
               // Free the old texture
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

         //private static Direct3DDevice9 CreateDevice(IntPtr handle)
         //{
         //    const int D3D_SDK_VERSION = 32;
         //    using (var d3d9 = Direct3D9Ex.Create(D3D_SDK_VERSION))
         //    {
         //        var present = new NativeStructs.D3DPRESENT_PARAMETERS();

         //        try
         //        {
         //            var wih = new System.Windows.Interop.WindowInteropHelper(App.Current.MainWindow);

         //            if (wih.Handle != IntPtr.Zero)
         //                handle = wih.Handle;
         //        }
         //        catch (Exception)
         //        {
         //        }

         //        present.BackBufferFormat = Format; 
         //        present.BackBufferHeight = 1;
         //        present.BackBufferWidth = 1;
         //        present.Windowed = 1; // TRUE
         //        present.SwapEffect = 1; // D3DSWAPEFFECT_DISCARD
         //        present.hDeviceWindow = handle;
         //        present.PresentationInterval = unchecked((int)0x80000000); // D3DPRESENT_INTERVAL_IMMEDIATE;

         //        return d3d9.CreateDevice(
         //            0, // D3DADAPTER_DEFAULT
         //            1, // D3DDEVTYPE_HAL
         //            handle,                        
         //            0x40 | 0x10,// D3DCREATE_HARDWARE_VERTEXPROCESSING | D3DCREATE_PUREDEVICE
         //            present,
         //            null);
         //    }
         //}
      }

      #endregion
   }
}
