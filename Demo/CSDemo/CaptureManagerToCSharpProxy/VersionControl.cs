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

using CaptureManagerToCSharpProxy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CaptureManagerToCSharpProxy
{

    class VersionControl : IVersionControl
    {
        CaptureManagerLibrary.IVersionControl mVersionControl = null;

        public VersionControl(CaptureManagerLibrary.IVersionControl aVersionControl)
        {
            mVersionControl = aVersionControl;
        }
        
        public bool getVersion(ref VersionStruct aVersionStruct)
        {
            bool lresult = false;

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                try
                {
                    if (mVersionControl == null)
                        break;

                    (mVersionControl as IVersionControlInner).getVersion(
                        out aVersionStruct.mMAJOR,
                        out aVersionStruct.mMINOR,
                        out aVersionStruct.mPATCH,
                        out lPtrXMLstring);

                    if (lPtrXMLstring != IntPtr.Zero)
                        aVersionStruct.mAdditionalLabel = Marshal.PtrToStringBSTR(lPtrXMLstring);

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

        public bool checkVersion(VersionStruct aVersionStruct)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mVersionControl == null)
                        break;

                    sbyte lcheckResult = 0;

                    (mVersionControl as IVersionControlInner).checkVersion(
                        aVersionStruct.mMAJOR,
                        aVersionStruct.mMINOR,
                        aVersionStruct.mPATCH,
                        out lcheckResult);


                    if (lcheckResult > 0)
                        lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool getXMLStringVersion(out string aPtrPtrXMLstring)
        {
            bool lresult = false;

            aPtrPtrXMLstring = "";

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                try
                {

                    if (mVersionControl == null)
                        break;

                    (mVersionControl as IVersionControlInner).getXMLStringVersion(
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
