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
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFDirectShowPlayer
{
   internal class CaptureManagerEVRMultiSinkFactory : ICaptureManagerEVRMultiSinkFactory
   {
      #region Constructors and destructors

      public CaptureManagerEVRMultiSinkFactory(IEVRMultiSinkFactory aIEVRMultiSinkFactory, uint aMaxVideoRenderStreamCount, IEVRStreamControl aIEVRStreamControl)
      {
         mIEVRMultiSinkFactory = aIEVRMultiSinkFactory;

         mMaxVideoRenderStreamCount = aMaxVideoRenderStreamCount;

         mIEVRStreamControl = aIEVRStreamControl;
      }

      #endregion

      #region  Fields

      private readonly IEVRMultiSinkFactory mIEVRMultiSinkFactory;

      private readonly IEVRStreamControl mIEVRStreamControl;

      private readonly uint mMaxVideoRenderStreamCount;

      #endregion

      #region Interface methods

      public bool createOutputNodes(IntPtr aHandle, object aPtrUnkSharedResource, uint aOutputNodeAmount, out List<object> aTopologyOutputNodesList)
      {
         if (mIEVRMultiSinkFactory == null) {
            aTopologyOutputNodesList = new List<object>();

            return false;
         }

         return mIEVRMultiSinkFactory.createOutputNodes(aHandle, aPtrUnkSharedResource, aOutputNodeAmount, out aTopologyOutputNodesList);
      }

      public IEVRStreamControl getIEVRStreamControl()
      {
         return mIEVRStreamControl;
      }

      public uint getMaxVideoRenderStreamCount()
      {
         return mMaxVideoRenderStreamCount;
      }

      #endregion
   }
}
