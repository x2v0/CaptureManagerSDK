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
    class VideoMixerControl : IVideoMixerControl
    {
        private CaptureManagerLibrary.IVideoMixerControl mIVideoMixerControl = null;

        public VideoMixerControl(
            CaptureManagerLibrary.IVideoMixerControl aIVideoMixerControl)
        {
            mIVideoMixerControl = aIVideoMixerControl;
        }

        public bool flush(object aPtrVideoMixerNode)
        {
            bool lresult = false;

            do
            {
                if (mIVideoMixerControl == null)
                    break;

                if (aPtrVideoMixerNode == null)
                    break;

                try
                {
                    mIVideoMixerControl.flush(
                        aPtrVideoMixerNode);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool setOpacity(object aPtrVideoMixerNode, float aOpacity)
        {
            bool lresult = false;

            do
            {
                if (mIVideoMixerControl == null)
                    break;

                if (aPtrVideoMixerNode == null)
                    break;

                try
                {
                    mIVideoMixerControl.setOpacity(
                        aPtrVideoMixerNode,
                        aOpacity);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool setPosition(object aPtrVideoMixerNode, float aLeft, float aRight, float aTop, float aBottom)
        {
            bool lresult = false;

            do
            {
                if (mIVideoMixerControl == null)
                    break;

                if (aPtrVideoMixerNode == null)
                    break;

                try
                {
                    mIVideoMixerControl.setPosition(
                        aPtrVideoMixerNode,
                        aLeft,
                        aRight,
                        aTop,
                        aBottom);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool setSrcPosition(object aPtrVideoMixerNode, float aLeft, float aRight, float aTop, float aBottom)
        {
            bool lresult = false;

            do
            {
                if (mIVideoMixerControl == null)
                    break;

                if (aPtrVideoMixerNode == null)
                    break;

                try
                {
                    mIVideoMixerControl.setSrcPosition(
                        aPtrVideoMixerNode,
                        aLeft,
                        aRight,
                        aTop,
                        aBottom);

                    lresult = true;
                }
                catch (Exception exc)
                {
                    LogManager.getInstance().write(exc.Message);
                }

            } while (false);

            return lresult;
        }

        public bool setZOrder(object aPtrVideoMixerNode, uint aZOrder)
        {
            bool lresult = false;

            do
            {
                if (mIVideoMixerControl == null)
                    break;

                if (aPtrVideoMixerNode == null)
                    break;

                try
                {
                    mIVideoMixerControl.setZOrder(
                        aPtrVideoMixerNode,
                        aZOrder);

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
