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
using System.Windows.Data;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFStreamingAudioRenderer
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
            mCaptureManager = new CaptureManager();
         }

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

         mISARVolumeControl = mCaptureManager.createSARVolumeControl();

         if (mISARVolumeControl == null) {
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


         mSinkControl.createSinkFactory(Guid.Empty, out mSARSinkFactory);
      }

      #endregion

      #region  Fields

      private readonly CaptureManager mCaptureManager;

      private readonly ISARVolumeControl mISARVolumeControl;

      private ISession mISession;

      private readonly ISessionControl mISessionControl;

      private readonly ISARSinkFactory mSARSinkFactory;

      private readonly ISinkControl mSinkControl;

      private readonly ISourceControl mSourceControl;

      #endregion

      #region Private methods

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         if (mISession != null) {
            mISession.stopSession();

            mISession.closeSession();

            mISession = null;

            mTitleTxtBlk.Text = "Start playing";

            return;
         }

         var lSourceNodes = new List<object>();

         object lSARSinkOutputNode = null;

         mSARSinkFactory.createOutputNode(out lSARSinkOutputNode);

         if (lSARSinkOutputNode == null) {
            return;
         }


         var l_AudioSourceXmlNode = m_AudioSourceComboBox.SelectedItem as XmlNode;

         if (l_AudioSourceXmlNode == null) {
            return;
         }

         var lNode = l_AudioSourceXmlNode.SelectSingleNode("Source.Attributes/Attribute" + "[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']" + "/SingleValue/@Value");

         if (lNode == null) {
            return;
         }

         var lSymbolicLink = lNode.Value;

         object lSourceNode = null;

         mSourceControl.createSourceNode(lSymbolicLink, 0, 0, lSARSinkOutputNode, out lSourceNode);

         if (lSourceNode == null) {
            return;
         }

         lSourceNodes.Add(lSourceNode);

         mISession = mISessionControl.createSession(lSourceNodes.ToArray());

         if (mISession == null) {
            return;
         }

         if (mISession.startSession(0, Guid.Empty)) {
            mTitleTxtBlk.Text = "Stop playing";

            uint lChannelCount = 0;

            mISARVolumeControl.getChannelCount(lSARSinkOutputNode, out lChannelCount);

            if (lChannelCount > 0) {
               float lLevel = 0;

               mISARVolumeControl.getChannelVolume(lSARSinkOutputNode, 0, out lLevel);

               mRVolume.ValueChanged += (s, ev) => { mISARVolumeControl.setChannelVolume(lSARSinkOutputNode, 0, (float) ev.NewValue); };

               mRVolume.Value = lLevel;

               if (lChannelCount > 1) {
                  lLevel = 0;

                  mISARVolumeControl.getChannelVolume(lSARSinkOutputNode, 1, out lLevel);

                  mLVolume.ValueChanged += (s, ev) => { mISARVolumeControl.setChannelVolume(lSARSinkOutputNode, 1, (float) ev.NewValue); };

                  mLVolume.Value = lLevel;
               }
            }
         }
      }

      private void MainWindow_WriteDelegateEvent(string aMessage)
      {
         MessageBox.Show(aMessage);
      }

      #endregion
   }
}
