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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using CaptureManagerToCSharpProxy.Interfaces;

namespace WPFRecording
{
   internal class NetworkStreamSink : AbstractSink
   {
      #region Constructors and destructors

      public NetworkStreamSink(IByteStreamSinkFactory aSinkFactory, string aMIME)
      {
         mSinkFactory = aSinkFactory;

         mMIME = aMIME;
      }

      #endregion

      #region  Fields

      private IMFByteStream mIMFByteStream;

      private readonly string mMIME = "";

      private readonly IByteStreamSinkFactory mSinkFactory;

      #endregion

      #region Enums

      [Flags] [UnmanagedName("MFASYNC_* defines")]
      public enum MFASync
      {
         None = 0,
         FastIOProcessingCallback = 0x00000001,
         SignalCallback = 0x00000002,
         BlockingCallback = 0x00000004,
         ReplyCallback = 0x00000008,
         LocalizeRemoteCallback = 0x00000010
      }

      [UnmanagedName("MFASYNC_CALLBACK_QUEUE_ defines")]
      public enum MFAsyncCallbackQueue
      {
         Undefined = 0x00000000,
         Standard = 0x00000001,
         RT = 0x00000002,
         IO = 0x00000003,
         Timer = 0x00000004,
         MultiThreaded = 0x00000005,
         LongFunction = 0x00000007,
         PrivateMask = unchecked((int) 0xFFFF0000),
         All = unchecked((int) 0xFFFFFFFF)
      }

      [Flags] [UnmanagedName("MFBYTESTREAM_* defines")]
      public enum MFByteStreamCapabilities
      {
         None = 0x00000000,
         IsReadable = 0x00000001,
         IsWritable = 0x00000002,
         IsSeekable = 0x00000004,
         IsRemote = 0x00000008,
         IsDirectory = 0x00000080,
         HasSlowSeek = 0x00000100,
         IsPartiallyDownloaded = 0x00000200,
         ShareWrite = 0x00000400,
         DoesNotUseNetwork = 0x00000800
      }

      [Flags] [UnmanagedName("MFBYTESTREAM_SEEK_FLAG_ defines")]
      public enum MFByteStreamSeekingFlags
      {
         None = 0,
         CancelPendingIO = 1
      }

      [UnmanagedName("MFBYTESTREAM_SEEK_ORIGIN")]
      public enum MFByteStreamSeekOrigin
      {
         Begin,
         Current
      }

      #endregion

      #region Interfaces

      [ComImport] [SuppressUnmanagedCodeSecurity] [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] [Guid("A27003CF-2354-4F2A-8D6A-AB7CFF15437E")]
      public interface IMFAsyncCallback
      {
         [PreserveSig]
         int GetParameters(out MFASync pdwFlags, out MFAsyncCallbackQueue pdwQueue);

         [PreserveSig]
         int Invoke([In] [MarshalAs(UnmanagedType.Interface)]
                    IMFAsyncResult pAsyncResult);
      }

      [ComImport] [SuppressUnmanagedCodeSecurity] [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] [Guid("AC6B7889-0740-4D51-8619-905994A55CC6")]
      public interface IMFAsyncResult
      {
         [PreserveSig]
         int GetState([MarshalAs(UnmanagedType.IUnknown)]
                      out object ppunkState);

         [PreserveSig]
         int GetStatus();

         [PreserveSig]
         int SetStatus([In] [MarshalAs(UnmanagedType.Error)]
                       int hrStatus);

         [PreserveSig]
         int GetObject([MarshalAs(UnmanagedType.Interface)]
                       out object ppObject);

         [PreserveSig]
         IntPtr GetStateNoAddRef();
      }

      [ComImport] [SuppressUnmanagedCodeSecurity] [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)] [Guid("AD4C1B00-4BF7-422F-9175-756693D9130D")]
      public interface IMFByteStream
      {
         [PreserveSig]
         int GetCapabilities(out MFByteStreamCapabilities pdwCapabilities);

         [PreserveSig]
         int GetLength(out long pqwLength);

         [PreserveSig]
         int SetLength([In] long qwLength);

         [PreserveSig]
         int GetCurrentPosition(out long pqwPosition);

         [PreserveSig]
         int SetCurrentPosition([In] long qwPosition);

         [PreserveSig]
         int IsEndOfStream([MarshalAs(UnmanagedType.Bool)]
                           out bool pfEndOfStream);

         [PreserveSig]
         int Read(IntPtr pb, [In] int cb, out int pcbRead);

