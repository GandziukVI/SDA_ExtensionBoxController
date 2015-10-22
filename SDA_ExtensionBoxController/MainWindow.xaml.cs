using Agilent.AgilentU254x.Interop;
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

namespace SDA_ExtensionBoxController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double responce;

        public MainWindow()
        {
            InitializeComponent();

            var _driver = new VisaDevice("GPIB0::26::INSTR");
            var _device = new Keithley26xxB<Keithley2635B>(_driver);

            var _smu_channel = _device.ChannelCollection[0];

            _smu_channel.SMU_SourceMode = SourceMode.Voltage;
            _smu_channel.Averaging = 100;
            _smu_channel.NPLC = 1.0;
            _smu_channel.Compliance = 0.001;
            _smu_channel.SetSourceVoltage(0.007);
            _smu_channel.SwitchON();

            responce = _smu_channel.Resistance;

            _smu_channel.SwitchOFF();

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
        }
    }
}
