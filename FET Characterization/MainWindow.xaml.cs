using DeviceIO;
using ExperimentController;
using FET_Characterization.Experiments;
using Keithley26xx;
using Microsoft.Research.DynamicDataDisplay;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace FET_Characterization
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UIElement measurementInterface;
        IExperiment experiment;
        object expStartInfo;

        char[] delim = "\t\r\n".ToCharArray();

        Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point> ds;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void menuExpIV_Click(object sender, RoutedEventArgs e)
        {
            if (measurementInterface != null)
                if (expParentGrid.Children.Contains(measurementInterface))
                    expParentGrid.Children.Remove(measurementInterface);

            var control = new FET_IV();

            measurementInterface = control;

            Grid.SetRow(control, 1);
            Grid.SetColumn(control, 0);

            this.expParentGrid.Children.Add(control);

            control.cmdStartIV.Click += cmdStartIV_Click;
            control.cmdStopIV.Click += cmdStopIV_Click;

            expStartInfo = control.Settings;
        }     

        void cmdStartIV_Click(object sender, RoutedEventArgs e)
        {
            var settings = (sender as FET_IV).Settings;

            var driver = new VisaDevice(settings.KeithleyRscName);

            var measureDevice = new Keithley26xxB<Keithley2602B>(driver);

            var DrainSourceSMU = measureDevice[settings.VdsChannel];
            var GateSMU = measureDevice[settings.VgChannel];

            experiment = new FET_IV_Experiment(DrainSourceSMU, GateSMU) as IExperiment;

            experiment.DataArrived += expIV_FET_dataArrived;
            experiment.Start(expStartInfo);
        }

        void cmdStopIV_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Stop();
        }

        private void expIV_FET_dataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.Contains("Vg"))
            {
                ds = new Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point>();
                ds.SetXYMapping(p => p);

                Dispatcher.BeginInvoke(new Action(() => 
                {
                    (measurementInterface as FET_IV).expIV_FET_Chart.AddLineGraph(ds, 1.0, e.Data);
                }));                
            }
            else
            {
                var dataPoint = Array.ConvertAll(e.Data.Split(delim, StringSplitOptions.RemoveEmptyEntries), s => double.Parse(s, NumberFormatInfo.InvariantInfo));
                ds.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[1]));
            }
        }
    }
}
