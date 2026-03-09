using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace aesth_clic.Views.Roles.Pharmacist.Pages
{
    public class StringToBrushConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hex)
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                    byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                    byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    return new SolidColorBrush(Color.FromArgb(255, r, g, b));
                }
            }
            return new SolidColorBrush(Color.FromArgb(255, 91, 45, 142));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}