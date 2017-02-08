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
using Keithley26xx;
using SourceMeterUnit;
using MCBJ.Experiments;
using System.IO.Ports;
using MCS_Faulhaber;
using ExperimentController;

namespace MCBJ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var smuDriver = new VisaDevice("GPIB0::26::INSTR") as IDeviceIO;
            var keithley = new Keithley26xxB<Keithley2601B>(smuDriver);
            var smu = keithley[Keithley26xxB_Channels.Channel_A];

            var motorDriver = new SerialDevice("COM1", 115200, Parity.None, 8, StopBits.One);
            var motor = new SA_2036U012V(motorDriver);

            var experiment = new IV_DefinedResistance(smu, motor) as IExperiment;
            experiment.Start();
        }
    }
}
