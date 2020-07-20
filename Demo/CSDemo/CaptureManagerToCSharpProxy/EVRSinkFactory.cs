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
using System.Linq;
using System.Text;
using CaptureManagerToCSharpProxy.Interfaces;

namespace CaptureManagerToCSharpProxy
{
    class EVRSinkFactory : IEVRSinkFactory
    {
        private CaptureManagerLibrary.IEVRSinkFactory mIEVRSinkFactory;

        public EVRSinkFactory(
            CaptureManagerLibrary.IEVRSinkFactory aIEVRSinkFactory)
        {
            mIEVRSinkFactory = aIEVRSinkFactory;
        }

        public bool createOutputNode(
            IntPtr aHandle,
            out object aTopologyNode)
        {
            bool lresult = false;

            aTopologyNode = null;

            do
            {
                if (mIEVRSinkFactory == null)
                    break;
                
                try
                {
                    mIEVRSinkFactory.createOutputNode(
                        aHandle,
                        out aTopologyNode);
                    
                    if (aTopologyNode == null)
                        break;

                    lresult = true;
                }
                catch (Exception exc)
                {
                    if (mIEVRSinkFactory != null)
                    {
                        try
                        {
                            object[] largs = new object[] { aHandle.ToInt64()};

                            aTopologyNode = Win32NativeMethods.Invoke<object>(mIEVRSinkFactory, Win32NativeMethods.InvokeFlags.DISPATCH_METHOD, "createOutputNode", largs);

                            if (aTopologyNode == null)
                                break;

                            lresult = true;
                        }
                        catch (Exception exc1)
                        {
                            LogManager.getInstance().write(exc1.Message);
                        }
                    }
                    else
                        LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }
    }
}
