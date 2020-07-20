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
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;

namespace InterProcessRenderer.Communication
{

    public delegate void MessageDelegate(string aMessage);
    
    public class PipeProcessor
    {
        private string mPipeServerName;

        private string mPipeClientName;

        private NamedPipeServerStream mPipeServer;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event MessageDelegate MessageDelegateEvent;

        public PipeProcessor(
            string aPipeServerName,
            string aPipeClientName)
        {

            mPipeServerName = aPipeServerName;

            mPipeClientName = aPipeClientName;

            startListen();
        }

        private void startListen()
        {
            mPipeServer = new NamedPipeServerStream(
                mPipeServerName,
                PipeDirection.In,
                100,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            mPipeServer.BeginWaitForConnection(
                new AsyncCallback(callBack),
                mPipeServer);

        }

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
            try
            {                
                using( NamedPipeClientStream pipeStream = new NamedPipeClientStream
                   (".", mPipeClientName, PipeDirection.Out))
                {

                    pipeStream.Connect(TimeOut);
                
                    StreamString lStreamString = new StreamString(pipeStream);

                    lStreamString.WriteString(SendStr);
                }

            }
            catch (Exception)
            {
            }
        }
               
        private void callBack(IAsyncResult aIAsyncResult)
        {
            using (var lPipeServer = (NamedPipeServerStream)aIAsyncResult.AsyncState)
                {
                    try
                    {
                        lPipeServer.EndWaitForConnection(aIAsyncResult);
                    }
                    catch(Exception)
                    {
                    }

                    // we have a connection established, time to wait for new ones while this thread does its business with the client
                    // this may look like a recursive call, but it is not: remember, we're in a lambda expression
                    // if this bothers you, just export the lambda into a named private method, like you did in your question
                    startListen();

                    if (!lPipeServer.IsConnected)
                        return;

                    StreamString lStreamString = new StreamString(lPipeServer);


                    string stringData = lStreamString.ReadString();

                    lPipeServer.Close();
                
                    if (MessageDelegateEvent != null)
                        MessageDelegateEvent(stringData);
                }            
           
        }

    }
}
