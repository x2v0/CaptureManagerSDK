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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using WPFMediaFoundationPlayer.Interop;

namespace WPFMediaFoundationPlayer
{
   internal class EVRDisplay : ContentControl
   {
      #region Constructors and destructors

      public EVRDisplay()
      {
         var lTuple = D3D9Image.createD3D9Image();

         if (lTuple != null) {
            imageSource = lTuple.Item1;

            Surface = lTuple.Item2;
         }

         if (imageSource != null) {
            var image = new Image();
            image.Stretch = Stretch.Uniform;
            image.Source = imageSource;
            AddChild(image);

            Width = imageSource.Width;

            Height = imageSource.Height;

            // To greatly reduce flickering we're only going to AddDirtyRect
            // when WPF is rendering.
            CompositionTarget.Rendering += CompositionTargetRendering;
         }
      }

      #endregion

      #region  Fields

      private readonly D3DImage imageSource;

      #endregion

      #region Public properties

      public Direct3DSurface9 Surface
      {
         get;
      }

      #endregion

      #region Private methods

      private void CompositionTargetRendering(object sender, EventArgs e)
      {
         if ((imageSource != null) &&
             imageSource.IsFrontBufferAvailable) {
            imageSource.Lock();
            imageSource.AddDirtyRect(new Int32Rect(0, 0, imageSource.PixelWidth, imageSource.PixelHeight));
            imageSource.Unlock();
         }
      }

      #endregion
   }
}
