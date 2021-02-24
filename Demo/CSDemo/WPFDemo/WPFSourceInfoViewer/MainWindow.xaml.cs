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
using System.Windows.Data;
using System.Xml;
using CaptureManagerToCSharpProxy;

namespace WPFSourceInfoViewer
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

         CaptureManager lCaptureManager = null;

         try {
            lCaptureManager = new CaptureManager("CaptureManager.dll");
         } catch (Exception exc) {
            try {
               lCaptureManager = new CaptureManager();
            } catch (Exception exc1) {
            }
         }

         if (lCaptureManager == null) {
            return;
         }

         var lXmlDataProvider = (XmlDataProvider) Resources["XmlLogProvider"];

         if (lXmlDataProvider == null) {
            return;
         }

         var doc = new XmlDocument();

         var lxmldoc = "";

         lCaptureManager.getCollectionOfSources(ref lxmldoc);

         doc.LoadXml(lxmldoc);

         lXmlDataProvider.Document = doc;
      }

      #endregion
   }
}
