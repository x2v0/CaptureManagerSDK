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
using System.Runtime.InteropServices;

namespace CaptureManagerToCSharpProxy
{
    class EncoderControl : IEncoderControl
    {
        CaptureManagerLibrary.IEncoderControl mIEncoderControl;

        public EncoderControl(CaptureManagerLibrary.IEncoderControl aIEncoderControl)
        {
            mIEncoderControl = aIEncoderControl;
        }

        public bool createEncoderNodeFactory(
            Guid aRefEncoderTypeGUID,
            out IEncoderNodeFactory aIEncoderNodeFactory)
        {
            bool lresult = false;

            aIEncoderNodeFactory = null;

            do
            {
                if (mIEncoderControl == null)
                    break;
                
                try
                {
                    object IUnknown;

                    mIEncoderControl.createEncoderNodeFactory(
                        aRefEncoderTypeGUID,
                        typeof(CaptureManagerLibrary.IEncoderNodeFactory).GUID,
                        out IUnknown);

                    if (IUnknown == null)
                        break;

                    var lIEncoderNodeFactory = IUnknown as CaptureManagerLibrary.IEncoderNodeFactory;

                    if (lIEncoderNodeFactory == null)
                        break;
                    
                    aIEncoderNodeFactory = new EncoderNodeFactory(lIEncoderNodeFactory);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool getCollectionOfEncoders(out string aPtrPtrXMLstring)
        {
            bool lresult = false;

            aPtrPtrXMLstring = "";

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                if (mIEncoderControl == null)
                    break;

                try
                {
                    (mIEncoderControl as IEncoderControlInner).getCollectionOfEncoders(
                            out lPtrXMLstring);

                    if (lPtrXMLstring != IntPtr.Zero)
                        aPtrPtrXMLstring = Marshal.PtrToStringBSTR(lPtrXMLstring);
                    
                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            if (lPtrXMLstring != IntPtr.Zero)
                Marshal.FreeBSTR(lPtrXMLstring);

            return lresult;
        }
        
        public bool getMediaTypeCollectionOfEncoder(
            object aPtrUncompressedMediaType, 
            Guid aRefEncoderCLSID, 
            out string aPtrPtrXMLstring)
        {
            bool lresult = false;

            aPtrPtrXMLstring = "";

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                if (mIEncoderControl == null)
                    break;

                try
                {
                    (mIEncoderControl as IEncoderControlInner).getMediaTypeCollectionOfEncoder(
                            Marshal.GetIUnknownForObject(aPtrUncompressedMediaType),
                            ref aRefEncoderCLSID, 
                            out lPtrXMLstring);

                    if (lPtrXMLstring != IntPtr.Zero)
                        aPtrPtrXMLstring = Marshal.PtrToStringBSTR(lPtrXMLstring);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            if (lPtrXMLstring != IntPtr.Zero)
                Marshal.FreeBSTR(lPtrXMLstring);

            return lresult;
        }

    }
}
