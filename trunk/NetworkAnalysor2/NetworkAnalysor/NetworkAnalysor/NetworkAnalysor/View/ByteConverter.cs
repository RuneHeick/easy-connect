using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NetworkAnalysor.View
{
    [ValueConversion(typeof(byte[]), typeof(string))]
    public class ByteArrayConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                byte[] bytes = (byte[])value;
                return BitConverter.ToString(bytes).Replace("-", " ");
            }
            return ""; 
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
