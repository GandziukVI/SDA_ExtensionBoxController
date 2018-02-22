using MCBJ.Experiments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MCBJ
{
    public class SelectedIndexMultiBindingConverter : IMultiValueConverter
    {
        IValueConverter multConv;
        public SelectedIndexMultiBindingConverter()
        {
            multConv = new MultiplierConverter();
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(values[0]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new object[3];

            result[0] = System.Convert.ToInt32(value);
            result[1] = System.Convert.ToDouble(multConv.ConvertBack(value, typeof(double), null, CultureInfo.InvariantCulture), NumberFormatInfo.InvariantInfo);
            result[2] = System.Convert.ToDouble(result[0]) * System.Convert.ToDouble(result[1]);

            return result;
        }
    }

    /// <summary>
    /// Логика взаимодействия для ExtendedDoubleUpDown.xaml
    /// </summary>
    public partial class ExtendedDoubleUpDown : UserControl
    {
        public ExtendedDoubleUpDown()
        {
            this.InitializeComponent();
        }
    }
}