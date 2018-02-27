using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FETUI.Converters
{
    [ValueConversion(typeof(int), typeof(SMUSourceMode))]
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
}
