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
using System.Collections.Generic;
using System.Windows;
using DirectShowLib;
using Microsoft.Win32;

namespace WPFDirectShowPlayer
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
      }

      #endregion

      #region  Fields

      private readonly IGraphBuilder m_pGraph = (IGraphBuilder) new FilterGraph();

      #endregion

      #region Private methods

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         var lopenFileDialog = new OpenFileDialog();

         lopenFileDialog.AddExtension = true;

         var lresult = lopenFileDialog.ShowDialog();

         if (lresult != true) {
            return;
         }

         var lDSoundRender = new DSoundRender() as IBaseFilter;

         m_pGraph.AddFilter(lDSoundRender, "Audio Renderer");


         var k = 0;

         var lAudioRendererPins = new IPin[1];

         IEnumPins ppEnum;

         k = lDSoundRender.EnumPins(out ppEnum);

         k = ppEnum.Next(1, lAudioRendererPins, IntPtr.Zero);

         var lCaptureManagerEVRMultiSinkFactory = CaptureManagerVideoRendererMultiSinkFactory.getInstance().getICaptureManagerEVRMultiSinkFactory();

         var lMaxVideoRenderStreamCount = lCaptureManagerEVRMultiSinkFactory.getMaxVideoRenderStreamCount();

         if (lMaxVideoRenderStreamCount == 0) {
            return;
         }

         var lOutputNodesList = new List<object>();

         lCaptureManagerEVRMultiSinkFactory.createOutputNodes(IntPtr.Zero, mEVRDisplay.Surface.texture, 1, // lMaxVideoRenderStreamCount,
                                                              out lOutputNodesList);

         if (lOutputNodesList.Count == 0) {
            return;
         }

         var lVideoMixingRenderer9 = (IBaseFilter) lOutputNodesList[0];

         var h = m_pGraph.AddFilter(lVideoMixingRenderer9, "lVideoMixingRenderer9");


         var lVideoRendererPin = new IPin[1];


         k = lVideoMixingRenderer9.EnumPins(out ppEnum);

         k = ppEnum.Next(1, lVideoRendererPin, IntPtr.Zero);


         IBaseFilter m_SourceFilter = null;

         m_pGraph.AddSourceFilter(lopenFileDialog.FileName, null, out m_SourceFilter);

         IEnumPins lEnumPins = null;

         m_SourceFilter.EnumPins(out lEnumPins);

         var lPins = new IPin[1];

         while (lEnumPins.Next(1, lPins, IntPtr.Zero) == 0) {
            IEnumMediaTypes lIEnumMediaTypes;

            lPins[0].EnumMediaTypes(out lIEnumMediaTypes);

            var ppMediaTypes = new AMMediaType[1];

            while (lIEnumMediaTypes.Next(1, ppMediaTypes, IntPtr.Zero) == 0) {
               var gh = ppMediaTypes[0].subType;

               if (ppMediaTypes[0].majorType == MediaType.Video) {
                  k = m_pGraph.Connect(lPins[0], lVideoRendererPin[0]);
               }
            }

            foreach (var item in lPins) {
               k = m_pGraph.Render(item);
            }
         }

         var lIMediaControl = m_pGraph as IMediaControl;

         k = lIMediaControl.Run();
      }

      #endregion
   }
}
