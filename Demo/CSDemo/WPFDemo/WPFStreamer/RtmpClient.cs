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
using System.Runtime.InteropServices;

namespace WPFStreamer
{
   internal class RtmpClient
   {
      #region Constructors and destructors

      private RtmpClient()
      {
      }

      ~RtmpClient()
      {
         if (m_handler != -1) {
            Disconnect(m_handler);
         }
      }

      #endregion

      #region  Fields

      private int m_handler = -1;

      #endregion

      #region Public methods

      [DllImport("rtmp.dll", CharSet = CharSet.Auto)]
      public static extern int Connect([MarshalAs(UnmanagedType.LPStr)]
                                       string a_streamsXml,
                                       [MarshalAs(UnmanagedType.LPStr)]
                                       string a_url);

      public static RtmpClient createInstance(string a_streamsXml, string a_url)
      {
         RtmpClient l_instance = null;

         try {
            do {
               l_instance = new RtmpClient();

               l_instance.m_handler = Connect(a_streamsXml, a_url);
            } while (false);
         } catch (Exception) {
         }

         return l_instance;
      }

      [DllImport("rtmp.dll", CharSet = CharSet.Auto)]
      public static extern void Disconnect(int handler);

      [DllImport("rtmp.dll", CharSet = CharSet.Auto)]
      public static extern int Write(int handler, int sampleTime, IntPtr buf, int size, uint is_keyframe, int streamIdx, int isVideo);

      public void disconnect()
      {
         Disconnect(m_handler);
      }

      public void sendAudioData(int sampleTime, IntPtr buf, int size, uint sampleflags, int streamIdx)
      {
         Write(m_handler, sampleTime, buf, size, sampleflags, streamIdx, 0);
      }

      public void sendVideoData(int sampleTime, IntPtr buf, int size, uint sampleflags, int streamIdx)
      {
         Write(m_handler, sampleTime, buf, size, sampleflags, streamIdx, 1);
      }

      #endregion
   }
}
