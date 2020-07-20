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
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using CaptureManagerToCSharpProxy.Interfaces;


namespace CaptureManagerToCSharpProxy
{
    class Session : ISession
    {
        CaptureManagerLibrary.ISession mSession = null;

        Dictionary<UpdateStateDelegate, int> mStateDelegate = new Dictionary<UpdateStateDelegate, int>();
        
        public Session(CaptureManagerLibrary.ISession aSession)
        {
            mSession = aSession;
        }

        public CaptureManagerLibrary.ISession getNative()
        {
            return mSession;
        }

        public bool closeSession()
        {
            bool lresult = false;

            do
            {
                if (mSession == null)
                    break;

                try
                {
                    mSession.closeSession();

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

                mSession = null;

            } while (false);

            return lresult;
        }

        public bool pauseSession()
        {
            bool lresult = false;

            do
            {
                if (mSession == null)
                    break;

                try
                {
                    mSession.pauseSession();

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool startSession(
            long aStartPositionInHundredNanosecondUnits, 
            Guid aGUIDTimeFormat)
        {
            bool lresult = false;

            do
	        {
                if(mSession == null)
                    break;
                
                try
                {
                    mSession.startSession(
                        aStartPositionInHundredNanosecondUnits,
                        aGUIDTimeFormat);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }
	         
	        } while (false);

            return lresult;
        }

        public bool stopSession()
        {
            bool lresult = false;

            do
            {
                if (mSession == null)
                    break;

                try
                {
                    mSession.stopSession();

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool getSessionDescriptor(out uint aSessionDescriptor)
        {
            bool lresult = false;

            do
            {
                aSessionDescriptor = 0;

                if (mSession == null)
                    break;

                try
                {
                    mSession.getSessionDescriptor(out aSessionDescriptor);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }
        
        public bool registerUpdateStateDelegate(
            UpdateStateDelegate aUpdateStateDelegate)
        {
            bool lresult = false;

            do
            {
                if (mSession == null)
                    break;

                try
                {
                    object lUnknown;                                      

                    mSession.getIConnectionPointContainer(
                        typeof(IConnectionPointContainer).GUID,
                        out lUnknown);

                    if(lUnknown == null)
                        break;

                    var lConnectionPointContainer = lUnknown as IConnectionPointContainer;

                    if(lConnectionPointContainer == null)
                        break;

                    IConnectionPoint lConnectionPoint = null;

                    lConnectionPointContainer.FindConnectionPoint(
                        typeof(CaptureManagerLibrary.ISessionCallback).GUID,
                        out lConnectionPoint);

                    if(lConnectionPoint == null)
                        break;

                    int lId = 0;
                    
                    lConnectionPoint.Advise(
                        new SessionCallback(aUpdateStateDelegate),
                        out lId);

                    mStateDelegate[aUpdateStateDelegate] = lId;

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
