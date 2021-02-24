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
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;

namespace WPFPauseResumeRecording
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

      private CaptureManager mCaptureManager;

      private IEncoderControl mEncoderControl;

      private IEVRMultiSinkFactory mEVRMultiSinkFactory;

      private string mFilename;

      private IFileSinkFactory mFileSinkFactory;

      private ISession mISession;

      private ISessionControl mISessionControl;

      private State mRecordingState = State.Stopped;

      private ISinkControl mSinkControl;

      private ISourceControl mSourceControl;

      private ISpreaderNodeFactory mSpreaderNodeFactory;

      private State mState = State.Stopped;

      private IStreamControl mStreamControl;

      private ISwitcherControl mSwitcherControl;

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

      private object getCompressedMediaType(XmlNode aSourceNode, XmlNode aStreamNode, XmlNode aMediaTypeNode, XmlNode aEncoderNode, XmlNode aEncoderModeNode, int aCompressedMediaTypeIndex)
      {
         object lresult = null;

         do {
            if (aCompressedMediaTypeIndex < 0) {
               break;
            }


            if (aSourceNode == null) {
               break;
            }


            if (aStreamNode == null) {
               break;
            }


            if (aMediaTypeNode == null) {
               break;
            }


            if (aEncoderNode == null) {
               break;
            }


            if (aEncoderModeNode == null) {
               break;
            }

            var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
               break;
            }

            var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }


            if (aSourceNode == null) {
               break;
            }

            var lNode = aSourceNode.SelectSingleNode("Source.Attributes/Attribute" +
                                                     "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                                                     "/SingleValue/@Value");

            if (lNode == null) {
               break;
            }

            var lSymbolicLink = lNode.Value;

            if (aStreamNode == null) {
               break;
            }

            lNode = aStreamNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
               break;
            }

            if (aMediaTypeNode == null) {
               break;
            }

            lNode = aMediaTypeNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }

            object lSourceMediaType = null;

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            object lCompressedMediaType;

            if (!lEncoderNodeFactory.createCompressedMediaType(lSourceMediaType, lCLSIDEncoderMode, 50, (uint) aCompressedMediaTypeIndex, out lCompressedMediaType)) {
               break;
            }

            lresult = lCompressedMediaType;
         } while (false);

         return lresult;
      }

      private object getEncoder(XmlNode aSourceNode, XmlNode aStreamNode, XmlNode aMediaTypeNode, XmlNode aEncoderNode, XmlNode aEncoderModeNode, int aCompressedMediaTypeIndex, object aOutputNode)
      {
         object lresult = null;

         do {
            if (aCompressedMediaTypeIndex < 0) {
               break;
            }


            if (aSourceNode == null) {
               break;
            }


            if (aStreamNode == null) {
               break;
            }


            if (aMediaTypeNode == null) {
               break;
            }


            if (aEncoderNode == null) {
               break;
            }


            if (aEncoderModeNode == null) {
               break;
            }

            var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
               break;
            }

            var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }


            if (aSourceNode == null) {
               break;
            }

            var lNode = aSourceNode.SelectSingleNode("Source.Attributes/Attribute" +
                                                     "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                                                     "/SingleValue/@Value");

            if (lNode == null) {
               break;
            }

            var lSymbolicLink = lNode.Value;

            if (aStreamNode == null) {
               break;
            }

            lNode = aStreamNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
               break;
            }

            if (aMediaTypeNode == null) {
               break;
            }

            lNode = aMediaTypeNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }

            object lSourceMediaType = null;

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            object lEncoderNode;

            if (!lEncoderNodeFactory.createEncoderNode(lSourceMediaType, lCLSIDEncoderMode, 50, (uint) aCompressedMediaTypeIndex, aOutputNode, out lEncoderNode)) {
               break;
            }

            lresult = lEncoderNode;
         } while (false);

         return lresult;
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

            if (mFileSinkFactory == null) {
               break;
            }

            if (string.IsNullOrEmpty(mFilename)) {
               break;
            }

            mFileSinkFactory.createOutputNodes(aCompressedMediaTypeList, mFilename, out lresult);
         } while (false);

         return lresult;
      }

      private object getSourceNode(XmlNode aSourceNode,
                                   XmlNode aStreamNode,
                                   XmlNode aMediaTypeNode,
                                   XmlNode aEncoderNode,
                                   XmlNode aEncoderModeNode,
                                   int aCompressedMediaTypeIndex,
                                   object PreviewRenderNode,
                                   object aSwitcherNode)
      {
         object lresult = null;

         do {
            if (aCompressedMediaTypeIndex < 0) {
               break;
            }


            if (aSourceNode == null) {
               break;
            }


            if (aStreamNode == null) {
               break;
            }


            if (aMediaTypeNode == null) {
               break;
            }


            if (aEncoderNode == null) {
               break;
            }


            if (aEncoderModeNode == null) {
               break;
            }

            var lEncoderGuidAttr = aEncoderNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
               break;
            }

            var lEncoderModeGuidAttr = aEncoderModeNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }


            if (aSourceNode == null) {
               break;
            }

            var lNode = aSourceNode.SelectSingleNode("Source.Attributes/Attribute" +
                                                     "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' or @Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" +
                                                     "/SingleValue/@Value");

            if (lNode == null) {
               break;
            }

            var lSymbolicLink = lNode.Value;

            if (aStreamNode == null) {
               break;
            }

            lNode = aStreamNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
               break;
            }

            if (aMediaTypeNode == null) {
               break;
            }

            lNode = aMediaTypeNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }

            object lSourceMediaType = null;

            if (!mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lSourceMediaType)) {
               break;
            }

            if (lSourceMediaType == null) {
               break;
            }

            IEncoderNodeFactory lEncoderNodeFactory;

            if (!mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lEncoderNodeFactory)) {
               break;
            }

            if (lEncoderNodeFactory == null) {
               break;
            }

            var SpreaderNode = aSwitcherNode;

            if (PreviewRenderNode != null) {
               var lOutputNodeList = new List<object>();

               lOutputNodeList.Add(PreviewRenderNode);

               lOutputNodeList.Add(aSwitcherNode);

               mSpreaderNodeFactory.createSpreaderNode(lOutputNodeList, out SpreaderNode);
            }

            object lSourceNode;

            var lextendSymbolicLink = lSymbolicLink + " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>" + "<Option Type='Cursor' Visiblity='True'>" + "<Option.Extensions>" +
                                      "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" + "</Option.Extensions>" + "</Option>" + "</Options>";

            if (!mSourceControl.createSourceNode(lextendSymbolicLink, lStreamIndex, lMediaTypeIndex, SpreaderNode, out lSourceNode)) {
               break;
            }

            lresult = lSourceNode;
         } while (false);

         return lresult;
      }

      private object getSwitcher(object aEncoderNode)
      {
         object lresult = null;

         do {
            if (aEncoderNode == null) {
               break;
            }

            ISwitcherNodeFactory lSwitcherNodeFactory = null;

            if (!mStreamControl.createStreamControlNodeFactory(ref lSwitcherNodeFactory)) {
               break;
            }

            if (lSwitcherNodeFactory == null) {
               break;
            }

            lSwitcherNodeFactory.createSwitcherNode(aEncoderNode, out lresult);
         } while (false);

         return lresult;
      }

      private void m_PauseBtn_Click(object sender, RoutedEventArgs e)
      {
         if ((mState == State.Started) &&
             (mISession != null)) {
            if (mRecordingState == State.Paused) {
               m_PauseBtn.Content = "Pause Recording";

               mRecordingState = State.Started;

               mSwitcherControl.resumeSwitchers(mISession);

               mStateLabel.Text = "Recording";
            } else {
               m_PauseBtn.Content = "Resume Recording";

               mSwitcherControl.pauseSwitchers(mISession);

               mRecordingState = State.Paused;

               mStateLabel.Text = "Paused";
            }

            m_StopBtn.IsEnabled = true;

            mStateLabel.Visibility = Visibility.Visible;
         }
      }

      private void m_SelectFileBtn_Click(object sender, RoutedEventArgs e)
      {
         do {
            var lselectedNode = m_FileFormatComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lSelectedAttr = lselectedNode.Attributes["Value"];

            if (lSelectedAttr == null) {
               break;
            }

            var limageSourceDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var lsaveFileDialog = new SaveFileDialog();

            lsaveFileDialog.InitialDirectory = limageSourceDir;

            lsaveFileDialog.DefaultExt = "." + lSelectedAttr.Value.ToLower();

            lsaveFileDialog.AddExtension = true;

            lsaveFileDialog.CheckFileExists = false;

            lsaveFileDialog.Filter = "Media file (*." + lSelectedAttr.Value.ToLower() + ")|*." + lSelectedAttr.Value.ToLower();

            var lresult = lsaveFileDialog.ShowDialog();

            if (lresult != true) {
               break;
            }

            mFilename = lsaveFileDialog.FileName;

            lSelectedAttr = lselectedNode.Attributes["GUID"];

            if (lSelectedAttr == null) {
               break;
            }

            mSinkControl.createSinkFactory(Guid.Parse(lSelectedAttr.Value), out mFileSinkFactory);

            m_StartStopBtn.IsEnabled = true;
         } while (false);
      }

      private void m_StartBtn_Click(object sender, RoutedEventArgs e)
      {
         if ((mState == State.Paused) &&
             (mISession != null)) {
            mSwitcherControl.resumeSwitchers(mISession);

            mState = State.Started;

            m_PauseBtn.IsEnabled = true;

            m_StopBtn.IsEnabled = true;

            m_StartStopBtn.IsEnabled = false;

            return;
         }


         var lCompressedMediaTypeList = new List<object>();


         var lCompressedMediaType = getCompressedMediaType(m_VideoSourceComboBox.SelectedItem as XmlNode, m_VideoStreamComboBox.SelectedItem as XmlNode,
                                                           m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode, m_VideoEncodersComboBox.SelectedItem as XmlNode,
                                                           m_VideoEncodingModeComboBox.SelectedItem as XmlNode, m_VideoCompressedMediaTypesComboBox.SelectedIndex);

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


         var lRenderOutputNodesList = new List<object>();

         if (mEVRMultiSinkFactory != null) {
            mEVRMultiSinkFactory.createOutputNodes(IntPtr.Zero, m_EVRDisplay.Surface.texture, 1, out lRenderOutputNodesList);
         }

         if (lRenderOutputNodesList.Count == 1) {
            RenderNode = lRenderOutputNodesList[0];
         }

         var lEncoderNode = getEncoder(m_VideoSourceComboBox.SelectedItem as XmlNode, m_VideoStreamComboBox.SelectedItem as XmlNode, m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                                       m_VideoEncodersComboBox.SelectedItem as XmlNode, m_VideoEncodingModeComboBox.SelectedItem as XmlNode, m_VideoCompressedMediaTypesComboBox.SelectedIndex,
                                       lOutputNodes[lOutputIndex++]);

         if (lEncoderNode == null) {
            return;
         }

         var lSwitcherNode = getSwitcher(lEncoderNode);

         if (lSwitcherNode == null) {
            return;
         }

         var lSourceNode = getSourceNode(m_VideoSourceComboBox.SelectedItem as XmlNode, m_VideoStreamComboBox.SelectedItem as XmlNode, m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                                         m_VideoEncodersComboBox.SelectedItem as XmlNode, m_VideoEncodingModeComboBox.SelectedItem as XmlNode, m_VideoCompressedMediaTypesComboBox.SelectedIndex,
                                         RenderNode, lSwitcherNode);

         if (lSourceNodes != null) {
            lSourceNodes.Add(lSourceNode);
         }

         mISession = mISessionControl.createSession(lSourceNodes.ToArray());

         if (mISession == null) {
            return;
         }

         mSwitcherControl.pauseSwitchers(mISession);

         if (mISession.startSession(0, Guid.Empty)) {
            mState = State.Started;

            mRecordingState = State.Paused;

            mStateLabel.Visibility = Visibility.Collapsed;

            m_PauseBtn.IsEnabled = true;

            m_StopBtn.IsEnabled = true;

            m_StartStopBtn.IsEnabled = false;
         }
      }

      private void m_StopBtn_Click(object sender, RoutedEventArgs e)
      {
         if (mState != State.Stopped) {
            mState = State.Stopped;

            if (mISession == null) {
               return;
            }

            mStateLabel.Visibility = Visibility.Collapsed;

            mISession.stopSession();

            mISession.closeSession();

            mISession = null;


            m_PauseBtn.IsEnabled = false;

            m_StopBtn.IsEnabled = false;

            m_StartStopBtn.IsEnabled = true;

            m_PauseBtn.Content = "Start recording";
         }
      }

      private void m_VideoEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         do {
            if (mEncoderControl == null) {
               break;
            }

            var lselectedNode = m_VideoEncodersComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

            if (lCLSIDEncoderAttr == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder)) {
               break;
            }


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

            lSourceNode = m_VideoStreamComboBox.SelectedItem as XmlNode;

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

            lSourceNode = m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode;

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


            object lOutputMediaType;

            if (mSourceControl == null) {
               return;
            }

            mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);

            string lMediaTypeCollection;

            if (!mEncoderControl.getMediaTypeCollectionOfEncoder(lOutputMediaType, lCLSIDEncoder, out lMediaTypeCollection)) {
               break;
            }


            var lXmlEncoderModeDataProvider = (XmlDataProvider) Resources["XmlEncoderModeProvider"];

            if (lXmlEncoderModeDataProvider == null) {
               return;
            }

            var lEncoderModedoc = new XmlDocument();

            lEncoderModedoc.LoadXml(lMediaTypeCollection);

            lXmlEncoderModeDataProvider.Document = lEncoderModedoc;
         } while (false);
      }

      private void MainWindow_WriteDelegateEvent(string aMessage)
      {
         MessageBox.Show(aMessage);
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         try {
            mCaptureManager = new CaptureManager("CaptureManager.dll");
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

         mSwitcherControl = mCaptureManager.createSwitcherControl();

         if (mSwitcherControl == null) {
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
      }

      #endregion
   }
}
