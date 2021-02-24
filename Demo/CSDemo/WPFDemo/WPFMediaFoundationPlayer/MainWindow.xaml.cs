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
using System.Windows;
using System.Windows.Controls;
using MediaFoundation;
using MediaFoundation.Misc;

namespace WPFMediaFoundationPlayer
{
   /// <summary>
   ///    Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      #region Constructors and destructors

      public MainWindow()
      {
         InitializeComponent();

         mCaptureManagerLib.IsChecked = true;
      }

      #endregion

      #region Private methods

      private void init()
      {
         var lCaptureManagerEVRMultiSinkFactory = CaptureManagerVideoRendererMultiSinkFactory.getInstance().getICaptureManagerEVRMultiSinkFactory();


         var lMaxVideoRenderStreamCount = lCaptureManagerEVRMultiSinkFactory.getMaxVideoRenderStreamCount();

         if (lMaxVideoRenderStreamCount == 0) {
            return;
         }

         var lOutputNodesList = new List<object>();

         lCaptureManagerEVRMultiSinkFactory.createOutputNodes(IntPtr.Zero, mPlayerControl.Surface.texture, lMaxVideoRenderStreamCount, out lOutputNodesList);

         if (lOutputNodesList.Count == 0) {
            return;
         }

         var lEVRList = new List<IMFTopologyNode>();

         foreach (var item in lOutputNodesList) {
            var lRenderTopologyNode = (IMFTopologyNode) item;

            if (lRenderTopologyNode != null) {
               lEVRList.Add(lRenderTopologyNode);
            }
         }

         mPlayerControl.setRenderList(lEVRList, lCaptureManagerEVRMultiSinkFactory.getIEVRStreamControl(), lMaxVideoRenderStreamCount);
      }

      private void Library_Click(object sender, RoutedEventArgs e)
      {
         var lMenuItem = sender as MenuItem;

         if ((lMenuItem == null) ||
             (lMenuItem.Tag == null)) {
            return;
         }

         var ltag = lMenuItem.Tag.ToString();

         var lIndex = 0;

         if (int.TryParse(ltag, out lIndex)) {
            CaptureManagerVideoRendererMultiSinkFactory.getInstance().LibraryIndex = lIndex;

            mCaptureManagerLib.IsChecked = false;

            mCaptureManagerVideoRendererLib.IsChecked = false;

            lMenuItem.IsChecked = true;

            init();
         }
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         MFError throwonhr = MFExtern.MFStartup(0x10070, MFStartup.Full);

         if (throwonhr.Failed()) {
            return;
         }

         init();
      }

      #endregion
   }
}
