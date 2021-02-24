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
using System.Windows.Data;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using Microsoft.Win32;

namespace WPFMultiSourceRecorder
{
   internal delegate void ChangeState();

   /// <summary>
   ///    Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      #region Static fields

      public static CaptureManager mCaptureManager;

      public static List<ISource> mISourceItems = new List<ISource>();

      private static readonly List<ISource> mISources = new List<ISource>();

      #endregion

      #region Constructors and destructors

      public MainWindow()
      {
         InitializeComponent();

         mChangeState += MainWindow_mChangeState;
      }

      #endregion

      #region  Fields

      private IEncoderControl mEncoderControl;

      private IEVRMultiSinkFactory mEVRMultiSinkFactory;

      private string mFilename = "";

      private ISession mISession;


      private ISessionControl mISessionControl;

      private bool mIsStarted;

      private ISinkControl mSinkControl;

      private ISourceControl mSourceControl;

      private ISpreaderNodeFactory mSpreaderNodeFactory;

      private IStreamControl mStreamControl;

      #endregion

      #region Other events

      private static event ChangeState mChangeState;

      #endregion

      #region Public methods

      public static void addSourceControl(ISource aISource)
      {
         mISources.Add(aISource);

         if (mChangeState != null) {
            mChangeState();
         }
      }

      public static void removeSourceControl(ISource aISource)
      {
         mISources.Remove(aISource);

         if (mChangeState != null) {
            mChangeState();
         }
      }

      #endregion

      #region Private methods

      private List<object> getOutputNodes(List<object> aCompressedMediaTypeList, IFileSinkFactory aFileSinkFactory)
      {
         var lresult = new List<object>();

         do {
            if (aCompressedMediaTypeList == null) {
               break;
            }

            if (aCompressedMediaTypeList.Count == 0) {
               break;
            }

            if (aFileSinkFactory == null) {
               break;
            }

            if (string.IsNullOrEmpty(mFilename)) {
               break;
            }

            aFileSinkFactory.createOutputNodes(aCompressedMediaTypeList, mFilename, out lresult);
         } while (false);

         return lresult;
      }

      private void MainWindow_mChangeState()
      {
         if (mISources.Count > 0) {
            mSelectFileBtn.IsEnabled = true;
         } else {
            mSelectFileBtn.IsEnabled = false;

            m_StartStopBtn.IsEnabled = false;
         }
      }

      private void MainWindow_WriteDelegateEvent(string aMessage)
      {
         MessageBox.Show(aMessage);
      }

      private void mControlBtn_Click(object sender, RoutedEventArgs e)
      {
         if (mIsStarted) {
            mIsStarted = false;

            if (mISession == null) {
               return;
            }

            mISession.stopSession();

            mISession.closeSession();

            mISession = null;

            m_StartStopBtn.Content = "Start";

            foreach (var item in mISourceItems) {
               var lsourceitem = item;

               if (lsourceitem != null) {
                  lsourceitem.access(true);
               }
            }
         } else {
            IFileSinkFactory lFileSinkFactory = null;

            mSinkControl.createSinkFactory(Guid.Parse("A2A56DA1-EB84-460E-9F05-FEE51D8C81E3"), out lFileSinkFactory);

            if (lFileSinkFactory == null) {
               return;
            }

            var lCompressedMediaTypeList = new List<object>();

            foreach (var item in mISources) {
               var lCompressedMediaType = item.getCompressedMediaType();

               if (lCompressedMediaType != null) {
                  lCompressedMediaTypeList.Add(lCompressedMediaType);
               }
            }

            var lOutputNodes = getOutputNodes(lCompressedMediaTypeList, lFileSinkFactory);

            if ((lOutputNodes == null) ||
                (lOutputNodes.Count == 0)) {
               return;
            }

            var lSourceNodes = new List<object>();

            for (var i = 0; i < lOutputNodes.Count; i++) {
               var lSourceNode = mISources[i].getSourceNode(lOutputNodes[i]);

               if (lSourceNode != null) {
                  lSourceNodes.Add(lSourceNode);
               }
            }

            mISession = mISessionControl.createSession(lSourceNodes.ToArray());

            if (mISession == null) {
               return;
            }

            if (mISession.startSession(0, Guid.Empty)) {
               m_StartStopBtn.Content = "Stop";
            }

            mIsStarted = true;

            foreach (var item in mISourceItems) {
               var lsourceitem = item;

               if (lsourceitem != null) {
                  lsourceitem.access(false);
               }
            }
         }
      }

      private void mSelectFileBtn_Click(object sender, RoutedEventArgs e)
      {
         do {
            var limageSourceDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var lsaveFileDialog = new SaveFileDialog();

            lsaveFileDialog.InitialDirectory = limageSourceDir;

            lsaveFileDialog.DefaultExt = ".asf";

            lsaveFileDialog.AddExtension = true;

            lsaveFileDialog.CheckFileExists = false;

            lsaveFileDialog.Filter = "Media file (*.asf)|*.asf";

            var lresult = lsaveFileDialog.ShowDialog();

            if (lresult != true) {
               break;
            }

            mFilename = lsaveFileDialog.FileName;

            m_StartStopBtn.IsEnabled = true;
         } while (false);
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


         var lXmlDataProvider = (XmlDataProvider) Resources["XmlSources"];

         if (lXmlDataProvider == null) {
            return;
         }

         var doc = new XmlDocument();

         var lxmldoc = "";

         mCaptureManager.getCollectionOfSources(ref lxmldoc);

         doc.LoadXml(lxmldoc);

         lXmlDataProvider.Document = doc;

         //lXmlDataProvider = (XmlDataProvider)this.Resources["XmlEncoders"];

         //if (lXmlDataProvider == null)
         //    return;
      }

      #endregion
   }
}
