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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFWebViewerEVRDisplay
{
    public delegate void Callback(float aLeftNewValue, float aTopNewValue);

    /// <summary>
    /// Interaction logic for PositionControl.xaml
    /// </summary>
    public partial class PositionControl : UserControl
    {
        public static readonly DependencyProperty SceneProperty =
    DependencyProperty.Register("Scale", typeof(double), typeof(PositionControl), new UIPropertyMetadata(ScaleChangedCallback));

        public double Scale
        {
            get { return (double)this.GetValue(SceneProperty); }
            set { this.SetValue(SceneProperty, value); }
        }

        public event Callback mCallback;
        
        private static void ScaleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (PositionControl)d;
            
            instance.m_PositionerRect.Width = instance.ActualWidth * (double)e.NewValue;

            instance.m_PositionerRect.Height = instance.ActualHeight * (double)e.NewValue;
        }

        public PositionControl()
        {
            InitializeComponent();

            m_PositionerRect.Width = ActualWidth;

            m_PositionerRect.Height = ActualHeight;
        }

        bool lMoveStarted = false;

        Point mStartPosition = new Point();

        private void m_PositionerRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lMoveStarted = true;

            mStartPosition = e.MouseDevice.GetPosition(this);
        }

        private void m_PositionerRect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            lMoveStarted = false;
        }

        private void m_PositionerRect_MouseMove(object sender, MouseEventArgs e)
        {
            if(lMoveStarted)
            {
                var lPosition = e.MouseDevice.GetPosition(this) - mStartPosition;

                mStartPosition = e.MouseDevice.GetPosition(this);

                if(lPosition != null)
                {
                    double lLeft = Canvas.GetLeft(m_PositionerRect) + lPosition.X;

                    if ((m_PositionerRect.Width + lLeft) >= ActualWidth)
                        lLeft = ActualWidth - m_PositionerRect.Width;

                    if (lLeft <= 0)
                        lLeft = 0;

                    Canvas.SetLeft(m_PositionerRect, lLeft);


                    double lTop = Canvas.GetTop(m_PositionerRect) + lPosition.Y;

                    if ((m_PositionerRect.Height + lTop) >= ActualHeight)
                        lTop = ActualHeight - m_PositionerRect.Height;

                    if (lTop <= 0)
                        lTop = 0;

                    Canvas.SetTop(m_PositionerRect, lTop);

                    if(mCallback != null)
                    {
                        mCallback((float)(lLeft / ActualWidth), (float)(lTop / ActualHeight));
                    }
                }
            }
        }

        private void m_PositionerRect_MouseLeave(object sender, MouseEventArgs e)
        {
            lMoveStarted = false;
        }
    }
}
