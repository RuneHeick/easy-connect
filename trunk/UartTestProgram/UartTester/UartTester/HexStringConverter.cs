using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace UartTester
{
    public class HexStringConverter : IValueConverter
    {

        /// <summary>
        /// Converts a byte array to a multi-row hex string.  
        /// Each row is prefixed with the starting byte offset and contains
        /// a fixed number of bytes.
        /// </summary>
        /// <param name="value">The byte array to convert.</param>
        /// <param name="targetType"></param>
        /// <param name="parameter">A string, parsable to an integer, indicating
        /// how many bytes per row.  Typically 8 or 16.</param>
        /// <param name="culture"></param>
        /// <returns>A string that looks similar to the HexEdit plugin for Notepad++.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return null;
                List<byte> b = value as List<byte>;
                if (value is List<byte>)
                    return BitConverter.ToString((value as List<byte>).ToArray()).Replace("-", " ");
                else
                    return BitConverter.ToString((value as byte[])).Replace("-", " ");
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string hex = value as string;
            if (hex == null) return null;
            hex = hex.Trim();
            hex = hex.Replace(" ", ""); 
            try
            {

                if (targetType == typeof(byte[]))
                    return StringToByteArray(hex);
                else
                    return StringToByteArray(hex).ToList();
            }
            catch
            {

            }
            return null; 
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => System.Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
