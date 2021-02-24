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
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;

namespace InterProcessRenderer.Communication
{
   public delegate void MessageDelegate(string aMessage);

   public class PipeProcessor
   {
      #region Constructors and destructors

      public PipeProcessor(string aPipeServerName, string aPipeClientName)
      {
         mPipeServerName = aPipeServerName;

         mPipeClientName = aPipeClientName;

         startListen();
      }

      #endregion

      #region  Fields

      private readonly string mPipeClientName;

      private NamedPipeServerStream mPipeServer;
      private readonly string mPipeServerName;

      #endregion

      #region Public events

      [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
      public event MessageDelegate MessageDelegateEvent;

      #endregion

      #region Public methods

      public void closeConnection()
      {
         mPipeServer.Close();
      }

      public string getServerName()
      {
         return mPipeServerName;
      }

      public void send(string SendStr, int TimeOut = 10000)
      {
         try {
            using (var pipeStream = new NamedPipeClientStream(".", mPipeClientName, PipeDirection.Out)) {
               pipeStream.Connect(TimeOut);

               var lStreamString = new StreamString(pipeStream);

               lStreamString.WriteString(SendStr);
            }
         } catch (Exception) {
         }
      }

      #endregion

      #region Private methods

      private void callBack(IAsyncResult aIAsyncResult)
      {
         using (var lPipeServer = (NamedPipeServerStream) aIAsyncResult.AsyncState) {
            try {
               lPipeServer.EndWaitForConnection(aIAsyncResult);
            } catch (Exception) {
            }

            // we have a connection established, time to wait for new ones while this thread does its business with the client
            // this may look like a recursive call, but it is not: remember, we're in a lambda expression
            // if this bothers you, just export the lambda into a named private method, like you did in your question
            startListen();

            if (!lPipeServer.IsConnected) {
               return;
            }

            var lStreamString = new StreamString(lPipeServer);


            var stringData = lStreamString.ReadString();

            lPipeServer.Close();

            if (MessageDelegateEvent != null) {
               MessageDelegateEvent(stringData);
            }
         }
      }

      private void startListen()
      {
         mPipeServer = new NamedPipeServerStream(mPipeServerName, PipeDirection.In, 100, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

         mPipeServer.BeginWaitForConnection(callBack, mPipeServer);
      }

      #endregion
   }
}
