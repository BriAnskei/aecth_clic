using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Windows.UI;
using System;

namespace aesth_clic.Views.Roles.Doctor.Pages
{
    public class StringToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hex && hex.StartsWith("#"))
            {
                try
                {
                    hex = hex.Replace("#", "");
                    byte a = 255, r, g, b;

                    if (hex.Length == 6)
                    {
                        r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    }
                    else if (hex.Length == 8)
                    {
                        a = System.Convert.ToByte(hex.Substring(0, 2), 16);
                        r = System.Convert.ToByte(hex.Substring(2, 2), 16);
                        g = System.Convert.ToByte(hex.Substring(4, 2), 16);
                        b = System.Convert.ToByte(hex.Substring(6, 2), 16);
                    }
                    else return new SolidColorBrush(Colors.Gray);

                    return new SolidColorBrush(Color.FromArgb(a, r, g, b));
                }
                catch { }
            }

            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}