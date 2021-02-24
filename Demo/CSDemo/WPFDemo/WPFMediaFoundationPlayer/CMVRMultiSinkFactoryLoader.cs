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
using System.Runtime.InteropServices;
using System.Security;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFMediaFoundationPlayer
{
   internal class CMVRMultiSinkFactoryLoader
   {
      #region Static fields

      private static CMVRMultiSinkFactoryLoader mInstance;

      #endregion

      #region Constructors and destructors

      private CMVRMultiSinkFactoryLoader()
      {
         do {
            mIEVRMultiSinkFactory = new EVRMultiSinkFactory();

            uint lMaxPorts = 0;

            MaxPorts = 0;

            var lhresult = getMaxOutputNodeCount(out lMaxPorts);

            if (lhresult != 0) {
               break;
            }

            MaxPorts = lMaxPorts;


            var lPtrUnkIEVRStreamControl = IntPtr.Zero;

            lhresult = createEVRStreamControl(out lPtrUnkIEVRStreamControl);

            if (lhresult != 0) {
               break;
            }

            var lObjUnkIEVRStreamControl = Marshal.GetObjectForIUnknown(lPtrUnkIEVRStreamControl);

            Marshal.Release(lPtrUnkIEVRStreamControl);

            var lIEVRStreamControl = lObjUnkIEVRStreamControl as IEVRStreamControl;

            if (lIEVRStreamControl == null) {
               break;
            }

            mIEVRStreamControl = new EVRStreamControl(lIEVRStreamControl);
         } while (false);
      }

      #endregion

      #region  Fields

      public IEVRMultiSinkFactory mIEVRMultiSinkFactory;

      public CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl mIEVRStreamControl;

      #endregion

      #region Interfaces

      //[Guid("47F9883C-77B1-4A0B-9233-B3EAFA8F387E")]
      //[InterfaceType(ComInterfaceType.InterfaceIsIDispatch)]
      //private interface IEVRStreamControl
      //{
      //    [DispId(1)]
      //    void setPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);
      //    [DispId(2)]
      //    void setZOrder(object aPtrEVROutputNode, uint aZOrder);
      //    [DispId(3)]
      //    void getPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);
      //    [DispId(4)]
      //    void getZOrder(object aPtrEVROutputNode, out uint aPtrZOrder);
      //    [DispId(5)]
      //    void flush(object aPtrEVROutputNode);
      //    [DispId(6)]
      //    void setSrcPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);
      //    [DispId(7)]
      //    void getSrcPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);
      //    [DispId(8)]
      //    void getCollectionOfFilters(object aPtrEVROutputNode, out string aPtrPtrXMLstring);
      //    [DispId(9)]
      //    void setFilterParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue, int aIsEnabled);
      //    [DispId(10)]
      //    void getCollectionOfOutputFeatures(object aPtrEVROutputNode, out string aPtrPtrXMLstring);
      //    [DispId(11)]
      //    void setOutputFeatureParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue);
      //}


      [Guid("47F9883C-77B1-4A0B-9233-B3EAFA8F387E")]
      //[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
      [SuppressUnmanagedCodeSecurity]
      public interface IEVRStreamControl
      {
         #region Public methods

         [DispId(5)]
         void flush(IntPtr aPtrEVROutputNode);

         [DispId(8)]
         void getCollectionOfFilters(IntPtr aPtrEVROutputNode, out string aPtrPtrXMLstring);

         [DispId(10)]
         void getCollectionOfOutputFeatures(IntPtr aPtrEVROutputNode, out string aPtrPtrXMLstring);

         [DispId(3)]
         void getPosition(IntPtr aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);

         [DispId(7)]
         void getSrcPosition(IntPtr aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);

         [DispId(4)]
         void getZOrder(IntPtr aPtrEVROutputNode, out uint aPtrZOrder);

         [DispId(9)]
         void setFilterParametr(IntPtr aPtrEVROutputNode, uint aParametrIndex, int aNewValue, int aIsEnabled);

         [DispId(11)]
         void setOutputFeatureParametr(IntPtr aPtrEVROutputNode, uint aParametrIndex, int aNewValue);

         [DispId(1)]
         void setPosition(IntPtr aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);

         [DispId(6)]
         void setSrcPosition(IntPtr aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);

         [DispId(2)]
         void setZOrder(IntPtr aPtrEVROutputNode, uint aZOrder);

         #endregion
      }

      #endregion

      #region Public properties

      public uint MaxPorts
      {
         get;
         set;
      }

      #endregion

      #region Public methods

      [DllImport("CMVRMultiSinkFactory.dll", CharSet = CharSet.Unicode)]
      public static extern int createEVRStreamControl(out IntPtr aPtrPtrUnkIEVRStreamControl);

      [DllImport("CMVRMultiSinkFactory.dll", CharSet = CharSet.Unicode)]
      public static extern int createOutputNodes(IntPtr aHandle, IntPtr aPtrUnkTarget, uint aOutputNodeAmount, out IntPtr aRefOutputNodes);

      public static CMVRMultiSinkFactoryLoader getInstance()
      {
         if (mInstance == null) {
            mInstance = new CMVRMultiSinkFactoryLoader();
         }

         return mInstance;
      }

      [DllImport("CMVRMultiSinkFactory.dll", CharSet = CharSet.Unicode)]
      public static extern int getMaxOutputNodeCount(out uint aPtrOutputNodeAmount);

      #endregion

      #region Nested classes

      private class EVRMultiSinkFactory : IEVRMultiSinkFactory
      {
         #region Constructors and destructors

         #endregion

         #region Interface methods

         public bool createOutputNodes(IntPtr aHWND, uint aOutputNodeAmount, out List<object> aTopologyOutputNodesList)
         {
            var lresult = false;

            aTopologyOutputNodesList = new List<object>();

            do {
               try {
                  var lArrayMediaNodes = new object();


                  //mIEVRMultiSinkFactory.createOutputNodes(
                  //    aHWND,
                  //    null,
                  //    aOutputNodeAmount,
                  //    out lArrayMediaNodes);

                  if (lArrayMediaNodes == null) {
                     break;
                  }

                  var lArray = lArrayMediaNodes as object[];

                  if (lArray == null) {
                     break;
                  }

                  aTopologyOutputNodesList.AddRange(lArray);

                  lresult = true;
               } catch (Exception) {
               }
            } while (false);

            return lresult;
         }


         public bool createOutputNodes(IntPtr aHandle, object aPtrUnkSharedResource, uint aOutputNodeAmount, out List<object> aTopologyOutputNodesList)
         {
            var lresult = false;

            aTopologyOutputNodesList = new List<object>();

            do {
               try {
                  var lOutputNodes = IntPtr.Zero;

                  var lhresult = CMVRMultiSinkFactoryLoader.createOutputNodes(aHandle, Marshal.GetIUnknownForObject(aPtrUnkSharedResource), aOutputNodeAmount, out lOutputNodes);

                  var lOutputNodesArray = new IntPtr[aOutputNodeAmount];

                  Marshal.Copy(lOutputNodes, lOutputNodesArray, 0, (int) aOutputNodeAmount);

                  for (var i = 0; i < aOutputNodeAmount; i++) {
                     aTopologyOutputNodesList.Add(Marshal.GetObjectForIUnknown(lOutputNodesArray[i]));

                     Marshal.Release(lOutputNodesArray[i]);
                  }

                  Marshal.FreeCoTaskMem(lOutputNodes);

                  lresult = true;
               } catch (Exception) {
               }
            } while (false);

            return lresult;
         }

         #endregion
      }

      private class EVRStreamControl : CaptureManagerToCSharpProxy.Interfaces.IEVRStreamControl
      {
         #region Constructors and destructors

         public EVRStreamControl(IEVRStreamControl aIEVRStreamControl)
         {
            mIEVRStreamControl = aIEVRStreamControl;
         }

         #endregion

         #region  Fields

         private readonly IEVRStreamControl mIEVRStreamControl;

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
                  mIEVRStreamControl.flush(Marshal.GetIUnknownForObject(aPtrEVROutputNode));

                  lresult = true;
               } catch (Exception) {
               }
            } while (false);

            return lresult;
         }


         public void getCollectionOfFilters(object aPtrEVROutputNode, out string aPtrPtrXMLstring)
         {
            aPtrPtrXMLstring = "";

            do {
               if (mIEVRStreamControl == null) {
                  break;
               }

               if (aPtrEVROutputNode == null) {
                  break;
               }

               try {
                  mIEVRStreamControl.getCollectionOfFilters(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out aPtrPtrXMLstring);
               } catch (Exception) {
               }
            } while (false);
         }

         public void getCollectionOfOutputFeatures(object aPtrEVROutputNode, out string aPtrPtrXMLstring)
         {
            aPtrPtrXMLstring = "";

            do {
               if (mIEVRStreamControl == null) {
                  break;
               }

               if (aPtrEVROutputNode == null) {
                  break;
               }

               try {
                  mIEVRStreamControl.getCollectionOfOutputFeatures(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out aPtrPtrXMLstring);
               } catch (Exception) {
               }
            } while (false);
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
                  mIEVRStreamControl.getPosition(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out aPtrLeft, out aPtrRight, out aPtrTop, out aPtrBottom);

                  lresult = true;
               } catch (Exception) {
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
                  mIEVRStreamControl.getSrcPosition(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out aPtrLeft, out aPtrRight, out aPtrTop, out aPtrBottom);

                  lresult = true;
               } catch (Exception) {
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
                  mIEVRStreamControl.getZOrder(Marshal.GetIUnknownForObject(aPtrEVROutputNode), out aPtrZOrder);

                  lresult = true;
               } catch (Exception) {
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
                  mIEVRStreamControl.setFilterParametr(Marshal.GetIUnknownForObject(aPtrEVROutputNode), aParametrIndex, aNewValue, aIsEnabled ? 1 : 0);
               } catch (Exception) {
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
                  mIEVRStreamControl.setOutputFeatureParametr(Marshal.GetIUnknownForObject(aPtrEVROutputNode), aParametrIndex, aNewValue);
               } catch (Exception) {
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
                  mIEVRStreamControl.setPosition(Marshal.GetIUnknownForObject(aPtrEVROutputNode), aLeft, aRight, aTop, aBottom);

                  lresult = true;
               } catch (Exception) {
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
                  mIEVRStreamControl.setSrcPosition(Marshal.GetIUnknownForObject(aPtrEVROutputNode), aLeft, aRight, aTop, aBottom);

                  lresult = true;
               } catch (Exception) {
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
                  mIEVRStreamControl.setZOrder(Marshal.GetIUnknownForObject(aPtrEVROutputNode), aZOrder);

                  lresult = true;
               } catch (Exception) {
               }
            } while (false);

            return lresult;
         }

         #endregion
      }

      #endregion
   }
}
