using Agilent.AgilentU254x.Interop;
using Agilent_ExtensionBox.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AI_Channels : IEnumerable<AI_Channel>, IDisposable
    {
        private AI_Channel[] _channels;

        public AI_Channels(ref AgilentU254x Driver)
        {
            var _pulseBit = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, ref Driver), 0);
            var _setResetBit = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, ref Driver), 1);

            var _selector_A0 = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOB, ref Driver), 0);
            var _selector_A1 = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOB, ref Driver), 1);

            var _channelModeSwitch = new ChannelModeSwitch(_pulseBit, _setResetBit, _selector_A0, _selector_A1);
            var _filter = new Filter(new DigitalChannel(DigitalChannelsEnum.DIOA, ref Driver));

            var HOLD_CS = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOC, ref Driver), 2);
            HOLD_CS.Reset();

            var _gainAmplifier = new ProgrammableGainAmplifier(new DigitalChannel(DigitalChannelsEnum.DIOC, ref Driver));
            
            var _latch = new AnalogInLatch(
                _selector_A0,
                _selector_A1,
                new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, ref Driver), 2));

            _channels = new AI_Channel[4]
            {
                new AI_Channel(AnalogInChannelsEnum.AIn1, ref Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch),
                new AI_Channel(AnalogInChannelsEnum.AIn2, ref Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch),
                new AI_Channel(AnalogInChannelsEnum.AIn3, ref Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch),
                new AI_Channel(AnalogInChannelsEnum.AIn4, ref Driver, _channelModeSwitch, _filter, _gainAmplifier, _latch)
            };
        }

        public AI_Channel this[AnalogInChannelsEnum index]
        {
            get
            {
                return _channels[(int)index];
            }
        }

        public AI_Channel this[int ChannelNumber]
        {
            get
            {
                if (ChannelNumber < 0 || ChannelNumber > 3)
                    throw new ArgumentException();

                return _channels[ChannelNumber];
            }
        }

        public IEnumerator<AI_Channel> GetEnumerator()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                yield return _channels[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                yield return _channels[index];
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _channels.Length; i++)
                _channels[i].Dispose();
        }
    }
}
