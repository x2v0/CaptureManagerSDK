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
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFAreaScreenRecorder
{
    class ColorPartitionConvertor: IValueConverter
    {

        public string Channel
        {
            get;
            set;
        }

        private Color lastColor;
        

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            lastColor = (Color)value;

            if (Channel == "A")
            {
                return lastColor.A;
            }
            if (Channel == "R")
            {
                return lastColor.R;
            }
            else if (Channel == "G")
            {
                return lastColor.G;
            }
            else if (Channel == "B")
            {
                return lastColor.B;
            }

            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte valueByte = 0;

            if (byte.TryParse(value.ToString(), out valueByte))
            {
                if (Channel == "A")
                {
                    lastColor.A = valueByte;

                    return lastColor;
                }

                if (Channel == "R")
                {
                    lastColor.R = valueByte;

                    return lastColor;
                }
                else if (Channel == "G")
                {
                    lastColor.G = valueByte;

                    return lastColor;
                }
                else if (Channel == "B")
                {
                    lastColor.B = valueByte;

                    return lastColor;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Interaction logic for ColorPanel.xaml
    /// </summary>
    public partial class ColorPanel : UserControl
    {
        public ColorPanel()
        {
            InitializeComponent();
        }

        public System.Drawing.Color Color
        {
            get { return (System.Drawing.Color)GetValue(ColorProperty); }

            set
            {
                SetValue(ColorProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(System.Drawing.Color),
            typeof(ColorPanel), new PropertyMetadata(OnValueChanged),
            new ValidateValueCallback(validateValueCallback));

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            do
            {
                ColorPanel lcolorPanel = obj as ColorPanel;

                if (lcolorPanel == null)
                    break;

                System.Drawing.Color newColor = (System.Drawing.Color)e.NewValue;

                if (newColor == null)
                    break;
                
                lcolorPanel.HSVpickPanel.Color = System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
                                      

            } while (false);

        }



        public Color ProxyColor
        {
            get { return (Color)GetValue(ProxyColorProperty); }
            set { SetValue(ProxyColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProxyColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProxyColorProperty =
            DependencyProperty.Register("ProxyColor", typeof(Color), typeof(ColorPanel), new PropertyMetadata(ProxyOnValueChanged),
            new ValidateValueCallback(validateValueCallback));


        private static void ProxyOnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            do
            {
                ColorPanel lcolorPanel = obj as ColorPanel;

                if (lcolorPanel == null)
                    break;

                System.Windows.Media.Color newColor = (System.Windows.Media.Color)e.NewValue;

                if (newColor == null)
                    break;

                lcolorPanel.Color = System.Drawing.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);


            } while (false);

        }

                
        static private bool validateValueCallback(object value)
        {
            return true;
        }

        private void colorPanel_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
