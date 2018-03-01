using Keithley26xx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FETUI.Converters
{
	[ValueConversion (typeof(int), typeof(Keithley26xxB_Channels))]
    public class Ke26xxBChannelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var channel = (Keithley26xxB_Channels)value;

            switch (channel)
            {
                case Keithley26xxB_Channels.Channel_A:
                    return 9;                    
                case Keithley26xxB_Channels.Channel_B:
                    return 1;
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
                    return Keithley26xxB_Channels.Channel_A;
                case 1:
                    return Keithley26xxB_Channels.Channel_B;
                default:
                    throw new ArgumentException("Not able to convert units.");
            }            
        }
    }

}