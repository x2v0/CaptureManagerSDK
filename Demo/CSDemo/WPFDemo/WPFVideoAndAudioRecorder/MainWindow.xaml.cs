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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;

namespace WPFVideoAndAudioRecorder
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

      private IEVRSinkFactory mEVRSinkFactory;

      private string mFilename;

      private IFileSinkFactory mFileSinkFactory;

      private ISession mISession;

      private ISessionControl mISessionControl;

      private bool mIsStarted;

      private ISinkControl mSinkControl;

      private ISourceControl mSourceControl;

      private ISpreaderNodeFactory mSpreaderNodeFactory;

      private IStreamControl mStreamControl;

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
                                   object aOutputNode)
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


            var SpreaderNode = lEncoderNode;

            if (PreviewRenderNode != null) {
               var lOutputNodeList = new List<object>();

               //lOutputNodeList.Add(lEncoderNode);

               //lOutputNodeList.Add(PreviewRenderNode);

               lOutputNodeList.Add(PreviewRenderNode);

               lOutputNodeList.Add(lEncoderNode);

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

      private void m_AudioEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         do {
            if (mEncoderControl == null) {
               break;
            }

            var lselectedNode = m_AudioEncodersComboBox.SelectedItem as XmlNode;

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


            var lSourceNode = m_AudioSourceComboBox.SelectedItem as XmlNode;

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

            lSourceNode = m_AudioStreamComboBox.SelectedItem as XmlNode;

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

            lSourceNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;

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

            var t = new Thread(delegate()
            {
               try {
                  mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);

                  string lMediaTypeCollection;

                  if (!mEncoderControl.getMediaTypeCollectionOfEncoder(lOutputMediaType, lCLSIDEncoder, out lMediaTypeCollection)) {
                     return;
                  }


                  var lXmlEncoderModeDataProvider = (XmlDataProvider) Resources["XmlAudioEncoderModeProvider"];

                  if (lXmlEncoderModeDataProvider == null) {
                     return;
                  }

                  var lEncoderModedoc = new XmlDocument();

                  lEncoderModedoc.LoadXml(lMediaTypeCollection);

                  lXmlEncoderModeDataProvider.Document = lEncoderModedoc;
               } catch (Exception ex) {
               }
            });
            t.SetApartmentState(ApartmentState.MTA);

            t.Start();
         } while (false);
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

            var t = new Thread(delegate()
            {
               try {
                  mSinkControl.createSinkFactory(Guid.Parse(lSelectedAttr.Value), out mFileSinkFactory);
               } catch (Exception ex) {
               }
            });
            t.SetApartmentState(ApartmentState.MTA);

            t.Start();

            m_StartStopBtn.IsEnabled = true;
         } while (false);
      }

      private void m_StartStopBtn_Click(object sender, RoutedEventArgs e)
      {
         m_StartStopBtn.IsEnabled = false;

         if (mIsStarted) {
            mIsStarted = false;

            var tl = new Thread(delegate()
            {
               try {
                  if (mISession == null) {
                     return;
                  }

                  mISession.stopSession();

                  mISession.closeSession();

                  mISession = null;

                  Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                  {
                     m_BtnTxtBlk.Text = "Start";

                     m_StartStopBtn.IsEnabled = true;
                  }));
               } catch (Exception ex) {
               }
            });
            tl.SetApartmentState(ApartmentState.MTA);

            tl.Start();

            return;
         }

         var l_videoStreamEnabled = (bool) m_VideoStreamChkBtn.IsChecked && (m_VideoCompressedMediaTypesComboBox.SelectedIndex > -1);

         var l_previewEnabled = (bool) m_VideoStreamPreviewChkBtn.IsChecked;


         var l_VideoSourceXmlNode = m_VideoSourceComboBox.SelectedItem as XmlNode;
         var l_VideoStreamXmlNode = m_VideoStreamComboBox.SelectedItem as XmlNode;
         var l_VideoSourceMediaTypeXmlNode = m_VideoSourceMediaTypeComboBox.SelectedItem as XmlNode;
         var l_VideoEncodersXmlNode = m_VideoEncodersComboBox.SelectedItem as XmlNode;
         var l_VideoEncodingModeXmlNode = m_VideoEncodingModeComboBox.SelectedItem as XmlNode;
         var l_VideoCompressedMediaTypeSelectedIndex = m_VideoCompressedMediaTypesComboBox.SelectedIndex;


         var l_audioStreamEnabled = (bool) m_AudioStreamChkBtn.IsChecked && (m_AudioCompressedMediaTypesComboBox.SelectedIndex > -1);


         var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;
         var l_AudioStreamXmlNode = m_AudioStreamComboBox.SelectedItem as XmlNode;
         var l_AudioSourceMediaTypeXmlNode = m_AudioSourceMediaTypeComboBox.SelectedItem as XmlNode;
         var l_AudioEncodersXmlNode = m_AudioEncodersComboBox.SelectedItem as XmlNode;
         var l_AudioEncodingModeXmlNode = m_AudioEncodingModeComboBox.SelectedItem as XmlNode;
         var l_AudioCompressedMediaTypeSelectedIndexXmlNode = m_AudioCompressedMediaTypesComboBox.SelectedIndex;

         var lHandle = m_EVRDisplay.Handle;


         object RenderNode = null;

         if (l_previewEnabled) {
            //List<object> lRenderOutputNodesList = new List<object>();

            //if (mEVRMultiSinkFactory != null)
            //    mEVRMultiSinkFactory.createOutputNodes(
            //        lHandle,
            //        1,
            //        out lRenderOutputNodesList);

            //if (lRenderOutputNodesList.Count == 1)
            //{
            //    RenderNode = lRenderOutputNodesList[0];
            //}


            if (mEVRSinkFactory != null) {
               mEVRSinkFactory.createOutputNode(lHandle, out RenderNode);
            }
         }

         var t = new Thread(delegate()
         {
            try {
               var lCompressedMediaTypeList = new List<object>();

               if (l_videoStreamEnabled) {
                  var lCompressedMediaType = getCompressedMediaType(l_VideoSourceXmlNode, l_VideoStreamXmlNode, l_VideoSourceMediaTypeXmlNode, l_VideoEncodersXmlNode, l_VideoEncodingModeXmlNode,
                                                                    l_VideoCompressedMediaTypeSelectedIndex);

                  if (lCompressedMediaType != null) {
                     lCompressedMediaTypeList.Add(lCompressedMediaType);
                  }
               }

               if (l_audioStreamEnabled) {
                  var lCompressedMediaType = getCompressedMediaType(l_AudioSourceXmlNode, l_AudioStreamXmlNode, l_AudioSourceMediaTypeXmlNode, l_AudioEncodersXmlNode, l_AudioEncodingModeXmlNode,
                                                                    l_AudioCompressedMediaTypeSelectedIndexXmlNode);

                  if (lCompressedMediaType != null) {
                     lCompressedMediaTypeList.Add(lCompressedMediaType);
                  }
               }

               var lOutputNodes = getOutputNodes(lCompressedMediaTypeList);

               if ((lOutputNodes == null) ||
                   (lOutputNodes.Count == 0)) {
                  return;
               }


               var lOutputIndex = 0;

               var lSourceNodes = new List<object>();

               if (l_videoStreamEnabled) {
                  var lSourceNode = getSourceNode(l_VideoSourceXmlNode, l_VideoStreamXmlNode, l_VideoSourceMediaTypeXmlNode, l_VideoEncodersXmlNode, l_VideoEncodingModeXmlNode,
                                                  l_VideoCompressedMediaTypeSelectedIndex, RenderNode, lOutputNodes[lOutputIndex++]);

                  if (lSourceNodes != null) {
                     lSourceNodes.Add(lSourceNode);
                  }
               }

               if (l_audioStreamEnabled) {
                  var lSourceNode = getSourceNode(l_AudioSourceXmlNode, l_AudioStreamXmlNode, l_AudioSourceMediaTypeXmlNode, l_AudioEncodersXmlNode, l_AudioEncodingModeXmlNode,
                                                  l_AudioCompressedMediaTypeSelectedIndexXmlNode, null, lOutputNodes[lOutputIndex++]);

                  if (lSourceNodes != null) {
                     lSourceNodes.Add(lSourceNode);
                  }
               }

               mISession = mISessionControl.createSession(lSourceNodes.ToArray());

               if (mISession == null) {
                  return;
               }

               if (mISession.startSession(0, Guid.Empty)) {
                  Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                  {
                     m_BtnTxtBlk.Text = "Stop";

                     m_StartStopBtn.IsEnabled = true;
                  }));
               }

               mIsStarted = true;
            } catch (Exception ex) {
            }
         });
         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
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

            var t = new Thread(delegate()
            {
               try {
                  Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { m_VideoEncodingModeComboBox.IsEnabled = false; }));

                  mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);

                  string lMediaTypeCollection;

                  if (!mEncoderControl.getMediaTypeCollectionOfEncoder(lOutputMediaType, lCLSIDEncoder, out lMediaTypeCollection)) {
                     return;
                  }


                  var lXmlEncoderModeDataProvider = (XmlDataProvider) Resources["XmlEncoderModeProvider"];

                  if (lXmlEncoderModeDataProvider == null) {
                     return;
                  }

                  var lEncoderModedoc = new XmlDocument();

                  lEncoderModedoc.LoadXml(lMediaTypeCollection);

                  lXmlEncoderModeDataProvider.Document = lEncoderModedoc;


                  Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => { m_VideoEncodingModeComboBox.IsEnabled = true; }));
               } catch (Exception ex) {
               }
            });
            t.SetApartmentState(ApartmentState.MTA);

            t.Start();
         } while (false);
      }

      private void MainWindow_WriteDelegateEvent(string aMessage)
      {
         MessageBox.Show(aMessage);
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         var t = new Thread(delegate()
         {
            try {
               mCaptureManager = Program.mCaptureManager;

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

               mSinkControl.createSinkFactory(Guid.Empty, out mEVRSinkFactory);

               if (mEVRSinkFactory == null) {
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
            } catch (Exception ex) {
            }
         });
         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
      }

      #endregion

      //      private void m_StartBtn_Click(object sender, RoutedEventArgs e)
      //      {
      //          if(m_StartBtn.Content == "Stop")
      //          {
      //              m_StartBtn.Content = "Start";

      //              if(mISession != null)
      //              {
      //                  mISession.stopSession();

      //                  mISession.closeSession();
      //              }

      //              return;
      //          }

      //          string lAudioSourceSymbolicLink = "CaptureManager///Software///Sources///AudioEndpointCapture///AudioLoopBack";

      //          uint lAudioSourceIndexStream = 0;

      //          uint lAudioSourceIndexMediaType = 0;

      //          object lAudioStreamMediaType = null;

      //          if (!mSourceControl.getSourceOutputMediaType(
      //              lAudioSourceSymbolicLink,
      //              lAudioSourceIndexStream,
      //              lAudioSourceIndexMediaType,
      //              out lAudioStreamMediaType))
      //              return;

      //          if (lAudioStreamMediaType == null)
      //              return;


      //          Guid lAACEncoder = new Guid("93AF0C51-2275-45D2-A35B-F2BA21CAED00");

      //          string gh;

      //          //mEncoderControl.getMediaTypeCollectionOfEncoder(lAudioStreamMediaType, lAACEncoder, out gh);

      //          IEncoderNodeFactory lAACEncoderNodeFactory;

      //          if (!mEncoderControl.createEncoderNodeFactory(
      //              lAACEncoder,
      //              out lAACEncoderNodeFactory))
      //              return;

      //          if (lAACEncoderNodeFactory == null)
      //              return;

      //          Guid lConstantEncoderMode = new Guid("CA37E2BE-BEC0-4B17-946D-44FBC1B3DF55");

      //          object lAudioCompressedMediaType;

      //          if(!lAACEncoderNodeFactory.createCompressedMediaType(
      //              lAudioStreamMediaType,
      //              lConstantEncoderMode,
      //              75,
      //              0,
      //              out lAudioCompressedMediaType))
      //              return;

      //          if (lAudioCompressedMediaType == null)
      //              return;


      //          string hh = "";

      //          //mCaptureManager.getCollectionOfSinks(ref hh);

      //          Guid lSink = new Guid("E80A6BFD-D9C2-4A1F-95DC-14685CACEF3E"); //  MP4

      ////          <ValuePart Title="Container format" Value="MP4" MIME="video/mp4" Description="MPEG4 Media Container" MaxPortCount="2" GUID="{E80A6BFD-D9C2-4A1F-95DC-14685CACEF3E}" /> 
      ////<ValuePart Title="Container format" Value="AVI" MIME="" Description="AVI Media Container" MaxPortCount="12" GUID="{27F54603-A081-491C-91EE-0B1F2525813C}" /> 
      ////<ValuePart Title="Container format" Value="MKV" MIME="" Description="MKV Media Container" MaxPortCount="2" GUID="{1A4D4135-0712-47DF-B83C-2481A7A9CFA2}" /> 

      //          IFileSinkFactory lFileSinkFactory;

      //          mSinkControl.createSinkFactory(lSink, out lFileSinkFactory);

      //          if (lFileSinkFactory == null)
      //              return;

      //          List<object> lcompressedmediaTypes = new List<object>();

      //          lcompressedmediaTypes.Add(lAudioCompressedMediaType);

      //          List<object> loutputNodes = new List<object>();

      //          lFileSinkFactory.createOutputNodes(lcompressedmediaTypes, "video.mp4", out loutputNodes);

      //          object lAudioEncoderNode;

      //          if (!lAACEncoderNodeFactory.createEncoderNode(
      //              lAudioStreamMediaType,
      //              lConstantEncoderMode,
      //              75,
      //              0,
      //              loutputNodes[0],
      //              out lAudioEncoderNode))
      //              return;

      //          if (lAudioEncoderNode == null)
      //              return;

      //          object lAudioSourceNode = null;

      //          if (!mSourceControl.createSourceNode(
      //              lAudioSourceSymbolicLink,
      //              lAudioSourceIndexStream,
      //              lAudioSourceIndexMediaType,
      //              lAudioEncoderNode,
      //              out lAudioSourceNode))
      //              return;

      //          if (lAudioSourceNode == null)
      //              return;


      //          object[] lsourceNode = { lAudioSourceNode };

      //          mISession = mISessionControl.createSession(lsourceNode);

      //          if (mISession == null)
      //              return;

      //          mISession.startSession(0, Guid.Empty);

      //          m_StartBtn.Content = "Stop";

      //      }        
   }
}
