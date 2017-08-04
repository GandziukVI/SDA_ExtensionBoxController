using DeviceIO;
using ExperimentController;
using FET_Characterization.Experiments;
using Keithley26xx;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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

        char[] delim = "\r\n".ToCharArray();
        char[] sep = "\t".ToCharArray();

        Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point> dsMeasurement;
        //Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point> dsLeakage;

        EnumerableDataSource<Point> FETNoiseDataSource;
        LinkedList<Point> FETNoiseDataList;

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

            expParentGrid.Children.Add(control);

            control.cmdStartIV.Click += cmdStartIV_Click;
            control.cmdStopIV.Click += cmdStopIV_Click;

            control.cmdStartTransfer.Click += cmdStartTransfer_Click;
            control.cmdStopTransfer.Click += cmdStopTransfer_Click;
        }

        private void menuExpNoise_Click(object sender, RoutedEventArgs e)
        {
            if (measurementInterface != null)
                if (expParentGrid.Children.Contains(measurementInterface))
                    expParentGrid.Children.Remove(measurementInterface);

            var control = new FET_Noise();

            measurementInterface = control;

            Grid.SetRow(control, 1);
            Grid.SetColumn(control, 0);

            expParentGrid.Children.Add(control);

            control.cmdStart.Click += cmdStartNoise_Click;
            control.cmdStop.Click += cmdStopNoise_Click;
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
            experiment.Status += experimentStatus;
            experiment.Progress += experimentProgress;

            experiment.Start(expStartInfo);
        }

        void cmdStopIV_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
            {
                experiment.Stop();
            }

            experiment.DataArrived -= expIV_FET_dataArrived;
            experiment.Progress -= experimentProgress;
            experiment.Status -= experimentStatus;
        }

        private void expIV_FET_dataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var settings = expStartInfo as FET_IVModel;

            if (e.Data.Contains("Vg") || dsMeasurement == null)
            {
                var CurrentLinePen = new Pen();

                dsMeasurement = new Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point>();
                dsMeasurement.SetXYMapping(p => p);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CurrentLinePen = (measurementInterface as FET_IV).expIV_FET_Chart.AddLineGraph(dsMeasurement, 1.5, e.Data).LinePen;
                    CurrentLinePen = new Pen(CurrentLinePen.Brush, 1.0);
                }));               
            }
            else
            {
                var dataPoint = Array.ConvertAll(e.Data.Split("\t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries), s => double.Parse(s, NumberFormatInfo.InvariantInfo));
                if (settings.MeasureLeakage == true)
                {
                    dsMeasurement.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[1]));
                    //dsLeakage.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[2]));
                }
                else
                {
                    var iv_query = from ivPoint in e.Data.FromStringExtension()
                                   select new Point(ivPoint.Voltage, ivPoint.Current);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        dsMeasurement.AppendMany(iv_query);
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
            experiment.Progress += experimentProgress;
            experiment.Status += experimentStatus;

            experiment.Start(expStartInfo);
        }

        void cmdStopTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Stop();

            experiment.DataArrived -= expTransfer_FET_dataArrived;
            experiment.Progress -= experimentProgress;
            experiment.Status -= experimentStatus;
        }

        private void expTransfer_FET_dataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var settings = expStartInfo as FET_IVModel;

            if (e.Data.Contains("Vds") || dsMeasurement == null)
            {
                dsMeasurement = new Microsoft.Research.DynamicDataDisplay.DataSources.ObservableDataSource<Point>();
                dsMeasurement.SetXYMapping(p => p);

                Dispatcher.BeginInvoke(new Action(() =>
                {
                    (measurementInterface as FET_IV).expTransfer_FET_Chart.AddLineGraph(dsMeasurement, 1.5, e.Data);
                }));
            }
            else
            {
                var dataPoint = Array.ConvertAll(e.Data.Split(delim, StringSplitOptions.RemoveEmptyEntries), s => double.Parse(s, NumberFormatInfo.InvariantInfo));
                dsMeasurement.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[1]));
            }
        }

        #endregion

        #region Interface and logic for FET Noise measurement

        private void cmdStartNoise_Click(object sender, RoutedEventArgs e)
        {
            expStartInfo = (measurementInterface as FET_Noise).Settings;

            (measurementInterface as FET_Noise).chartFETOscilloscope.Children.RemoveAll(typeof(LineGraph));
            (measurementInterface as FET_Noise).chartFETOscilloscope.Legend.Visibility = System.Windows.Visibility.Collapsed;

            (measurementInterface as FET_Noise).chartFETNoise.Children.RemoveAll(typeof(LineGraph));
            (measurementInterface as FET_Noise).chartFETNoise.Legend.Visibility = System.Windows.Visibility.Collapsed;

            var settings = expStartInfo as FET_NoiseModel;
            var control = measurementInterface as FET_Noise;

            var psdGraph = new LineGraph(FETNoiseDataSource);
            psdGraph.AddToPlotter(control.chartFETNoise);
            control.chartFETNoise.Viewport.FitToView();

            if (experiment != null)
                experiment.Dispose();

            var calPath = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, "NoiseCalibration");
            var amplifierNoiseFilePath = string.Format("{0}\\{1}", calPath, "AmplifierNoise.dat");
            var frequencyResponceFilePath = string.Format("{0}\\{1}", calPath, "FrequencyResponce.dat");

            var amplifierNoise = ReadCalibrationFile(amplifierNoiseFilePath);
            var frequencyResponce = ReadCalibrationFile(frequencyResponceFilePath);

            experiment = new FET_Noise_Experiment(settings.AgilentU2542AResName, amplifierNoise, frequencyResponce);

            experiment.DataArrived += expFET_Noise_DataArrived;
            experiment.Status += experimentStatus;
            experiment.Progress += experimentProgress;

            if (expStartInfo != null)
                experiment.Start(expStartInfo);
        }

        void cmdStopNoise_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Stop();
        }

        void AddNoiseDataToPlot(object NoiseDataString)
        {
            FETNoiseDataList.Clear();

            var noiseDataString = (string)NoiseDataString;

            var dataPoints = noiseDataString.Substring(2)
                .Split(delim, StringSplitOptions.RemoveEmptyEntries)
                .Select(v => v.Split(sep, StringSplitOptions.RemoveEmptyEntries))
                .Select(v => Array.ConvertAll(v, x => double.Parse(x, NumberFormatInfo.InvariantInfo)))
                .Select(v => new Point(v[0], v[1])).ToArray();

            var toPlot = (from item in D3Helper.PointSelector.SelectNPointsPerDecade(ref dataPoints, 100)
                          where item.Y > 0
                          select item).ToArray();

            foreach (var item in toPlot)
                FETNoiseDataList.AddLast(item);

            Dispatcher.InvokeAsync(new Action(() =>
            {
                FETNoiseDataSource.RaiseDataChanged();
            }));
        }

        private void expFET_Noise_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("NS"))
            {
                var ts = new ParameterizedThreadStart(AddNoiseDataToPlot);
                var th = new Thread(ts);

                th.Start(e.Data);
            }
        }

        Point[] ReadCalibrationFile(string fileName)
        {
            var delim = "\r\n".ToCharArray();
            var sep = "\t".ToCharArray();

            var res = new Point[] { };

            using (var sr = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read)))
            {
                res = sr.ReadToEnd().Split(delim, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Split(sep, StringSplitOptions.RemoveEmptyEntries))
                    .Select(v => Array.ConvertAll(v, x => double.Parse(x, NumberFormatInfo.InvariantInfo)))
                    .Select(v => new Point(v[0], v[1])).ToArray();
            }

            return res;
        }

        #endregion

        private void experimentStatus(object sender, StatusEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expStatus.Text = e.StatusMessage;
            }));
        }

        void experimentProgress(object sender, ProgressEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expProgress.Value = e.Progress;
            }));
        }
    }
}