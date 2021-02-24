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
   internal class SessionControl : ISessionControl
   {
      #region Constructors and destructors

      public SessionControl(CaptureManagerLibrary.ISessionControl aISessionControl)
      {
         mISessionControl = aISessionControl;
      }

      #endregion

      #region  Fields

      private readonly CaptureManagerLibrary.ISessionControl mISessionControl;

      #endregion

      #region Interface methods

      public ISession createSession(object[] aArrayOfSourceNodesOfTopology)
      {
         ISession lresult = null;

         do {
            if (aArrayOfSourceNodesOfTopology == null) {
               break;
            }

            if (aArrayOfSourceNodesOfTopology.Length == 0) {
               break;
            }

            if (mISessionControl == null) {
               break;
            }

            try {
               object lUnknown;

               mISessionControl.createSession(aArrayOfSourceNodesOfTopology, typeof(CaptureManagerLibrary.ISession).GUID, out lUnknown);

               if (lUnknown == null) {
                  break;
               }

               var lSession = lUnknown as CaptureManagerLibrary.ISession;

               if (lSession == null) {
                  break;
               }

               lresult = new Session(lSession);
            } catch (Exception exc) {
               LogManager.getInstance().write(exc.Message);
            }
         } while (false);

         return lresult;
      }

      #endregion
   }
}
