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
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CaptureManagerToCSharpProxy.Interfaces;
using RtspClientExample;

namespace WPFRTSPClient
{
   internal class RTSPCaptureProcessor : ICaptureProcessor
   {
      #region Static fields

      private static Guid MFVideoFormat_H264 = new Guid("34363248-0000-0010-8000-00AA00389B71");

      #endregion

      #region Constructors and destructors

      private RTSPCaptureProcessor()
      {
      }

      #endregion

      #region  Fields

      // Create a RTSP Client
      private readonly RTSPClient m_client = new RTSPClient();

      private readonly MemoryStream m_proxyMemory = new MemoryStream();

      private ISourceRequestResult mISourceRequestResult;

      private AutoResetEvent mLockRead = new AutoResetEvent(false);

      private readonly AutoResetEvent mLockWrite = new AutoResetEvent(false);

      private string mPresentationDescriptor = "";

      private string mURL = "";

      #endregion

      #region Interface methods

      public void initilaize(IInitilaizeCaptureSource IInitilaizeCaptureSource)
      {
         if (IInitilaizeCaptureSource != null) {
            IInitilaizeCaptureSource.setPresentationDescriptor(mPresentationDescriptor);
         }
      }

      public void pause()
      {
      }

      public void setCurrentMediaType(ICurrentMediaType aICurrentMediaType)
      {
         if (aICurrentMediaType == null) {
            throw new NotImplementedException();
         }

         uint lStreamIndex = 0;

         uint lMediaTypeIndex = 0;

         aICurrentMediaType.getStreamIndex(out lStreamIndex);

         aICurrentMediaType.getMediaTypeIndex(out lMediaTypeIndex);

         if ((lStreamIndex != 0) ||
             (lMediaTypeIndex != 0)) {
            throw new NotImplementedException();
         }
      }

      public void shutdown()
      {
         mISourceRequestResult = null;

         if (m_client != null) {
            m_client.Stop();
         }

         mLockWrite.Set();
      }

      public void sourceRequest(ISourceRequestResult aISourceRequestResult)
      {
         if (aISourceRequestResult == null) {
            return;
         }

         mLockWrite.Set();

         if (mISourceRequestResult == null) {
            mISourceRequestResult = aISourceRequestResult;

            if (m_client != null) {
               m_client.Connect(mURL, RTSPClient.RTP_TRANSPORT.TCP);
            }
         }
      }

      public void start(long aStartPositionInHundredNanosecondUnits, ref Guid aGUIDTimeFormat)
      {
      }

      public void stop()
      {
      }

      #endregion

      #region Public methods

