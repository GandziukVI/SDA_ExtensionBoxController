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
        static FrameworkPropertyMetadata ValuePropertyMetadata = new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(onValuePropertyChanged));
        static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(Double),
            typeof(ExtendedDoubleUpDown),
            ValuePropertyMetadata);

        static FrameworkPropertyMetadata FormatStringMetadata = new FrameworkPropertyMetadata("", new PropertyChangedCallback(onFormatStringPropertyChanged));
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
            "FormatString",
            typeof(string),
            typeof(ExtendedDoubleUpDown),
            FormatStringMetadata);

        static FrameworkPropertyMetadata UnitAliasMetadata = new FrameworkPropertyMetadata("", new PropertyChangedCallback(onUnitAliasPropertyChanged));
        public static readonly DependencyProperty UnitAliasProperty = DependencyProperty.Register(
            "UnitAlias",
            typeof(string),
            typeof(ExtendedDoubleUpDown),
            UnitAliasMetadata);

        static FrameworkPropertyMetadata MultiplierMetadata = new FrameworkPropertyMetadata(1.0, new PropertyChangedCallback(onMultiplierPropertyChanged));
        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(
            "Multiplier",
            typeof(double),
            typeof(ExtendedDoubleUpDown),
            MultiplierMetadata);

        static void onValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.Value = (double)e.NewValue;
        }

        static void onFormatStringPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.FormatString = (string)e.NewValue;
        }

        static void onUnitAliasPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.UnitAlias = (string)e.NewValue;
        }

        static void onMultiplierPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ExtendedDoubleUpDown;
            control.Multiplier = (double)e.NewValue;
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

        public string UnitAlias
        {
            get { return (string)GetValue(UnitAliasProperty); }
            set
            {
                for (int i = 0; i < DataMultiplier.Items.Count; i++)
                {
                    var element = (ComboBoxItem)DataMultiplier.Items[i];
                    var elementInside = (TextBlock)element.Content;
                    var elementInsideText = elementInside.Text;

                    if (!string.IsNullOrEmpty(elementInsideText))
                        elementInsideText = elementInsideText.Substring(0, 1) + value;
                    else
                        elementInsideText = value;

                    var temp = new ComboBoxItem();
                    var tempText = new TextBlock();
                    tempText.Text = elementInsideText;

                    temp.Content = tempText;

                    DataMultiplier.Items[i] = temp;
                }

                DataMultiplier.SelectedIndex = 0;

                SetValue(UnitAliasProperty, value);
            }
        }

        public double Multiplier
        {
            get { return (double)GetValue(MultiplierProperty); }
            set { SetValue(MultiplierProperty, value); }
        }

        public double RealValue
        { 
            get { return Value * Multiplier; } 
        }
    }
}