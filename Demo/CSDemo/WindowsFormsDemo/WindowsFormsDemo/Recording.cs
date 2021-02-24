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
using System.Windows.Forms;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;
using WPFRecording;

namespace WindowsFormsDemo
{
   public partial class Recording : Form
   {
      #region Constructors and destructors

      public Recording()
      {
         InitializeComponent();

         try {
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

         mSourceControl = mCaptureManager.createSourceControl();

         mISessionControl = mCaptureManager.createSessionControl();

         mSinkControl = mCaptureManager.createSinkControl();

         mEncoderControl = mCaptureManager.createEncoderControl();

         var doc = new XmlDocument();

         var lxmldoc = "";

         mCaptureManager.getCollectionOfSources(ref lxmldoc);

         if (string.IsNullOrEmpty(lxmldoc)) {
            return;
         }

         doc.LoadXml(lxmldoc);

         var
            lSourceNodes = doc.DocumentElement.
                               ChildNodes; // .SelectNodes("//*[Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_MEDIA_TYPE']/Value.ValueParts/ValuePart[@Value='MFMediaType_Video']]");

         if (lSourceNodes != null) {
            foreach (var item in lSourceNodes) {
               var lNode = (XmlNode) item;

               if (lNode != null) {
                  var lvalueNode = lNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME']/SingleValue/@Value");

                  var lSourceItem = new ContainerItem {
                     mFriendlyName = lvalueNode.Value,
                     mXmlNode = lNode
                  };

                  sourceComboBox.Items.Add(lSourceItem);
               }
            }
         }
      }

      #endregion

      #region  Fields

      private readonly CaptureManager mCaptureManager;

      private readonly IEncoderControl mEncoderControl;

      private readonly ISessionControl mISessionControl;

      private readonly ISinkControl mSinkControl;

      private readonly ISourceControl mSourceControl;

      private ISession mSession;

      private AbstractSink mSink;

      #endregion

      #region Private methods

      private void button1_Click(object sender, EventArgs e)
      {
         do {
            var lSelectedFormatItem = (ContainerItem) formatComboBox.SelectedItem;

            if (lSelectedFormatItem == null) {
               return;
            }

            var lselectedNode = lSelectedFormatItem.mXmlNode;

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

            if (lresult != DialogResult.OK) {
               break;
            }

            mDo.Enabled = true;

            lSelectedAttr = lselectedNode.Attributes["GUID"];

            if (lSelectedAttr == null) {
               break;
            }

            IFileSinkFactory lFileSinkFactory;

            mSinkControl.createSinkFactory(Guid.Parse(lSelectedAttr.Value), out lFileSinkFactory);

            mSink = new FileSink(lFileSinkFactory);

            mSink.setOptions(lsaveFileDialog.FileName);
         } while (false);
      }

      private void compressedMediaTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         var lxmldoc = "";

         mCaptureManager.getCollectionOfSinks(ref lxmldoc);

         var doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lsinkFactoryNodes = doc.SelectNodes("SinkFactories/SinkFactory");

         sinkComboBox.Items.Clear();

         foreach (var item in lsinkFactoryNodes) {
            var lNode = (XmlNode) item;

            if (lNode != null) {
               var lAttr = lNode.Attributes["GUID"];

               if (lAttr == null) {
                  throw new Exception("GUID is empty");
               }

               if (lAttr.Value == "{D6E342E3-7DDD-4858-AB91-4253643864C2}") {
                  var lvalueNode = lNode.SelectSingleNode("@Title");

                  var lSourceItem = new ContainerItem {
                     mFriendlyName = lvalueNode.Value,
                     mXmlNode = lNode
                  };

                  sinkComboBox.Items.Add(lSourceItem);
               }
            }
         }
      }

