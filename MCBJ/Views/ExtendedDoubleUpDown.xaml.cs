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
            multConv = new MultiplierConverter();;
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToInt32(values[0]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            var result = new object[2];

            result[0] = System.Convert.ToInt32(value);
            result[1] = System.Convert.ToDouble(multConv.ConvertBack(value, typeof(double), null, CultureInfo.InvariantCulture), NumberFormatInfo.InvariantInfo);

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

            
            var multiValueConverter = new SelectedIndexMultiBindingConverter();

            var multiBinding = new MultiBinding() { NotifyOnSourceUpdated = true };

            multiBinding.Converter = multiValueConverter;

            multiBinding.Bindings.Add(new Binding("MultiplierIndex") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            multiBinding.Bindings.Add(new Binding("Multiplier") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });

            DataMultiplier.SetBinding(ComboBox.SelectedIndexProperty, multiBinding);

            var formatStrBinding = new Binding("FormatString") { Source = this, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged, NotifyOnSourceUpdated = true };

            DataInput.SetBinding(Xceed.Wpf.Toolkit.DoubleUpDown.FormatStringProperty, formatStrBinding);
        }

        // Flags for properties
        static FrameworkPropertyMetadataOptions flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure;

        #region Value property

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        static FrameworkPropertyMetadata ValuePropertyMetadata = new FrameworkPropertyMetadata(Double.NaN, flags, new PropertyChangedCallback(onValuePropertyChanged));
        static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            ValuePropertyMetadata);

        static void onValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            sender.SetValue(RealValueProperty, System.Convert.ToDouble(e.NewValue) * System.Convert.ToDouble(sender.GetValue(MultiplierProperty)));
        }

        #endregion

        #region RealValue property

        public double RealValue
        {
            get { return (Double)GetValue(RealValueProperty); }
            set { SetValue(RealValueProperty, value); }
        }

        static FrameworkPropertyMetadata RealValueMetadata = new FrameworkPropertyMetadata(double.NaN, flags, new PropertyChangedCallback(onRealValuePropertyChanged));
        public static readonly DependencyProperty RealValueProperty = DependencyProperty.Register(
            "RealValue",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            RealValueMetadata);

        static void onRealValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region FormatString property

        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        static FrameworkPropertyMetadata FormatStringMetadata = new FrameworkPropertyMetadata("F3", flags, new PropertyChangedCallback(onFormatStringPropertyChanged));
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            "FormatString",
            typeof(String),
            typeof(ExtendedDoubleUpDown),
            FormatStringMetadata);

        static void onFormatStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion

        #region UnitAlias property

        public String UnitAlias
        {
            get { return (String)GetValue(UnitAliasProperty); }
            set { SetValue(UnitAliasProperty, value); }
        }

        static FrameworkPropertyMetadata UnitAliasMetadata = new FrameworkPropertyMetadata("", flags, new PropertyChangedCallback(onUnitAliasPropertyChanged), new CoerceValueCallback(CoerseUnitAlias));
        public static readonly DependencyProperty UnitAliasProperty = DependencyProperty.Register(
            "UnitAlias",
            typeof(String),
            typeof(ExtendedDoubleUpDown),
            UnitAliasMetadata);

        static void onUnitAliasPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.CoerceValue(UnitAliasProperty);
        }

        static object CoerseUnitAlias(DependencyObject sender, object value)
        {
            var control = (ExtendedDoubleUpDown)sender;
            var result = (String)value;

            for (int i = 0; i < control.DataMultiplier.Items.Count; i++)
            {
                var element = (ComboBoxItem)control.DataMultiplier.Items[i];
                var elementInside = (TextBlock)element.Content;
                var elementInsideText = elementInside.Text;

                if (!string.IsNullOrEmpty(elementInsideText) && i != 0)
                    elementInsideText = elementInsideText.Substring(0, 1) + result;
                else
                    elementInsideText = result;

                var temp = new ComboBoxItem();
                var tempText = new TextBlock();
                tempText.Text = elementInsideText;

                temp.Content = tempText;

                control.DataMultiplier.Items[i] = temp;
            }

            control.DataMultiplier.SelectedIndex = 0;

            return result;
        }

        #endregion

        #region Multiplier property

        public double Multiplier
        {
            get { return (double)GetValue(MultiplierProperty); }
            set { SetValue(MultiplierProperty, value); }
        }

        static FrameworkPropertyMetadata MultiplierMetadata = new FrameworkPropertyMetadata(1.0, flags, new PropertyChangedCallback(onMultiplierPropertyChanged));

        static void onMultiplierPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            sender.SetValue(RealValueProperty, System.Convert.ToDouble(sender.GetValue(ValueProperty), NumberFormatInfo.InvariantInfo) * System.Convert.ToDouble(e.NewValue, NumberFormatInfo.InvariantInfo));
        }

        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(
            "Multiplier",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            MultiplierMetadata);

        #endregion

        #region MultiplierIndex property

        public int MultiplierIndex
        {
            get { return (int)GetValue(MultiplierIndexProperty); }
            set { SetValue(MultiplierIndexProperty, value); }
        }

        static FrameworkPropertyMetadata MultiplierIndexMetadata = new FrameworkPropertyMetadata(0, flags, new PropertyChangedCallback(onMultiplierIndexPropertyChanged));

        private static void onMultiplierIndexPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
        
        public static readonly DependencyProperty MultiplierIndexProperty =
            DependencyProperty.Register("MultiplierIndex", typeof(int), typeof(ExtendedDoubleUpDown), MultiplierIndexMetadata);

        

        #endregion
    }
}