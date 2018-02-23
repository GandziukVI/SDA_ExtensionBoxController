using Keithley26xx;
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
    [ValueConversion(typeof(int), typeof(Keithley26xxB_Channels))]
    public class ChannelNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var channel = (Keithley26xxB_Channels)value;

            switch (channel)
            {
                case Keithley26xxB_Channels.Channel_A:
                    return 0;
                case Keithley26xxB_Channels.Channel_B:
                    return 1;
                default:
                    throw new ArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (int)value;

            if (index == 0)
                return Keithley26xxB_Channels.Channel_A;
            else if (index == 1)
                return Keithley26xxB_Channels.Channel_B;
            else
                throw new ArgumentException();
        }
    }
}