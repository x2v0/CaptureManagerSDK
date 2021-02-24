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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFViewerTrigger
{
   public delegate void ChangeState(bool isEnable);

   /// <summary>
   ///    Interaction logic for Thumbnail.xaml
   /// </summary>
   public partial class Thumbnail : UserControl
   {
      #region Constructors and destructors

      public Thumbnail()
      {
         InitializeComponent();

         mTimer.Interval = new TimeSpan(0, 0, 1);
      }

      #endregion

      #region  Fields

      public bool mEnableTrigger = false;

      public float mThreshold = 50.0f;

      private int mChannels;

      private byte[] mData;

      private Guid mReadMode;

      private ISampleGrabberCallSinkFactory mSampleGrabberSinkFactory;

      private ISinkControl mSinkControl;

      private readonly DispatcherTimer mTimer = new DispatcherTimer();

      private uint mVideoHeight;
      private uint mVideoWidth;

      #endregion

      #region Public events

      public event ChangeState mChangeState;

      #endregion

      #region Public methods

      public object init(XmlNode aMediaTypeXmlNode)
      {
         initInterface();

         object lresult = null;

         do {
            if (aMediaTypeXmlNode == null) {
               break;
            }

            var lNode = aMediaTypeXmlNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }

            lNode = aMediaTypeXmlNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

            if (lNode == null) {
               break;
            }

            if (!uint.TryParse(lNode.Value, out mVideoWidth)) {
               break;
            }

            lNode = aMediaTypeXmlNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2]/@Value");

            if (lNode == null) {
               break;
            }

            if (!uint.TryParse(lNode.Value, out mVideoHeight)) {
               break;
            }

            var MFMediaType_Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

            var MFVideoFormat_RGB24 = new Guid(20, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

            var MFVideoFormat_RGB32 = new Guid(22, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

            int lWidthInBytes;

            MainWindow.mCaptureManager.getStrideForBitmapInfoHeader(MFVideoFormat_RGB32, mVideoWidth, out lWidthInBytes);

            var lsampleByteSize = (uint) Math.Abs(lWidthInBytes) * mVideoHeight;

            ISampleGrabberCall lSampleGrabberCall;

            mChannels = 4;

            mSampleGrabberSinkFactory.createOutputNode(MFMediaType_Video, MFVideoFormat_RGB32, lsampleByteSize, out lSampleGrabberCall);

            if (lSampleGrabberCall == null) {
               break;
            }

            mData = new byte[lsampleByteSize];

            lresult = lSampleGrabberCall.getTopologyNode();

            mTimer.Tick += delegate
            {
               var lByteSize = (uint) mData.Length;

               try {
                  lSampleGrabberCall.readData(mData, out lByteSize);

                  if (mEnableTrigger && (mChangeState != null)) {
                     float lvalue = 0;

                     for (var i = 0; i < lByteSize; i++) {
                        lvalue += mData[i];
                     }

                     lvalue = ((lvalue / lByteSize) * 100) / 255.0f;

                     if (lvalue >= mThreshold) {
                        mChangeState(true);
                     } else {
                        mChangeState(false);
                     }
                  }
               } finally {
                  updateDisplayImage();
               }
            };
         } while (false);

         return lresult;
      }

      public void start()
      {
         mTimer.Start();
      }

      public void stop()
      {
         mTimer.Stop();
      }

      #endregion

      #region Private methods

      private static BitmapSource FromArray(byte[] data, uint w, uint h, int ch)
      {
         var format = PixelFormats.Default;

         if (ch == 1) {
            format = PixelFormats.Gray8; //grey scale image 0-255
         }

         if (ch == 3) {
            format = PixelFormats.Bgr24; //RGB
         }

         if (ch == 4) {
            format = PixelFormats.Bgr32; //RGB + alpha
         }

         var wbm = new WriteableBitmap((int) w, (int) h, 96, 96, format, null);
         wbm.WritePixels(new Int32Rect(0, 0, (int) w, (int) h), data, ch * (int) w, 0);

         return wbm;
      }

      private void initInterface()
      {
         mSinkControl = MainWindow.mCaptureManager.createSinkControl();

         if (mSinkControl == null) {
            return;
         }

         var lxmldoc = "";

         MainWindow.mCaptureManager.getCollectionOfSinks(ref lxmldoc);

         var doc = new XmlDocument();

         doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{759D24FF-C5D6-4B65-8DDF-8A2B2BECDE39}']");

         if (lSinkNode == null) {
            return;
         }

         var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[3]");

         if (lContainerNode == null) {
            return;
         }

         setContainerFormat(lContainerNode);

         mSinkControl.createSinkFactory(mReadMode, out mSampleGrabberSinkFactory);
      }

      private void setContainerFormat(XmlNode aXmlNode)
      {
         do {
            if (aXmlNode == null) {
               break;
            }

            var lAttrNode = aXmlNode.SelectSingleNode("@Value");

            if (lAttrNode == null) {
               break;
            }

            lAttrNode = aXmlNode.SelectSingleNode("@GUID");

            if (lAttrNode == null) {
               break;
            }

            Guid lContainerFormatGuid;

            if (Guid.TryParse(lAttrNode.Value, out lContainerFormatGuid)) {
               mReadMode = lContainerFormatGuid;
            }
         } while (false);
      }

      private void updateDisplayImage()
      {
         if (mData != null) {
            mDisplayImage.Source = FromArray(mData, mVideoWidth, mVideoHeight, mChannels);
         }
      }

      #endregion
   }
}
