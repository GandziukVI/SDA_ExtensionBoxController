//using Agilent.AgilentU254x.Interop;
using Agilent_ExtensionBox;
using Agilent_ExtensionBox.Internal;
using Agilent_ExtensionBox.IO;
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

using Keithley26xx;
using DeviceIO;
using System.Threading;
using System.Globalization;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace SDA_ExtensionBoxController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //BoxController b = new BoxController();
            //b.Init("USB0::0x0957::0x1718::TW54334510::INSTR");

            //var _ch = new AI_ChannelConfig[4]
            //{
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25}
            //};

            //b.ConfigureAI_Channels(_ch);
            //b.AcquireSingleShot(499712);
            //b.StartAnalogAcquisition(499712);

            //b.Close();

            InitializeComponent();
            this.KeyDown += MainWindow_KeyDown;
        }

        private void pressPutton(Button btn)
        {
            var peer = new ButtonAutomationPeer(btn);
            var invokeProv = peer.GetPattern(PatternInterface.Invoke)
                as IInvokeProvider;
            invokeProv.Invoke();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                pressPutton(cmd_Left);
            else if (e.Key == Key.Up)
                pressPutton(cmd_Up);
            else if (e.Key == Key.Right)
                pressPutton(cmd_Right);
            else if (e.Key == Key.Down)
                pressPutton(cmd_Down);
        }

        private double[] dx = new double[] { 0.001, 0.005, 0.01, 0.05, 0.1, 0.25, 0.5, 1.0, 2.5, 5, 10.0 };
        private int index = 7;

        private double alpha()
        {
            return (double)(6.0 * parameter_U.Value * 0.000000001 * parameter_t.Value * 0.01 / (parameter_L.Value * 0.001 * parameter_L.Value * 0.001));
        }

        private void cmd_Left_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (index >= 0)
                index -= 1;

            inputMovementVal.Value = dx[index];
        }

        private void cmd_Right_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (index < dx.Length)
                index += 1;

            inputMovementVal.Value = dx[index];
        }

        private void cmd_Up_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var device = new SerialDevice(COMPortName.Text, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
            {
                var toSet = Math.Ceiling((double)((inputMovementVal.Value * 0.0000000001 / alpha()) * 1000.0 * 1526.0 * 3000.0 / 0.5)).ToString(NumberFormatInfo.InvariantInfo);

                device.SendCommandRequest("en");
                device.SendCommandRequest(string.Format("lr{0}", toSet));

                device.SendCommandRequest("m");

                Thread.Sleep((int)(500.0 * inputMovementVal.Value));
                device.SendCommandRequest("di");
            }
        }

        private void cmd_Down_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var device = new SerialDevice(COMPortName.Text, 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
            {
                var toSet = Math.Ceiling((double)((inputMovementVal.Value * 0.0000000001 / alpha()) * 1000.0 * 1526.0 * 3000.0 / 0.5)).ToString(NumberFormatInfo.InvariantInfo);

                device.SendCommandRequest("en");
                device.SendCommandRequest(string.Format("lr-{0}", toSet));

                device.SendCommandRequest("m");

                Thread.Sleep((int)(500.0 * inputMovementVal.Value));
                device.SendCommandRequest("di");
            }
        }
    }
}
