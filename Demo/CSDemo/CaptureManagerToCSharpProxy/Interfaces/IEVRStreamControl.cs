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

namespace CaptureManagerToCSharpProxy.Interfaces
{
   public interface IEVRStreamControl
   {
      #region Public methods

      bool flush(object aPtrEVROutputNode);

      void getCollectionOfFilters(object aPtrEVROutputNode, out string aPtrPtrXMLstring);

      void getCollectionOfOutputFeatures(object aPtrEVROutputNode, out string aPtrPtrXMLstring);

      bool getPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);

      bool getSrcPosition(object aPtrEVROutputNode, out float aPtrLeft, out float aPtrRight, out float aPtrTop, out float aPtrBottom);

      bool getZOrder(object aPtrEVROutputNode, out uint aPtrZOrder);

      void setFilterParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue, bool aIsEnabled);

      void setOutputFeatureParametr(object aPtrEVROutputNode, uint aParametrIndex, int aNewValue);

      bool setPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);

      bool setSrcPosition(object aPtrEVROutputNode, float aLeft, float aRight, float aTop, float aBottom);

      bool setZOrder(object aPtrEVROutputNode, uint aZOrder);

      #endregion
   }
}
