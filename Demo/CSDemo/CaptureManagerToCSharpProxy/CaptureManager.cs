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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using CaptureManagerToCSharpProxy.Interfaces;


namespace CaptureManagerToCSharpProxy
{
    public class CaptureManager
    {
#if DEBUG
        CaptureManagerLibrary.ILogPrintOutControl mILogPrintOutControl;
#endif

        CaptureManagerLibrary.ICaptureManagerControl mICaptureManagerControl;

        private string getFullFilePath(string aFileName)
        {
            return System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "") + "\\" + aFileName;
        }

        public CaptureManager()
        {
            try
            {
                do
                {
                    
#if DEBUG
                    mILogPrintOutControl = new CaptureManagerLibrary.CoLogPrintOut() as CaptureManagerLibrary.ILogPrintOutControl;

                    if (mILogPrintOutControl == null)
                        break;

                    mILogPrintOutControl.addPrintOutDestination(
                        //(int)CaptureManagerLibrary.LogLevel.INFO_LEVEL,
                        (int)CaptureManagerLibrary.LogLevel.ERROR_LEVEL,
                        getFullFilePath("Log.txt"));

                    mILogPrintOutControl.addPrintOutDestination(
                        (int)CaptureManagerLibrary.LogLevel.INFO_LEVEL,
                        getFullFilePath("Log.txt"));
#endif
                    mICaptureManagerControl = new CaptureManagerLibrary.CoCaptureManager();

                    if (mICaptureManagerControl == null)
                        break;
                    
                } while (false);

            }
            catch (Exception exc)
            {
                LogManager.getInstance().write(exc.Message);

                throw exc;
            }
        }

        public CaptureManager(string aFileName)
        {
            try
            {
                string lFullFilePath = aFileName;

                if (!File.Exists(lFullFilePath))
                {
                    lFullFilePath = getFullFilePath(aFileName);

                    if (!File.Exists(lFullFilePath))
                    {
                        throw new Exception("File " + aFileName + " is not accessseble!!!");
                    }
                }

                do
                {
                    var lDLLModuleAddr = Win32NativeMethods.LoadLibrary(lFullFilePath);

                    if (lDLLModuleAddr == null)
                        break;

                    var lEnterProcAddr = Win32NativeMethods.GetProcAddress(lDLLModuleAddr, "DllGetClassObject");

                    if (lEnterProcAddr == null)
                        break;

                    var lGetClassObject = Marshal.GetDelegateForFunctionPointer(lEnterProcAddr,
                        typeof(Win32NativeMethods.DllGetClassObjectDelegate))
                        as Win32NativeMethods.DllGetClassObjectDelegate;

                    if (lGetClassObject == null)
                        break;

                    var CLSID_CoLogPrintOut = new Guid("4563EE3E-DA1E-4911-9F40-88A284E2DD69");

                    var CLSID_CoCaptureManager = new Guid("D5F07FB8-CE60-4017-B215-95C8A0DDF42A");

                    object lUnknown;

                    IClassFactory lFactory;

                    lGetClassObject(
                        CLSID_CoLogPrintOut,
                        typeof(IClassFactory).GUID,
                        out lUnknown);

                    lFactory = lUnknown as IClassFactory;

                    if (lFactory == null)
                        break;

                    lFactory.CreateInstance(
                        null,
                        typeof(CaptureManagerLibrary.ILogPrintOutControl).GUID,
                        out lUnknown);

                    lFactory.LockServer(true);
                    
#if DEBUG
                    mILogPrintOutControl = lUnknown as CaptureManagerLibrary.ILogPrintOutControl;

                    if (mILogPrintOutControl == null)
                        break;

                    mILogPrintOutControl.addPrintOutDestination(
                        (int)CaptureManagerLibrary.LogLevel.INFO_LEVEL,
                        //(int)CaptureManagerLibrary.LogLevel.ERROR_LEVEL,
                        getFullFilePath("Log.txt"));

                    mILogPrintOutControl.addPrintOutDestination(
                        (int)CaptureManagerLibrary.LogLevel.ERROR_LEVEL,
                        getFullFilePath("Log.txt"));
#endif
                    lGetClassObject(
                        CLSID_CoCaptureManager,
                        typeof(IClassFactory).GUID,
                        out lUnknown);

                    lFactory = lUnknown as IClassFactory;

                    if (lFactory == null)
                        break;

                    lFactory.CreateInstance(
                        null,
                        typeof(CaptureManagerLibrary.ICaptureManagerControl).GUID,
                        out lUnknown);

                    lFactory.LockServer(true);

                    mICaptureManagerControl = lUnknown as CaptureManagerLibrary.ICaptureManagerControl;

                    if (mICaptureManagerControl == null)
                        break;

                } while (false);
            }
            catch (Exception exc)
            {
                LogManager.getInstance().write(exc.Message);

                throw exc;
            }

        }
        
        private bool checkICaptureManagerControl()
        {
            return mICaptureManagerControl == null;
        }