         [PreserveSig]
         int BeginRead(IntPtr pb,
                       [In] int cb,
                       [In] [MarshalAs(UnmanagedType.Interface)]
                       IMFAsyncCallback pCallback,
                       [In] [MarshalAs(UnmanagedType.IUnknown)]
                       object pUnkState);

         [PreserveSig]
         int EndRead([In] [MarshalAs(UnmanagedType.Interface)]
                     IMFAsyncResult pResult,
                     out int pcbRead);

         [PreserveSig]
         int Write(IntPtr pb, [In] int cb, out int pcbWritten);

         [PreserveSig]
         int BeginWrite(IntPtr pb,
                        [In] int cb,
                        [In] [MarshalAs(UnmanagedType.Interface)]
                        IMFAsyncCallback pCallback,
                        [In] [MarshalAs(UnmanagedType.IUnknown)]
                        object pUnkState);

         [PreserveSig]
         int EndWrite([In] [MarshalAs(UnmanagedType.Interface)]
                      IMFAsyncResult pResult,
                      out int pcbWritten);

         [PreserveSig]
         int Seek([In] MFByteStreamSeekOrigin SeekOrigin, [In] long llSeekOffset, [In] MFByteStreamSeekingFlags dwSeekFlags, out long pqwCurrentPosition);

         [PreserveSig]
         int Flush();

         [PreserveSig]
         int Close();
      }

      #endregion

      #region Public methods

      public override object getOutputNode(object aUpStreamMediaType)
      {
         var lCompressedMediaTypeList = new List<object>();

         lCompressedMediaTypeList.Add(aUpStreamMediaType);

         var lTopologyOutputNodesList = new List<object>();

         if (mIMFByteStream != null) {
            mSinkFactory.createOutputNodes(lCompressedMediaTypeList, mIMFByteStream, out lTopologyOutputNodesList);
         }

         return lTopologyOutputNodesList[0];
      }

      public override void setOptions(string aOptions)
      {
         var lConfig = new HttpOutputByteStream.WebServerConfiguration();

         lConfig.ServerName = "ASF server";

         lConfig.IPAddress = IPAddress.Loopback;

         lConfig.MIME = mMIME;

         ushort lport;

         if (ushort.TryParse(aOptions, out lport)) {
            lConfig.Port = lport;
         } else {
            lConfig.Port = 8080;
         }

         mIMFByteStream = HttpOutputByteStream.createByteStream(lConfig);
      }

      #endregion

      #region Nested classes

