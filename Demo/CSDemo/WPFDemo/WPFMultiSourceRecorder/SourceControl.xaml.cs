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
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFMultiSourceRecorder
{
   /// <summary>
   ///    Interaction logic for SourceControl.xaml
   /// </summary>
   public partial class SourceControl : UserControl, ISource
   {
      #region Static fields

      // Using a DependencyProperty as the backing store for FriendlyName.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty FriendlyNameProperty = DependencyProperty.Register("FriendlyName", typeof(string), typeof(SourceControl), new UIPropertyMetadata(string.Empty));

      // Using a DependencyProperty as the backing store for SymbolicLink.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty SymbolicLinkProperty = DependencyProperty.Register("SymbolicLink", typeof(string), typeof(SourceControl), new UIPropertyMetadata(string.Empty));

      // Using a DependencyProperty as the backing store for TypeSource.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty TypeSourceProperty = DependencyProperty.Register("TypeSource", typeof(string), typeof(SourceControl), new UIPropertyMetadata(string.Empty));

      #endregion

      #region Constructors and destructors

      public SourceControl()
      {
         InitializeComponent();
      }

      #endregion

      #region  Fields

      private bool isLoaded;

      private IEncoderControl mEncoderControl;

      private IEVRMultiSinkFactory mEVRMultiSinkFactory;

      private ISinkControl mSinkControl;

      private ISourceControl mSourceControl;

      private ISpreaderNodeFactory mSpreaderNodeFactory;

      private IStreamControl mStreamControl;

      #endregion

      #region Public properties

      public string FriendlyName
      {
         get => (string) GetValue(FriendlyNameProperty);
         set => SetValue(FriendlyNameProperty, value);
      }


      public string SymbolicLink
      {
         get => (string) GetValue(SymbolicLinkProperty);
         set => SetValue(SymbolicLinkProperty, value);
      }


      public string TypeSource
      {
         get => (string) GetValue(TypeSourceProperty);
         set => SetValue(TypeSourceProperty, value);
      }

      #endregion

      #region Interface methods

      public void access(bool aState)
      {
         mUsingChkBx.IsEnabled = aState;
      }


      public object getCompressedMediaType()
      {
         object lresult = null;

         do {
            var lselectedNode = m_EncodersComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lEncoderGuidAttr = lselectedNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
               break;
            }


            lselectedNode = m_EncodingModeComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lEncoderModeGuidAttr = lselectedNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }

            var lSymbolicLink = SymbolicLink;

            if (m_StreamComboBox.SelectedIndex < 0) {
               break;
            }

            var lStreamIndex = (uint) m_StreamComboBox.SelectedIndex;


            if (m_MediaTypeComboBox.SelectedIndex < 0) {
               break;
            }

            var lMediaTypeIndex = (uint) m_MediaTypeComboBox.SelectedIndex;

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

            if (!lEncoderNodeFactory.createCompressedMediaType(lSourceMediaType, lCLSIDEncoderMode, 50, (uint) m_CompressedMediaTypesComboBox.SelectedIndex, out lCompressedMediaType)) {
               break;
            }

            lresult = lCompressedMediaType;
         } while (false);

         return lresult;
      }

      public object getSourceNode(object aOutputNode)
      {
         object lresult = null;

         do {
            var lselectedNode = m_EncodersComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lEncoderGuidAttr = lselectedNode.Attributes["CLSID"];

            if (lEncoderGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lEncoderGuidAttr.Value, out lCLSIDEncoder)) {
               break;
            }


            lselectedNode = m_EncodingModeComboBox.SelectedItem as XmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lEncoderModeGuidAttr = lselectedNode.Attributes["GUID"];

            if (lEncoderModeGuidAttr == null) {
               break;
            }

            Guid lCLSIDEncoderMode;

            if (!Guid.TryParse(lEncoderModeGuidAttr.Value, out lCLSIDEncoderMode)) {
               break;
            }

            var lSymbolicLink = SymbolicLink;

            if (m_StreamComboBox.SelectedIndex < 0) {
               break;
            }

            var lStreamIndex = (uint) m_StreamComboBox.SelectedIndex;


            if (m_MediaTypeComboBox.SelectedIndex < 0) {
               break;
            }

            var lMediaTypeIndex = (uint) m_MediaTypeComboBox.SelectedIndex;

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

            if (!lEncoderNodeFactory.createEncoderNode(lSourceMediaType, lCLSIDEncoderMode, 50, (uint) m_CompressedMediaTypesComboBox.SelectedIndex, aOutputNode, out lEncoderNode)) {
               break;
            }


            var SpreaderNode = lEncoderNode;

            if (TypeSource == "Video") {
               object PreviewRenderNode = null;

               // if ((bool)m_VideoStreamPreviewChkBtn.IsChecked)
               {
                  var lRenderOutputNodesList = new List<object>();

                  if (mEVRMultiSinkFactory != null) {
                     mEVRMultiSinkFactory.createOutputNodes(IntPtr.Zero, m_EVRDisplay.Surface.texture, 1, out lRenderOutputNodesList);
                  }

                  if (lRenderOutputNodesList.Count == 1) {
                     PreviewRenderNode = lRenderOutputNodesList[0];
                  }
               }


               var lOutputNodeList = new List<object>();

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

      #endregion

      #region Public methods

      public static UserControl create()
      {
         var lSourceControl = new SourceControl();

         return lSourceControl;
      }

      #endregion

      #region Private methods

      private void CheckBox_Checked(object sender, RoutedEventArgs e)
      {
         if ((bool) mUsingChkBx.IsChecked) {
            if (isLoaded) {
               return;
            }

            var lXmlDataProvider = (XmlDataProvider) Resources["XmlEncoders"];

            if (lXmlDataProvider == null) {
               return;
            }

            var doc = new XmlDocument();

            var lxmldoc = "";

            MainWindow.mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            if (TypeSource == "Video") {
               m_EncodersComboBox.SelectedIndex = 0;

               lXmlDataProvider.XPath = "EncoderFactories/Group[@GUID='{73646976-0000-0010-8000-00AA00389B71}']/EncoderFactory";
            } else {
               m_EncodersComboBox.SelectedIndex = 1;

               lXmlDataProvider.XPath = "EncoderFactories/Group[@GUID='{73647561-0000-0010-8000-00AA00389B71}']/EncoderFactory";
            }

            lXmlDataProvider.Document = doc;

            MainWindow.addSourceControl(this);

            isLoaded = true;
         } else {
            mExpander.IsExpanded = false;

            MainWindow.removeSourceControl(this);
         }
      }

      private void m_EncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         do {
            if (mEncoderControl == null) {
               break;
            }

            var lselectedNode = m_EncodersComboBox.SelectedItem as XmlNode;

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


            var lSymbolicLink = SymbolicLink;

            if (m_StreamComboBox.SelectedIndex < 0) {
               return;
            }

            var lStreamIndex = (uint) m_StreamComboBox.SelectedIndex;


            if (m_MediaTypeComboBox.SelectedIndex < 0) {
               return;
            }

            var lMediaTypeIndex = (uint) m_MediaTypeComboBox.SelectedIndex;

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

      private void UserControl_Loaded(object sender, RoutedEventArgs e)
      {
         MainWindow.mISourceItems.Add(this);

         mEncoderControl = MainWindow.mCaptureManager.createEncoderControl();

         if (mEncoderControl == null) {
            return;
         }

         mSourceControl = MainWindow.mCaptureManager.createSourceControl();

         if (mSourceControl == null) {
            return;
         }


         mStreamControl = MainWindow.mCaptureManager.createStreamControl();

         if (mStreamControl == null) {
            return;
         }

         mStreamControl.createStreamControlNodeFactory(ref mSpreaderNodeFactory);

         if (mSpreaderNodeFactory == null) {
            return;
         }


         mSinkControl = MainWindow.mCaptureManager.createSinkControl();

         if (mSinkControl == null) {
            return;
         }

         mSinkControl.createSinkFactory(Guid.Empty, out mEVRMultiSinkFactory);

         if (mEVRMultiSinkFactory == null) {
         }
      }

      private void UserControl_Unloaded(object sender, RoutedEventArgs e)
      {
         MainWindow.mISourceItems.Remove(this);
      }

      #endregion
   }
}
