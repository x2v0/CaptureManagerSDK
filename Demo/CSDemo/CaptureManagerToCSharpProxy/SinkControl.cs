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
using CaptureManagerToCSharpProxy.Interfaces;

namespace CaptureManagerToCSharpProxy
{
   internal class SinkControl : ISinkControl
   {
      #region Constructors and destructors

      public SinkControl(CaptureManagerLibrary.ISinkControl aSinkControl)
      {
         mSinkControl = aSinkControl;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.ISinkControl mSinkControl;

      #endregion

      #region Interface methods

      public bool createCompatibleEVRMultiSinkFactory(Guid aContainerTypeGUID, out IEVRMultiSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, new Guid("{A2224D8D-C3C1-4593-8AC9-C0FCF318FF05}"), // typeof(CaptureManagerLibrary.IEVRMultiSinkFactory).GUID,
                                              out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lEVRSinkFactory = lIUnknown as CaptureManagerLibrary.IEVRMultiSinkFactory;

               if (lEVRSinkFactory == null) {
                  break;
               }

               aSinkFactory = new EVRMultiSinkFactory(lEVRSinkFactory);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createSinkFactory(Guid aContainerTypeGUID, out IFileSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.IFileSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lFileSinkFactory = lIUnknown as CaptureManagerLibrary.IFileSinkFactory;

               if (lFileSinkFactory == null) {
                  break;
               }

               aSinkFactory = new FileSinkFactory(lFileSinkFactory);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createSinkFactory(Guid aContainerTypeGUID, out ISampleGrabberCallSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.ISampleGrabberCallSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lFileSinkFactory = lIUnknown as CaptureManagerLibrary.ISampleGrabberCallSinkFactory;

               if (lFileSinkFactory == null) {
                  break;
               }

               aSinkFactory = new SampleGrabberCallSinkFactory(lFileSinkFactory);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createSinkFactory(Guid aContainerTypeGUID, out ISampleGrabberCallbackSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.ISampleGrabberCallbackSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lFileSinkFactory = lIUnknown as CaptureManagerLibrary.ISampleGrabberCallbackSinkFactory;

               if (lFileSinkFactory == null) {
                  break;
               }

               aSinkFactory = new SampleGrabberCallbackSinkFactory(lFileSinkFactory);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }


      public bool createSinkFactory(Guid aContainerTypeGUID, out IEVRSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.IEVRSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lEVRSinkFactory = lIUnknown as CaptureManagerLibrary.IEVRSinkFactory;

               if (lEVRSinkFactory == null) {
                  break;
               }

               aSinkFactory = new EVRSinkFactory(lEVRSinkFactory);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }


      public bool createSinkFactory(Guid aContainerTypeGUID, out IByteStreamSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.IByteStreamSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lByteStreamSinkFactory = lIUnknown as CaptureManagerLibrary.IByteStreamSinkFactory;

               if (lByteStreamSinkFactory == null) {
                  break;
               }

               aSinkFactory = new ByteStreamSinkFactory(lByteStreamSinkFactory);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createSinkFactory(Guid aContainerTypeGUID, out IEVRMultiSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.IEVRMultiSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lEVRSinkFactory = lIUnknown as CaptureManagerLibrary.IEVRMultiSinkFactory;

               if (lEVRSinkFactory == null) {
                  break;
               }

               aSinkFactory = new EVRMultiSinkFactory(lIUnknown);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      public bool createSinkFactory(Guid aContainerTypeGUID, out ISARSinkFactory aSinkFactory)
      {
         var lresult = false;

         aSinkFactory = null;

         do {
            if (mSinkControl == null) {
               break;
            }


            try {
               object lIUnknown;

               mSinkControl.createSinkFactory(aContainerTypeGUID, typeof(CaptureManagerLibrary.ISARSinkFactory).GUID, out lIUnknown);

               if (lIUnknown == null) {
                  break;
               }

               var lSinkFactory = lIUnknown as CaptureManagerLibrary.ISARSinkFactory;

               if (lSinkFactory == null) {
                  break;
               }

               aSinkFactory = new SARSinkFactory(lIUnknown);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      #endregion
   }
}
