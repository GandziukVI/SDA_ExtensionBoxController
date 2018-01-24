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

using DeviceIO;
using SourceMeterUnit;
using Keithley26xx;
using MotionManager;
using MCS_Faulhaber;
using MCBJ.Experiments;
using System.IO.Ports;
using ExperimentController;
using System.IO;
using System.Globalization;
using DynamicDataDisplay.Markers.DataSources;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Research.DynamicDataDisplay;
using System.Threading;
using System.Runtime.ExceptionServices;
using System.IO.MemoryMappedFiles;

namespace MCBJ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UIElement measurementInterface;

        IExperiment experiment;
        object expStartInfo;

        EnumerableDataSource<Point> ds;
        LinkedList<Point> dList;

        public MainWindow()
        {
            dList = new LinkedList<Point>();
            ds = new EnumerableDataSource<Point>(dList);
            ds.SetXYMapping(p => p);

            InitializeComponent();

            // Work on parsing command line arguments
            var arguments = Environment.GetCommandLineArgs();

            if (arguments.Length > 1)
            {
                // First arg contains the experiment type
                if (arguments[1].Equals("MCBJNoise", StringComparison.InvariantCultureIgnoreCase))
                {
                    onNoisedefR_Click(this, new RoutedEventArgs());
                    var settings = (measurementInterface as Noise_at_DefinedResistance).Settings;

                    settings.AgilentU2542AResName = arguments[2];
                    settings.VoltageDeviation = double.Parse(arguments[3], NumberFormatInfo.InvariantInfo);
                    settings.MinVoltageTreshold = double.Parse(arguments[4], NumberFormatInfo.InvariantInfo);
                    settings.VoltageTreshold = double.Parse(arguments[5], NumberFormatInfo.InvariantInfo);
                    settings.ConductanceDeviation = double.Parse(arguments[6], NumberFormatInfo.InvariantInfo);
                    settings.StabilizationTime = double.Parse(arguments[7], NumberFormatInfo.InvariantInfo);
                    settings.MotionMinSpeed = double.Parse(arguments[8], NumberFormatInfo.InvariantInfo);
                    settings.MotionMaxSpeed = double.Parse(arguments[9], NumberFormatInfo.InvariantInfo);
                    settings.MotorMinPos = double.Parse(arguments[10], NumberFormatInfo.InvariantInfo);
                    settings.MotorMaxPos = double.Parse(arguments[11], NumberFormatInfo.InvariantInfo);
                    settings.NAveragesFast = int.Parse(arguments[12], NumberFormatInfo.InvariantInfo);
                    settings.NAveragesSlow = int.Parse(arguments[13], NumberFormatInfo.InvariantInfo);
                    settings.LoadResistance = double.Parse(arguments[14], NumberFormatInfo.InvariantInfo);
                    settings.NSubSamples = int.Parse(arguments[15], NumberFormatInfo.InvariantInfo);
                    settings.SpectraAveraging = int.Parse(arguments[16], NumberFormatInfo.InvariantInfo);
                    settings.UpdateNumber = int.Parse(arguments[17], NumberFormatInfo.InvariantInfo);
                    settings.KPreAmpl = double.Parse(arguments[18], NumberFormatInfo.InvariantInfo);
                    settings.KAmpl = double.Parse(arguments[19], NumberFormatInfo.InvariantInfo);
                    settings.Temperature0 = double.Parse(arguments[20], NumberFormatInfo.InvariantInfo);
                    settings.TemperatureE = double.Parse(arguments[21], NumberFormatInfo.InvariantInfo);

                    if (arguments[22].Equals("y", StringComparison.InvariantCultureIgnoreCase))
                        settings.RecordTimeTraces = true;
                    else if (arguments[22].Equals("n", StringComparison.InvariantCultureIgnoreCase))
                        settings.RecordTimeTraces = false;

                    settings.RecordingFrequency = int.Parse(arguments[23], NumberFormatInfo.InvariantInfo);

                    settings.FilePath = arguments[24];
                    settings.SaveFileName = arguments[25];

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
                            settings.ScanningVoltageCollection = (double[])converter.ConvertBack(response, typeof(double[]), null, CultureInfo.InvariantCulture);
                        }
                    }

                    // Reading conductances set
                    using (var ConductanceMMF = MemoryMappedFile.OpenExisting(@"ConductanceSet", MemoryMappedFileRights.Read, HandleInheritability.Inheritable))
                    {
                        var streamLength = 0;
                        using (var ConductanceMMFStream = ConductanceMMF.CreateViewStream(0, sizeof(int), MemoryMappedFileAccess.Read))
                        {
                            var toRead = new byte[sizeof(Int32)];
                            ConductanceMMFStream.Read(toRead, 0, toRead.Length);
                            streamLength = BitConverter.ToInt32(toRead, 0);
                        }
                        using (var ConductanceMMFStream = ConductanceMMF.CreateViewStream(sizeof(int), streamLength, MemoryMappedFileAccess.Read))
                        {
                            var toRead = new byte[streamLength];
                            ConductanceMMFStream.Read(toRead, 0, streamLength);
                            var response = Encoding.ASCII.GetString(toRead);
                            var converter = new ValueCollectionConverter();
                            settings.SetConductanceCollection = (double[])converter.ConvertBack(response, typeof(double[]), null, CultureInfo.InvariantCulture);
                        }
                    }

                    //on_cmd_startNoiseDefR(this, new RoutedEventArgs());
                }
            }
        }

        private void onMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (experiment != null)
                if (experiment.IsRunning)
                    experiment.Stop();
        }

        #region I-V at defined resistance implementation

        private void onIVdefR_Click(object sender, RoutedEventArgs e)
        {
            if (measurementInterface != null)
                if (expParentGrid.Children.Contains(measurementInterface))
                    expParentGrid.Children.Remove(measurementInterface);

            var control = new IV_at_DefinedResistance();

            measurementInterface = control;

            Grid.SetRow(control, 1);
            Grid.SetColumn(control, 0);

            control.cmdStart.Click += cmdStartIV_at_defR_Click;
            control.cmdStop.Click += cmdStopIV_at_defR_Click;

            this.expParentGrid.Children.Add(control);

            var psdGraph = new LineGraph(ds);
            psdGraph.AddToPlotter(control.chartIV);
            control.chartIV.Viewport.FitToView();

            expStartInfo = control.Settings;
        }

        void cmdStartIV_at_defR_Click(object sender, RoutedEventArgs e)
        {
            // Has to be implemented in another section of code

            var smuDriver = new VisaDevice("GPIB0::26::INSTR") as IDeviceIO;
            var keithley = new Keithley26xxB<Keithley2602B>(smuDriver);
            var smu = keithley[Keithley26xxB_Channels.Channel_B];

            var motorDriver = new SerialDevice("COM1", 115200, Parity.None, 8, StopBits.One);
            var motor = new SA_2036U012V(motorDriver) as IMotionController1D;

            experiment = new IV_DefinedResistance(smu, motor) as IExperiment;
            experiment.DataArrived += experimentIV_at_def_R_DataArrived;

            experiment.Status += experimentStatus;
            experiment.Progress += experimentProgress;

            if (expStartInfo != null)
                experiment.Start(expStartInfo);
        }

        void cmdStopIV_at_defR_Click(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Stop();
        }

        void experimentIV_at_def_R_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var ts = new ParameterizedThreadStart(addIVDataToPlot);
            var th = new Thread(ts);

            th.Start(e.Data);
        }

        void addIVDataToPlot(object IVDataString)
        {
            dList.Clear();

            var data = ((string)IVDataString).FromStringExtension();

            var points = from dataPoint in data
                         select new Point(dataPoint.Voltage, dataPoint.Current);

            foreach (var item in points)
                dList.AddLast(item);

            Dispatcher.InvokeAsync(new Action(() =>
            {
                ds.RaiseDataChanged();
            }));
        }

        #endregion

        #region Noise at defined resistance implenentation

        private void onNoisedefR_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (measurementInterface != null)
                if (expParentGrid.Children.Contains(measurementInterface))
                    expParentGrid.Children.Remove(measurementInterface);


            var control = new Noise_at_DefinedResistance();

            measurementInterface = control;

            Grid.SetRow(measurementInterface, 1);
            Grid.SetColumn(measurementInterface, 0);

            control.cmdStart.Click += on_cmd_startNoiseDefR;
            control.cmdStop.Click += on_cmd_stopNoiseDefR;

            this.expParentGrid.Children.Add(measurementInterface);

            var psdGraph = new LineGraph(ds);
            psdGraph.AddToPlotter(control.chartIV);
            control.chartIV.Viewport.FitToView();

            expStartInfo = control.Settings;
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

        void on_cmd_startNoiseDefR(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Dispose();

            var calPath = string.Format("{0}\\{1}", AppDomain.CurrentDomain.BaseDirectory, "NoiseCalibration");
            var amplifierNoiseFilePath = string.Format("{0}\\{1}", calPath, "AmplifierNoise.dat");
            var frequencyResponceFilePath = string.Format("{0}\\{1}", calPath, "FrequencyResponce.dat");

            var amplifierNoise = ReadCalibrationFile(amplifierNoiseFilePath);
            var frequencyResponce = ReadCalibrationFile(frequencyResponceFilePath);


            var motorDriver = new SerialDevice("COM1", 115200, Parity.None, 8, StopBits.One);
            IMotionController1D motor = new SA_2036U012V(motorDriver) as IMotionController1D;

            experiment = new Noise_DefinedResistance((expStartInfo as Noise_DefinedResistanceInfo).AgilentU2542AResName, motor, amplifierNoise, frequencyResponce);
            //experiment = new Noise_DefinedResistance((expStartInfo as Noise_DefinedResistanceInfo).AgilentU2542AResName, null, amplifierNoise, frequencyResponce);

            experiment.DataArrived += Noise_at_der_R_DataArrived;

            experiment.Status += experimentStatus;
            experiment.Progress += experimentProgress;

            if (expStartInfo != null)
                experiment.Start(expStartInfo);
        }

        void on_cmd_stopNoiseDefR(object sender, RoutedEventArgs e)
        {
            if (experiment != null)
                experiment.Stop();
        }

        char[] delim = "\r\n".ToCharArray();
        char[] sep = "\t".ToCharArray();

        private static object addDataToPlotLocker = new object();

        [HandleProcessCorruptedStateExceptions]
        void AddNoiseDataToPlot(string NoiseDataString)
        {
            lock (addDataToPlotLocker)
            {
                try
                {
                    dList.Clear();

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
                        dList.AddLast(item);

                    Dispatcher.InvokeAsync(new Action(() =>
                    {
                        ds.RaiseDataChanged();
                    }));
                }
                catch { }
            }
        }

        private static object noiseDefinedRDataArrivedLock = new object();

        [HandleProcessCorruptedStateExceptions]
        void Noise_at_der_R_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            lock (noiseDefinedRDataArrivedLock)
            {
                try
                {
                    if (e.Data.StartsWith("NS"))
                    {
                        AddNoiseDataToPlot(e.Data);
                        //var ts = new ParameterizedThreadStart(AddNoiseDataToPlot);
                        //var th = new Thread(ts);

                        //th.Start(e.Data);
                    }
                }
                catch { }
            }
        }

        #endregion

        #region Status and progress for all experiments

        private static object experimentStatusLocker = new object();
        
        [HandleProcessCorruptedStateExceptions]
        private void experimentStatus(object sender, StatusEventArgs e)
        {
            lock (experimentStatusLocker)
            {
                try
                {
                    Dispatcher.InvokeAsync(new Action(() =>
                    {
                        expStatus.Text = e.StatusMessage;
                    }));
                }
                catch { }
            }
        }

        private static object experimentProgressLocker = new object();

        [HandleProcessCorruptedStateExceptions]
        void experimentProgress(object sender, ProgressEventArgs e)
        {
            lock (experimentProgressLocker)
            {
                try
                {
                    Dispatcher.InvokeAsync(new Action(() =>
                    {
                        expProgress.Value = e.Progress;
                    }));
                }
                catch { }
            }
        }

        #endregion
    }
}
