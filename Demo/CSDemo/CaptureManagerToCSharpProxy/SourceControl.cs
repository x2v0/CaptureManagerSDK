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
    class SourceControl : ISourceControl
    {
        CaptureManagerLibrary.ISourceControl mSourceControl;
        
        public SourceControl(
            CaptureManagerLibrary.ISourceControl aSourceControl)
        {
            mSourceControl = aSourceControl;
        }

        public bool getSourceOutputMediaType(
            string aSymbolicLink, 
            uint aIndexStream, 
            uint aIndexMediaType, 
            out object aPtrPtrOutputMediaType)
        {
            bool lresult = false;

            aPtrPtrOutputMediaType = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.getSourceOutputMediaType(
                        aSymbolicLink,
                        aIndexStream,
                        aIndexMediaType,
                        out aPtrPtrOutputMediaType);
                    
                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }
                
            } while (false);

            return lresult;
        }
               
        public bool createSourceNode(
           string aSymbolicLink, 
           uint aIndexStream,
           uint aIndexMediaType,
           object aPtrDownStreamTopologyNode,
           out object aPtrPtrTopologyNode)
        {
            bool lresult = false;

            aPtrPtrTopologyNode = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.createSourceNodeWithDownStreamConnection(
                        aSymbolicLink,
                        aIndexStream,
                        aIndexMediaType,
                        aPtrDownStreamTopologyNode,
                        out aPtrPtrTopologyNode);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IWebCamControl createWebCamControl(string aSymbolicLink)
        {
            IWebCamControl lresult = null;

            do
            {

                try
                {
                    if (mSourceControl == null)
                        break;

                    object lUnknown;

                    mSourceControl.createSourceControl(
                        aSymbolicLink,
                        typeof(CaptureManagerLibrary.IWebCamControl).GUID,
                        out lUnknown);

                    var lWebCamControl = lUnknown as CaptureManagerLibrary.IWebCamControl;

                    if (lWebCamControl == null)
                        break;

                    lresult = new WebCamControl(lWebCamControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool createSource(
            string aSymbolicLink, 
            out object aPtrPtrMediaSource)
        {
            bool lresult = false;

            aPtrPtrMediaSource = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.createSource(
                        aSymbolicLink,
                        out aPtrPtrMediaSource);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool createSourceFromCaptureProcessor(
            object aPtrCaptureProcessor, 
            out object aPtrPtrMediaSource)
        {
            bool lresult = false;

            aPtrPtrMediaSource = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {
                    ICaptureProcessor lICaptureProcessor = aPtrCaptureProcessor as ICaptureProcessor;
                    
                    if (lICaptureProcessor != null)
                    {
                        CaptureProcessor lCaptureProcessor = new CaptureProcessor(lICaptureProcessor);

                        mSourceControl.createSourceFromCaptureProcessor(
                            lCaptureProcessor,
                            out aPtrPtrMediaSource);
                    }
                    else 
                    {
                        var lNativeCaptureProcessor = aPtrCaptureProcessor as CaptureManagerLibrary.ICaptureProcessor;

                        if (lNativeCaptureProcessor == null)
                            break;

                        mSourceControl.createSourceFromCaptureProcessor(
                            lNativeCaptureProcessor,
                            out aPtrPtrMediaSource);
                    }

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool createSourceNode(
            string aSymbolicLink, 
            uint aIndexStream, 
            uint aIndexMediaType, 
            out object aPtrPtrTopologyNode)
        {
            bool lresult = false;

            aPtrPtrTopologyNode = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.createSourceNode(
                        aSymbolicLink, 
                        aIndexStream, 
                        aIndexMediaType, 
                        out aPtrPtrTopologyNode);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool createSourceNodeFromExternalSource(
            object aPtrMediaSource, 
            uint aIndexStream, 
            uint aIndexMediaType, 
            out object aPtrPtrTopologyNode)
        {
            bool lresult = false;

            aPtrPtrTopologyNode = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.createSourceNodeFromExternalSource(
                        aPtrMediaSource,
                        aIndexStream,
                        aIndexMediaType,
                        out aPtrPtrTopologyNode);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool createSourceNodeFromExternalSourceWithDownStreamConnection(
            object aPtrMediaSource, 
            uint aIndexStream, 
            uint aIndexMediaType, 
            object aPtrDownStreamTopologyNode, 
            out object aPtrPtrTopologyNode)
        {
            bool lresult = false;

            aPtrPtrTopologyNode = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.createSourceNodeFromExternalSourceWithDownStreamConnection(
                        aPtrMediaSource,
                        aIndexStream,
                        aIndexMediaType,
                        aPtrDownStreamTopologyNode,
                        out aPtrPtrTopologyNode);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool getCollectionOfSources(
            ref string aPtrPtrXMLstring)
        {
            bool lresult = false;

            IntPtr lPtrXMLstring = IntPtr.Zero;
            
            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    (mSourceControl as ISourceControlInner).getCollectionOfSources(out lPtrXMLstring);
                    
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

        public bool getSourceOutputMediaTypeFromMediaSource(
            object aPtrMediaSource, 
            uint aIndexStream, 
            uint aIndexMediaType, 
            out object aPtrPtrOutputMediaType)
        {
            bool lresult = false;

            aPtrPtrOutputMediaType = null;

            do
            {
                if (mSourceControl == null)
                    break;

                try
                {

                    mSourceControl.getSourceOutputMediaTypeFromMediaSource(
                        aPtrMediaSource,
                        aIndexStream,
                        aIndexMediaType,
                        out aPtrPtrOutputMediaType);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }
    }
}