        public bool getCollectionOfSources(ref string aInfoString)
        {
            bool lresult = false;

            IntPtr lPtrXMLstring = IntPtr.Zero;
            
            do
            {
                try
                {
                    
                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.ISourceControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;
                    
                    (lUnknown as ISourceControlInner).getCollectionOfSources(out lPtrXMLstring);
                    
                    if (lPtrXMLstring != IntPtr.Zero)
                        aInfoString = Marshal.PtrToStringBSTR(lPtrXMLstring);

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
        
        public bool getCollectionOfSinks(ref string aInfoString)
        {
            bool lresult = false;

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                try
                {

                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.ISinkControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    (lUnknown as ISinkControlInner).getCollectionOfSinks(out lPtrXMLstring);
                    
                    if (lPtrXMLstring != IntPtr.Zero)
                        aInfoString = Marshal.PtrToStringBSTR(lPtrXMLstring);

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

        public bool getCollectionOfEncoders(ref string aInfoString)
        {
            bool lresult = false;

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                try
                {

                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.IEncoderControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    (lUnknown as IEncoderControlInner).getCollectionOfEncoders(out lPtrXMLstring);
                                        
                    if (lPtrXMLstring != IntPtr.Zero)
                        aInfoString = Marshal.PtrToStringBSTR(lPtrXMLstring);

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

        public ISinkControl createSinkControl()
        {
            ISinkControl lresult = null;

            do
            {
                try
                {

                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.ISinkControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lSinkControl = lUnknown as CaptureManagerLibrary.ISinkControl;

                    if (lSinkControl == null)
                        break;

                    lresult = new SinkControl(lSinkControl);  

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }
  
            } while (false);

            return lresult;
        }

        public ISourceControl createSourceControl()
        {
            ISourceControl lresult = null;

            do
            {
                try
                {

                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.ISourceControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lSourceControl = lUnknown as CaptureManagerLibrary.ISourceControl;

                    if (lSourceControl == null)
                        break;

                    lresult = new SourceControl(lSourceControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public ISessionControl createSessionControl()
        {
            ISessionControl lresult = null;

            do
            {

                try
                {
                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.ISessionControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lSessionControl = lUnknown as CaptureManagerLibrary.ISessionControl;

                    if (lSessionControl == null)
                        break;

                    lresult = new SessionControl(lSessionControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IStreamControl createStreamControl()
        {
            IStreamControl lresult = null;

            do
            {

                try
                {
                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.IStreamControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lSessionControl = lUnknown as CaptureManagerLibrary.IStreamControl;

                    if (lSessionControl == null)
                        break;

                    lresult = new StreamControl(lSessionControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IEncoderControl createEncoderControl()
        {
            IEncoderControl lresult = null;

            do
            {

                try
                {
                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createControl(
                        typeof(CaptureManagerLibrary.IEncoderControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lEncoderControl = lUnknown as CaptureManagerLibrary.IEncoderControl;

                    if (lEncoderControl == null)
                        break;

                    lresult = new EncoderControl(lEncoderControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }
        
        public bool parseMediaType(
            object aMediaType,
            out string aInfoString)
        {
            bool lresult = false;

            aInfoString = "";

            IntPtr lPtrXMLstring = IntPtr.Zero;

            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    if (aMediaType == null)
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IMediaTypeParser).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    (lUnknown as IMediaTypeParserInner).parse(Marshal.GetIUnknownForObject(aMediaType), out lPtrXMLstring);
                                        
                    if (lPtrXMLstring != IntPtr.Zero)
                        aInfoString = Marshal.PtrToStringBSTR(lPtrXMLstring);

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

        public bool getStrideForBitmapInfoHeader(
                    Guid aMFVideoFormat,
                    uint aWidthInPixels,
                    out int aPtrStride)
        {
            bool lresult = false;

            aPtrStride = 0;

            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IStrideForBitmap).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lStrideForBitmap = lUnknown as CaptureManagerLibrary.IStrideForBitmap;

                    if (lStrideForBitmap == null)
                        break;

                    lStrideForBitmap.getStrideForBitmap(
                                    aMFVideoFormat,
                                    aWidthInPixels,
                                    out aPtrStride);

                    lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IVersionControl getVersionControl()
        {
            IVersionControl lresult = null;
            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IVersionControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lVersionControl = lUnknown as CaptureManagerLibrary.IVersionControl;

                    if (lVersionControl == null)
                        break;
                    
                    lresult = new VersionControl(lVersionControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IEVRStreamControl createEVRStreamControl()
        {
            IEVRStreamControl lresult = null;
            
            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IEVRStreamControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lEVRStreamControl = lUnknown as CaptureManagerLibrary.IEVRStreamControl;

                    if (lEVRStreamControl == null)
                        break;

                    lresult = new EVRStreamControl(lEVRStreamControl);
                    
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IRenderingControl createRenderingControl()
        {
            IRenderingControl lresult = null;
            
            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IRenderingControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lRenderingControl = lUnknown as CaptureManagerLibrary.IRenderingControl;

                    if (lRenderingControl == null)
                        break;

                    lresult = new RenderingControl(lRenderingControl);
                    
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public ISwitcherControl createSwitcherControl()
        {
            ISwitcherControl lresult = null;

            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.ISwitcherControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lSwitcherControl = lUnknown as CaptureManagerLibrary.ISwitcherControl;

                    if (lSwitcherControl == null)
                        break;

                    lresult = new SwitcherControl(lSwitcherControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IVideoMixerControl createVideoMixerControl()
        {
            IVideoMixerControl lresult = null;

            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IVideoMixerControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lVideoMixerControl = lUnknown as CaptureManagerLibrary.IVideoMixerControl;

                    if (lVideoMixerControl == null)
                        break;

                    lresult = new VideoMixerControl(lVideoMixerControl);

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public IAudioMixerControl createAudioMixerControl()
        {
            IAudioMixerControl lresult = null;

            do
            {
                try
                {


                    if (checkICaptureManagerControl())
                        break;

                    object lUnknown;

                    mICaptureManagerControl.createMisc(
                        typeof(CaptureManagerLibrary.IAudioMixerControl).GUID,
                        out lUnknown);

                    if (lUnknown == null)
                        break;

                    var lAudioMixerControl = lUnknown as CaptureManagerLibrary.IAudioMixerControl;

                    if (lAudioMixerControl == null)
                        break;

                    lresult = new AudioMixerControl(lAudioMixerControl);

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
