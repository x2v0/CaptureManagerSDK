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

using System.Collections.Generic;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFRecording
{
   internal class FileSink : AbstractSink
   {
      #region Constructors and destructors

      public FileSink(IFileSinkFactory aSinkFactory)
      {
         mSinkFactory = aSinkFactory;
      }

      #endregion

      #region  Fields

      private readonly IFileSinkFactory mSinkFactory;

      private string mFilePath;

      #endregion

      #region Public methods

      public override object getOutputNode(object aUpStreamMediaType)
      {
         var lCompressedMediaTypeList = new List<object>();

         lCompressedMediaTypeList.Add(aUpStreamMediaType);

         List<object> lTopologyOutputNodesList;

         mSinkFactory.createOutputNodes(lCompressedMediaTypeList, mFilePath, out lTopologyOutputNodesList);

         if (lTopologyOutputNodesList.Count == 0) {
            return null;
         }

         return lTopologyOutputNodesList[0];
      }

      public override void setOptions(string aOptions)
      {
         mFilePath = aOptions;
      }

      #endregion
   }
}
