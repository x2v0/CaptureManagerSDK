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
using System.Runtime.InteropServices;
using CaptureManagerToCSharpProxy.Interfaces;

namespace CaptureManagerToCSharpProxy
{
   internal class WebCamControl : IWebCamControl
   {
      #region Constructors and destructors

      public WebCamControl(CaptureManagerLibrary.IWebCamControl aIWebCamControl)
      {
         mIWebCamControl = aIWebCamControl;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.IWebCamControl mIWebCamControl;

      #endregion

      #region Interface methods

      public void getCamParametr(uint aParametrIndex, out int aCurrentValue, out int aMin, out int aMax, out int aStep, out int aDefault, out int aFlag)
      {
         if (mIWebCamControl == null) {
            aCurrentValue = 0;
            aMin = 0;
            aMax = 0;
            aStep = 0;
            aDefault = 0;
            aFlag = 0;

            return;
         }

         mIWebCamControl.getCamParametr(aParametrIndex, out aCurrentValue, out aMin, out aMax, out aStep, out aDefault, out aFlag);
      }

      public void getCamParametrs(out string aXMLstring)
      {
         aXMLstring = "";

         if (mIWebCamControl == null) {
            return;
         }

         var lPtrXMLstring = IntPtr.Zero;

         do {
            try {
               (mIWebCamControl as IWebCamControlInner).getCamParametrs(out lPtrXMLstring);

               if (lPtrXMLstring != IntPtr.Zero) {
                  aXMLstring = Marshal.PtrToStringBSTR(lPtrXMLstring);
               }
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         if (lPtrXMLstring != IntPtr.Zero) {
            Marshal.FreeBSTR(lPtrXMLstring);
         }
      }

      public void setCamParametr(uint aParametrIndex, int aNewValue, int aFlag)
      {
         if (mIWebCamControl == null) {
            return;
         }

         mIWebCamControl.setCamParametr(aParametrIndex, aNewValue, aFlag);
      }

      #endregion
   }
}
