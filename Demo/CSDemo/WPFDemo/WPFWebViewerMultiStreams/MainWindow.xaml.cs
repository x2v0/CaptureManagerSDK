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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFWebViewerMultiStreams
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

         var lXmlDataProvider = (XmlDataProvider) Resources["XmlLogProvider"];

         if (lXmlDataProvider == null) {
            return;
         }

         var doc = new XmlDocument();

         var lxmldoc = "";

         mCaptureManager.getCollectionOfSources(ref lxmldoc);

         if (string.IsNullOrEmpty(lxmldoc)) {
            return;
         }

         doc.LoadXml(lxmldoc);

         lXmlDataProvider.Document = doc;

         mWebCamParametrsTab.AddHandler(RangeBase.ValueChangedEvent, new RoutedEventHandler(mParametrSlider_ValueChanged));

         mWebCamParametrsTab.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));

         mWebCamParametrsTab.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(mParametrSlider_Checked));
      }

      #endregion

      #region  Fields

      private readonly CaptureManager mCaptureManager;

      private List<object> mEVROutputNodes;

      private readonly IDictionary<int, ISession> mISessions = new Dictionary<int, ISession>();

      private int mStreamCount;

      private readonly uint mStreams = 2;

      #endregion

      #region Private methods

      private void mLaunchButton_Click(object sender, RoutedEventArgs e)
      {
         var lButton = (Button) sender;

         if (lButton == null) {
            return;
         }

         if (lButton.Tag == null) {
            lButton.Tag = mStreamCount++;
         }

         var lSessionIndex = (int) lButton.Tag;

         if (lButton.Content.ToString() == "Stop") {
            if (mISessions.ContainsKey(lSessionIndex)) {
               mISessions[lSessionIndex].closeSession();

               //System.Threading.Thread.Sleep(200);

               var lEVRStreamControl1 = mCaptureManager.createEVRStreamControl();

               if (lEVRStreamControl1 != null) {
                  lEVRStreamControl1.flush(mEVROutputNodes[lSessionIndex]);
               }

               mISessions.Remove(lSessionIndex);
            }

            lButton.Content = "Launch";

            return;
         }

         var lSymbolicLink = "CaptureManager///Software///Sources///ScreenCapture///ScreenCapture";

         var lextendSymbolicLink = lSymbolicLink + " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>" + "<Option Type='Cursor' Visiblity='True'>" + "<Option.Extensions>" +
                                   "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" + "</Option.Extensions>" + "</Option>" + "</Options>";

         uint lStreamIndex = 0;

         uint lMediaTypeIndex = 4;

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

         IEVRMultiSinkFactory lSinkFactory;

         var lSinkControl = mCaptureManager.createSinkControl();

         lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

         if (mEVROutputNodes == null) {
            lSinkFactory.createOutputNodes(mVideoPanel.Handle, mStreams, out mEVROutputNodes);
         }

         if (mEVROutputNodes == null) {
            return;
         }

         if (mEVROutputNodes.Count == 0) {
            return;
         }

         var lSourceControl = mCaptureManager.createSourceControl();

         if (lSourceControl == null) {
            return;
         }


         object lPtrSourceNode;

         lSourceControl.createSourceNode(lextendSymbolicLink, lStreamIndex, lMediaTypeIndex, mEVROutputNodes[lSessionIndex], out lPtrSourceNode);


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

         mISessions.Add(lSessionIndex, lISession);

         var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

         if (lEVRStreamControl != null) {
            lEVRStreamControl.setPosition(mEVROutputNodes[lSessionIndex], 0.5f * lSessionIndex, 0.5f + (0.5f * lSessionIndex), 0.5f * lSessionIndex, 0.5f + (0.5f * lSessionIndex));


            lEVRStreamControl.setZOrder(mEVROutputNodes[lSessionIndex], 1);

            float lLeft;
            float lRight;
            float lTop;
            float lBottom;

            lEVRStreamControl.getPosition(mEVROutputNodes[lSessionIndex], out lLeft, out lRight, out lTop, out lBottom);

            uint lZOrder;

            lEVRStreamControl.getZOrder(mEVROutputNodes[lSessionIndex], out lZOrder);
         }

         lButton.Content = "Stop";
      }

      private void mParametrSlider_Checked(object sender, RoutedEventArgs e)
      {
      }

      private void mParametrSlider_ValueChanged(object sender, RoutedEventArgs e)
      {
         var lslider = e.OriginalSource as Slider;

         if (lslider == null) {
            return;
         }

         if (!lslider.IsFocused) {
            return;
         }

         var lParametrNode = lslider.Tag as XmlNode;

         if (lParametrNode == null) {
            return;
         }

         var lAttr = lParametrNode.Attributes["Index"];

         if (lAttr == null) {
            return;
         }

         var lindex = uint.Parse(lAttr.Value);

         var lvalue = (int) lslider.Value;
      }

      private void mShowBtn_Click(object sender, RoutedEventArgs e)
      {
         if (mShowBtn.Content.ToString() == "Show") {
            Canvas.SetBottom(mWebCamParametrsPanel, 0);

            mShowBtn.Content = "Hide";
         } else if (mShowBtn.Content.ToString() == "Hide") {
            Canvas.SetBottom(mWebCamParametrsPanel, -150);

            mShowBtn.Content = "Show";
         }
      }

      private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         if ((mEVROutputNodes != null) &&
             (mEVROutputNodes.Count > 0)) {
            var lEVRStreamControl = mCaptureManager.createEVRStreamControl();

            if (lEVRStreamControl != null) {
               lEVRStreamControl.setSrcPosition(mEVROutputNodes[0], 0.0f, (float) e.NewValue, 0.0f, (float) e.NewValue);
            }
         }
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
         if (mISessions.Count > 0) {
            var ltimer = new DispatcherTimer();

            ltimer.Interval = new TimeSpan(0, 0, 0, 1);

            ltimer.Tick += delegate(object sender1, EventArgs e1)
            {
               if (mLaunchButton.Content.ToString() == "Stop") {
                  foreach (var item in mISessions) {
                     item.Value.closeSession();
                  }

                  mISessions.Clear();

                  mLaunchButton.Content = "Launch";
               }

               Close();

               (sender1 as DispatcherTimer).Stop();
            };

            ltimer.Start();

            e.Cancel = true;
         }
      }

      #endregion
   }
}
