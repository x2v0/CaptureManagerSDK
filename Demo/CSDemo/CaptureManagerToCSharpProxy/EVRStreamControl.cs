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
   internal class EVRStreamControl : IEVRStreamControl
   {
      #region Constructors and destructors

      public EVRStreamControl(CaptureManagerLibrary.IEVRStreamControl aIEVRStreamControl)
      {
         mIEVRStreamControl = aIEVRStreamControl;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.IEVRStreamControl mIEVRStreamControl;

      #endregion

      #region Interface methods

      public bool flush(object aPtrEVROutputNode)
      {
         var lresult = false;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.flush(aPtrEVROutputNode);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }


      public void getCollectionOfFilters(object aPtrEVROutputNode, out string aPtrPtrXMLstring)
      {
         aPtrPtrXMLstring = "";

         var lPtrXMLstring = IntPtr.Zero;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               (mIEVRStreamControl as IEVRStreamControlInner).getCollectionOfFilters(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out lPtrXMLstring);

               if (lPtrXMLstring != IntPtr.Zero) {
                  aPtrPtrXMLstring = Marshal.PtrToStringBSTR(lPtrXMLstring);
               }
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         if (lPtrXMLstring != IntPtr.Zero) {
            Marshal.FreeBSTR(lPtrXMLstring);
         }
      }

      public void getCollectionOfOutputFeatures(object aPtrEVROutputNode, out string aPtrPtrXMLstring)
      {
         aPtrPtrXMLstring = "";

         var lPtrXMLstring = IntPtr.Zero;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               (mIEVRStreamControl as IEVRStreamControlInner).getCollectionOfOutputFeatures(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out lPtrXMLstring);

               if (lPtrXMLstring != IntPtr.Zero) {
                  aPtrPtrXMLstring = Marshal.PtrToStringBSTR(lPtrXMLstring);
               }
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         if (lPtrXMLstring != IntPtr.Zero) {
            Marshal.FreeBSTR(lPtrXMLstring);
         }
      }

      public bool getPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom)
      {
         var lresult = false;

         aPtrLeft = 0.0f;

         aPtrRight = 0.0f;

         aPtrTop = 0.0f;

         aPtrBottom = 0.0f;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.getPosition(aPtrEVROutputNode, out aPtrLeft, out aPtrRight, out aPtrTop, out aPtrBottom);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool getSrcPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom)
      {
         var lresult = false;

         aPtrLeft = 0.0f;

         aPtrRight = 0.0f;

         aPtrTop = 0.0f;

         aPtrBottom = 0.0f;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.getSrcPosition(aPtrEVROutputNode, out aPtrLeft, out aPtrRight, out aPtrTop, out aPtrBottom);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool getZOrder(object aPtrEVROutputNode, out uint aPtrZOrder)
      {
         var lresult = false;

         aPtrZOrder = 0;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.getZOrder(aPtrEVROutputNode, out aPtrZOrder);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public void setFilterParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue, bool aIsEnabled)
      {
         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.setFilterParametr(aPtrEVROutputNode, aParametrIndex, aNewValue, aIsEnabled ? 1 : 0);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);
      }

      public void setOutputFeatureParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue)
      {
         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.setOutputFeatureParametr(aPtrEVROutputNode, aParametrIndex, aNewValue);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);
      }

      public bool setPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom)
      {
         var lresult = false;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.setPosition(aPtrEVROutputNode, aLeft, aRight, aTop, aBottom);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }


      public bool setSrcPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom)
      {
         var lresult = false;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.setSrcPosition(aPtrEVROutputNode, aLeft, aRight, aTop, aBottom);

               lresult = true;
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool setZOrder(object aPtrEVROutputNode, uint aZOrder)
      {
         var lresult = false;

         do {
            if (mIEVRStreamControl == null) {
               break;
            }

            if (aPtrEVROutputNode == null) {
               break;
            }

            try {
               mIEVRStreamControl.setZOrder(aPtrEVROutputNode, aZOrder);

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
