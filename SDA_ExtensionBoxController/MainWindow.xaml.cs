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
        double responce;
        LinkedList<TraceData> listData;
        static SerialDevice device;

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
        }

        void _smu_channel_TraceDataArrived(object sender, TraceDataArrived_EventArgs e)
        {
            listData.AddLast(e.DataPoint);
        }

        private void cmd_Move_Click(object sender, RoutedEventArgs e)
        {
            using (device = new SerialDevice("COM1", 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One))
            {
                var toSet = Math.Ceiling((double)(30520.0 * inputMovementVal.Value)).ToString(NumberFormatInfo.InvariantInfo);

                device.SendCommandRequest("en");
                if(radio_up.IsChecked == true)
                    device.SendCommandRequest(string.Format("lr{0}", toSet));
                else
                    device.SendCommandRequest(string.Format("lr-{0}", toSet));

                device.SendCommandRequest("m");

                Thread.Sleep((int)(1000.0 * inputMovementVal.Value));
                device.SendCommandRequest("di");
            }
        }

        private void wnd_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.Key == Key.Up)
            //{
            //    FocusManager.SetFocusedElement(progWND, cmd_Move);
            //    radio_up.IsChecked = true;
            //}
            //else if (e.Key == Key.Down)
            //{
            //    FocusManager.SetFocusedElement(progWND, cmd_Move);
            //    radio_down.IsChecked = true;
            //}

            //var peer = new ButtonAutomationPeer(cmd_Move);
            //var invokeProv = peer.GetPattern(PatternInterface.Invoke) 
            //    as IInvokeProvider;
            //invokeProv.Invoke();
        }
    }
}
