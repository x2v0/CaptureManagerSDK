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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFAreaScreenRecorder
{
   /// <summary>
   ///    Interaction logic for AreaWindow.xaml
   /// </summary>
   public partial class AreaWindow : Window
   {
      #region Static fields

      public static Rect mSelectedRegion = Rect.Empty;

      #endregion

      #region Constructors and destructors

      public AreaWindow()
      {
         InitializeComponent();

         canvas.MouseDown += canvas_MouseDown;
         canvas.MouseUp += canvas_MouseUp;
         canvas.MouseMove += canvas_MouseMove;
         Loaded += Window_Loaded;
      }

      #endregion

      #region  Fields

      private bool mMoveRectangle;
      private Point startDrag;

      private Point startDragRect;

      #endregion

      #region Private methods

      private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
      {
         if (mMoveRectangle) {
            return;
         }

         if (e.RightButton == MouseButtonState.Pressed) {
            return;
         }

         //Set the start point
         startDrag = e.GetPosition(canvas);
         //Move the selection marquee on top of all other objects in canvas
         Panel.SetZIndex(rectangle, canvas.Children.Count);
         //Capture the mouse
         if (!canvas.IsMouseCaptured) {
            canvas.CaptureMouse();
         }

         canvas.Cursor = Cursors.Cross;
      }

      private void canvas_MouseMove(object sender, MouseEventArgs e)
      {
         if (mMoveRectangle) {
            return;
         }

         if (canvas.IsMouseCaptured) {
            var currentPoint = e.GetPosition(canvas);

            //Calculate the top left corner of the rectangle regardless of drag direction
            var x = startDrag.X < currentPoint.X ? startDrag.X : currentPoint.X;
            var y = startDrag.Y < currentPoint.Y ? startDrag.Y : currentPoint.Y;

            if (rectangle.Visibility == Visibility.Hidden) {
               rectangle.Visibility = Visibility.Visible;
            }

            //Move the rectangle to proper place
            rectangle.RenderTransform = new TranslateTransform(x, y);
            //Set its size
            rectangle.Width = Math.Abs(e.GetPosition(canvas).X - startDrag.X);
            rectangle.Height = Math.Abs(e.GetPosition(canvas).Y - startDrag.Y);
         }
      }

      private void canvas_MouseUp(object sender, MouseButtonEventArgs e)
      {
         if (mMoveRectangle) {
            return;
         }

         //Release the mouse
         if (canvas.IsMouseCaptured) {
            canvas.ReleaseMouseCapture();
         }

         canvas.Cursor = Cursors.Arrow;
      }


      private void MenuItem_Click(object sender, RoutedEventArgs e)
      {
         var source = PresentationSource.FromVisual(this);

         double ScaleX = 1.0, ScaleY = 1.0;
         if (source != null) {
            ScaleX = source.CompositionTarget.TransformToDevice.M11;
            ScaleY = source.CompositionTarget.TransformToDevice.M22;
         }

         mSelectedRegion = Rect.Empty;

         var lTranslateTransform = rectangle.RenderTransform as TranslateTransform;

         if (lTranslateTransform != null) {
            mSelectedRegion = new Rect(lTranslateTransform.X * ScaleX, lTranslateTransform.Y * ScaleY, rectangle.Width * ScaleX, rectangle.Height * ScaleY);
         }

         Close();
      }

      private void MenuItem_Click_1(object sender, RoutedEventArgs e)
      {
         mSelectedRegion = Rect.Empty;

         Close();
      }

      private void rectangle_MouseLeave(object sender, MouseEventArgs e)
      {
         mMoveRectangle = false;
      }

      private void rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         mMoveRectangle = true;

         //Set the start point
         startDragRect = e.GetPosition(canvas);
      }

      private void rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
      {
         mMoveRectangle = false;
      }

      private void rectangle_MouseMove(object sender, MouseEventArgs e)
      {
         if (mMoveRectangle) {
            var lTranslateTransform = rectangle.RenderTransform as TranslateTransform;

            if (lTranslateTransform == null) {
               return;
            }

            var currentPoint = e.GetPosition(canvas);

            lTranslateTransform.X += currentPoint.X - startDragRect.X;

            lTranslateTransform.Y += currentPoint.Y - startDragRect.Y;


            var lRightBotder = lTranslateTransform.X + rectangle.Width;

            if (lRightBotder > canvas.ActualWidth) {
               lTranslateTransform.X = canvas.ActualWidth - rectangle.Width;
            } else if (lTranslateTransform.X < 0) {
               lTranslateTransform.X = 0;
            }


            var lBottonBotder = lTranslateTransform.Y + rectangle.Height;

            if (lBottonBotder > canvas.ActualHeight) {
               lTranslateTransform.Y = canvas.ActualHeight - rectangle.Height;
            } else if (lTranslateTransform.Y < 0) {
               lTranslateTransform.Y = 0;
            }


            startDragRect = currentPoint;
         }
      }

      /*You can use this event for all the Windows*/
      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         var senderWindow = sender as Window;
         senderWindow.WindowState = WindowState.Maximized;
      }

      #endregion
   }
}
