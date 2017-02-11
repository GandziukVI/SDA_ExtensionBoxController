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

namespace MCBJ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IExperiment experiment;
        StringBuilder sb;
        int dataCounter = 0;

        public MainWindow()
        {
            InitializeComponent();

            var smuDriver = new VisaDevice("GPIB0::26::INSTR") as IDeviceIO;
            var keithley = new Keithley26xxB<Keithley2602B>(smuDriver);
            var smu = keithley[Keithley26xxB_Channels.Channel_A];

            var motorDriver = new SerialDevice("COM1", 115200, Parity.None, 8, StopBits.One);
            var motor = new SA_2036U012V(motorDriver) as IMotionController1D;

            experiment = new IV_DefinedResistance(smu, motor) as IExperiment;

            experiment.DataArrived += experiment_DataArrived;

            experiment.Start();
        }

        void experiment_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var fileName = "E:\\MCBJ\\2017\\2017.02.10\\IV\\Temp\\CurrentI-V.csv";

            if(dataCounter == 0)
            {
                sb = new StringBuilder();

                var data = e.Data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var dataQuery = from item in data
                            select new
                            {
                                voltage = double.Parse(item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0], NumberFormatInfo.InvariantInfo),
                                current = double.Parse(item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[1], NumberFormatInfo.InvariantInfo)
                            };
                var query = from item in dataQuery
                            where item.voltage >= 0
                            select item;

                foreach (var item in query)
                    sb.AppendFormat("{0},{1}\r\n", item.voltage.ToString(NumberFormatInfo.InvariantInfo), item.current.ToString(NumberFormatInfo.InvariantInfo));
            }
            else if (dataCounter == 1)
            {
                var data = e.Data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var dataQuery = from item in data
                                select new
                                {
                                    voltage = double.Parse(item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[0], NumberFormatInfo.InvariantInfo),
                                    current = double.Parse(item.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)[1], NumberFormatInfo.InvariantInfo)
                                };
                var query = from item in dataQuery
                            where item.voltage < 0
                            select item;

                foreach (var item in query)
                    sb.AppendFormat("{0},{1}\r\n", item.voltage.ToString(NumberFormatInfo.InvariantInfo), item.current.ToString(NumberFormatInfo.InvariantInfo));

                using (var sw = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write)))
                {
                    sw.Write(sb.ToString());
                }
            }

            ++dataCounter;
        }

        private void onMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (experiment.IsRunning)
                experiment.Stop();
        }
    }
}
