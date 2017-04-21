using DeviceIO;
using ExperimentController;
using FET_Characterization.Experiments;
using Keithley26xx;
using Microsoft.Research.DynamicDataDisplay;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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

        IDeviceIO driver;
        Keithley26xxB<Keithley2602B> measureDevice;

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

            control.cmdStartTransfer.Click += cmdStartTransfer_Click;
            control.cmdStopTransfer.Click += cmdStopTransfer_Click;
        }

        #region Interface and logic for FET I-V measurement

        void cmdStartIV_Click(object sender, RoutedEventArgs e)
        {
            expStartInfo = (measurementInterface as FET_IV).Settings;

            (measurementInterface as FET_IV).expIV_FET_Chart.Children.RemoveAll(typeof(LineGraph));
            (measurementInterface as FET_IV).expIV_FET_Chart.Legend.Visibility = System.Windows.Visibility.Visible;

            var settings = expStartInfo as FET_IVModel;

            if (driver != null)
                driver.Dispose();
            if (measureDevice != null)
                measureDevice.Dispose();

            driver = new VisaDevice(settings.KeithleyRscName);
            measureDevice = new Keithley26xxB<Keithley2602B>(driver);

            var DrainSourceSMU = measureDevice[settings.VdsChannel];
            var GateSMU = measureDevice[settings.VgChannel];

            experiment = new FET_IV_Experiment(DrainSourceSMU, GateSMU) as IExperiment;

            experiment.DataArrived += expIV_FET_dataArrived;
            experiment.Progress += experiment_Progress;
            experiment.Status += experimentStatus;

            experiment.Start(expStartInfo);
        }

        void cmdStopIV_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
            {
                experiment.Stop();
            }

            experiment.DataArrived -= expIV_FET_dataArrived;
            experiment.Progress -= experiment_Progress;
            experiment.Status -= experimentStatus;
        }

        private void expIV_FET_dataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var settings = expStartInfo as FET_IVModel;

            if (e.Data.Contains("Vg") || ds == null)
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
                if (settings.MeasureLeakage == true)
                    ds.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[1]));
                else
                {
                    var iv_query = from ivPoint in e.Data.FromStringExtension()
                                   select new Point(ivPoint.Voltage, ivPoint.Current);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ds.AppendMany(iv_query);
                    }));
                }
            }
        }

        #endregion

        #region Interface and logic for FET Transfer characteristics measurement

        void cmdStartTransfer_Click(object sender, RoutedEventArgs e)
        {
            expStartInfo = (measurementInterface as FET_IV).Settings;

            (measurementInterface as FET_IV).expTransfer_FET_Chart.Children.RemoveAll(typeof(LineGraph));
            (measurementInterface as FET_IV).expTransfer_FET_Chart.Legend.Visibility = System.Windows.Visibility.Visible;

            var settings = expStartInfo as FET_IVModel;

            if (driver != null)
                driver.Dispose();
            if (measureDevice != null)
                measureDevice.Dispose();

            driver = new VisaDevice(settings.KeithleyRscName);
            measureDevice = new Keithley26xxB<Keithley2602B>(driver);

            var DrainSourceSMU = measureDevice[settings.TransferVdsChannel];
            var GateSMU = measureDevice[settings.TransferVgChannel];

            experiment = new FET_Transfer_Experiment(DrainSourceSMU, GateSMU) as IExperiment;

            experiment.DataArrived += expTransfer_FET_dataArrived;
            experiment.Progress += experiment_Progress;
            experiment.Status += experimentStatus;

            experiment.Start(expStartInfo);
        }

        void cmdStopTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Stop();

            experiment.DataArrived -= expTransfer_FET_dataArrived;
            experiment.Progress -= experiment_Progress;
            experiment.Status -= experimentStatus;
        }

        private void expTransfer_FET_dataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var settings = expStartInfo as FET_IVModel;

            if (e.Data.Contains("Vds") || ds == null)
            {
                ds = new Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point>();
                ds.SetXYMapping(p => p);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    (measurementInterface as FET_IV).expTransfer_FET_Chart.AddLineGraph(ds, 1.0, e.Data);
                }));
            }
            else
            {
                var dataPoint = Array.ConvertAll(e.Data.Split(delim, StringSplitOptions.RemoveEmptyEntries), s => double.Parse(s, NumberFormatInfo.InvariantInfo));
                ds.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[1]));
            }
        }

        #endregion

        private void experimentStatus(object sender, StatusEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expStatus.Text = e.StatusMessage;
            }));
        }

        void experiment_Progress(object sender, ProgressEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expProgress.Value = e.Progress;
            }));
        }
    }
}