      private void encoderComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         do {
            if (mEncoderControl == null) {
               break;
            }


            var lSelectedEncoderItem = (ContainerItem) encoderComboBox.SelectedItem;

            if (lSelectedEncoderItem == null) {
               return;
            }

            var lselectedNode = lSelectedEncoderItem.mXmlNode;

            if (lselectedNode == null) {
               break;
            }

            var lEncoderNameAttr = lselectedNode.Attributes["Title"];

            if (lEncoderNameAttr == null) {
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

            var lSelectedSourceItem = (ContainerItem) sourceComboBox.SelectedItem;

            if (lSelectedSourceItem == null) {
               return;
            }

            var lSourceNode = lSelectedSourceItem.mXmlNode;

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

            var lSelectedStreamItem = (ContainerItem) streamComboBox.SelectedItem;

            if (lSelectedStreamItem == null) {
               return;
            }

            lSourceNode = lSelectedStreamItem.mXmlNode;

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

            var lSelectedMediaTypeItem = (ContainerItem) mediaTypeComboBox.SelectedItem;

            if (lSelectedMediaTypeItem == null) {
               return;
            }

            lSourceNode = lSelectedMediaTypeItem.mXmlNode;

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


            var lEncoderModedoc = new XmlDocument();

            lEncoderModedoc.LoadXml(lMediaTypeCollection);

            var lEncoderNodes = lEncoderModedoc.SelectNodes("EncoderMediaTypes/Group");

            encoderModeComboBox.Items.Clear();

            foreach (var item in lEncoderNodes) {
               lNode = (XmlNode) item;

               if (lNode != null) {
                  var lvalueNode = lNode.SelectSingleNode("@Title");

                  var lSourceItem = new ContainerItem {
                     mFriendlyName = lvalueNode.Value,
                     mXmlNode = lNode
                  };

                  encoderModeComboBox.Items.Add(lSourceItem);
               }
            }
         } while (false);
      }

      private void encoderModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         var lSelectedStreamItem = (ContainerItem) streamComboBox.SelectedItem;

         if (lSelectedStreamItem == null) {
            return;
         }

         var lSelectedEncoderModeItem = (ContainerItem) encoderModeComboBox.SelectedItem;

         if (lSelectedEncoderModeItem == null) {
            return;
         }

         var lcompressedMediaTypeNodes = lSelectedEncoderModeItem.mXmlNode.SelectNodes("MediaTypes/MediaType");

         compressedMediaTypeComboBox.Items.Clear();

