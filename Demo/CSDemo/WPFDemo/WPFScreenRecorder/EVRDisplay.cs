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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPFScreenRecorder
{
    internal class EVRDisplay : System.Windows.Controls.ContentControl
    {
        private System.Windows.Interop.D3DImage imageSource = null;

        private Interop.Direct3DSurface9 surface = null;

        public Interop.Direct3DSurface9 Surface { get { return surface; } }

        public EVRDisplay()
        {
            var lTuple = D3D9Image.createD3D9Image();

            if(lTuple != null)
            {
                this.imageSource = lTuple.Item1;

                this.surface = lTuple.Item2;
            }
            
            if (this.imageSource != null)
            {
                var image = new System.Windows.Controls.Image();
                image.Stretch = System.Windows.Media.Stretch.Uniform;
                image.Source = this.imageSource;
                this.AddChild(image);

                // To greatly reduce flickering we're only going to AddDirtyRect
                // when WPF is rendering.
                System.Windows.Media.CompositionTarget.Rendering += this.CompositionTargetRendering;
            }
        }

        private void CompositionTargetRendering(object sender, EventArgs e)
        {
            if (this.imageSource != null && this.imageSource.IsFrontBufferAvailable)
            {
                this.imageSource.Lock();
                this.imageSource.AddDirtyRect(new Int32Rect(0, 0, this.imageSource.PixelWidth, this.imageSource.PixelHeight));
                this.imageSource.Unlock();
            }
        }
    }
}
