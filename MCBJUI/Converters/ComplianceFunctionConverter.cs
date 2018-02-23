using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MCBJUI.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class ComplianceFunctionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var mode = (SMUSourceMode)value;

            switch (mode)
            {
                case SMUSourceMode.Voltage:
                    return "A";
                case SMUSourceMode.Current:
                    return "V";
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
                    return SMUSourceMode.Current;
                case "A":
                    return SMUSourceMode.Voltage;
                case "":
                    return SMUSourceMode.ModeNotSet;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }
        }
    }
}