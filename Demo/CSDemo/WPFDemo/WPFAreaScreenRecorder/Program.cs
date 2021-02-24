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

using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using CaptureManagerToCSharpProxy;

namespace WPFAreaScreenRecorder
{
   internal class Program
   {
      #region Static fields

      public static CaptureManager mCaptureManager;

      #endregion

      #region Private methods

      /// <summary>
      ///    The main entry point for the application.
      /// </summary>
      [MTAThread]
      private static void Main(string[] args)
      {
         var lpriority = Process.GetCurrentProcess().PriorityClass;

         Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;


         try {
            mCaptureManager = new CaptureManager("CaptureManager.dll");
         } catch (Exception) {
            try {
               mCaptureManager = new CaptureManager();
            } catch (Exception) {
            }
         }


         var t = new Thread(delegate()
         {
            try {
               var lApplication = new Application();

               lApplication.Run(new ControlWindow());
            } catch (Exception ex) {
            }
         });
         t.SetApartmentState(ApartmentState.STA);

         t.Start();
      }

      #endregion
   }
}
