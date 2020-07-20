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
using System.Text;

namespace CaptureManagerToCSharpProxy
{
    class SwitcherControl : ISwitcherControl
    {
        CaptureManagerLibrary.ISwitcherControl mSwitcherControl = null;

        public SwitcherControl(CaptureManagerLibrary.ISwitcherControl aSwitcherControl)
        {
            mSwitcherControl = aSwitcherControl;
        }

        public bool pauseSwitchers(ISession aSession)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mSwitcherControl == null)
                        break;

                    if (aSession == null)
                        break;

                    uint lSessionDescriptor = 0;

                    if (!aSession.getSessionDescriptor(out lSessionDescriptor))
                        break;

                    mSwitcherControl.pauseSwitchers(lSessionDescriptor);

                    lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool resumeSwitchers(ISession aSession)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mSwitcherControl == null)
                        break;

                    if (aSession == null)
                        break;

                    uint lSessionDescriptor = 0;

                    if (!aSession.getSessionDescriptor(out lSessionDescriptor))
                        break;

                    mSwitcherControl.resumeSwitchers(lSessionDescriptor);

                    lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }
        
        public bool detachSwitchers(ISession aSession)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mSwitcherControl == null)
                        break;

                    if (aSession == null)
                        break;

                    uint lSessionDescriptor = 0;

                    if (!aSession.getSessionDescriptor(out lSessionDescriptor))
                        break;

                    mSwitcherControl.detachSwitchers(lSessionDescriptor);

                    lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool atttachSwitcher(object aSwitcherNode, object aAttachedNode)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mSwitcherControl == null)
                        break;

                    if (aSwitcherNode == null)
                        break;

                    if (aAttachedNode == null)
                        break;
                                        
                    mSwitcherControl.attachSwitcher(aSwitcherNode, aAttachedNode);

                    lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }


        public bool pauseSwitcher(object aSwitcherNode)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mSwitcherControl == null)
                        break;

                    if (aSwitcherNode == null)
                        break;

                    mSwitcherControl.pauseSwitcher(aSwitcherNode);

                    lresult = true;

                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool resumeSwitcher(object aSwitcherNode)
        {
            bool lresult = false;

            do
            {
                try
                {
                    if (mSwitcherControl == null)
                        break;

                    if (aSwitcherNode == null)
                        break;

                    mSwitcherControl.resumeSwitcher(aSwitcherNode);

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