      [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
      public class UnmanagedNameAttribute : Attribute
      {
         #region Constructors and destructors

         public UnmanagedNameAttribute(string s)
         {
            m_Name = s;
         }

         #endregion

         #region  Fields

         private readonly string m_Name;

         #endregion

         #region Public methods

         public override string ToString()
         {
            return m_Name;
         }

         #endregion
      }

      private class HttpOutputByteStream : IMFByteStream
      {
         #region Constructors and destructors

         private HttpOutputByteStream(WebServerConfiguration aConfig)
         {
            mWebServer = new WebServer(aConfig);

            mWebServer.Start();
         }

         #endregion

         #region  Fields

         private readonly WebServer mWebServer;

         #endregion

         #region Enums

         /// <summary>
         ///    HTTPS StatusCodes
         /// </summary>
         internal enum StatusCode
         {
            /// <summary>
            ///    200 OK
            /// </summary>
            OK,

            /// <summary>
            ///    400 Bad Request
            /// </summary>
            BadRequest,

            /// <summary>
            ///    403 Access Forbidden
            /// </summary>
            Forbiden
         }

         #endregion

         #region Interface methods

         public int BeginRead(IntPtr pb, int cb, IMFAsyncCallback pCallback, object pUnkState)
         {
            throw new NotImplementedException();
         }

         public int BeginWrite(IntPtr pb, int cb, IMFAsyncCallback pCallback, object pUnkState)
         {
            mWebServer.sendRawData(pb, cb);

            var lAsyncWriteData = new AsyncWriteData {
               cb = cb,
               pCallback = pCallback,
               punkState = pUnkState
            };

            ThreadPool.QueueUserWorkItem(lAsyncWriteData.execute);

            return 0;
         }

         public int Close()
         {
            mWebServer.Stop();

            return 0;
         }

         public int EndRead(IMFAsyncResult pResult, out int pcbRead)
         {
            throw new NotImplementedException();
         }

         public int EndWrite(IMFAsyncResult pResult, out int pcbWritten)
         {
            pcbWritten = 0;

            do {
               if (pResult == null) {
                  break;
               }

               var lstatus = pResult.GetStatus();

               if (lstatus != 0) {
                  break;
               }

               object lIUnknow;

               var lresult = pResult.GetObject(out lIUnknow);

               if (lresult != 0) {
                  break;
               }

               var lAsyncWriteData = lIUnknow as AsyncWriteData;

               if (lAsyncWriteData == null) {
                  break;
               }

               pcbWritten = lAsyncWriteData.cb;
            } while (false);

            return 0;
         }

         public int Flush()
         {
            return 0;
         }

         public int GetCapabilities(out MFByteStreamCapabilities pdwCapabilities)
         {
            pdwCapabilities = MFByteStreamCapabilities.IsWritable;

            return 0;
         }

         public int GetCurrentPosition(out long pqwPosition)
         {
            throw new NotImplementedException();
         }

         public int GetLength(out long pqwLength)
         {
            throw new NotImplementedException();
         }

         public int IsEndOfStream(out bool pfEndOfStream)
         {
            throw new NotImplementedException();
         }

         public int Read(IntPtr pb, int cb, out int pcbRead)
         {
            throw new NotImplementedException();
         }

         public int Seek(MFByteStreamSeekOrigin SeekOrigin, long llSeekOffset, MFByteStreamSeekingFlags dwSeekFlags, out long pqwCurrentPosition)
         {
            throw new NotImplementedException();
         }

         public int SetCurrentPosition(long qwPosition)
         {
            throw new NotImplementedException();
         }

         public int SetLength(long qwLength)
         {
            throw new NotImplementedException();
         }

         public int Write(IntPtr pb, int cb, out int pcbWritten)
         {
            mWebServer.writeHeader(pb, cb);

            pcbWritten = cb;

            return 0;
         }

         #endregion

         #region Public methods

         public static IMFByteStream createByteStream(WebServerConfiguration aConfig)
         {
            return new HttpOutputByteStream(aConfig);
         }

         #endregion

         #region Nested classes

         public class WebServerConfiguration
         {
            #region  Fields

            #endregion

            #region Public properties

            /// <summary>
            ///    IPAddress assigned to web server
            /// </summary>
            public IPAddress IPAddress
            {
               get;
               set;
            } = IPAddress.Loopback;

            /// <summary>
            ///    Server name to be sent in headers
            /// </summary>
            public string MIME
            {
               get;
               set;
            } = "";

            /// <summary>
            ///    Commucation port for incoming connections
            /// </summary>
            public int Port
            {
               get;
               set;
            } = 8080;

            /// <summary>
            ///    Server name to be sent in headers
            /// </summary>
            public string ServerName
            {
               get;
               set;
            } = "HttpOutputByteStreamWebServer";

            #endregion
         }

         private class AsyncResult : IMFAsyncResult
         {
            #region  Fields

            public int mHRStatus;

            public object pCallback;

            public object punkState;

            #endregion

            #region Interface methods

            public int GetObject(out object ppObject)
            {
               ppObject = pCallback;

               return 0;
            }

            public int GetState(out object ppunkState)
            {
               ppunkState = punkState;

               return 0;
            }

            public IntPtr GetStateNoAddRef()
            {
               var lUnknow = Marshal.GetIUnknownForObject(punkState);

               Marshal.Release(lUnknow);

               return lUnknow;
            }

            public int GetStatus()
            {
               return mHRStatus;
            }

            public int SetStatus(int hrStatus)
            {
               mHRStatus = hrStatus;

               return 0;
            }

            #endregion
         }


         private class AsyncWriteData
         {
            #region  Fields

            public int cb;
            public IMFAsyncCallback pCallback;

            public object punkState;

            #endregion

            #region Public methods

            public void execute(object state)
            {
               var lAsyncResult = new AsyncResult {
                  mHRStatus = 0,
                  pCallback = this,
                  punkState = punkState
               };

               var l = pCallback.Invoke(lAsyncResult);
            }

            #endregion
         }

         private class WebServer
         {
            #region Constructors and destructors

            /// <summary>
            ///    New WebServer
            /// </summary>
            /// <param name="webServerConf">WebServer Configuration</param>
            public WebServer(WebServerConfiguration webServerConf)
            {
               Configuration = webServerConf;
            }

            #endregion

            #region  Fields

            private readonly WebServerConfiguration Configuration;
            private readonly ConcurrentDictionary<TcpClient, TcpClient> mClientBag = new ConcurrentDictionary<TcpClient, TcpClient>();

            private byte[] mHeaderMemory;
            private TcpListener tcpListener;

            #endregion

            #region Public methods

            public void sendRawData(IntPtr pb, int cb)
            {
               do {
                  var lTempMemory = new byte[cb];

                  Marshal.Copy(pb, lTempMemory, 0, cb);

                  foreach (var item in mClientBag) {
                     if (!SendToClient(lTempMemory, cb, item.Value)) {
                        TcpClient lclient;

                        mClientBag.TryRemove(item.Key, out lclient);

                        if (lclient != null) {
                           lclient.Client.Close();

                           lclient.Close();

                           lclient.Client.Dispose();
                        }
                     }
                  }
               } while (false);
            }

            /// <summary>
            ///    Starts the WebServer thread
            /// </summary>
            public void Start()
            {
               try {
                  tcpListener = new TcpListener(Configuration.IPAddress, Configuration.Port);

                  tcpListener.Start();

                  tcpListener.BeginAcceptTcpClient(callBack, tcpListener);
               } catch (Exception) {
               }
            }

            /// <summary>
            ///    Stops the WebServer thread
            /// </summary>
            public void Stop()
            {
               try {
                  tcpListener.Stop();


                  foreach (var item in mClientBag) {
                     item.Value.Client.Close();

                     item.Value.Client.Dispose();

                     item.Value.Close();
                  }

                  tcpListener.Server.Dispose();
               } catch (Exception) {
               }
            }

            public void writeHeader(IntPtr pb, int cb)
            {
               do {
                  lock (this) {
                     mHeaderMemory = new byte[cb];

                     Marshal.Copy(pb, mHeaderMemory, 0, cb);

                     sendRawData(pb, cb);
                  }
               } while (false);
            }

            #endregion

            #region Private methods

            private void callBack(IAsyncResult aIAsyncResult)
            {
               var ltcpListener = (TcpListener) aIAsyncResult.AsyncState;

               if (ltcpListener == null) {
                  return;
               }

               TcpClient lclient = null;

               try {
                  lclient = ltcpListener.EndAcceptTcpClient(aIAsyncResult);
               } catch (Exception) {
                  return;
               }

               if ((lclient != null) &&
                   lclient.Client.Connected) {
                  var streamReader = new StreamReader(lclient.GetStream());

                  // Read full request with client header
                  var receivedData = new StringBuilder();
                  while (streamReader.Peek() > -1) {
                     receivedData.Append(streamReader.ReadLine());
                  }

                  var request = GetRequest(receivedData.ToString());

                  if (!SuportedMethod(request)) {
                     SendError(StatusCode.BadRequest, "Only GET is supported.", lclient);

                     lclient.Client.Close();
                     lclient.Close();
                  } else {
                     var socket = lclient.Client;

                     if (socket.Connected) {
                        SendHeader(StatusCode.OK, lclient);

                        lock (this) {
                           if (mHeaderMemory != null) {
                              var sentBytes = socket.Send(mHeaderMemory);
                           }

                           mClientBag[lclient] = lclient;
                        }
                     }
                  }
               }

               ltcpListener.BeginAcceptTcpClient(callBack, ltcpListener);
            }

            /// <summary>
            ///    Generates error page
            /// </summary>
            /// <param name="statusCode">StatusCode</param>
            /// <param name="message">Message</param>
            /// <returns>ErrorPage</returns>
            private string GetErrorPage(StatusCode statusCode, string message)
            {
               var status = GetStatusCode(statusCode);

               var errorMessage = new StringBuilder();
               errorMessage.Append("<html>\n");
               errorMessage.Append("<head>\n");
               errorMessage.Append(string.Format("<title>{0}</title>\n", status));
               errorMessage.Append("</head>\n");
               errorMessage.Append("<body>\n");
               errorMessage.Append(string.Format("<h1>{0}</h1>\n", status));
               errorMessage.Append(string.Format("<p>{0}</p>\n", message));
               errorMessage.Append("<hr>\n");
               errorMessage.Append(string.Format("<address>{0} Server at {1} Port {2} </address>\n", Configuration.ServerName, Configuration.IPAddress, Configuration.Port));
               errorMessage.Append("</body>\n");
               errorMessage.Append("</html>\n");
               return errorMessage.ToString();
            }

            /// <summary>
            ///    Gets request from input string
            /// </summary>
            /// <param name="data">Input string</param>
            /// <returns>Request</returns>
            private string GetRequest(string data)
            {
               var m = Regex.Match(data, @"^[^\n]*");
               return m.Value;
            }

            /// <summary>
            ///    Gets URI from request
            /// </summary>
            /// <param name="request">Reuqest string</param>
            /// <returns>URI</returns>
            private string GetRequestedURI(string request)
            {
               var startPos = request.IndexOf("HTTP", 1);
               request = startPos > 0 ? request.Substring(0, startPos - 1) : request.Substring(0);

               request = request.Replace("\\", "/");

               if ((request.IndexOf(".") < 1) &&
                   !request.EndsWith("/")) {
                  request = request + "/ ";
               }

               return request.Substring(request.IndexOf("/")).Trim();
            }

            /// <summary>
            ///    Gets string representation for the status code
            /// </summary>
            /// <param name="statusCode">Status code</param>
            /// <returns>Status code as HTTP string</returns>
            private string GetStatusCode(StatusCode statusCode)
            {
               string code;

               switch (statusCode) {
                  case StatusCode.OK:
                     code = "200 OK";
                     break;
                  case StatusCode.BadRequest:
                     code = "400 Bad Request";
                     break;
                  case StatusCode.Forbiden:
                     code = "403 Forbidden";
                     break;
                  default:
                     code = "202 Accepted";
                     break;
               }

               return code;
            }

            /// <summary>
            ///    Sends error page to the client
            /// </summary>
            /// <param name="statusCode">Status code</param>
            /// <param name="message">Error message</param>
            private void SendError(StatusCode statusCode, string message, TcpClient aTcpClient)
            {
               var page = GetErrorPage(statusCode, message);
               SendHeader(null, page.Length, statusCode, aTcpClient);
               SendToClient(page, aTcpClient);
            }


            /// <summary>
            ///    Sends HTTP header
            /// </summary>
            /// <param name="mimeType">Mime Type</param>
            /// <param name="totalBytes">Length of the response</param>
            /// <param name="statusCode">Status code</param>
            private void SendHeader(string mimeType, long totalBytes, StatusCode statusCode, TcpClient aTcpClient)
            {
               var header = new StringBuilder();
               header.Append(string.Format("HTTP/1.1 {0}\r\n", GetStatusCode(statusCode)));
               header.Append(string.Format("Content-Type: {0}\r\n", mimeType));
               header.Append("Accept-Ranges: bytes\r\n");
               header.Append(string.Format("Server: {0}\r\n", Configuration.ServerName));
               header.Append("Connection: close\r\n");
               header.Append(string.Format("Content-Length: {0}\r\n", totalBytes));
               header.Append("\r\n");

               SendToClient(header.ToString(), aTcpClient);
            }

            private void SendHeader(StatusCode statusCode, TcpClient aTcpClient)
            {
               var header = new StringBuilder();
               header.Append(string.Format("HTTP/1.1 {0}\r\n", GetStatusCode(statusCode)));
               header.Append(string.Format("Content-Type: {0}\r\n", Configuration.MIME));
               header.Append(string.Format("Server: {0}\r\n", Configuration.ServerName));
               header.Append("Accept-Ranges: none\r\n");
               header.Append("TransferMode.DLNA.ORG: Streaming\r\n");
               header.Append("Connection: keep-alive\r\n");
               header.Append("\r\n");

               SendToClient(header.ToString(), aTcpClient);
            }

            /// <summary>
            ///    Send string data to client
            /// </summary>
            /// <param name="data">String data</param>
            private void SendToClient(string data, TcpClient aTcpClient)
            {
               var bytes = Encoding.ASCII.GetBytes(data);
               SendToClient(bytes, bytes.Length, aTcpClient);
            }

            /// <summary>
            ///    Sends byte array to client
            /// </summary>
            /// <param name="data">Data array</param>
            /// <param name="bytesTosend">Data length</param>
            private bool SendToClient(byte[] data, int bytesTosend, TcpClient aTcpClient)
            {
               var lresult = true;

               try {
                  var socket = aTcpClient.Client;

                  if (socket.Connected) {
                     var sentBytes = socket.Send(data, 0, bytesTosend, 0);
                     if (sentBytes < bytesTosend) {
                        Console.WriteLine("Data was not completly send.");
                     }
                  } else {
                     lresult = false;

                     Console.WriteLine("Connection lost");
                  }
               } catch (Exception e) {
                  Console.WriteLine("Error: " + e);

                  lresult = false;
               }

               return lresult;
            }

            /// <summary>
            ///    Checks whether the method in request is supported
            /// </summary>
            /// <param name="request">Request</param>
            /// <returns>True if method is supported</returns>
            private bool SuportedMethod(string request)
            {
               if ((request == null) ||
                   (request.Length < 3)) {
                  return false;
               }

               return request.Substring(0, 3) != "GET" ? false : true;
            }

            #endregion
         }

         #endregion
      }

      #endregion
   }
}
