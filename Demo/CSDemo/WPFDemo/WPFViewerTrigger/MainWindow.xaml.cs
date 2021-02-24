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
using System.Windows.Data;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFViewerTrigger
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
      }

      #endregion

      #region  Fields

      private IEVRMultiSinkFactory mEVRMultiSinkFactory;

      private ISession mISession;

      private ISessionControl mISessionControl;

      private bool mIsPausedDisplay;

      private ISinkControl mSinkControl;

      private ISourceControl mSourceControl;

      private ISpreaderNodeFactory mSpreaderNodeFactory;

      private IStreamControl mStreamControl;

      private ISwitcherControl mSwitcherControl;

      #endregion

      #region Private methods

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         var lThumbnailOutputNode = m_Thumbnail.init(m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode);

         if (lThumbnailOutputNode == null) {
            return;
         }

         var lSwitcherNode = getSwitcher();

         if (lSwitcherNode == null) {
            return;
         }

         var SpreaderNode = lSwitcherNode;


         var lOutputNodeList = new List<object>();

         lOutputNodeList.Add(lSwitcherNode);

         lOutputNodeList.Add(lThumbnailOutputNode);

         mSpreaderNodeFactory.createSpreaderNode(lOutputNodeList, out SpreaderNode);

         var lSourceNode = getSourceNode(m_VideoSourceComboBox.SelectedItem as XmlNode, m_VideoStreamComboBox.SelectedItem as XmlNode, m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode,
                                         SpreaderNode);

         var lSourceNodes = new List<object>();

         lSourceNodes.Add(lSourceNode);

         mISession = mISessionControl.createSession(lSourceNodes.ToArray());

         if (mISession == null) {
            return;
         }

         mSwitcherControl.pauseSwitchers(mISession);

         mIsPausedDisplay = true;

         if (mISession.startSession(0, Guid.Empty)) {
            m_Thumbnail.start();
         }
      }

      private object getSourceNode(XmlNode aSourceNode, XmlNode aStreamNode, XmlNode aMediaTypeNode, object aOutputNode)
      {
         object lresult = null;

         do {
            if (aSourceNode == null) {
               break;
            }


            if (aStreamNode == null) {
               break;
            }


            if (aMediaTypeNode == null) {
               break;
            }


            if (aOutputNode == null) {
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

            object lSourceNode;

            var lextendSymbolicLink = lSymbolicLink + " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>" + "<Option Type='Cursor' Visiblity='True'>" + "<Option.Extensions>" +
                                      "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" + "</Option.Extensions>" + "</Option>" + "</Options>";

            if (!mSourceControl.createSourceNode(lextendSymbolicLink, lStreamIndex, lMediaTypeIndex, aOutputNode, out lSourceNode)) {
               break;
            }

            lresult = lSourceNode;
         } while (false);

         return lresult;
      }

      private object getSwitcher()
      {
         object lresult = null;

         do {
            object RenderNode = null;

            var lRenderOutputNodesList = new List<object>();

            if (mEVRMultiSinkFactory != null) {
               mEVRMultiSinkFactory.createOutputNodes(IntPtr.Zero, m_EVRDisplay.Surface.texture, 1, out lRenderOutputNodesList);
            }

            if (lRenderOutputNodesList.Count == 1) {
               RenderNode = lRenderOutputNodesList[0];
            }

            ISwitcherNodeFactory lSwitcherNodeFactory = null;

            if (!mStreamControl.createStreamControlNodeFactory(ref lSwitcherNodeFactory)) {
               break;
            }

            if (lSwitcherNodeFactory == null) {
               break;
            }

            lSwitcherNodeFactory.createSwitcherNode(RenderNode, out lresult);
         } while (false);

         return lresult;
      }

      private void m_Thumbnail_mChangeState(bool isEnable)
      {
         if (isEnable) {
            mSwitcherControl.resumeSwitchers(mISession);
         } else {
            mSwitcherControl.pauseSwitchers(mISession);
         }
      }

      private void MainWindow_WriteDelegateEvent(string aMessage)
      {
         MessageBox.Show(aMessage);
      }

      private void mPauseResumeDisplayBtn_Click(object sender, RoutedEventArgs e)
      {
         if (mISession != null) {
            if (mIsPausedDisplay) {
               mSwitcherControl.resumeSwitchers(mISession);

               //m_EVRDisplay.Visibility = System.Windows.Visibility.Visible;

               mPauseResumeDisplayBtn.Content = "Pause Display";
            } else {
               mSwitcherControl.pauseSwitchers(mISession);

               //m_EVRDisplay.Visibility = System.Windows.Visibility.Hidden;

               mPauseResumeDisplayBtn.Content = "Resume Display";
            }

            mIsPausedDisplay = !mIsPausedDisplay;
         }
      }

      private void mTrigger_Click(object sender, RoutedEventArgs e)
      {
         m_Thumbnail.mEnableTrigger = (bool) mTrigger.IsChecked;
      }

      private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         var lSlider = (Slider) sender;

         if (lSlider != null) {
            m_Thumbnail.mThreshold = (float) lSlider.Value * 100.0f;
         }
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

         m_Thumbnail.mChangeState += m_Thumbnail_mChangeState;

         LogManager.getInstance().WriteDelegateEvent += MainWindow_WriteDelegateEvent;

         if (mCaptureManager == null) {
            return;
         }

         mSourceControl = mCaptureManager.createSourceControl();

         if (mSourceControl == null) {
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
      }

      #endregion
   }
}
