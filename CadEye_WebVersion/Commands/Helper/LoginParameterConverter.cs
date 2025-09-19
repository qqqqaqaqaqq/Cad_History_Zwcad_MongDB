using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CadEye_WebVersion.Commands.Helper
{
    public class LoginParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return new Tuple<string, string, string, Window>(
                values[0]?.ToString() ?? string.Empty,
                values[1]?.ToString() ?? string.Empty,
                values[2]?.ToString() ?? string.Empty,
                values[3] as Window
            );
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
