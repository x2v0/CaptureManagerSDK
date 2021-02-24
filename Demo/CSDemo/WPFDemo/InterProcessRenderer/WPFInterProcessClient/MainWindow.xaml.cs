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
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using InterProcessRenderer.Communication;

namespace WPFInterProcessClient
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
         } catch (Exception exc) {
            try {
               mCaptureManager = new CaptureManager();
            } catch (Exception exc1) {
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
      }

      #endregion

      #region  Fields

      private Timer _Timer;
      private readonly CaptureManager mCaptureManager;

      private bool mIsStarted;

      private Process mOffScreenCaptureProcess;

      private PipeProcessor mPipeProcessor;

      private IWebCamControl mWebCamControl;

      #endregion

      #region  Other properties

      private Timer HeartBeatTimer
      {
         get
         {
            if (_Timer == null) {
               _Timer = new Timer(500);
               _Timer.Elapsed += _Timer_Elapsed;
            }

            return _Timer;
         }
      }

      #endregion

      #region Public methods

      public void closeOffScreenCapture()
      {
         if ((mPipeProcessor != null) &&
             (mOffScreenCaptureProcess != null)) {
            mPipeProcessor.send("Close");
         }
      }

      #endregion

      #region Private methods

      private void _Timer_Elapsed(object sender, ElapsedEventArgs e)
      {
         mPipeProcessor.send("HeartBeat");
      }

      private void lPipeProcessor_MessageDelegateEvent(string aMessage)
      {
         switch (aMessage) {
            case "Initilized":
               mPipeProcessor.send("Start");
               break;
         }

         if (aMessage.Contains("Get_EVRStreamFilters=")) {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
               var lXmlDataProvider = (XmlDataProvider) Resources["XmlEVRStreamFiltersProvider"];

               if (lXmlDataProvider == null) {
                  return;
               }

               var doc = new XmlDocument();

               doc.LoadXml(aMessage.Replace("Get_EVRStreamFilters=", ""));

               lXmlDataProvider.Document = doc;
            }));
         }

         if (aMessage.Contains("Get_EVRStreamOutputFeatures=")) {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
               var lXmlDataProvider = (XmlDataProvider) Resources["XmlEVRStreamOutputFeaturesProvider"];

               if (lXmlDataProvider == null) {
                  return;
               }

               var doc = new XmlDocument();

               doc.LoadXml(aMessage.Replace("Get_EVRStreamOutputFeatures=", ""));

               lXmlDataProvider.Document = doc;
            }));
         }
      }

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

         mPipeProcessor.send("Set_EVRStreamFilters=" + lindex + "_0_" + (bool) lCheckBox.IsChecked);

         //if (mIEVRStreamControl != null)
         //    mIEVRStreamControl.setFilterParametr(
         //        mEVROutputNode,
         //        lindex,
         //        0,
         //        (bool)lCheckBox.IsChecked);
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

         mPipeProcessor.send("Set_EVRStreamFilters=" + lindex + "_" + lvalue + "_True");

         //if (mIEVRStreamControl != null)
         //    mIEVRStreamControl.setFilterParametr(
         //        mEVROutputNode,
         //        lindex,
         //        lvalue,
         //        true);
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

         mPipeProcessor.send("Set_EVRStreamOutputFeatures=" + lindex + "_" + lvalue);

         //if (mIEVRStreamControl != null)
         //    mIEVRStreamControl.setOutputFeatureParametr(
         //        mEVROutputNode,
         //        lindex,
         //        lvalue);
      }

      private void mLaunchButton_Click(object sender, RoutedEventArgs e)
      {
         if (mLaunchButton.Content == "Stop") {
            if (mIsStarted) {
               closeOffScreenCapture();

               mLaunchButton.Content = "Launch";
            }

            mIsStarted = false;

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

         var lextendSymbolicLink = lSymbolicLink + " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>" + "<Option Type='Cursor' Visiblity='True'>" + "<Option.Extensions>" +
                                   "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" + "</Option.Extensions>" + "</Option>" + "</Options>";


         lextendSymbolicLink += " --normalize=Landscape";

         startOffScreenCapture(lextendSymbolicLink, lStreamIndex, lMediaTypeIndex);

         mLaunchButton.Content = "Stop";
      }

      private void mOffScreenCaptureProcess_Exited(object sender, EventArgs e)
      {
         var lProcess = sender as Process;

         if (lProcess != null) {
            //lProcess.ExitCode
         }

         mOffScreenCaptureProcess = null;
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

         var lvalue = (bool) lCheckBox.IsChecked ? 2 : 1;

         int lCurrentValue;
         int lMin;
         int lMax;
         int lStep;
         int lDefault;
         int lFlag;

         mWebCamControl.getCamParametr(lindex, out lCurrentValue, out lMin, out lMax, out lStep, out lDefault, out lFlag);

         mWebCamControl.setCamParametr(lindex, lCurrentValue, lvalue);
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

         mWebCamControl.setCamParametr(lindex, lvalue, 2);
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

            if (!mIsStarted) {
               return;
            }

            mPipeProcessor.send("Get_EVRStreamFilters");

            mPipeProcessor.send("Get_EVRStreamOutputFeatures");


            //lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEVRStreamOutputFeaturesProvider"];

            //if (lXmlDataProvider == null)
            //    return;


            //doc = new System.Xml.XmlDocument();

            //lxmldoc = "";

            //mIEVRStreamControl.getCollectionOfOutputFeatures(
            //    mEVROutputNode,
            //    out lxmldoc);

            //if (string.IsNullOrEmpty(lxmldoc))
            //    return;

            //doc.LoadXml(lxmldoc);

            //lXmlDataProvider.Document = doc;
         } else if (mShowBtn.Content.ToString() == "Hide") {
            mEVRStreamParametrsTab.IsEnabled = false;

            mShowBtn.Content = "Show";
         }
      }

      private void startOffScreenCapture(string aSymbolicLink, uint aStreamIndex, uint aMediaTypeIndex)
      {
         mOffScreenCaptureProcess = new Process();

         mOffScreenCaptureProcess.StartInfo.FileName = "InterProcessRenderer.exe";

         mOffScreenCaptureProcess.EnableRaisingEvents = true;

         mOffScreenCaptureProcess.Exited += mOffScreenCaptureProcess_Exited;

         mPipeProcessor = new PipeProcessor("Server", "Client");

         mPipeProcessor.MessageDelegateEvent += lPipeProcessor_MessageDelegateEvent;

         mOffScreenCaptureProcess.StartInfo.Arguments = "SymbolicLink=" + aSymbolicLink + " " + "StreamIndex=" + aStreamIndex + " " + "MediaTypeIndex=" + aMediaTypeIndex + " " + "WindowHandler=" +
                                                        mVideoPanel.Handle.ToInt64();

         mOffScreenCaptureProcess.StartInfo.UseShellExecute = false;

         try {
            mIsStarted = mOffScreenCaptureProcess.Start();

            HeartBeatTimer.Start();
         } catch (Exception) {
            mIsStarted = false;
         }
      }


      private void Window_Closing(object sender, CancelEventArgs e)
      {
         if (mIsStarted) {
            var ltimer = new DispatcherTimer();

            ltimer.Interval = new TimeSpan(0, 0, 0, 1);

            ltimer.Tick += delegate(object sender1, EventArgs e1)
            {
               if (mLaunchButton.Content == "Stop") {
                  if (mIsStarted) {
                     closeOffScreenCapture();
                  }

                  mIsStarted = false;

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
