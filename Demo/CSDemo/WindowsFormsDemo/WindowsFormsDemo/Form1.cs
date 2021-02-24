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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using CaptureManagerToCSharpProxy;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WindowsFormsDemo
{
   public partial class WebViewer : Form
   {
      #region Constructors and destructors

      public WebViewer()
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

         fillSourceCmboBox();
      }

      #endregion

      #region  Fields

      private readonly CaptureManager mCaptureManager;

      private ISession mISession;

      #endregion

      #region Private methods

      private void aLlToolStripMenuItem_Click(object sender, EventArgs e)
      {
      }

      private void dSCrossbarToolStripMenuItem_Click(object sender, EventArgs e)
      {
         fillSourceCmboBox();
      }

      private void fillSourceCmboBox()
      {
         if (mCaptureManager == null) {
            return;
         }

         var doc = new XmlDocument();

         var lxmldoc = "";

         mCaptureManager.getCollectionOfSources(ref lxmldoc);

         if (string.IsNullOrEmpty(lxmldoc)) {
            return;
         }

         doc.LoadXml(lxmldoc);

         var lXPath = "//*[";

         if (toolStripMenuItem1.Checked) {
            lXPath += "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_CATEGORY']/SingleValue[@Value='CLSID_WebcamInterfaceDeviceCategory']";
         }

         if (toolStripMenuItem2.Checked) {
            if (toolStripMenuItem1.Checked) {
               lXPath += "or ";
            }

            lXPath += "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_HW_SOURCE']/SingleValue[@Value='Software device']";
         }

         if (dSCrossbarToolStripMenuItem.Checked) {
            if (toolStripMenuItem1.Checked ||
                toolStripMenuItem2.Checked) {
               lXPath += "or ";
            }

            lXPath += "Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_CATEGORY']/SingleValue[@Value='CLSID_VideoInputDeviceCategory']";
         }

         lXPath += "]";

         XmlNodeList lSourceNodes = null;

         try {
            lSourceNodes = doc.SelectNodes(lXPath);
         } catch (Exception) {
         }

         sourceComboBox.Items.Clear();

         if (lSourceNodes == null) {
            return;
         }

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

      private void label1_Click(object sender, EventArgs e)
      {
      }

      private void label2_Click(object sender, EventArgs e)
      {
      }

      private void label3_Click(object sender, EventArgs e)
      {
      }

      private void mediaTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
      {
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

      private void start_stopBtn_Click(object sender, EventArgs e)
      {
         if (start_stopBtn.Text == "Stop") {
            if (mISession != null) {
               mISession.closeSession();

               start_stopBtn.Text = "Start";
            }

            mISession = null;

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

         var lNode = lSourceNode.SelectSingleNode("Source.Attributes/Attribute[@Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK']/SingleValue/@Value");

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

         var lMediaTypeItem = (ContainerItem) mediaTypeComboBox.SelectedItem;

         if (lMediaTypeItem == null) {
            return;
         }

         lSourceNode = lMediaTypeItem.mXmlNode;

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


         var lxmldoc = "";

         mCaptureManager.getCollectionOfSinks(ref lxmldoc);

         var doc = new XmlDocument();

         doc.LoadXml(lxmldoc);

         var lSinkNode = doc.SelectSingleNode("SinkFactories/SinkFactory[@GUID='{2F34AF87-D349-45AA-A5F1-E4104D5C458E}']");

         if (lSinkNode == null) {
            return;
         }

         var lContainerNode = lSinkNode.SelectSingleNode("Value.ValueParts/ValuePart[1]");

         if (lContainerNode == null) {
            return;
         }

         IEVRSinkFactory lSinkFactory;

         var lSinkControl = mCaptureManager.createSinkControl();

         lSinkControl.createSinkFactory(Guid.Empty, out lSinkFactory);

         object lEVROutputNode;

         lSinkFactory.createOutputNode(mVideoPanel.Handle, out lEVROutputNode);

         if (lEVROutputNode == null) {
            return;
         }

         object lPtrSourceNode;

         var lSourceControl = mCaptureManager.createSourceControl();

         if (lSourceControl == null) {
            return;
         }


         lSourceControl.createSourceNode(lSymbolicLink, lStreamIndex, lMediaTypeIndex, lEVROutputNode, out lPtrSourceNode);


         var lSourceMediaNodeList = new List<object>();

         lSourceMediaNodeList.Add(lPtrSourceNode);

         var lSessionControl = mCaptureManager.createSessionControl();

         if (lSessionControl == null) {
            return;
         }

         mISession = lSessionControl.createSession(lSourceMediaNodeList.ToArray());

         if (mISession == null) {
            return;
         }

         mISession.startSession(0, Guid.Empty);

         start_stopBtn.Text = "Stop";

         Marshal.ReleaseComObject(lEVROutputNode);
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
               var lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[1]/@Value");

               var mTitle = lvalueNode.Value;

               lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_SIZE']/Value.ValueParts/ValuePart[2 ]/@Value");

               mTitle += "x" + lvalueNode.Value;

               lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_FRAME_RATE']/RatioValue/@Value");

               mTitle += ", " + lvalueNode.Value + " FPS, ";

               lvalueNode = lNode.SelectSingleNode("MediaTypeItem[@Name='MF_MT_SUBTYPE']/SingleValue/@Value");

               mTitle += lvalueNode.Value.Replace("MFVideoFormat_", "");


               var lSourceItem = new ContainerItem {
                  mFriendlyName = mTitle, // lvalueNode.Value.Replace("MFMediaType_", ""),
                  mXmlNode = lNode
               };

               mediaTypeComboBox.Items.Add(lSourceItem);
            }
         }
      }

      private void toolStripMenuItem1_Click(object sender, EventArgs e)
      {
      }

      private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
      {
         fillSourceCmboBox();
      }

      private void toolStripMenuItem2_Click(object sender, EventArgs e)
      {
         fillSourceCmboBox();
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
