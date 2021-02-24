﻿/*
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
using CaptureManagerToCSharpProxy.Interfaces;

namespace CaptureManagerToCSharpProxy
{
   internal class RenderingControl : IRenderingControl
   {
      #region Constructors and destructors

      public RenderingControl(CaptureManagerLibrary.IRenderingControl aIRenderingControl)
      {
         mIRenderingControl = aIRenderingControl;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.IRenderingControl mIRenderingControl;

      #endregion

      #region Interface methods

      public bool enableInnerRendering(object aPtrEVROutputNode, bool aIsInnerRendering)
      {
         var lresult = false;

         do {
            if (mIRenderingControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIRenderingControl.enableInnerRendering(aPtrEVROutputNode, aIsInnerRendering ? 1 : 0);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool renderToTarget(object aPtrEVROutputNode, object aPtrRenderTarget, bool aCopyMode)
      {
         var lresult = false;

         do {
            if (mIRenderingControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            if (aPtrRenderTarget == null) {
               break;
            }

            try {
               mIRenderingControl.renderToTarget(aPtrEVROutputNode, aPtrRenderTarget, aCopyMode ? 1 : 0);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      #endregion
   }
}
