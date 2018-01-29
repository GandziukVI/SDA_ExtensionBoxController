using DeviceIO;
using ExperimentController;
using FET_Characterization.Experiments;
using Keithley26xx;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.Concurrent;
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
using System.IO.MemoryMappedFiles;
using System.Collections;

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

        EnumerableDataSource<Point> FETTimeTraceDataSource;
        LinkedList<Point> FETTimeTraceDataList;

        IDeviceIO driver;
        Keithley26xxB<Keithley2602B> measureDevice;

        public MainWindow()
        {
            FETNoiseDataList = new LinkedList<Point>();
            FETTimeTraceDataList = new LinkedList<Point>();

            FETNoiseDataSource = new EnumerableDataSource<Point>(FETNoiseDataList);
            FETNoiseDataSource.SetXYMapping(p => p);

            FETTimeTraceDataSource = new EnumerableDataSource<Point>(FETTimeTraceDataList);
            FETTimeTraceDataSource.SetXYMapping(p => p);

            InitializeComponent();

            // Work on parsing command line arguments
            var arguments = Environment.GetCommandLineArgs();

            if (arguments.Length > 1)
            {
                // First arg contains the experiment type
                if (arguments[1].Equals("FETNoise", StringComparison.InvariantCultureIgnoreCase))
                {
                    menuExpNoise_Click(this, new RoutedEventArgs());

                    var settings = (measurementInterface as FET_Noise).Settings;

                    settings.AgilentU2542AResName = arguments[2];

                    if (arguments[3].Equals("Transfer", StringComparison.InvariantCultureIgnoreCase))
                        settings.IsTransferCurveMode = true;
                    else if (arguments[3].Equals("Output", StringComparison.InvariantCultureIgnoreCase))
                        settings.IsOutputCurveMode = true;

                    settings.VoltageDeviation = double.Parse(arguments[4], NumberFormatInfo.InvariantInfo);
                    settings.NAveragesFast = int.Parse(arguments[5], NumberFormatInfo.InvariantInfo);
                    settings.NAveragesSlow = int.Parse(arguments[6], NumberFormatInfo.InvariantInfo);
                    settings.StabilizationTime = double.Parse(arguments[7], NumberFormatInfo.InvariantInfo);
                    settings.LoadResistance = double.Parse(arguments[8], NumberFormatInfo.InvariantInfo);
                    settings.NSubSamples = int.Parse(arguments[9], NumberFormatInfo.InvariantInfo);
                    settings.SpectraAveraging = int.Parse(arguments[10], NumberFormatInfo.InvariantInfo);
                    settings.UpdateNumber = int.Parse(arguments[11], NumberFormatInfo.InvariantInfo);
                    settings.KPreAmpl = double.Parse(arguments[12], NumberFormatInfo.InvariantInfo);
                    settings.KAmpl = double.Parse(arguments[13], NumberFormatInfo.InvariantInfo);
                    settings.Temperature0 = double.Parse(arguments[14], NumberFormatInfo.InvariantInfo);
                    settings.TemperatureE = double.Parse(arguments[15], NumberFormatInfo.InvariantInfo);

                    if (arguments[16].Equals("y", StringComparison.InvariantCultureIgnoreCase))
                        settings.RecordTimeTraces = true;
                    else if (arguments[16].Equals("n", StringComparison.InvariantCultureIgnoreCase))
                        settings.RecordTimeTraces = false;

                    settings.RecordingFrequency = int.Parse(arguments[17], NumberFormatInfo.InvariantInfo);

                    var saveDirInfo = new DirectoryInfo(arguments[18]);

                    settings.FilePath = saveDirInfo.FullName;
                    settings.SaveFileName = arguments[19];

                    // Reading gate voltage set
                    using (var VgMMF = MemoryMappedFile.OpenExisting(@"VgSet", MemoryMappedFileRights.Read, HandleInheritability.Inheritable))
                    {
                        var streamLength = 0;
                        using (var VgMMFStream = VgMMF.CreateViewStream(0, sizeof(int), MemoryMappedFileAccess.Read))
                        {
                            var toRead = new byte[sizeof(Int32)];
                            VgMMFStream.Read(toRead, 0, toRead.Length);
                            streamLength = BitConverter.ToInt32(toRead, 0);
                        }
                        using (var VgMMFStream = VgMMF.CreateViewStream(sizeof(int), streamLength, MemoryMappedFileAccess.Read))
                        {
                            var toRead = new byte[streamLength];
                            VgMMFStream.Read(toRead, 0, streamLength);
                            var response = Encoding.ASCII.GetString(toRead);
                            var converter = new ValueCollectionConverter();
                            settings.GateVoltageCollection = (double[])converter.ConvertBack(response, typeof(double[]), null, CultureInfo.InvariantCulture);
                        }
                    }

                    // Reading drain-source voltage set
                    using (var VdsMMF = MemoryMappedFile.OpenExisting(@"VdsSet", MemoryMappedFileRights.Read, HandleInheritability.Inheritable))
                    {
                        var streamLength = 0;
                        using (var VgMMFStream = VdsMMF.CreateViewStream(0, sizeof(int), MemoryMappedFileAccess.Read))
                        {
                            var toRead = new byte[sizeof(Int32)];
                            VgMMFStream.Read(toRead, 0, toRead.Length);
                            streamLength = BitConverter.ToInt32(toRead, 0);
                        }
                        using (var VgMMFStream = VdsMMF.CreateViewStream(sizeof(int), streamLength, MemoryMappedFileAccess.Read))
                        {
                            var toRead = new byte[streamLength];
                            VgMMFStream.Read(toRead, 0, streamLength);
                            var response = Encoding.ASCII.GetString(toRead);
                            var converter = new ValueCollectionConverter();
                            settings.DSVoltageCollection = (double[])converter.ConvertBack(response, typeof(double[]), null, CultureInfo.InvariantCulture);
                        }
                    }

                    cmdStartNoise_Click(this, new RoutedEventArgs());
                    
                    if (experiment != null)
                        experiment.ExpFinished += onAppExit;
                }
            }
        }

        void onAppExit(object sender, FinishedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(()=>
            {
                Application.Current.Shutdown(0);
            }));
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

            Grid.SetRow(control, 1);
            Grid.SetColumn(control, 0);

            expParentGrid.Children.Add(control);

            control.cmdStart.Click += cmdStartNoise_Click;
            control.cmdStop.Click += cmdStopNoise_Click;

            control.Settings.PropertyChanged += FET_Exp_Property_Changed;

            measurementInterface = control;
        }

        private void FET_Exp_Property_Changed(object sender, PropertyChangedEventArgs e)
        {
            var exp = (measurementInterface as FET_Noise);
            var settings = exp.Settings;

            switch (e.PropertyName)
            {
                case "OscilloscopeVoltageRange":
                    {
                        if (settings.OscilloscopeVoltageRange != 0.0)
                        {
                            var restr = new D3Helper.ViewportAxesRangeRestriction();
                            restr.YRange = new D3Helper.DisplayRange(-1.0 * Math.Abs(settings.OscilloscopeVoltageRange), Math.Abs(settings.OscilloscopeVoltageRange));
                            exp.chartFETOscilloscope.Restrictions.Add(restr);
                        }
                    } break;
            }
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
                var dataPoint = Array.ConvertAll(e.Data.TrimEnd(delim).Split(sep, StringSplitOptions.RemoveEmptyEntries), s => double.Parse(s, NumberFormatInfo.InvariantInfo));
                dsMeasurement.AppendAsync(Dispatcher, new Point(dataPoint[0], dataPoint[1]));
            }
        }

        #endregion

        #region Interface and logic for FET Noise measurement

        void cmdStartNoise_Click(object sender, RoutedEventArgs e)
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

        void AddNoiseDataToPlot(string NoiseDataString)
        {
            try
            {
                FETNoiseDataList.Clear();

                var noiseDataString = NoiseDataString;

                var dataPoints = noiseDataString.Substring(2)
                    .Split(delim, StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => v.Split(sep, StringSplitOptions.RemoveEmptyEntries))
                    .Select(v => Array.ConvertAll(v, x => double.Parse(x, NumberFormatInfo.InvariantInfo)))
                    .Select(v => new Point(v[0], v[1])).ToArray();

                var toPlot = from item in D3Helper.PointSelector.SelectNPointsPerDecade(ref dataPoints, 100)
                             where item.Y > 0
                             select item;

                foreach (var item in toPlot)
                    FETNoiseDataList.AddLast(item);

                Dispatcher.InvokeAsync(new Action(() =>
                {
                    FETNoiseDataSource.RaiseDataChanged();
                }));
            }
            catch { }
        }

        ConcurrentQueue<string[]> timeTraceDataQueue = new ConcurrentQueue<string[]>();

        void AddTimeTraceDataToPlotContiniously()
        {
            var settings = expStartInfo as FET_NoiseModel;
            var exp = experiment as FET_Noise_Experiment;

            while (exp.IsRunning)
            {
                string[] temp = new string[] { };
                string[] data = new string[] { };

                if (timeTraceDataQueue != null && timeTraceDataQueue.Count > 0)
                {
                    var dataDequeuingSuccess = new bool[timeTraceDataQueue.Count];

                    for (int i = 0; i < timeTraceDataQueue.Count; i++)
                    {
                        dataDequeuingSuccess[i] = timeTraceDataQueue.TryDequeue(out temp);

                        var dataInitialLength = data.Length;
                        Array.Resize<string>(ref data, dataInitialLength + temp.Length);
                        Array.Copy(temp, 0, data, dataInitialLength, temp.Length);
                    }

                    if (dataDequeuingSuccess.All(x => x))
                    {
                        var nPointsToConsider = (int)(settings.SamplingFrequency * settings.OscilloscopeTimeRange);
                        var nPointsRest = (int)(settings.SamplingFrequency - nPointsToConsider);

                        var N = (int)(nPointsToConsider / settings.OscilloscopePointsPerGraph);

                        if (nPointsToConsider <= settings.OscilloscopePointsPerGraph)
                        {
                            var toPlot = data
                                .Select(v => v.Split(sep, StringSplitOptions.RemoveEmptyEntries))
                                .Select(v => Array.ConvertAll(v, x => double.Parse(x, NumberFormatInfo.InvariantInfo)))
                                .Select(v => new Point(v[0], v[1]));

                            foreach (var item in toPlot)
                                FETTimeTraceDataList.AddLast(item);
                        }
                        else
                        {
                            var toPlot = data
                                .Select(v => v.Split(sep, StringSplitOptions.RemoveEmptyEntries))
                                .Take(nPointsToConsider)
                                .Where((value, index) => index % N == 0)
                                .Select(v => Array.ConvertAll(v, x => double.Parse(x, NumberFormatInfo.InvariantInfo)))
                                .Select(v => new Point(v[0], v[1]));

                            foreach (var item in toPlot)
                                FETTimeTraceDataList.AddLast(item);

                            var restData = new string[nPointsRest];
                            Array.Copy(data, nPointsToConsider, restData, 0, nPointsRest);

                            timeTraceDataQueue.Enqueue(restData);
                        }

                        Dispatcher.InvokeAsync(new Action(() =>
                        {
                            FETTimeTraceDataSource.RaiseDataChanged();
                        }));
                    }
                }
            }
        }

        private void expFET_Noise_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("NS"))
            {
                //var ts = new ParameterizedThreadStart(AddNoiseDataToPlot);
                //var th = new Thread(ts);

                //th.Start(e.Data);

                AddNoiseDataToPlot(e.Data);
            }
            else if (e.Data.StartsWith("TT"))
            {
                //var splitPointsData = e.Data.Substring(2).Split(delim, StringSplitOptions.RemoveEmptyEntries);
                //timeTraceDataQueue.Enqueue(splitPointsData);

                //var ts = new ThreadStart(AddTimeTraceDataToPlotContiniously);
                //var th = new Thread(ts);

                //th.Start();
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

        void experimentStatus(object sender, StatusEventArgs e)
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