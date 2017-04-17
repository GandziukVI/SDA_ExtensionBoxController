using FET_Characterization.Experiments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FET_Characterization
{
    /// <summary>
    /// Логика взаимодействия для ExtendedDoubleUpDown.xaml
    /// </summary>
    public partial class ExtendedDoubleUpDown : UserControl
    {
        static FrameworkPropertyMetadataOptions flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure;

        static FrameworkPropertyMetadata ValuePropertyMetadata = new FrameworkPropertyMetadata(Double.NaN, flags, new PropertyChangedCallback(onValuePropertyChanged), new CoerceValueCallback(CoerseValue));
        static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            ValuePropertyMetadata);

        static FrameworkPropertyMetadata FormatStringMetadata = new FrameworkPropertyMetadata("F3", flags, new PropertyChangedCallback(onFormatStringPropertyChanged));
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            "FormatString",
            typeof(String),
            typeof(ExtendedDoubleUpDown),
            FormatStringMetadata);

        static FrameworkPropertyMetadata UnitAliasMetadata = new FrameworkPropertyMetadata("", flags, new PropertyChangedCallback(onUnitAliasPropertyChanged), new CoerceValueCallback(CoerseUnitAlias));
        public static readonly DependencyProperty UnitAliasProperty = DependencyProperty.Register(
            "UnitAlias",
            typeof(String),
            typeof(ExtendedDoubleUpDown),
            UnitAliasMetadata);

        static FrameworkPropertyMetadata MultiplierMetadata = new FrameworkPropertyMetadata(1.0, flags, new PropertyChangedCallback(onMultiplierPropertyChanged));
        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(
            "Multiplier",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            MultiplierMetadata);

        static FrameworkPropertyMetadata RealValueMetadata = new FrameworkPropertyMetadata(double.NaN, flags, new PropertyChangedCallback(onRealValuePropertyChanged), new CoerceValueCallback(CoerseRealValue));
        public static readonly DependencyProperty RealValueProperty = DependencyProperty.Register(
            "RealValue",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            RealValueMetadata);

        static void onValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.Value = (Double)e.NewValue;
            onRealValuePropertyChanged(sender, e);
        }

        static object CoerseValue(DependencyObject sender, object value)
        {
            return value;
        }

        static void onFormatStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.FormatString = (string)e.NewValue;
        }

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

        static void onMultiplierPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.Multiplier = (double)e.NewValue;
            onRealValuePropertyChanged(sender, e);
        }

        static void onRealValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.RealValue = control.Value * control.Multiplier;
        }

        static object CoerseRealValue(DependencyObject sender, object value)
        {
            return value;
        }

        public ExtendedDoubleUpDown()
        {
            this.InitializeComponent();
        }

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string FormatString
        {
            get { return (string)GetValue(FormatStringProperty); }
            set { SetValue(FormatStringProperty, value); }
        }

        public String UnitAlias
        {
            get { return (String)GetValue(UnitAliasProperty); }
            set { SetValue(UnitAliasProperty, value); }
        }

        public double Multiplier
        {
            get { return (double)GetValue(MultiplierProperty); }
            set { SetValue(MultiplierProperty, value); }
        }

        public double RealValue
        {
            get { return (Double)GetValue(RealValueProperty); }
            set { SetValue(RealValueProperty, value); }
        }
    }
}