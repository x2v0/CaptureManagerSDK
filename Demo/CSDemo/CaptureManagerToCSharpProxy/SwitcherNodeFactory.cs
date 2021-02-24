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
   internal class SwitcherNodeFactory : ISwitcherNodeFactory
   {
      #region Constructors and destructors

      public SwitcherNodeFactory(CaptureManagerLibrary.ISwitcherNodeFactory aSwitcherNodeFactory)
      {
         mSwitcherNodeFactory = aSwitcherNodeFactory;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.ISwitcherNodeFactory mSwitcherNodeFactory;

      #endregion

      #region Interface methods

      public bool createSwitcherNode(object aPtrDownStreamTopologyNode, out object aPtrPtrTopologySwitcherNode)
      {
         var lresult = false;

         do {
            aPtrPtrTopologySwitcherNode = null;

            if (mSwitcherNodeFactory == null) {
               break;
            }

            try {
               mSwitcherNodeFactory.createSwitcherNode(aPtrDownStreamTopologyNode, out aPtrPtrTopologySwitcherNode);

               if (aPtrPtrTopologySwitcherNode == null) {
                  break;
               }

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
