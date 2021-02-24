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
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy.Interfaces;
using WPFAreaScreenRecorder.Properties;
using Application = System.Windows.Application;

namespace WPFAreaScreenRecorder
{
   internal class MediaTypeConvertor : IValueConverter
   {
      #region Public properties

      public string Type
      {
         get;
         set;
      }

      #endregion

      #region Interface methods

      object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var lresult = "";

         do {
            var lnode = value as XmlNode;

            if ((lnode == null) ||
                (lnode.LocalName != "MediaType")) {
               break;
            }

            if (Type == "Video") {
               var lAttr = lnode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

               if (lAttr != null) {
                  lresult = lAttr.Value.Replace("MFVideoFormat_", "");
               }

               var ltempNode = lnode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_MPEG2_PROFILE']/SingleValue/@Value");

               if (ltempNode != null) {
                  switch (ltempNode.Value) {
                     case "66":
                        lresult += ", " + "Baseline Profile";
                        break;
                     case "77":
                        lresult += ", " + "Main Profile";
                        break;
                     case "100":
                        lresult += ", " + "High Profile";
                        break;
                  }
               }
            }
         } while (false);

         return lresult;
      }

      object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return null;
      }

      #endregion
   }

   internal class RemoveConvertor : IValueConverter
   {
      #region Public properties

      public string Template
      {
         get;
         set;
      }

      #endregion

      #region Interface methods

      object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         var lresult = "";

         do {
            if (value == null) {
               break;
            }

            lresult = value.ToString();

            var lremoves = Template.Split('|');

            foreach (var item in lremoves) {
               lresult = lresult.Replace(item, "");
            }
         } while (false);

         return lresult;
      }

      object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         return null;
      }

      #endregion
   }

   /// <summary>
   ///    Interaction logic for ConfigWindow.xaml
   /// </summary>
   public partial class ConfigWindow : Window
   {
      #region Static fields

      public static string mAudioSymbolicLink = "CaptureManager///Software///Sources///AudioEndpointCapture///AudioLoopBack";
      public static string mCurrentSymbolicLink = "";

      #endregion

      #region Constructors and destructors

      public ConfigWindow()
      {
         InitializeComponent();
      }

      #endregion

      #region  Fields

      private IEncoderControl mEncoderControl;

      private ISourceControl mSourceControl;

      #endregion

      #region Private methods

      private void m_AudioEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         var lEncoderXmlNode = m_AudioEncodersComboBox.SelectedItem as XmlNode;

         var t = new Thread(delegate()
         {
            do {
               if (mEncoderControl == null) {
                  break;
               }

               var lselectedNode = lEncoderXmlNode;

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


               var lSymbolicLink = mAudioSymbolicLink;

               uint lStreamIndex = 0;

               uint lMediaTypeIndex = 0;

               object lOutputMediaType;

               mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);

               string lMediaTypeCollection;

               if (!mEncoderControl.getMediaTypeCollectionOfEncoder(lOutputMediaType, lCLSIDEncoder, out lMediaTypeCollection)) {
                  break;
               }


               var lXmlEncoderModeDataProvider = (XmlDataProvider) Resources["XmlAudioEncoderModeProvider"];

               if (lXmlEncoderModeDataProvider == null) {
                  return;
               }

               var lEncoderModedoc = new XmlDocument();

               lEncoderModedoc.LoadXml(lMediaTypeCollection);

               lXmlEncoderModeDataProvider.Document = lEncoderModedoc;
            } while (false);
         });

         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
      }

      private void m_VideoEncodersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         var lEncoderXmlNode = m_VideoEncodersComboBox.SelectedItem as XmlNode;

         var t = new Thread(delegate()
         {
            do {
               if (mEncoderControl == null) {
                  break;
               }

               var lselectedNode = lEncoderXmlNode;

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


               var lSymbolicLink = mCurrentSymbolicLink;

               uint lStreamIndex = 0;

               uint lMediaTypeIndex = 0;

               object lOutputMediaType;

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
         });

         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
      }

      private void mCancelBtn_Click(object sender, RoutedEventArgs e)
      {
         Settings.Default.Reload();

         Close();
      }

      private void mConfirmBtn_Click(object sender, RoutedEventArgs e)
      {
         Settings.Default.Save();

         Close();
      }

      private void mResetBtn_Click(object sender, RoutedEventArgs e)
      {
         Settings.Default.Reset();
      }

      private void mWDBtn_Click(object sender, RoutedEventArgs e)
      {
         using (var dialog = new FolderBrowserDialog()) {
            dialog.SelectedPath = Settings.Default.StoringDir;

            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) {
               Settings.Default.StoringDir = dialog.SelectedPath;
            }
         }
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         if (ControlWindow.mCaptureManager == null) {
            return;
         }


         var t = new Thread(delegate()
         {
            try {
               mSourceControl = ControlWindow.mCaptureManager.createSourceControl();

               if (mSourceControl == null) {
                  return;
               }

               mEncoderControl = ControlWindow.mCaptureManager.createEncoderControl();

               if (mEncoderControl == null) {
                  return;
               }

               var doc = new XmlDocument();

               var lxmldoc = "";

               var lXmlDataProvider = (XmlDataProvider) Resources["XmlEncoders"];

               if (lXmlDataProvider == null) {
                  return;
               }

               doc = new XmlDocument();

               ControlWindow.mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

               doc.LoadXml(lxmldoc);

               lXmlDataProvider.Document = doc;


               ControlWindow.mCaptureManager.getCollectionOfSinks(ref lxmldoc);


               lXmlDataProvider = (XmlDataProvider) Resources["XmlContainerTypeProvider"];

               if (lXmlDataProvider == null) {
                  return;
               }

               doc = new XmlDocument();

               doc.LoadXml(lxmldoc);

               lXmlDataProvider.Document = doc;


               ControlWindow.mCaptureManager.getCollectionOfSources(ref lxmldoc);

               doc = new XmlDocument();

               doc.LoadXml(lxmldoc);

               var lxpath = "//*[Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue[@Value='Temp']]";

               lxpath = lxpath.Replace("Temp", mCurrentSymbolicLink);

               var lNode = doc.SelectSingleNode(lxpath);

               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) delegate
               {
                  if (lNode != null) {
                     m_VideoStreamComboBox.DataContext = lNode;
                  }

                  mVideoSourceName.Text = mCurrentSymbolicLink;
               });


               lxpath = "//*[Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']/SingleValue[@Value='Temp']]";

               lxpath = lxpath.Replace("Temp", mAudioSymbolicLink);

               var lAudioNode = doc.SelectSingleNode(lxpath);

               Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, (ThreadStart) delegate
               {
                  if (lNode != null) {
                     m_AudioStreamComboBox.DataContext = lAudioNode;
                  }

                  mAudioSourceName.Text = mAudioSymbolicLink;
               });
            } catch (Exception) {
            }
         });
         t.SetApartmentState(ApartmentState.MTA);

         t.Start();
      }

      #endregion
   }
}
