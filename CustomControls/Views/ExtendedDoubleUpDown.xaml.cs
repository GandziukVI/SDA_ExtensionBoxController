using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CustomControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ExtendedDoubleUpDown : UserControl
    {
        public ExtendedDoubleUpDown()
        {
            InitializeComponent();
        }

        //public double Value
        //{
        //    get { return (double)GetValue(ValueProperty); }
        //    set { SetValue(ValueProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty ValueProperty =
        //    DependencyProperty.Register("Value", typeof(double), typeof(ExtendedDoubleUpDown), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        //public double RealValue
        //{
        //    get { return (double)GetValue(RealValueProperty); }
        //    protected set { SetValue(RealValuePropertyKey, value); }
        //}

        //// Using a DependencyProperty as the backing store for RealValue.  This enables animation, styling, binding, etc...
        //public static readonly DependencyPropertyKey RealValuePropertyKey =
        //    DependencyProperty.RegisterReadOnly("RealValue", typeof(double), typeof(ExtendedDoubleUpDown), new PropertyMetadata(0));

        //public static readonly DependencyProperty RealValueProperty = RealValuePropertyKey.DependencyProperty;

        //public double Multiplier
        //{
        //    get { return (double)GetValue(MultiplierProperty); }
        //    set { SetValue(MultiplierProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for Multiplier.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MultiplierProperty =
        //    DependencyProperty.Register("Multiplier", typeof(double), typeof(ExtendedDoubleUpDown), new PropertyMetadata(0));

        //public int MultiplierIndex
        //{
        //    get { return (int)GetValue(MultiplierIndexProperty); }
        //    set { SetValue(MultiplierIndexProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MultiplierIndex.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MultiplierIndexProperty =
        //    DependencyProperty.Register("MultiplierIndex", typeof(int), typeof(ExtendedDoubleUpDown), new PropertyMetadata(0));

        //public string[] MultiplierStrings
        //{
        //    get { return (string[])GetValue(MultiplierStringsProperty); }
        //    set { SetValue(MultiplierStringsProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MultiplierStrings.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MultiplierStringsProperty =
        //    DependencyProperty.Register("MultiplierStrings", typeof(string[]), typeof(ExtendedDoubleUpDown), new PropertyMetadata(0));        

        //public string UnitAlias
        //{
        //    get { return (string)GetValue(UnitAliasProperty); }
        //    set { SetValue(UnitAliasProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for UnitAlias.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty UnitAliasProperty =
        //    DependencyProperty.Register("UnitAlias", typeof(string), typeof(ExtendedDoubleUpDown), new PropertyMetadata(0));
    }
}
