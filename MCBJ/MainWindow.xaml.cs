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
        IExperiment experiment;
        object expStartInfo;

        EnumerableDataSource<Point> ds;
        LinkedList<Point> dList;

        public MainWindow()
        {
            var firstIdentifyer = 0x0957;
            var secondIdentifyer = 0x1718;
            var visaBuilder = new StringBuilder();

            visaBuilder.AppendFormat("USB0::{0}::{1}::TW54334510::INSTR", firstIdentifyer.ToString(NumberFormatInfo.InvariantInfo), secondIdentifyer.ToString(NumberFormatInfo.InvariantInfo));

            var motorDriver = new SerialDevice("COM1", 115200, Parity.None, 8, StopBits.One);
            var motor = new SA_2036U012V(motorDriver) as IMotionController1D;

            var a = new Noise_DefinedResistance(visaBuilder.ToString(), motor);
            a.Status += a_Status;

            a.Start();
            
            dList = new LinkedList<Point>();
            ds = new EnumerableDataSource<Point>(dList);
            ds.SetXYMapping(p => p);

            InitializeComponent();
        }

        void a_Status(object sender, StatusEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expStatus.Text = e.StatusMessage;
            }));
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
            var control = new IV_at_DefinedResistance();

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

            if (expStartInfo != null)
                experiment.Start(expStartInfo);

            experiment.Status += experimentIV_at_def_R_Status;
            experiment.Progress += experimentIV_at_def_R_Progress;
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

            var data = IV_Data.FromString((string)IVDataString);

            var points = from dataPoint in data
                         select new Point(dataPoint.Voltage, dataPoint.Current);

            foreach (var item in points)
                dList.AddLast(item);

            Dispatcher.InvokeAsync(new Action(() =>
            {
                ds.RaiseDataChanged();
            }));
        }

        private void experimentIV_at_def_R_Status(object sender, StatusEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expStatus.Text = e.StatusMessage;
            }));
        }

        void experimentIV_at_def_R_Progress(object sender, ProgressEventArgs e)
        {
            Dispatcher.InvokeAsync(new Action(() =>
            {
                expProgress.Value = e.Progress;
            }));
        }

        #endregion
    }
}
