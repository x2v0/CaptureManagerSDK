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
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFIPCameraMJPEGViewer
{
   /// <summary>
   ///    Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      #region Static fields

      public static CaptureManager mCaptureManager;

      #endregion

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

         mISourceControl = mCaptureManager.createSourceControl();
      }

      #endregion

      #region  Fields

      private ISession mISession;

      private readonly ISourceControl mISourceControl;

      #endregion

      #region Private methods

      private async void mLaunchButton_Click(object sender, RoutedEventArgs e)
      {
         if (mLaunchButton.Content.ToString() == "Stop") {
            if (mISession != null) {
               mISession.closeSession();

               mLaunchButton.Content = "Launch";
            }

            mISession = null;

            return;
         }

         if (mISourceControl == null) {
            return;
         }

         ICaptureProcessor lICaptureProcessor = null;

         try {
            lICaptureProcessor = await IPCameraMJPEGCaptureProcessor.createCaptureProcessor();
         } catch (Exception exc) {
            MessageBox.Show(exc.Message);

            return;
         }

         if (lICaptureProcessor == null) {
            return;
         }

         object lMediaSource = null;

         mISourceControl.createSourceFromCaptureProcessor(lICaptureProcessor, out lMediaSource);

         if (lMediaSource == null) {
            return;
         }


         var lxmldoc = "";

         mCaptureManager.getCollectionOfSinks(ref lxmldoc);

         var doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{2F34AF87-D349-45AA-A5F1-E4104D5C458E}']");

         if (lSinkNode == null) {
            return;
         }

         var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

         if (lContainerNode == null) {
            return;
         }

         IEVRSinkFactory lSinkFactory;

         var lSinkControl = mCaptureManager.createSinkControl();

         lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

         object lEVROutputNode;

         lSinkFactory.createOutputNode(mVideoPanel.Handle, out lEVROutputNode);

         if (lEVROutputNode == null) {
            return;
         }

         object lPtrSourceNode;

         var lSourceControl = mCaptureManager.createSourceControl();

         if (lSourceControl == null) {
            return;
         }


         lSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(lMediaSource, 0, 0, lEVROutputNode, out lPtrSourceNode);


         var lSourceMediaNodeList = new List<object>();

         lSourceMediaNodeList.Add(lPtrSourceNode);

         var lSessionControl = mCaptureManager.createSessionControl();

         if (lSessionControl == null) {
            return;
         }

         mISession = lSessionControl.createSession(lSourceMediaNodeList.ToArray());

         if (mISession == null) {
            return;
         }


         mISession.registerUpdateStateDelegate(UpdateStateDelegate);

         mISession.startSession(0, Guid.Empty);

         mLaunchButton.Content = "Stop";
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
               Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => mLaunchButton_Click(null, null)));
            }
               break;
         }
      }

      private void Window_Closing(object sender, CancelEventArgs e)
      {
         if (mISession != null) {
            var ltimer = new DispatcherTimer();

            ltimer.Interval = new TimeSpan(0, 0, 0, 1);

            ltimer.Tick += delegate(object sender1, EventArgs e1)
            {
               if (mLaunchButton.Content.ToString() == "Stop") {
                  if (mISession != null) {
                     mISession.closeSession();
                  }

                  mLaunchButton.Content = "Launch";
               }

               mISession = null;

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
