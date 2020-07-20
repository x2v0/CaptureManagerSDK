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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using CaptureManagerToCSharpProxy.Interfaces;
using DISPPARAMS = System.Runtime.InteropServices.ComTypes.DISPPARAMS;
using EXCEPINFO = System.Runtime.InteropServices.ComTypes.EXCEPINFO;

namespace CaptureManagerToCSharpProxy
{

    class EVRMultiSinkFactory: IEVRMultiSinkFactory
    {
        public static bool GetComMethod<T, U>(T comObj, int slot, out U method) where U : class
        {
            IntPtr objectAddress = Marshal.GetComInterfaceForObject(comObj, typeof(T));
            if (objectAddress == IntPtr.Zero)
            {
                method = null;
                return false;
            }

            try
            {
                IntPtr vTable = Marshal.ReadIntPtr(objectAddress, 0);
                IntPtr methodAddress = Marshal.ReadIntPtr(vTable, slot * IntPtr.Size);

                // We can't have a Delegate constraint, so we have to cast to
                // object then to our desired delegate
                method = (U)((object)Marshal.GetDelegateForFunctionPointer(methodAddress, typeof(U)));
                return true;
            }
            finally
            {
                Marshal.Release(objectAddress); // Prevent memory leak
            }
        }

        private CaptureManagerLibrary.IEVRMultiSinkFactory mIEVRMultiSinkFactory;

        private object mIUnknown;

        public EVRMultiSinkFactory(object aIUnknown)
        {
            mIEVRMultiSinkFactory = aIUnknown as CaptureManagerLibrary.IEVRMultiSinkFactory;

            mIUnknown = aIUnknown;
        }
        
        public bool createOutputNodes(
            IntPtr aHandle, 
            uint aOutputNodeAmount, 
            out List<object> aTopologyOutputNodesList)
        {
            bool lresult = false;
            
            aTopologyOutputNodesList = new List<object>();

            do
            {
                if (mIEVRMultiSinkFactory == null)
                    break;


                try
                {
                    object lArrayMediaNodes = new Object();

                    mIEVRMultiSinkFactory.createOutputNodes(
                        aHandle,
                        null,
                        aOutputNodeAmount,
                        out lArrayMediaNodes);

                    if (lArrayMediaNodes == null)
                        break;

                    object[] lArray = lArrayMediaNodes as object[];

                    if (lArray == null)
                        break;

                    aTopologyOutputNodesList.AddRange(lArray);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    if (mIUnknown != null)
                    {                        
                        try
                        {
                            object[] largs = new object[] { aHandle.ToInt64(), null, aOutputNodeAmount };

                            object lArrayMediaNodes = Win32NativeMethods.Invoke<object>(mIUnknown, Win32NativeMethods.InvokeFlags.DISPATCH_METHOD, "createOutputNodes", largs);

                            if (lArrayMediaNodes == null)
                                break;

                            object[] lArray = lArrayMediaNodes as object[];

                            if (lArray == null)
                                break;

                            aTopologyOutputNodesList.AddRange(lArray);

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


        public bool createOutputNodes(IntPtr aHandle, object aPtrUnkSharedResource, uint aOutputNodeAmount, out List<object> aTopologyOutputNodesList)
        {
            bool lresult = false;

            aTopologyOutputNodesList = new List<object>();

            do
            {
                if (mIEVRMultiSinkFactory == null)
                    break;


                try
                {
                    object lArrayMediaNodes = new Object();

                    mIEVRMultiSinkFactory.createOutputNodes(
                        aHandle,
                        aPtrUnkSharedResource,
                        aOutputNodeAmount,
                        out lArrayMediaNodes);

                    if (lArrayMediaNodes == null)
                        break;

                    object[] lArray = lArrayMediaNodes as object[];

                    if (lArray == null)
                        break;

                    aTopologyOutputNodesList.AddRange(lArray);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    if (mIUnknown != null)
                    {


                        try
                        {
                            object[] largs = new object[] { aHandle.ToInt64(), aPtrUnkSharedResource, aOutputNodeAmount};

                            object lArrayMediaNodes = Win32NativeMethods.Invoke<object>(mIUnknown, Win32NativeMethods.InvokeFlags.DISPATCH_METHOD, "createOutputNodes", largs);
                            
                            if (lArrayMediaNodes == null)
                                break;

                            object[] lArray = lArrayMediaNodes as object[];

                            if (lArray == null)
                                break;

                            aTopologyOutputNodesList.AddRange(lArray);

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
