using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MCBJ.Experiments
{
    [ValueConversion (typeof(int), typeof(SMUSourceMode))]
    public class SourceModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mode = (SMUSourceMode)value;

            switch (mode)
            {
                case SMUSourceMode.Voltage:
                    return 0;
                case SMUSourceMode.Current:
                    return 1;
                case SMUSourceMode.ModeNotSet:
                    return 2;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (int)value;

            switch (index)
            {
                case 0:
                    return SMUSourceMode.Voltage;
                case 1:
                    return SMUSourceMode.Current;
                case 2:
                    return SMUSourceMode.ModeNotSet;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }

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
