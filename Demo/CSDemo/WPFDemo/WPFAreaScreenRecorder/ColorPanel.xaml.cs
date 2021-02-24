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
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFAreaScreenRecorder
{
   internal class ColorPartitionConvertor : IValueConverter
   {
      #region  Fields

      private Color lastColor;

      #endregion

      #region Public properties

      public string Channel
      {
         get;
         set;
      }

      #endregion

      #region Interface methods

      object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
      {
         lastColor = (Color) value;

         if (Channel == "A") {
            return lastColor.A;
         }

         if (Channel == "R") {
            return lastColor.R;
         }

         if (Channel == "G") {
            return lastColor.G;
         }

         if (Channel == "B") {
            return lastColor.B;
         }

         return null;
      }

      object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      {
         byte valueByte = 0;

         if (byte.TryParse(value.ToString(), out valueByte)) {
            if (Channel == "A") {
               lastColor.A = valueByte;

               return lastColor;
            }

            if (Channel == "R") {
               lastColor.R = valueByte;

               return lastColor;
            }

            if (Channel == "G") {
               lastColor.G = valueByte;

               return lastColor;
            }

            if (Channel == "B") {
               lastColor.B = valueByte;

               return lastColor;
            }
         }

         return null;
      }

      #endregion
   }

   /// <summary>
   ///    Interaction logic for ColorPanel.xaml
   /// </summary>
   public partial class ColorPanel : UserControl
   {
      #region Static fields

      // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ColorProperty =
         DependencyProperty.Register("Color", typeof(System.Drawing.Color), typeof(ColorPanel), new PropertyMetadata(OnValueChanged), validateValueCallback);

      // Using a DependencyProperty as the backing store for ProxyColor.  This enables animation, styling, binding, etc...
      public static readonly DependencyProperty ProxyColorProperty =
         DependencyProperty.Register("ProxyColor", typeof(Color), typeof(ColorPanel), new PropertyMetadata(ProxyOnValueChanged), validateValueCallback);

      #endregion

      #region Constructors and destructors

      public ColorPanel()
      {
         InitializeComponent();
      }

      #endregion

      #region Public properties

      public System.Drawing.Color Color
      {
         get => (System.Drawing.Color) GetValue(ColorProperty);

         set => SetValue(ColorProperty, value);
      }


      public Color ProxyColor
      {
         get => (Color) GetValue(ProxyColorProperty);
         set => SetValue(ProxyColorProperty, value);
      }

      #endregion

      #region Private methods

      private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         do {
            var lcolorPanel = obj as ColorPanel;

            if (lcolorPanel == null) {
               break;
            }

            var newColor = (System.Drawing.Color) e.NewValue;

            if (newColor == null) {
               break;
            }

            lcolorPanel.HSVpickPanel.Color = System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
         } while (false);
      }


      private static void ProxyOnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
      {
         do {
            var lcolorPanel = obj as ColorPanel;

            if (lcolorPanel == null) {
               break;
            }

            var newColor = (Color) e.NewValue;

            if (newColor == null) {
               break;
            }

            lcolorPanel.Color = System.Drawing.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
         } while (false);
      }


      private static bool validateValueCallback(object value)
      {
         return true;
      }

      private void colorPanel_Loaded(object sender, RoutedEventArgs e)
      {
      }

      #endregion
   }
}
