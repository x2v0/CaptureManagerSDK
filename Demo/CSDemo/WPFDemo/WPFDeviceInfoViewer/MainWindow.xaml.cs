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

using CaptureManagerToCSharpProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFDeviceInfoViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            CaptureManager lCaptureManager = null;

            try
            {
                lCaptureManager = new CaptureManager("CaptureManager.dll");
            }
            catch (System.Exception exc)
            {
                try
                {
                    lCaptureManager = new CaptureManager();
                }
                catch (System.Exception exc1)
                {

                }
            }

            if (lCaptureManager == null)
                return;

            XmlDataProvider lXmlDataProvider = (XmlDataProvider)this.Resources["XmlLogProvider"];

            if (lXmlDataProvider == null)
                return;

            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();

            string lxmldoc = "";

            lCaptureManager.getCollectionOfSources(ref lxmldoc);

            doc.LoadXml(lxmldoc);

            var lDeviceLinkNodeList = doc.SelectNodes("Sources/Source/Source.Attributes/Attribute[@Name='CM_DEVICE_LINK']/SingleValue/@Value");

            List<string> lDeviceLinkList = new List<string>();

            for (int i = 0; i < lDeviceLinkNodeList.Count; i++)
            {
                if (!lDeviceLinkList.Contains(lDeviceLinkNodeList.Item(i).Value))
                {
                    lDeviceLinkList.Add(lDeviceLinkNodeList.Item(i).Value);
                }
            }
            
            System.Xml.XmlDocument groupDoc = new System.Xml.XmlDocument();

            var lroot = groupDoc.CreateElement("Sources");

            groupDoc.AppendChild(lroot);

            foreach (var item in lDeviceLinkList)
            {
                var ldevices = doc.SelectNodes("Sources/Source[Source.Attributes/Attribute[@Name='CM_DEVICE_LINK']/SingleValue[@Value='" + item + "']]");

                if (ldevices != null)
                {
                    var lgroup = groupDoc.CreateElement("DeviceGroup");

                    var lTitle = groupDoc.CreateAttribute("Title");

                    lTitle.Value = item;

                    lgroup.Attributes.Append(lTitle);

                    foreach (var node in ldevices)
                    {
                        var lSourceNode = groupDoc.ImportNode((node as System.Xml.XmlNode), true);

                        lgroup.AppendChild(lSourceNode);
                    }

                    lroot.AppendChild(lgroup);
                }
            }

            
            lXmlDataProvider.XPath = "Sources/DeviceGroup";

            lXmlDataProvider.Document = groupDoc;
        }
    }
}
