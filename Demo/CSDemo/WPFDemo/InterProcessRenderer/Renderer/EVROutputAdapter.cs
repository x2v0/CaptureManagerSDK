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

namespace InterProcessRenderer
{
   internal class EVROutputAdapter
   {
      #region Constructors and destructors

      public EVROutputAdapter(Program viewer, IntPtr handler)
      {
         Viewer = viewer;

         _handler = handler;
      }

      #endregion

      #region  Fields

      private readonly IntPtr _handler = IntPtr.Zero;

      private object _Node;

      #endregion

      #region Public properties

      public object Node
      {
         get
         {
            if (_Node == null) {
               IEVRMultiSinkFactory lSinkFactory;

               var lSinkControl = Viewer.CaptureManager.createSinkControl();

               lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

               var ltemp = new List<object>();

               lSinkFactory.createOutputNodes(_handler, 1, out ltemp);

               _Node = ltemp[0];
            }

            return _Node;
         }
         set => _Node = value;
      }

      public Program Viewer
      {
         get;
      }

      #endregion
   }
}