      public static async Task<ICaptureProcessor> createCaptureProcessor(string a_URL)
      {
         var lPresentationDescriptor = "<?xml version='1.0' encoding='UTF-8'?>" + "<PresentationDescriptor StreamCount='1'>" +
                                       "<PresentationDescriptor.Attributes Title='Attributes of Presentation'>" +
                                       "<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_SOURCE_TYPE_VIDCAP_SYMBOLIC_LINK' GUID='{58F0AAD8-22BF-4F8A-BB3D-D2C4978C6E2F}' Title='The symbolic link for a video capture driver.' Description='Contains the unique symbolic link for a video capture driver.'>" +
                                       "<SingleValue Value='RTSPCaptureProcessor' />" + "</Attribute>" +
                                       "<Attribute Name='MF_DEVSOURCE_ATTRIBUTE_FRIENDLY_NAME' GUID='{60D0E559-52F8-4FA2-BBCE-ACDB34A8EC01}' Title='The display name for a device.' Description='The display name is a human-readable string, suitable for display in a user interface.'>" +
                                       "<SingleValue Value='RTSP Capture Processor' />" + "</Attribute>" + "</PresentationDescriptor.Attributes>" +
                                       "<StreamDescriptor Index='0' MajorType='MFMediaType_Video' MajorTypeGUID='{73646976-0000-0010-8000-00AA00389B71}'>" + "<MediaTypes TypeCount='1'>" +
                                       "<MediaType Index='0'>" +
                                       "<MediaTypeItem Name='MF_MT_MAJOR_TYPE' GUID='{48EBA18E-F8C9-4687-BF11-0A74C9F96A8F}' Title='Major type GUID for a media type.' Description='The major type defines the overall category of the media data.'>" +
                                       "<SingleValue Value='MFMediaType_Video' GUID='{73646976-0000-0010-8000-00AA00389B71}' />" + "</MediaTypeItem>" +
                                       "<MediaTypeItem Name='CM_DIRECT_CALL' GUID='{DD0570F7-0D02-4897-A55E-F65BFACA1955}' Title='Independent of samples.' Description='Specifies for a media type whether each sample is independent of the other samples in the stream.'>" +
                                       "<SingleValue Value='True' />" + "</MediaTypeItem>" +
                                       "<MediaTypeItem Name='MF_MT_SUBTYPE' GUID='{F7E34C9A-42E8-4714-B74B-CB29D72C35E5}' Title='Subtype GUID for a media type.' Description='The subtype GUID defines a specific media format type within a major type.'>" +
                                       "<SingleValue GUID='{Temp_SubTypeGUID}' />" + "</MediaTypeItem>" + "</MediaType>" + "</MediaTypes>" + "</StreamDescriptor>" + "</PresentationDescriptor>";

         var lICaptureProcessor = new RTSPCaptureProcessor();

         lICaptureProcessor.mURL = a_URL;


         // The SPS/PPS comes from the SDP data
         // or it is the first SPS/PPS from the H264 video stream
         lICaptureProcessor.m_client.Received_SPS_PPS += (sps, pps) =>
         {
            if (lICaptureProcessor.mISourceRequestResult != null) {
               lICaptureProcessor.m_proxyMemory.Position = 0;

               lICaptureProcessor.m_proxyMemory.Write(new byte[] {
                  0x00, 0x00, 0x00, 0x01
               }, 0, 4); // Write Start Code
               lICaptureProcessor.m_proxyMemory.Write(sps, 0, sps.Length);
               lICaptureProcessor.m_proxyMemory.Write(new byte[] {
                  0x00, 0x00, 0x00, 0x01
               }, 0, 4); // Write Start Code
               lICaptureProcessor.m_proxyMemory.Write(pps, 0, pps.Length);

               var ldata = lICaptureProcessor.m_proxyMemory.ToArray();

               var lptrData = Marshal.AllocHGlobal(ldata.Length);

               Marshal.Copy(ldata, 0, lptrData, ldata.Length);

               lICaptureProcessor.mISourceRequestResult.setData(lptrData, (uint) ldata.Length, 1);

               Marshal.FreeHGlobal(lptrData);
            }

            Thread.Sleep(500);
         };

         // Video NALs. May also include the SPS and PPS in-band for H264
         lICaptureProcessor.m_client.Received_NALs += nal_units =>
         {
            foreach (var nal_unit in nal_units) {
               lICaptureProcessor.write(nal_unit);

               lICaptureProcessor.mLockWrite.WaitOne();

               lICaptureProcessor.mLockWrite.Reset();

               if (lICaptureProcessor.mISourceRequestResult == null) {
                  break;
               }
            }
         };


         lPresentationDescriptor = lPresentationDescriptor.Replace("Temp_SubTypeGUID", MFVideoFormat_H264.ToString());

         lICaptureProcessor.mPresentationDescriptor = lPresentationDescriptor;

         return lICaptureProcessor;
      }

      #endregion

      #region Private methods

      private void write(byte[] nal_unit)
      {
         if (mISourceRequestResult != null) {
            var l_proxyMemory = new MemoryStream();
            l_proxyMemory.Position = 0;

            l_proxyMemory.Write(new byte[] {
               0x00, 0x00, 0x00, 0x01
            }, 0, 4); // Write Start Code
            l_proxyMemory.Write(nal_unit, 0, nal_unit.Length); // Write NAL

            var ldata = l_proxyMemory.ToArray();

            var lptrData = Marshal.AllocHGlobal(ldata.Length);

            Marshal.Copy(ldata, 0, lptrData, ldata.Length);

            mISourceRequestResult.setData(lptrData, (uint) ldata.Length, 1);

            Marshal.FreeHGlobal(lptrData);
         }
      }

      #endregion
   }
}
