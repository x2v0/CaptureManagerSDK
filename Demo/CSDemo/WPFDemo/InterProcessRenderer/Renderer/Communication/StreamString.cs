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

using System.IO;
using System.Text;

namespace InterProcessRenderer.Communication
{
   internal class StreamString
   {
      #region Constructors and destructors

      public StreamString(Stream ioStream)
      {
         this.ioStream = ioStream;
         streamEncoding = new UnicodeEncoding();
      }

      #endregion

      #region  Fields

      private readonly Stream ioStream;
      private readonly UnicodeEncoding streamEncoding;

      #endregion

      #region Public methods

      public string ReadString()
      {
         var len = 0;

         len = ioStream.ReadByte() << 24;

         len += ioStream.ReadByte() << 16;

         len += ioStream.ReadByte() << 8;

         len += ioStream.ReadByte();

         var inBuffer = new byte[len];

         ioStream.Read(inBuffer, 0, len);

         return streamEncoding.GetString(inBuffer);
      }

      public int WriteString(string outString)
      {
         var outBuffer = streamEncoding.GetBytes(outString);
         var len = outBuffer.Length;
         ioStream.WriteByte((byte) (len >> 24));
         ioStream.WriteByte((byte) (len >> 16));
         ioStream.WriteByte((byte) (len >> 8));
         ioStream.WriteByte((byte) len);
         ioStream.Write(outBuffer, 0, len);
         ioStream.Flush();
         return outBuffer.Length + 4;
      }

      #endregion
   }
}
