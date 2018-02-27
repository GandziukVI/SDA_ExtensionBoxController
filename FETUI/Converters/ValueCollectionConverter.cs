using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FETUI.Converters
{
    [ValueConversion(typeof(string), typeof(double[]))]
    public class ValueCollectionConverter : IValueConverter
    {
        char[] separators = "[], ".ToCharArray();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var values = (double[])value;

            var sb = new StringBuilder();
            sb.Append("[ ");

            for (int i = 0; i < values.Length; i++)
                sb.AppendFormat("{0}, ", values[i].ToString("0.000", NumberFormatInfo.InvariantInfo));

            sb.Append("]");

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var arrayString = (string)value;

            var query = from item in arrayString.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                        select double.Parse(item, NumberFormatInfo.InvariantInfo);

            return query.ToArray();
        }
    }
}