         foreach (var item in lcompressedMediaTypeNodes) {
            var lNode = (XmlNode) item;

            if (lNode != null) {
               var lvalueNode = lSelectedStreamItem.mXmlNode.SelectSingleNode("@MajorType");

               var mTitle = "";

               if ((lvalueNode != null) &&
                   (lvalueNode.Value == "MFMediaType_Video")) {
                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

                  mTitle = lvalueNode.Value;

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2 ]/@Value");

                  mTitle += "x" + lvalueNode.Value;

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_RATE']/RatioValue/@Value");

                  mTitle += ", " + lvalueNode.Value + " FPS, ";

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                  mTitle += lvalueNode.Value.Replace("MFVideoFormat_", "");
               } else if ((lvalueNode != null) &&
                          (lvalueNode.Value == "MFMediaType_Audio")) {
                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_BITS_PER_SAMPLE']/SingleValue/@Value");

                  if (lvalueNode != null) {
                     mTitle = lvalueNode.Value;
                  }

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_NUM_CHANNELS']/SingleValue/@Value");

                  if (lvalueNode != null) {
                     mTitle += "x" + lvalueNode.Value;
                  }

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_SAMPLES_PER_SECOND']/SingleValue/@Value");

                  mTitle += ", ";

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                  if (lvalueNode != null) {
                     mTitle += lvalueNode.Value.Replace("MFVideoFormat_", "");
                  }
               }


               var lSourceItem = new ContainerItem {
                  mFriendlyName = mTitle, // lvalueNode.Value.Replace("MFMediaType_", ""),
                  mXmlNode = lNode
               };


               compressedMediaTypeComboBox.Items.Add(lSourceItem);
            }
         }
      }

      private void label5_Click(object sender, EventArgs e)
      {
      }

      private void mDo_Click(object sender, EventArgs e)
      {
         if (mSession != null) {
            mSession.closeSession();

            mSession = null;

            mDo.Text = "Stopped";

            return;
         }

         if (mSink == null) {
            return;
         }

         var lSelectedSourceItem = (ContainerItem) sourceComboBox.SelectedItem;

         if (lSelectedSourceItem == null) {
            return;
         }

         var lSourceNode = lSelectedSourceItem.mXmlNode;

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

         var lSelectedStreamItem = (ContainerItem) streamComboBox.SelectedItem;

         if (lSelectedStreamItem == null) {
            return;
         }

         lSourceNode = lSelectedStreamItem.mXmlNode;

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

         var lSelectedMediaTypeItem = (ContainerItem) mediaTypeComboBox.SelectedItem;

         if (lSelectedMediaTypeItem == null) {
            return;
         }

         lSourceNode = lSelectedMediaTypeItem.mXmlNode;

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

         mSourceControl.getSourceOutputMediaType(lSymbolicLink, lStreamIndex, lMediaTypeIndex, out lOutputMediaType);


         var lSelectedEncoderItem = (ContainerItem) encoderComboBox.SelectedItem;

         if (lSelectedEncoderItem == null) {
            return;
         }


         var lselectedNode = lSelectedEncoderItem.mXmlNode;

         if (lselectedNode == null) {
            return;
         }

         var lEncoderNameAttr = lselectedNode.Attributes["Title"];

         if (lEncoderNameAttr == null) {
            return;
         }

         var lCLSIDEncoderAttr = lselectedNode.Attributes["CLSID"];

         if (lCLSIDEncoderAttr == null) {
            return;
         }

         Guid lCLSIDEncoder;

         if (!Guid.TryParse(lCLSIDEncoderAttr.Value, out lCLSIDEncoder)) {
            return;
         }

         IEncoderNodeFactory lEncoderNodeFactory;

         mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lEncoderNodeFactory);


         var lSelectedEncoderModeItem = (ContainerItem) encoderModeComboBox.SelectedItem;

         if (lSelectedEncoderModeItem == null) {
            return;
         }

         lselectedNode = lSelectedEncoderModeItem.mXmlNode;

         if (lselectedNode == null) {
            return;
         }

         var lGUIDEncodingModeAttr = lselectedNode.Attributes["GUID"];

         if (lGUIDEncodingModeAttr == null) {
            return;
         }

         Guid lGUIDEncodingMode;

         if (!Guid.TryParse(lGUIDEncodingModeAttr.Value, out lGUIDEncodingMode)) {
            return;
         }


         if (compressedMediaTypeComboBox.SelectedIndex < 0) {
            return;
         }

         object lCompressedMediaType;

         lEncoderNodeFactory.createCompressedMediaType(lOutputMediaType, lGUIDEncodingMode, 70, (uint) compressedMediaTypeComboBox.SelectedIndex, out lCompressedMediaType);

         var lOutputNode = mSink.getOutputNode(lCompressedMediaType);

         IEncoderNodeFactory lIEncoderNodeFactory;

         mEncoderControl.createEncoderNodeFactory(lCLSIDEncoder, out lIEncoderNodeFactory);

         object lEncoderNode;

         lIEncoderNodeFactory.createEncoderNode(lOutputMediaType, lGUIDEncodingMode, 70, (uint) compressedMediaTypeComboBox.SelectedIndex, lOutputNode, out lEncoderNode);

         object lSourceMediaNode;


         mSourceControl.createSourceNode(lSymbolicLink, lStreamIndex, lMediaTypeIndex, lEncoderNode, out lSourceMediaNode);

         var lSourcesList = new List<object>();

         lSourcesList.Add(lSourceMediaNode);

         mSession = mISessionControl.createSession(lSourcesList.ToArray());


         if (mSession != null) {
            mSession.startSession(0, Guid.Empty);
         }

         mDo.Text = "Record is executed!!!";
      }

      private void mediaTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         var lSelectedStreamItem = (ContainerItem) streamComboBox.SelectedItem;

         if (lSelectedStreamItem == null) {
            return;
         }

         var lValueNode = lSelectedStreamItem.mXmlNode.SelectSingleNode("@MajorTypeGUID");

         var lXPath = "EncoderFactories/Group[@GUID='blank']/EncoderFactory";

         lXPath = lXPath.Replace("blank", lValueNode.Value);


         var lxmldoc = "";

         mCaptureManager.getCollectionOfEncoders(ref lxmldoc);

         var doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lEncoderNodes = doc.SelectNodes(lXPath);

         encoderComboBox.Items.Clear();

         foreach (var item in lEncoderNodes) {
            var lNode = (XmlNode) item;

            if (lNode != null) {
               var lvalueNode = lNode.SelectSingleNode("@Title");

               var lSourceItem = new ContainerItem {
                  mFriendlyName = lvalueNode.Value,
                  mXmlNode = lNode
               };

               encoderComboBox.Items.Add(lSourceItem);
            }
         }
      }

      private void sinkComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         var lSelectedSinkItem = (ContainerItem) sinkComboBox.SelectedItem;

         if (lSelectedSinkItem == null) {
            return;
         }

         var lContainerNodes = lSelectedSinkItem.mXmlNode.SelectNodes("Value.ValueParts/ValuePart");

         formatComboBox.Items.Clear();

         foreach (var item in lContainerNodes) {
            var lNode = (XmlNode) item;

            if (lNode != null) {
               var lvalueNode = lNode.SelectSingleNode("@Value");

               var lSourceItem = new ContainerItem {
                  mFriendlyName = lvalueNode.Value,
                  mXmlNode = lNode
               };

               formatComboBox.Items.Add(lSourceItem);
            }
         }
      }

      private void sourceComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         var lSelectedSourceItem = (ContainerItem) sourceComboBox.SelectedItem;

         if (lSelectedSourceItem == null) {
            return;
         }

         var lStreamNodes = lSelectedSourceItem.mXmlNode.SelectNodes("PresentationDescriptor/StreamDescriptor");

         if (lStreamNodes == null) {
            return;
         }

         streamComboBox.Items.Clear();

         foreach (var item in lStreamNodes) {
            var lNode = (XmlNode) item;

            if (lNode != null) {
               var lvalueNode = lNode.SelectSingleNode("@MajorType");

               var lSourceItem = new ContainerItem {
                  mFriendlyName = lvalueNode.Value.Replace("MFMediaType_", ""),
                  mXmlNode = lNode
               };

               streamComboBox.Items.Add(lSourceItem);
            }
         }
      }

      private void streamComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
         var lSelectedStreamItem = (ContainerItem) streamComboBox.SelectedItem;

         if (lSelectedStreamItem == null) {
            return;
         }

         var lMediaTypeNodes = lSelectedStreamItem.mXmlNode.SelectNodes("MediaTypes/MediaType");

         if (lMediaTypeNodes == null) {
            return;
         }

         mediaTypeComboBox.Items.Clear();

         foreach (var item in lMediaTypeNodes) {
            var lNode = (XmlNode) item;

            if (lNode != null) {
               var lvalueNode = lSelectedStreamItem.mXmlNode.SelectSingleNode("@MajorType");

               var mTitle = "";

               if ((lvalueNode != null) &&
                   (lvalueNode.Value == "MFMediaType_Video")) {
                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

                  mTitle = lvalueNode.Value;

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2 ]/@Value");

                  mTitle += "x" + lvalueNode.Value;

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_RATE']/RatioValue/@Value");

                  mTitle += ", " + lvalueNode.Value + " FPS, ";

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                  mTitle += lvalueNode.Value.Replace("MFVideoFormat_", "");
               } else if ((lvalueNode != null) &&
                          (lvalueNode.Value == "MFMediaType_Audio")) {
                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_BITS_PER_SAMPLE']/SingleValue/@Value");

                  if (lvalueNode != null) {
                     mTitle = lvalueNode.Value;
                  }

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_NUM_CHANNELS']/SingleValue/@Value");

                  if (lvalueNode != null) {
                     mTitle += "x" + lvalueNode.Value;
                  }

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_AUDIO_SAMPLES_PER_SECOND']/SingleValue/@Value");

                  mTitle += ", ";

                  lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

                  if (lvalueNode != null) {
                     mTitle += lvalueNode.Value.Replace("MFVideoFormat_", "");
                  }
               }


               var lSourceItem = new ContainerItem {
                  mFriendlyName = mTitle, // lvalueNode.Value.Replace("MFMediaType_", ""),
                  mXmlNode = lNode
               };

               mediaTypeComboBox.Items.Add(lSourceItem);
            }
         }
      }

      #endregion

      #region Nested classes

      private class ContainerItem
      {
         #region  Fields

         public string mFriendlyName = "SourceItem";

         public XmlNode mXmlNode;

         #endregion

         #region Public methods

         public override string ToString()
         {
            return mFriendlyName;
         }

         #endregion
      }

      #endregion
   }
}
