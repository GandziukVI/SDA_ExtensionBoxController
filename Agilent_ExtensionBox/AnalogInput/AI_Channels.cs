using Agilent.AgilentU254x.Interop;
using Agilent_ExtensionBox.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AI_Channels
    {
        private AI_Channel[] _channels;

        public AI_Channels(AgilentU254xClass Driver)
        {
            var _pulsBit = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, Driver), 0);
            var _setResetBit = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, Driver), 1);

            var _selector_A0 = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOB, Driver), 0);
            var _selector_A1 = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOB, Driver), 1);

            var _channelModeSwitch = new ChannelModeSwitch(_pulsBit, _setResetBit, _selector_A0, _selector_A1);
            var _filter = new Filter(new DigitalChannel(DigitalChannelsEnum.DIOA, Driver));
            var _gainAmplifier = new ProgrammableGainAmplifier(new DigitalChannel(DigitalChannelsEnum.DIOC, Driver));
            var _latch = new AnalogInLatch(new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOB, Driver), 0), new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOB, Driver), 1), new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, Driver), 2));

            _channels = new AI_Channel[4]
            {
                new AI_Channel(AnalogInChannelsEnum.AIn1, Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch),
                new AI_Channel(AnalogInChannelsEnum.AIn2, Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch),
                new AI_Channel(AnalogInChannelsEnum.AIn3, Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch),
                new AI_Channel(AnalogInChannelsEnum.AIn4, Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch)
            };
        }

        public AI_Channel this[AnalogInChannelsEnum index]
        {
            get
            {
                return _channels[(int)index];
            }
        }
    }
}
