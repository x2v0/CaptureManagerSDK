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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFMultiSourceViewer
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

         try {
            mCaptureManager = new CaptureManager("CaptureManager.dll");
         } catch (Exception) {
            try {
               mCaptureManager = new CaptureManager();
            } catch (Exception) {
            }
         }

         if (mCaptureManager == null) {
            return;
         }

         mSourceControl = mCaptureManager.createSourceControl();

         if (mSourceControl == null) {
            return;
         }

         var lxmldoc = "";

         mCaptureManager.getCollectionOfSinks(ref lxmldoc);

         var doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{10E52132-A73F-4A9E-A91B-FE18C91D6837}']");

         if (lSinkNode == null) {
            return;
         }

         var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

         if (lContainerNode == null) {
            return;
         }

         var lMaxPortCountAttr = lContainerNode.Attributes["MaxPortCount"];

         if ((lMaxPortCountAttr == null) ||
             string.IsNullOrEmpty(lMaxPortCountAttr.Value)) {
            return;
         }

         uint lValue = 0;

         if (uint.TryParse(lMaxPortCountAttr.Value, out lValue)) {
            mStreams = lValue;
         }


         IEVRMultiSinkFactory lSinkFactory;

         var lSinkControl = mCaptureManager.createSinkControl();

         lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

         if (mEVROutputNodes == null) {
            lSinkFactory.createOutputNodes(IntPtr.Zero, m_EVRDisplay.Surface.texture, mStreams, out mEVROutputNodes);
         }

         if ((mEVROutputNodes == null) ||
             (mEVROutputNodes.Count == 0)) {
            return;
         }

         initEVRStreams();
      }

      #endregion

      #region  Fields

      private readonly CaptureManager mCaptureManager;

      private int mColumnCurrent;

      private readonly int mColumnMax = 4;

      private readonly List<object> mEVROutputNodes;

      private readonly IDictionary<int, ISession> mISessions = new Dictionary<int, ISession>();

      private int mRowCurrent;

      private readonly ISourceControl mSourceControl;

      private readonly uint mStreams = 1;

      private readonly string[] URLs = {
         //"http://144.139.80.151:8080/mjpg/video.mjpg", 
         "http://mx.cafesydney.com:8888/mjpg/video.mjpg", "http://118.127.105.138:8080/-wvhttp-01-/GetOneShot?image_size=640x480&frame_count=1000000000#.XxAFnqhd7LI.link"
      };

      #endregion

      #region Private methods

      private void initEVRStreams()
      {
         mRowCurrent = ((int) mStreams / mColumnMax) + 1;

         mColumnCurrent = mRowCurrent > 1 ? mColumnMax : (int) mStreams;

         for (var i = 0; i < mRowCurrent; i++) {
            mControlGrid.RowDefinitions.Add(new RowDefinition());
         }

         for (var i = 0; i < mColumnCurrent; i++) {
            mControlGrid.ColumnDefinitions.Add(new ColumnDefinition());
         }


         var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

         for (var i = 0; i < mStreams; i++) {
            var lRow = i / mColumnCurrent;

            var lBorder = new Border();

            lBorder.BorderBrush = Brushes.Red;

            lBorder.BorderThickness = new Thickness(2);

            mControlGrid.Children.Add(lBorder);

            var lButton = new Button();

            lButton.Width = 80;

            lButton.Height = 80;

            lButton.HorizontalAlignment = HorizontalAlignment.Right;

            lButton.VerticalAlignment = VerticalAlignment.Bottom;

            lButton.Click += lButton_Click;

            lButton.Tag = i;

            lButton.Content = "Start";

            lButton.FontSize = 30;

            mControlGrid.Children.Add(lButton);

            var lColomnIndex = i % mColumnCurrent;

            Grid.SetColumn(lButton, lColomnIndex);

            Grid.SetRow(lButton, lRow);

            Grid.SetColumn(lBorder, lColomnIndex);

            Grid.SetRow(lBorder, lRow);


            if (lEVRStreamControl != null) {
               lEVRStreamControl.setPosition(mEVROutputNodes[i], lColomnIndex / (float) mColumnCurrent, (lColomnIndex + 1) / (float) mColumnCurrent, lRow / (float) mRowCurrent,
                                             (lRow + 1) / (float) mRowCurrent);
            }
         }
      }

      private void lButton_Click(object sender, RoutedEventArgs e)
      {
         var lButton = (Button) sender;

         do {
            if (mSourceControl == null) {
               return;
            }

            if (mISessions == null) {
               break;
            }

            if (lButton.Tag == null) {
               break;
            }

            var lIndex = (int) lButton.Tag;

            if (lButton.Content.ToString() == "Stop") {
               if (mISessions.ContainsKey(lIndex)) {
                  mISessions[lIndex].closeSession();

                  mISessions.Remove(lIndex);

                  lButton.Content = "Start";
               }

               break;
            }

            ICaptureProcessor lICaptureProcessor = null;

            try {
               lICaptureProcessor = IPCameraMJPEGCaptureProcessor.createCaptureProcessor(URLs[lIndex % 2]);
            } catch (Exception exc) {
               MessageBox.Show(exc.Message);

               return;
            }

            if (lICaptureProcessor == null) {
               return;
            }

            object lMediaSource = null;

            mSourceControl.createSourceFromCaptureProcessor(lICaptureProcessor, out lMediaSource);

            if (lMediaSource == null) {
               return;
            }


            object lPtrSourceNode;

            var lSourceControl = mCaptureManager.createSourceControl();

            if (lSourceControl == null) {
               return;
            }

            lSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(lMediaSource, 0, 0, mEVROutputNodes[lIndex], out lPtrSourceNode);


            var lSourceMediaNodeList = new List<object>();

            lSourceMediaNodeList.Add(lPtrSourceNode);

            var lSessionControl = mCaptureManager.createSessionControl();

            if (lSessionControl == null) {
               return;
            }

            var lISession = lSessionControl.createSession(lSourceMediaNodeList.ToArray());

            if (lISession == null) {
               return;
            }


            lISession.registerUpdateStateDelegate(UpdateStateDelegate);

            lISession.startSession(0, Guid.Empty);

            mISessions[lIndex] = lISession;

            lButton.Content = "Stop";
         } while (false);
      }

      private void UpdateStateDelegate(uint aCallbackEventCode, uint aSessionDescriptor)
      {
         var k = (SessionCallbackEventCode) aCallbackEventCode;

         switch (k) {
            case SessionCallbackEventCode.Unknown:
               break;
            case SessionCallbackEventCode.Error:
               break;
            case SessionCallbackEventCode.Status_Error:
               break;
            case SessionCallbackEventCode.Execution_Error:
               break;
            case SessionCallbackEventCode.ItIsReadyToStart:
               break;
            case SessionCallbackEventCode.ItIsStarted:
               break;
            case SessionCallbackEventCode.ItIsPaused:
               break;
            case SessionCallbackEventCode.ItIsStopped:
               break;
            case SessionCallbackEventCode.ItIsEnded:
               break;
            case SessionCallbackEventCode.ItIsClosed:
               break;
            case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
            {
               //Dispatcher.Invoke(
               //DispatcherPriority.Normal,
               //new Action(() => mLaunchButton_Click(null, null)));
            }
               break;
         }
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         foreach (var item in mISessions) {
            item.Value.closeSession();
         }
      }

      #endregion
   }
}
