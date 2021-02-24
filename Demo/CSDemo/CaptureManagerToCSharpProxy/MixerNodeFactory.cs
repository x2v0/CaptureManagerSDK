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

namespace CaptureManagerToCSharpProxy
{
   internal class MixerNodeFactory : IMixerNodeFactory
   {
      #region Constructors and destructors

      public MixerNodeFactory(CaptureManagerLibrary.IMixerNodeFactory aIMixerNodeFactory)
      {
         mIMixerNodeFactory = aIMixerNodeFactory;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.IMixerNodeFactory mIMixerNodeFactory;

      #endregion

      #region Interface methods

      public bool createMixerNodes(object aPtrDownStreamTopologyNode, uint aInputNodeAmount, out List<object> aTopologyInputNodesList)
      {
         var lresult = false;

         aTopologyInputNodesList = new List<object>();

         do {
            if (mIMixerNodeFactory == null) {
               break;
            }


            try {
               var lArrayMediaNodes = new object();

               mIMixerNodeFactory.createMixerNodes(aPtrDownStreamTopologyNode, aInputNodeAmount, out lArrayMediaNodes);

               if (lArrayMediaNodes == null) {
                  break;
               }

               var lArray = lArrayMediaNodes as object[];

               if (lArray == null) {
                  break;
               }

               aTopologyInputNodesList.AddRange(lArray);

               lresult = true;
            } catch (Exception) {
            }
         } while (false);

         return lresult;
      }

      #endregion
   }
}
