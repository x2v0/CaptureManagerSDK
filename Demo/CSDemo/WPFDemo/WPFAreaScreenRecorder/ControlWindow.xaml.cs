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
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using WPFAreaScreenRecorder.Properties;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace WPFAreaScreenRecorder
{
   /// <summary>
   ///    Interaction logic for ControlWindow.xaml
   /// </summary>
   public partial class ControlWindow : Window
   {
      #region Static fields

      public static CaptureManager mCaptureManager;

      #endregion

      #region Constructors and destructors

      public ControlWindow()
      {
         InitializeComponent();

         if (string.IsNullOrEmpty(Settings.Default.StoringDir)) {
            Settings.Default.StoringDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
         }
      }

      #endregion

      #region  Fields

      private string m_VideoSymbolicLink = "";

      private Guid mCLSIDAudioEncoder;

      private Guid mCLSIDVideoEncoder;

      private IEncoderControl mEncoderControl;

      private IEVRMultiSinkFactory mEVRMultiSinkFactory;

      private ISession mISession;

      private ISessionControl mISessionControl;

      private ISinkControl mSinkControl;

      private ISourceControl mSourceControl;

      private ISpreaderNodeFactory mSpreaderNodeFactory;

      private State mState = State.Stopped;

      private IStreamControl mStreamControl;

      private Rectangle SelectionWindowArea;

      #endregion

      #region Enums

      private enum State
      {
         Stopped,
         Started,
         Paused
      }

      #endregion

      #region Private methods

      private static string HexConverter(Color c)
      {
         return "0x" + c.A.ToString("X2") + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
      }

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         ConfigWindow.mCurrentSymbolicLink = getVideoSybolicLink();

         new ConfigWindow().ShowDialog();
      }

      private void Button_Click_1(object sender, RoutedEventArgs e)
      {
         var lSourceNode = m_VideoSourceComboBox.SelectedItem as XmlNode;

         if (lSourceNode == null) {
            return;
         }

         var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute" +
                                                  "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                                                  "/SingleValue/@Value");

         if (lNode == null) {
            return;
         }

         var lSymbolicLink = lNode.Value;

         ConfigWindow.mCurrentSymbolicLink = lSymbolicLink;

         Window lAreaWindow = new AreaWindow();

         lAreaWindow.Top = SelectionWindowArea.Top;

         lAreaWindow.Height = SelectionWindowArea.Height;

         lAreaWindow.Left = SelectionWindowArea.Left;

         lAreaWindow.Width = SelectionWindowArea.Width;

         lAreaWindow.ShowDialog();

         if (AreaWindow.mSelectedRegion.IsEmpty) {
            mAreaLable.Text = "Whole Area";
         } else {
            int lLeft = 0, lTop = 0, lWidth = 0, lHeight = 0;

            lLeft = ((int) AreaWindow.mSelectedRegion.X >> 1) << 1;

            lTop = ((int) AreaWindow.mSelectedRegion.Y >> 1) << 1;

            lWidth = ((int) AreaWindow.mSelectedRegion.Width >> 1) << 1;

            lHeight = ((int) AreaWindow.mSelectedRegion.Height >> 1) << 1;

            mAreaLable.Text = string.Format("Left {0}, Top {1}, Widt {2}, Height {3}", lLeft, lTop, lWidth, lHeight);
         }
      }

      private object getAudioCompressedMediaType()
      {
         object lresult = null;

         do {
            var lCLSIDEncoderMode = getAudioEncoderMode();

            var lSymbolicLink = getAudioSybolicLink();

            var lStreamIndex = Settings.Default.AudioSourceStream;

            var lMediaTypeIndex = Settings.Default.AudioSourceMediaType;

            object lSourceMediaType = null;

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(mCLSIDAudioEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            object lCompressedMediaType;

            if (!lEncoderNodeFactory.createCompressedMediaType(lSourceMediaType, lCLSIDEncoderMode, (uint) Settings.Default.AudioCompressionQuality, Settings.Default.AudioEncoderMediaType,
                                                               out lCompressedMediaType)) {
               break;
            }

            lresult = lCompressedMediaType;
         } while (false);

         return lresult;
      }

      private Guid getAudioEncoderMode()
      {
         var lEncoderMode = Guid.Empty;

         do {
            if (mEncoderControl == null) {
               break;
            }

            var lSymbolicLink = getAudioSybolicLink();

            var lStreamIndex = Settings.Default.AudioSourceStream;

            var lMediaTypeIndex = Settings.Default.AudioSourceMediaType;

            object lOutputMediaType;

            mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);

            string lMediaTypeCollection;

            if (!mEncoderControl.getMediaTypeCollectionOfEncoder(lOutputMediaType, mCLSIDAudioEncoder, out lMediaTypeCollection)) {
               break;
            }


            var lEncoderModedoc = new XmlDocument();

            lEncoderModedoc.LoadXml(lMediaTypeCollection);

            var lNodes = lEncoderModedoc.SelectNodes("EncoderMediaTypes/Group");

            var lXmlNode = lNodes.Item((int) Settings.Default.AudioEncoderMode);

            if (lXmlNode == null) {
               break;
            }

            var lEncoderModeGuidAttr = lXmlNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }

            lEncoderMode = lCLSIDEncoderMode;
         } while (false);

         return lEncoderMode;
      }

      private object getAudioSourceNode(object aOutputNode)
      {
         object lresult = null;

         do {
            var lCLSIDEncoderMode = getAudioEncoderMode();

            var lSymbolicLink = getAudioSybolicLink();

            var lStreamIndex = Settings.Default.AudioSourceStream;

            var lMediaTypeIndex = Settings.Default.AudioSourceMediaType;

            object lSourceMediaType = null;

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(mCLSIDAudioEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            object lEncoderNode;

            if (!lEncoderNodeFactory.createEncoderNode(lSourceMediaType, lCLSIDEncoderMode, (uint) Settings.Default.AudioCompressionQuality, Settings.Default.AudioEncoderMediaType, aOutputNode,
                                                       out lEncoderNode)) {
               break;
            }


            var SpreaderNode = lEncoderNode;

            object lSourceNode;

            if (!mSourceControl.createSourceNode(lSymbolicLink, lStreamIndex, lMediaTypeIndex, SpreaderNode, out lSourceNode)) {
               break;
            }

            lresult = lSourceNode;
         } while (false);

         return lresult;
      }

      private string getAudioSybolicLink()
      {
         return ConfigWindow.mAudioSymbolicLink;
      }

      private void getEncoderInfo()
      {
         var doc = new XmlDocument();

         var lxmldoc = "";

         doc = new XmlDocument();

         mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

         doc.LoadXml(lxmldoc);

         var lNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory");

         var lXmlNode = lNodes.Item((int) Settings.Default.VideoEncoderNumber);

         if (lXmlNode == null) {
            return;
         }

         var lEncoderGuidAttr = lXmlNode.Attributes["CLSID"];

         if (lEncoderGuidAttr == null) {
            return;
         }

         Guid lCLSIDEncoder;

         if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
            return;
         }

         mCLSIDVideoEncoder = lCLSIDEncoder;


         lNodes = doc.SelectNodes("EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory");

         lXmlNode = lNodes.Item((int) Settings.Default.AudioEncoderNumber);

         if (lXmlNode == null) {
            return;
         }

         lEncoderGuidAttr = lXmlNode.Attributes["CLSID"];

         if (lEncoderGuidAttr == null) {
            return;
         }

         if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
            return;
         }

         mCLSIDAudioEncoder = lCLSIDEncoder;
      }

      private XmlNode getFileFormat()
      {
         XmlNode lXmlNode = null;

         do {
            if (mEncoderControl == null) {
               break;
            }

            var lxmldoc = "";

            mCaptureManager.getCollectionOfSinks(ref lxmldoc);

            var doc = new XmlDocument();

            doc.LoadXml(lxmldoc);

            var lNodes = doc.SelectNodes("SinkFactories/SinkFactory[@GUID='{D6E342E3-7DDD-4858-AB91-4253643864C2}']/Value.ValueParts/ValuePart");

            lXmlNode = lNodes.Item((int) Settings.Default.FileFormatNumber);

            if (lXmlNode == null) {
               break;
            }
         } while (false);

         return lXmlNode;
      }

      private List<object> getOutputNodes(List<object> aCompressedMediaTypeList)
      {
         var lresult = new List<object>();

         do {
            if (aCompressedMediaTypeList == null) {
               break;
            }

            if (aCompressedMediaTypeList.Count == 0) {
               break;
            }

            var lselectedNode = getFileFormat();


            var lSelectedAttr = lselectedNode.Attributes["GUID"];

            if (lSelectedAttr == null) {
               break;
            }

            IFileSinkFactory mFileSinkFactory = null;

            mSinkControl.createSinkFactory(Guid.Parse(lSelectedAttr.Value), out mFileSinkFactory);

            if (mFileSinkFactory == null) {
               break;
            }

            lSelectedAttr = lselectedNode.Attributes["Value"];

            if (lSelectedAttr == null) {
               break;
            }

            var s = string.Format("Video_{0:yyyy_MM_dd_HH_mm_ss}.", DateTime.Now);

            var mFilename = s + lSelectedAttr.Value.ToLower();

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) delegate { mStatus.Text = "File: " + mFilename; });

            mFilename = Settings.Default.StoringDir + @"\" + mFilename;

            if (string.IsNullOrEmpty(mFilename)) {
               break;
            }

            mFileSinkFactory.createOutputNodes(aCompressedMediaTypeList, mFilename, out lresult);
         } while (false);

         return lresult;
      }

      private string getScreenCaptureSymbolicLink(string aSymbolicLink)
      {
         var loptions = " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>";

         {
            var lcursorOption = "<Option Type='Cursor' Visiblity='Temp_Visiblity'>";

            lcursorOption = lcursorOption.Replace("Temp_Visiblity", Settings.Default.ShowCursor.ToString());


            if (Settings.Default.CursorMask > 0) {
               var lshape = "";

               switch (Settings.Default.CursorMask) {
                  case 1:
                     lshape = "Rectangle";
                     break;

                  case 2:
                     lshape = "Ellipse";
                     break;
               }

               if (!string.IsNullOrEmpty(lshape)) {
                  lcursorOption += "<Option.Extensions>" + "<Extension Type='BackImage' Height='100' Width='100' Fill='Temp_Fill' Shape='Temp_Shape' />" + "</Option.Extensions>";

                  lcursorOption = lcursorOption.Replace("Temp_Shape", lshape).Replace("Temp_Fill", HexConverter(Settings.Default.MaskColor));
               }
            }

            lcursorOption += "</Option>";

            loptions += lcursorOption;
         }


         if (!AreaWindow.mSelectedRegion.IsEmpty) {
            int lLeft = 0, lTop = 0, lWidth = 0, lHeight = 0;

            lLeft = ((int) AreaWindow.mSelectedRegion.X >> 1) << 1;

            lTop = ((int) AreaWindow.mSelectedRegion.Y >> 1) << 1;

            lWidth = ((int) AreaWindow.mSelectedRegion.Width >> 1) << 1;

            lHeight = ((int) AreaWindow.mSelectedRegion.Height >> 1) << 1;

            var lcursorOption = "<Option Type='Clip'>" + "<Option.Extensions>" + "<Extension Left='Temp_Left' Top='Temp_Top' Height='Temp_Height' Width='Temp_Width'/>" + "</Option.Extensions>" +
                                "</Option>";

            loptions += lcursorOption.Replace("Temp_Left", lLeft.ToString()).Replace("Temp_Top", lTop.ToString()).Replace("Temp_Height", lHeight.ToString()).Replace("Temp_Width", lWidth.ToString());
         }

         loptions += "</Options>";

         return aSymbolicLink + loptions;
      }

      private object getVideoCompressedMediaType()
      {
         object lresult = null;

         do {
            var lCLSIDEncoderMode = getVideoEncoderMode();

            var lStreamIndex = Settings.Default.VideoSourceStream;

            var lMediaTypeIndex = Settings.Default.VideoSourceMediaType;

            object lSourceMediaType = null;

            var lSymbolicLink = getScreenCaptureSymbolicLink(m_VideoSymbolicLink);

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(mCLSIDVideoEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            object lCompressedMediaType;

            if (!lEncoderNodeFactory.createCompressedMediaType(lSourceMediaType, lCLSIDEncoderMode, (uint) Settings.Default.VideoCompressionQuality, Settings.Default.VideoEncoderMediaType,
                                                               out lCompressedMediaType)) {
               break;
            }

            lresult = lCompressedMediaType;
         } while (false);

         return lresult;
      }

      private Guid getVideoEncoderMode()
      {
         var lEncoderMode = Guid.Empty;

         do {
            if (mEncoderControl == null) {
               break;
            }

            var lStreamIndex = Settings.Default.VideoSourceStream;

            var lMediaTypeIndex = Settings.Default.VideoSourceMediaType;

            object lOutputMediaType;

            mSourceControl.getSourceOutputMediaType(m_VideoSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);

            string lMediaTypeCollection;

            if (!mEncoderControl.getMediaTypeCollectionOfEncoder(lOutputMediaType, mCLSIDVideoEncoder, out lMediaTypeCollection)) {
               break;
            }


            var lEncoderModedoc = new XmlDocument();

            lEncoderModedoc.LoadXml(lMediaTypeCollection);

            var lNodes = lEncoderModedoc.SelectNodes("EncoderMediaTypes/Group");

            var lXmlNode = lNodes.Item((int) Settings.Default.VideoEncoderMode);

            if (lXmlNode == null) {
               break;
            }

            var lEncoderModeGuidAttr = lXmlNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }

            lEncoderMode = lCLSIDEncoderMode;
         } while (false);

         return lEncoderMode;
      }

      private object getVideoSourceNode(object PreviewRenderNode, object aOutputNode)
      {
         object lresult = null;

         do {
            var lCLSIDEncoderMode = getVideoEncoderMode();

            var lStreamIndex = Settings.Default.VideoSourceStream;

            var lMediaTypeIndex = Settings.Default.VideoSourceMediaType;

            object lSourceMediaType = null;

            var lSymbolicLink = getScreenCaptureSymbolicLink(m_VideoSymbolicLink);

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(mCLSIDVideoEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            object lEncoderNode;

            if (!lEncoderNodeFactory.createEncoderNode(lSourceMediaType, lCLSIDEncoderMode, (uint) Settings.Default.VideoCompressionQuality, Settings.Default.VideoEncoderMediaType, aOutputNode,
                                                       out lEncoderNode)) {
               break;
            }


            var SpreaderNode = lEncoderNode;

            if (PreviewRenderNode != null) {
               var lOutputNodeList = new List<object>();

               lOutputNodeList.Add(PreviewRenderNode);

               lOutputNodeList.Add(lEncoderNode);

               mSpreaderNodeFactory.createSpreaderNode(lOutputNodeList, out SpreaderNode);
            }

            object lSourceNode;

            if (!mSourceControl.createSourceNode(lSymbolicLink, lStreamIndex, lMediaTypeIndex, SpreaderNode, out lSourceNode)) {
               break;
            }

            lresult = lSourceNode;
         } while (false);

         return lresult;
      }

      private string getVideoSybolicLink()
      {
         var lSourceNode = m_VideoSourceComboBox.SelectedItem as XmlNode;

         if (lSourceNode == null) {
            return "";
         }

         var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute" +
                                                  "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                                                  "/SingleValue/@Value");

         if (lNode == null) {
            return "";
         }

         return lNode.Value;
      }

      private void m_VideoSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         AreaWindow.mSelectedRegion = Rect.Empty;

         var lSymbolicLink = getVideoSybolicLink();

         Screen lselectedScreen = null;

         if (!lSymbolicLink.Contains("DISPLAY")) {
            lselectedScreen = Screen.PrimaryScreen;
         } else {
            foreach (var item in Screen.AllScreens) {
               if (lSymbolicLink.Contains(item.DeviceName)) {
                  lselectedScreen = item;

                  break;
               }
            }
         }

         SelectionWindowArea = lselectedScreen.WorkingArea;
      }

      private void MainWindow_WriteDelegateEvent(string aMessage)
      {
         MessageBox.Show(aMessage);
      }

      private void mStartStop_Click(object sender, RoutedEventArgs e)
      {
         m_VideoSymbolicLink = getVideoSybolicLink();

         var l_is_VideoStreamPreview = (bool) m_VideoStreamPreviewChkBtn.IsChecked;

         mStartStop.IsEnabled = false;

         var t = new Thread(delegate()
         {
            if (mState == State.Started) {
               mISession.stopSession();

               mISession.closeSession();

               mISession = null;

               mState = State.Stopped;

               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) delegate
               {
                  mStartStop.Content = "Start";

                  mStartStop.IsEnabled = true;
               });

               return;
            }

            getEncoderInfo();

            var lCompressedMediaTypeList = new List<object>();

            var lCompressedMediaType = getVideoCompressedMediaType();

            if (lCompressedMediaType != null) {
               lCompressedMediaTypeList.Add(lCompressedMediaType);
            }

            lCompressedMediaType = getAudioCompressedMediaType();

            if (lCompressedMediaType != null) {
               lCompressedMediaTypeList.Add(lCompressedMediaType);
            }

            var lOutputNodes = getOutputNodes(lCompressedMediaTypeList);

            if ((lOutputNodes == null) ||
                (lOutputNodes.Count == 0)) {
               return;
            }

            var lOutputIndex = 0;

            var lSourceNodes = new List<object>();


            object RenderNode = null;

            if (l_is_VideoStreamPreview) {
               var lRenderOutputNodesList = new List<object>();

               if (mEVRMultiSinkFactory != null) {
                  mEVRMultiSinkFactory.createOutputNodes(m_EVRDisplay.Handle, null, 1, out lRenderOutputNodesList);
               }

               if (lRenderOutputNodesList.Count == 1) {
                  RenderNode = lRenderOutputNodesList[0];
               }
            }


            var lSourceNode = getVideoSourceNode(RenderNode, lOutputNodes[lOutputIndex++]);

            if (lSourceNodes != null) {
               lSourceNodes.Add(lSourceNode);
            }


            lSourceNode = getAudioSourceNode(lOutputNodes[lOutputIndex++]);

            if (lSourceNodes != null) {
               lSourceNodes.Add(lSourceNode);
            }

            mISession = mISessionControl.createSession(lSourceNodes.ToArray());

            if (mISession == null) {
               return;
            }

            if (mISession.startSession(0, Guid.Empty)) {
               mState = State.Started;

               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) delegate
               {
                  mStartStop.Content = "Stop";

                  mStartStop.IsEnabled = true;
               });
            }
         });
         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         try {
            mCaptureManager = Program.mCaptureManager;

            App.mHotKeyHost = new HotKeyHost((HwndSource) PresentationSource.FromVisual(Application.Current.MainWindow));

            App.mHotKeyHost.AddHotKey(new CustomHotKey("Record",
                                                       () => { Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) delegate { mStartStop_Click(null, null); }); },
                                                       Key.P, ModifierKeys.Control | ModifierKeys.Shift, true));
         } catch (Exception) {
            try {
               mCaptureManager = new CaptureManager();
            } catch (Exception) {
            }
         }

         LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

         if (mCaptureManager == null) {
            return;
         }


         var t = new Thread(delegate()
         {
            try {
               mSourceControl = mCaptureManager.createSourceControl();

               if (mSourceControl == null) {
                  return;
               }

               mEncoderControl = mCaptureManager.createEncoderControl();

               if (mEncoderControl == null) {
                  return;
               }

               mSinkControl = mCaptureManager.createSinkControl();

               if (mSinkControl == null) {
                  return;
               }

               mISessionControl = mCaptureManager.createSessionControl();

               if (mISessionControl == null) {
                  return;
               }

               mStreamControl = mCaptureManager.createStreamControl();

               if (mStreamControl == null) {
                  return;
               }

               mStreamControl.createStreamControlNodeFactory(ref mSpreaderNodeFactory);

               if (mSpreaderNodeFactory == null) {
                  return;
               }

               mSinkControl.createSinkFactory(Guid.Empty, out mEVRMultiSinkFactory);

               if (mEVRMultiSinkFactory == null) {
                  return;
               }


               var lXmlDataProvider = (XmlDataProvider) Resources["XmlSources"];

               if (lXmlDataProvider == null) {
                  return;
               }

               var doc = new XmlDocument();

               var lxmldoc = "";

               mCaptureManager.getCollectionOfSources(ref lxmldoc);

               doc.LoadXml(lxmldoc);

               lXmlDataProvider.Document = doc;

               lXmlDataProvider = (XmlDataProvider) Resources["XmlEncoders"];

               if (lXmlDataProvider == null) {
                  return;
               }

               doc = new XmlDocument();

               mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

               doc.LoadXml(lxmldoc);

               lXmlDataProvider.Document = doc;


               mCaptureManager.getCollectionOfSinks(ref lxmldoc);


               lXmlDataProvider = (XmlDataProvider) Resources["XmlContainerTypeProvider"];

               if (lXmlDataProvider == null) {
                  return;
               }

               doc = new XmlDocument();

               doc.LoadXml(lxmldoc);

               lXmlDataProvider.Document = doc;
            } catch (Exception) {
            }
         });
         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
      }

      #endregion
   }
}
