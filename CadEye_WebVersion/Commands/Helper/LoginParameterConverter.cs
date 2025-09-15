using System.Globalization;
using System.Windows.Data;

namespace CadEye_WebVersion.Commands.Helper
{
    public class LoginParameterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string id = values[0] as string;
            string password = values[1] as string;

            if(string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
            {
                return null;
            }   

            return Tuple.Create(id, password); // 두 개를 묶어서 전달
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
