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

namespace WPFWebViewerEVR
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


         mEVRStreamFiltersTabItem.AddHandler(RangeBase.ValueChangedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_ValueChanged));

         mEVRStreamFiltersTabItem.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_Checked));

         mEVRStreamFiltersTabItem.AddHandler(ToggleButton.UncheckedEvent, new RoutedEventHandler(mEVRStreamFilterSlider_Checked));


         mEVRStreamOutputFeaturesTabItem.AddHandler(RangeBase.ValueChangedEvent, new RoutedEventHandler(mEVRStreamOutputFeaturesSlider_ValueChanged));


         mIEVRStreamControl = mCaptureManager.createEVRStreamControl();
      }

      #endregion

      #region  Fields

      private readonly CaptureManager mCaptureManager;

      private object mEVROutputNode;

      private readonly IEVRStreamControl mIEVRStreamControl;

      private ISession mISession;

      #endregion

      #region Private methods

      private void mEVRStreamFilterSlider_Checked(object sender, RoutedEventArgs e)
      {
         var lCheckBox = e.OriginalSource as CheckBox;

         if (lCheckBox == null) {
            return;
         }

         if (!lCheckBox.IsFocused) {
            return;
         }

         var lAttr = lCheckBox.Tag as XmlAttribute;

         if (lAttr == null) {
            return;
         }

         var lindex = uint.Parse(lAttr.Value);

         if (mIEVRStreamControl != null) {
            mIEVRStreamControl.setFilterParametr(mEVROutputNode, lindex, 0, (bool) lCheckBox.IsChecked);
         }
      }


      private void mEVRStreamFilterSlider_ValueChanged(object sender, RoutedEventArgs e)
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

         if (mIEVRStreamControl != null) {
            mIEVRStreamControl.setFilterParametr(mEVROutputNode, lindex, lvalue, true);
         }
      }


      private void mEVRStreamOutputFeaturesSlider_ValueChanged(object sender, RoutedEventArgs e)
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

         if (mIEVRStreamControl != null) {
            mIEVRStreamControl.setOutputFeatureParametr(mEVROutputNode, lindex, lvalue);
         }
      }

      private void mLaunchButton_Click(object sender, RoutedEventArgs e)
      {
         if (mLaunchButton.Content.ToString() == "Stop") {
            if (mISession != null) {
               mISession.closeSession();

               mLaunchButton.Content = "Launch";
            }

            mISession = null;

            //mEVROutputNode = null;

            return;
         }

         var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

         if (lSourceNode == null) {
            return;
         }

         var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

         if (lNode == null) {
            return;
         }

         var lSymbolicLink = lNode.Value;

         lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

         if (lSourceNode == null) {
            return;
         }

         lNode = lSourceNode.SelectSingleNode("@Index");

         if (lNode == null) {
            return;
         }

         uint lStreamIndex = 0;

         if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
            return;
         }

         lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

         if (lSourceNode == null) {
            return;
         }

         lNode = lSourceNode.SelectSingleNode("@Index");

         if (lNode == null) {
            return;
         }

         uint lMediaTypeIndex = 0;

         if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
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

         if (mEVROutputNode == null) {
            lSinkFactory.createOutputNode(mVideoPanel.Handle, out mEVROutputNode);
         }

         if (mEVROutputNode == null) {
            return;
         }

         object lPtrSourceNode;

         var lSourceControl = mCaptureManager.createSourceControl();

         if (lSourceControl == null) {
            return;
         }

         var lextendSymbolicLink = lSymbolicLink + " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>" + "<Option Type='Cursor' Visiblity='True'>" + "<Option.Extensions>" +
                                   "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" + "</Option.Extensions>" + "</Option>" + "</Options>";


         lextendSymbolicLink += " --normalize=Landscape";

         lSourceControl.createSourceNode(lextendSymbolicLink, lStreamIndex, lMediaTypeIndex, mEVROutputNode, out lPtrSourceNode);


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

      private void mOpacitySlr_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         //if (mIEVRStreamControl != null)
         //    mIEVRStreamControl.setFilterParametr(
         //        mEVROutputNode,
         //        lindex,
         //        lvalue,
         //        true);
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

      private void ShowEVRStream(object sender, RoutedEventArgs e)
      {
         if (mShowBtn.Content.ToString() == "Show") {
            mEVRStreamParametrsTab.IsEnabled = true;

            mShowBtn.Content = "Hide";

            if (mIEVRStreamControl == null) {
               return;
            }


            var lXmlDataProvider = (XmlDataProvider) Resources["XmlEVRStreamFiltersProvider"];

            if (lXmlDataProvider == null) {
               return;
            }


            var doc = new XmlDocument();

            var lxmldoc = "";

            mIEVRStreamControl.getCollectionOfFilters(mEVROutputNode, out lxmldoc);

            if (string.IsNullOrEmpty(lxmldoc)) {
               return;
            }

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;


            lXmlDataProvider = (XmlDataProvider) Resources["XmlEVRStreamOutputFeaturesProvider"];

            if (lXmlDataProvider == null) {
               return;
            }


            doc = new XmlDocument();

            lxmldoc = "";

            mIEVRStreamControl.getCollectionOfOutputFeatures(mEVROutputNode, out lxmldoc);

            if (string.IsNullOrEmpty(lxmldoc)) {
               return;
            }

            doc.LoadXml(lxmldoc);

            lXmlDataProvider.Document = doc;
         } else if (mShowBtn.Content.ToString() == "Hide") {
            mEVRStreamParametrsTab.IsEnabled = false;

            mShowBtn.Content = "Show";
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

               mEVROutputNode = null;

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
