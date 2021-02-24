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
   internal class StreamControl : IStreamControl
   {
      #region Constructors and destructors

      public StreamControl(CaptureManagerLibrary.IStreamControl aIStreamControl)
      {
         mIStreamControl = aIStreamControl;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.IStreamControl mIStreamControl;

      #endregion

      #region Interface methods

      public bool createStreamControlNodeFactory(ref ISpreaderNodeFactory aISpreaderNodeFactory)
      {
         var lresult = false;

         do {
            try {
               if (mIStreamControl == null) {
                  break;
               }

               object lIUnknown;

               mIStreamControl.createStreamControlNodeFactory(typeof(CaptureManagerLibrary.ISpreaderNodeFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lSpreaderNodeFactory = lIUnknown as CaptureManagerLibrary.ISpreaderNodeFactory;

               if (lSpreaderNodeFactory == null) {
                  break;
               }

               aISpreaderNodeFactory = new SpreaderNodeFactory(lSpreaderNodeFactory);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      bool IStreamControl.createStreamControlNodeFactory(Guid aIID, ref ISpreaderNodeFactory aISpreaderNodeFactory)
      {
         var lresult = false;

         do {
            try {
               if (mIStreamControl == null) {
                  break;
               }

               object lIUnknown;

               mIStreamControl.createStreamControlNodeFactory(aIID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lSpreaderNodeFactory = lIUnknown as CaptureManagerLibrary.ISpreaderNodeFactory;

               if (lSpreaderNodeFactory == null) {
                  break;
               }

               aISpreaderNodeFactory = new SpreaderNodeFactory(lSpreaderNodeFactory);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createStreamControlNodeFactory(ref ISwitcherNodeFactory aISwitcherNodeFactory)
      {
         var lresult = false;

         do {
            try {
               if (mIStreamControl == null) {
                  break;
               }

               object lIUnknown;

               mIStreamControl.createStreamControlNodeFactory(typeof(CaptureManagerLibrary.ISwitcherNodeFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lSwitcherNodeFactory = lIUnknown as CaptureManagerLibrary.ISwitcherNodeFactory;

               if (lSwitcherNodeFactory == null) {
                  break;
               }

               aISwitcherNodeFactory = new SwitcherNodeFactory(lSwitcherNodeFactory);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createStreamControlNodeFactory(ref IMixerNodeFactory aIMixerNodeFactory)
      {
         var lresult = false;

         do {
            try {
               if (mIStreamControl == null) {
                  break;
               }

               object lIUnknown;

               mIStreamControl.createStreamControlNodeFactory(typeof(CaptureManagerLibrary.IMixerNodeFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lMixerNodeFactory = lIUnknown as CaptureManagerLibrary.IMixerNodeFactory;

               if (lMixerNodeFactory == null) {
                  break;
               }

               aIMixerNodeFactory = new MixerNodeFactory(lMixerNodeFactory);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool getCollectionOfStreamControlNodeFactories(ref string aInfoString)
      {
         var lresult = false;

         var lPtrXMLstring = IntPtr.Zero;

         do {
            try {
               if (mIStreamControl == null) {
                  break;
               }

               (mIStreamControl as IStreamControlInner).getCollectionOfStreamControlNodeFactories(out lPtrXMLstring);

               if (lPtrXMLstring != IntPtr.Zero) {
                  aInfoString = Marshal.PtrToStringBSTR(lPtrXMLstring);
               }

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         if (lPtrXMLstring != IntPtr.Zero) {
            Marshal.FreeBSTR(lPtrXMLstring);
         }

         return lresult;
      }

      #endregion
   }
}
