using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FETUI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SourceFunctionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mode = (SMUSourceMode)value;

            switch (mode)
            {
                case SMUSourceMode.Voltage:
                    return "V";
                case SMUSourceMode.Current:
                    return "A";
                case SMUSourceMode.ModeNotSet:
                    return "";
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (string)value;

            switch (index)
            {
                case "V":
                    return SMUSourceMode.Voltage;
                case "A":
                    return SMUSourceMode.Current;
                case "":
                    return SMUSourceMode.ModeNotSet;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }
}
