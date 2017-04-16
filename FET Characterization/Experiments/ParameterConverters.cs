using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using SourceMeterUnit;
using Keithley26xx;

namespace FET_Characterization.Experiments
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

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        #endregion
    }

    [ValueConversion(typeof(int), typeof(double))]
    public class MultiplierConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var multiplier = (double)value;

            if (multiplier == 1.0)
                return 0;
            else if (multiplier == 1e-3)
                return 1;
            else if (multiplier == 1e-6)
                return 2;
            else if (multiplier == 1e-9)
                return 3;
            else
                return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var index = (int)value;

            if (index == 0)
                return 1.0;
            else if (index == 1)
                return 1e-3;
            else if (index == 2)
                return 1e-6;
            else if (index == 3)
                return 1e-9;
            else
                return double.NaN;
        }
    }
}
