using System;
using System.Collections.Generic;
using System.Text;
using Agilent.AgilentU254x.Interop;
using System.Collections;
using System.Threading;
using Agilent_ExtensionBox.Internal;

namespace Agilent_ExtensionBox.IO
{
    public class AI_Channel
    {
        private AnalogInChannelsEnum _channelName;
        private AgilentU254xClass _driver;
        private AgilentU254xAnalogInChannel _channel;
        private ChannelModeSwitch _modeSwitch;


        public AI_Channel(AnalogInChannelsEnum channelName, AgilentU254xClass Driver, ChannelModeSwitch ModeSwitch, Filter ChannelFilter, ProgrammableGainAmplifier ChannelPGA, AnalogInLatch CommonLatch)
        {
            _channelName = channelName;
            _driver = Driver;
            _modeSwitch = ModeSwitch;
            Parameters = new ChannelParams(_channelName, ChannelFilter, ChannelPGA, CommonLatch);
            InitDriverChannel(_channelName, out _channel);
        }

        private void InitDriverChannel(AnalogInChannelsEnum ChannelName, out AgilentU254xAnalogInChannel channel)
        {
            switch (ChannelName)
            {
                case AnalogInChannelsEnum.AIn1:
                    channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN1);
                    break;
                case AnalogInChannelsEnum.AIn2:
                    channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN2);
                    break;
                case AnalogInChannelsEnum.AIn3:
                    channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN3);
                    break;
                case AnalogInChannelsEnum.AIn4:
                    channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN4);
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        public bool Enabled
        {
            get { return _channel.Enabled; }
            set { _channel.Enabled = value; }
        }

        public PolarityEnum Polarity
        {
            get 
            {
                switch (_channel.Polarity)
                {
                    case AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar:
                        return PolarityEnum.Polarity_Bipolar;
                    case AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityUnipolar:
                        return PolarityEnum.Polarity_Unipolar;
                    default:
                        return PolarityEnum.Polarity_Bipolar;
                }
            }
            set 
            {
                switch (value)
                {
                    case PolarityEnum.Polarity_Bipolar:
                        _channel.Polarity = AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar;
                        break;
                    case PolarityEnum.Polarity_Unipolar:
                        _channel.Polarity = AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityUnipolar;
                        break;
                }
            }
        }


        private RangesEnum _Range;
        public RangesEnum Range
        {
            get { return _Range; }
            set
            {
                var r = AvailableRanges.FromRangeEnum(value);
                _channel.Range = r;
                _Range = value;
            }
        }

        private ChannelModeEnum _Mode;
        public ChannelModeEnum Mode
        {
            get { return _Mode; }
            set
            {
                _modeSwitch.SetChannelMode(_channelName, value);
                _Mode = value;
            }
        }

        public ChannelParams Parameters
        {
            get;
            private set;
        }
    }
}
