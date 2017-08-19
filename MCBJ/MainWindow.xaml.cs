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

            var motorDriver = new SerialDevice("COM3", 115200, Parity.None, 8, StopBits.One);
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


            var motorDriver = new SerialDevice("COM3", 115200, Parity.None, 8, StopBits.One);
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

        void AddNoiseDataToPlot(object NoiseDataString)
        {
            dList.Clear();

            var noiseDataString = (string)NoiseDataString;

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

        void Noise_at_der_R_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("NS"))
            {
                var ts = new ParameterizedThreadStart(AddNoiseDataToPlot);
                var th = new Thread(ts);

                th.Start(e.Data);
            }
        }

        #endregion

        #region Status and progress for all experiments

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

        #endregion
    }
}
