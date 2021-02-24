﻿/*
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
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFRtspServer
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
            ThreadPool.SetMinThreads(15, 10);

            mCaptureManager = new CaptureManager("CaptureManager.dll");
         } catch (Exception) {
            try {
               mCaptureManager = new CaptureManager();
            } catch (Exception) {
            }
         }

         if (mCaptureManager == null) {
            return;
         }

         var lXmlDataProvider = (XmlDataProvider) Resources["XmlSourcesProvider"];

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


         mIEVRStreamControl = mCaptureManager.createEVRStreamControl();

         mSourceControl = mCaptureManager.createSourceControl();


         lXmlDataProvider = (XmlDataProvider) Resources["XmlEncodersProvider"];

         if (lXmlDataProvider == null) {
            return;
         }


         mEncoderControl = mCaptureManager.createEncoderControl();

         mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

         doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         lXmlDataProvider.Document = doc;
      }

      #endregion

      #region  Fields

      private uint currentmillisecond;
      private readonly CaptureManager mCaptureManager;

      private readonly IEncoderControl mEncoderControl;


      private readonly Guid MFMediaType_Audio = new Guid(0x73647561, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

      private readonly Guid MFMediaType_Video = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xAA, 0x00, 0x38, 0x9B, 0x71);

      private IEVRStreamControl mIEVRStreamControl;

      private ISession mISession;

      private readonly ISourceControl mSourceControl;

      private RtspServer s;


      private readonly Guid StreamingCBR = new Guid("8F6FF1B6-534E-49C0-B2A8-16D534EAF135");

      #endregion

      #region Private methods

      private Tuple<object, RtspServer.StreamType, int> createAudioStream(ISampleGrabberCallbackSinkFactory aISampleGrabberCallbackSinkFactory, int aIndexCount)
      {
         object result = null;

         var type = RtspServer.StreamType.Audio;

         var index = 0;

         do {
            var lAudioFormat = Guid.Empty;

            var lAudioCompressedNode = mAudioMediaTypeComboBox.SelectedItem as XmlNode;

            if (lAudioCompressedNode != null) {
               var lGUID = lAudioCompressedNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@GUID");

               if (lGUID != null) {
                  Guid.TryParse(lGUID.Value, out lAudioFormat);
               }
            }

            ISampleGrabberCallback lSampleGrabberCallback;

            aISampleGrabberCallbackSinkFactory.createOutputNode(MFMediaType_Audio, lAudioFormat, out lSampleGrabberCallback);

            if (lSampleGrabberCallback != null) {
               lSampleGrabberCallback.mUpdateFullEvent += delegate(uint aSampleFlags, long aSampleTime, long aSampleDuration, byte[] aData, uint aLength)
               {
                  if (s != null) {
                     lock (s) {
                        currentmillisecond = (uint) aSampleTime / 10000;

                        s.sendData(aIndexCount, (int) type, currentmillisecond * 90, aData);
                     }
                  }
               };

               var lSampleGrabberCallNode = lSampleGrabberCallback.getTopologyNode();

               if (lSampleGrabberCallNode != null) {
                  var mEncoderControl = mCaptureManager.createEncoderControl();

                  var lEncoderNode = mAudioEncoderComboBox.SelectedItem as XmlNode;

                  if (lEncoderNode == null) {
                     break;
                  }

                  var lNode = lEncoderNode.SelectSingleNode("@CLSID");

                  if (lNode == null) {
                     break;
                  }

                  Guid lCLSIDEncoder;

                  if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder)) {
                     break;
                  }

                  IEncoderNodeFactory lIEncoderNodeFactory;

                  mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lIEncoderNodeFactory);

                  if (lIEncoderNodeFactory != null) {
                     var lSourceNode = mAudioSourcesComboBox.SelectedItem as XmlNode;

                     if (lSourceNode == null) {
                        break;
                     }

                     lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

                     if (lNode == null) {
                        break;
                     }

                     var lSymbolicLink = lNode.Value;

                     lSourceNode = mAudioStreamsComboBox.SelectedItem as XmlNode;

                     if (lSourceNode == null) {
                        break;
                     }

                     lNode = lSourceNode.SelectSingleNode("@Index");

                     if (lNode == null) {
                        break;
                     }

                     uint lStreamIndex = 0;

                     if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
                        break;
                     }

                     lSourceNode = mAudioMediaTypesComboBox.SelectedItem as XmlNode;

                     if (lSourceNode == null) {
                        break;
                     }

                     lNode = lSourceNode.SelectSingleNode("@Index");

                     if (lNode == null) {
                        break;
                     }

                     uint lMediaTypeIndex = 0;

                     if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
                        break;
                     }


                     object lAudioSourceOutputMediaType;

                     mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lAudioSourceOutputMediaType);

                     object lAudioEncoder;

                     lIEncoderNodeFactory.createEncoderNode(lAudioSourceOutputMediaType,
                                                            //new Guid(0xee8c3745, 0xf45b, 0x42b3, 0xa8, 0xcc, 0xc7, 0xa6, 0x96, 0x44, 0x9, 0x55),
                                                            //new Guid(0xca37e2be, 0xbec0, 0x4b17, 0x94, 0x6d, 0x44, 0xfb, 0xc1, 0xb3, 0xdf, 0x55),
                                                            StreamingCBR, 75, 0, lSampleGrabberCallNode, out lAudioEncoder);

                     mSourceControl.createSourceNode(lSymbolicLink, lStreamIndex, lMediaTypeIndex, lAudioEncoder, out result);

                     if (result != null) {
                        index = aIndexCount;
                     }
                  }
               }
            }
         } while (false);

         return Tuple.Create(result, type, index);
      }

      private Tuple<object, RtspServer.StreamType, int, string> createVideoStream(ISampleGrabberCallbackSinkFactory aISampleGrabberCallbackSinkFactory, int aIndexCount)
      {
         object result = null;

         var type = RtspServer.StreamType.Video;

         var index = 0;

         var format = "";

         do {
            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null) {
               break;
            }

            var lSymbolicLink = lNode.Value;

            lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
               break;
            }

            lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }


            IEVRSinkFactory lSinkFactory;

            var lSinkControl = mCaptureManager.createSinkControl();

            lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

            object lEVROutputNode = null;

            lSinkFactory.createOutputNode(mVideoPanel.Handle, out lEVROutputNode);

            if (lEVROutputNode == null) {
               break;
            }

            var lVideoFormat = Guid.Empty;

            var lVideoCompressedNode = mVideoMediaTypeComboBox.SelectedItem as XmlNode;

            if (lVideoCompressedNode != null) {
               var lGUID = lVideoCompressedNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@GUID");

               if (lGUID != null) {
                  Guid.TryParse(lGUID.Value, out lVideoFormat);
               }


               var lvalue = lVideoCompressedNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

               if ((lvalue != null) &&
                   (lvalue.Value != null)) {
                  var lsplit = lvalue.Value.Split('_');

                  if ((lsplit != null) &&
                      (lsplit.Length == 2)) {
                     format = lsplit[1];
                  }

                  if (format == "HEVC") {
                     format = "H265";
                  }
               }
            }


            ISampleGrabberCallback lSampleGrabberCallback = null;

            aISampleGrabberCallbackSinkFactory.createOutputNode(MFMediaType_Video, lVideoFormat, out lSampleGrabberCallback);

            var lOutputNode = lEVROutputNode;

            if (lSampleGrabberCallback != null) {
               lSampleGrabberCallback.mUpdateFullEvent += delegate(uint aSampleFlags, long aSampleTime, long aSampleDuration, byte[] aData, uint aLength)
               {
                  if (s != null) {
                     lock (s) {
                        currentmillisecond += 1;

                        s.sendData(aIndexCount, (int) type, currentmillisecond * 90, aData);

                        currentmillisecond += 20;
                     }
                  }
               };

               var lSampleGrabberCallNode = lSampleGrabberCallback.getTopologyNode();

               if (lSampleGrabberCallNode != null) {
                  var streamControl = mCaptureManager.createStreamControl();
                  ISpreaderNodeFactory spreaderNodeFactory = null;
                  object spreaderNode = null;
                  var outputNodeList = new List<object>();


                  var mEncoderControl = mCaptureManager.createEncoderControl();

                  var lEncoderNode = mVideoEncoderComboBox.SelectedItem as XmlNode;

                  if (lEncoderNode == null) {
                     break;
                  }

                  lNode = lEncoderNode.SelectSingleNode("@CLSID");

                  if (lNode == null) {
                     break;
                  }

                  Guid lCLSIDEncoder;

                  if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder)) {
                     break;
                  }


                  var lBitRate = (uint) mBitRateComboBox.SelectedItem;


                  IEncoderNodeFactory lIEncoderNodeFactory;

                  mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lIEncoderNodeFactory);

                  if (lIEncoderNodeFactory != null) {
                     object lVideoSourceOutputMediaType;

                     if (mSourceControl == null) {
                        break;
                     }

                     mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lVideoSourceOutputMediaType);


                     object lVideoEncoderNode;

                     lIEncoderNodeFactory.createEncoderNode(lVideoSourceOutputMediaType, StreamingCBR, lBitRate, 0, lSampleGrabberCallNode, out lVideoEncoderNode);


                     streamControl.createStreamControlNodeFactory(ref spreaderNodeFactory);
                     outputNodeList.Add(lEVROutputNode);
                     outputNodeList.Add(lVideoEncoderNode);
                     spreaderNodeFactory.createSpreaderNode(outputNodeList, out spreaderNode);


                     if (spreaderNode != null) {
                        lOutputNode = spreaderNode;
                     }
                  }
               }
            }

            var lextendSymbolicLink = lSymbolicLink + " --options=" + "<?xml version='1.0' encoding='UTF-8'?>" + "<Options>" + "<Option Type='Cursor' Visiblity='True'>" + "<Option.Extensions>" +
                                      "<Extension Type='BackImage' Height='100' Width='100' Fill='0x7055ff55' />" + "</Option.Extensions>" + "</Option>" + "</Options>";


            lextendSymbolicLink += " --normalize=Landscape";

            mSourceControl.createSourceNode(lextendSymbolicLink, lStreamIndex, lMediaTypeIndex, lOutputNode, out result);

            if (result != null) {
               index = aIndexCount;
            }
         } while (false);

         return Tuple.Create(result, type, index, format);
      }

      private void fadeInAnimation_Completed(object sender, EventArgs e)
      {
         mPopupCopy.IsOpen = false;
      }

      private void Hyperlink_Click(object sender, RoutedEventArgs e)
      {
         var lHyperlink = sender as Hyperlink;

         if (lHyperlink == null) {
            return;
         }

         var lRun = lHyperlink.Inlines.FirstInline as Run;

         if (lRun == null) {
            return;
         }

         Clipboard.SetText(lRun.Text);

         mPopupCopy.IsOpen = true;

         var fadeInAnimation = new DoubleAnimation(1.0, new Duration(TimeSpan.FromMilliseconds(1000)));

         fadeInAnimation.Completed += fadeInAnimation_Completed;
         fadeInAnimation.BeginTime = TimeSpan.FromMilliseconds(0);
         mPopupCopy.Opacity = 0.0;
         mPopupCopy.BeginAnimation(OpacityProperty, (AnimationTimeline) fadeInAnimation.GetAsFrozen());
      }

      private void mAudioEncoderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         do {
            mAudioBitRateComboBox.Items.Clear();


            var lSourceNode = mAudioSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_AUDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null) {
               break;
            }

            var lSymbolicLink = lNode.Value;

            lSourceNode = mAudioStreamsComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
               break;
            }

            lSourceNode = mAudioMediaTypesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }


            object lAudioSourceOutputMediaType;

            if (mSourceControl == null) {
               break;
            }

            mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lAudioSourceOutputMediaType);


            var lAudioEncoderNode = mAudioEncoderComboBox.SelectedItem as XmlNode;

            if (lAudioEncoderNode == null) {
               break;
            }

            lNode = lAudioEncoderNode.SelectSingleNode("@CLSID");

            if (lNode == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder)) {
               break;
            }

            string lxmlDoc;

            mEncoderControl.getMediaTypeCollectionOfEncoder(lAudioSourceOutputMediaType, lCLSIDEncoder, out lxmlDoc);

            var lXmlDataProvider = (XmlDataProvider) Resources["XmlAudioCompressedMediaTypesProvider"];

            if (lXmlDataProvider == null) {
               return;
            }

            var doc = new XmlDocument();

            doc.LoadXml(lxmlDoc);

            lXmlDataProvider.Document = doc;


            var lGroup = doc.SelectSingleNode("EncoderMediaTypes/Group[@GUID='{8F6FF1B6-534E-49C0-B2A8-16D534EAF135}']");

            if (lGroup != null) {
               uint lMaxBitRate = 0;

               uint lMinBitRate = 0;

               var lAttr = lGroup.SelectSingleNode("@MaxBitRate");

               if (lAttr != null) {
                  if (uint.TryParse(lAttr.Value, out lMaxBitRate)) {
                  }
               }

               lAttr = lGroup.SelectSingleNode("@MinBitRate");

               if (lAttr != null) {
                  if (uint.TryParse(lAttr.Value, out lMinBitRate)) {
                  }
               }

               for (var i = lMaxBitRate; i >= lMinBitRate; i >>= 1) {
                  mAudioBitRateComboBox.Items.Add(i);
               }
            }
         } while (false);
      }

      private void mLaunchButton_Click(object sender, RoutedEventArgs e)
      {
         if (mLaunchButton.Content == "Stop") {
            if (s != null) {
               s.StopListen();
            }

            s = null;

            if (mISession != null) {
               mISession.closeSession();

               mLaunchButton.Content = "Launch";
            }

            mISession = null;

            return;
         }


         var lxmldoc = "";

         mCaptureManager.getCollectionOfSinks(ref lxmldoc);

         var doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{3D64C48E-EDA4-4EE1-8436-58B64DD7CF13}']");

         if (lSinkNode == null) {
            return;
         }

         var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

         if (lContainerNode == null) {
            return;
         }

         var lReadMode = setContainerFormat(lContainerNode);

         var lSinkControl = mCaptureManager.createSinkControl();

         ISampleGrabberCallbackSinkFactory lSampleGrabberCallbackSinkFactory = null;

         lSinkControl.createSinkFactory(lReadMode, out lSampleGrabberCallbackSinkFactory);

         var lIndexCount = 120;

         var lVideoStreamSourceNode = createVideoStream(lSampleGrabberCallbackSinkFactory, lIndexCount++);

         var lAudioStreamSourceNode = createAudioStream(lSampleGrabberCallbackSinkFactory, lIndexCount++);


         var lSourceMediaNodeList = new List<object>();

         var streams = new List<Tuple<RtspServer.StreamType, int, string>>();

         if (lVideoStreamSourceNode.Item1 != null) {
            lSourceMediaNodeList.Add(lVideoStreamSourceNode.Item1);

            streams.Add(Tuple.Create(lVideoStreamSourceNode.Item2, lVideoStreamSourceNode.Item3, lVideoStreamSourceNode.Item4));
         }

         if (lAudioStreamSourceNode.Item1 != null) {
            lSourceMediaNodeList.Add(lAudioStreamSourceNode.Item1);

            streams.Add(Tuple.Create(lAudioStreamSourceNode.Item2, lAudioStreamSourceNode.Item3, ""));
         }

         var lSessionControl = mCaptureManager.createSessionControl();

         if (lSessionControl == null) {
            return;
         }

         mISession = lSessionControl.createSession(lSourceMediaNodeList.ToArray());

         if (mISession == null) {
            return;
         }


         mISession.registerUpdateStateDelegate(UpdateStateDelegate);

         mISession.startSession(0, Guid.Empty);

         mLaunchButton.Content = "Stop";

         startServer(streams);
      }

      private void mVideoEncoderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         do {
            mBitRateComboBox.Items.Clear();


            var lSourceNode = mSourcesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

            if (lNode == null) {
               break;
            }

            var lSymbolicLink = lNode.Value;

            lSourceNode = mStreamsComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lStreamIndex = 0;

            if (!uint.TryParse(lNode.Value, out lStreamIndex)) {
               break;
            }

            lSourceNode = mMediaTypesComboBox.SelectedItem as XmlNode;

            if (lSourceNode == null) {
               break;
            }

            lNode = lSourceNode.SelectSingleNode("@Index");

            if (lNode == null) {
               break;
            }

            uint lMediaTypeIndex = 0;

            if (!uint.TryParse(lNode.Value, out lMediaTypeIndex)) {
               break;
            }


            object lVideoSourceOutputMediaType;

            if (mSourceControl == null) {
               break;
            }

            mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lVideoSourceOutputMediaType);


            var lVideoEncoderNode = mVideoEncoderComboBox.SelectedItem as XmlNode;

            if (lVideoEncoderNode == null) {
               break;
            }

            lNode = lVideoEncoderNode.SelectSingleNode("@CLSID");

            if (lNode == null) {
               break;
            }

            Guid lCLSIDEncoder;

            if (!Guid.TryParse(lNode.Value, out lCLSIDEncoder)) {
               break;
            }

            string lxmlDoc;

            mEncoderControl.getMediaTypeCollectionOfEncoder(lVideoSourceOutputMediaType, lCLSIDEncoder, out lxmlDoc);

            var lXmlDataProvider = (XmlDataProvider) Resources["XmlVideoCompressedMediaTypesProvider"];

            if (lXmlDataProvider == null) {
               return;
            }

            var doc = new XmlDocument();

            doc.LoadXml(lxmlDoc);

            lXmlDataProvider.Document = doc;


            var lGroup = doc.SelectSingleNode("EncoderMediaTypes/Group[@GUID='{8F6FF1B6-534E-49C0-B2A8-16D534EAF135}']");

            if (lGroup != null) {
               uint lMaxBitRate = 0;

               uint lMinBitRate = 0;

               var lAttr = lGroup.SelectSingleNode("@MaxBitRate");

               if (lAttr != null) {
                  if (uint.TryParse(lAttr.Value, out lMaxBitRate)) {
                  }
               }

               lAttr = lGroup.SelectSingleNode("@MinBitRate");

               if (lAttr != null) {
                  if (uint.TryParse(lAttr.Value, out lMinBitRate)) {
                  }
               }

               for (var i = lMaxBitRate; i >= lMinBitRate; i >>= 1) {
                  mBitRateComboBox.Items.Add(i);
               }
            }
         } while (false);
      }

      private Guid setContainerFormat(XmlNode aXmlNode)
      {
         var lContainerFormatGuid = Guid.Empty;

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

            if (Guid.TryParse(lAttrNode.Value, out lContainerFormatGuid)) {
            }
         } while (false);

         return lContainerFormatGuid;
      }

      //rtsp://127.0.0.1:8554

      private void startServer(List<Tuple<RtspServer.StreamType, int, string>> streams)
      {
         var port = 8554;
         var username = "user"; // or use NUL if there is no username
         var password = "password"; // or use NUL if there is no password

         if (s == null) {
            s = new RtspServer(streams, port, null, null);
         }

         s.StartListen();
      }

      private void UpdateStateDelegate(uint aCallbackEventCode, uint aSessionDescriptor)
      {
         var k = (SessionCallbackEventCode) aCallbackEventCode;

         switch (k) {
            case SessionCallbackEventCode.Unknown:
               break;
            case SessionCallbackEventCode.Error:
               break;
            case SessionCallbackEventCode.Status_Error:
               break;
            case SessionCallbackEventCode.Execution_Error:
               break;
            case SessionCallbackEventCode.ItIsReadyToStart:
               break;
            case SessionCallbackEventCode.ItIsStarted:
               break;
            case SessionCallbackEventCode.ItIsPaused:
               break;
            case SessionCallbackEventCode.ItIsStopped:
               break;
            case SessionCallbackEventCode.ItIsEnded:
               break;
            case SessionCallbackEventCode.ItIsClosed:
            {
               Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => mLaunchButton.Content = "Launch"));

               Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
               {
                  if (s != null) {
                     s.StopListen();
                  }
               }));
            }
               break;
            case SessionCallbackEventCode.VideoCaptureDeviceRemoved:
            {
               Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => mLaunchButton_Click(null, null)));
            }
               break;
         }
      }

      #endregion
   }
}
