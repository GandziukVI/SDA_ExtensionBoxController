using Agilent.AgilentU254x.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AO_Channels : IEnumerable<AO_Channel>
    {
        public DigitalBit LatchEnable { get; private set; }

        private AO_Channel[] _channels;
        public AO_Channels(AgilentU254x Driver)
        {
            LatchEnable = new DigitalBit(new DigitalChannel(DigitalChannelsEnum.DIOD, Driver), 3);

            _channels = new AO_Channel[2]
            {
                new AO_Channel(AnalogOutChannelsEnum.AOut1, Driver),
                new AO_Channel(AnalogOutChannelsEnum.AOut2, Driver)
            };
        }

        public AO_Channel this[AnalogOutChannelsEnum index]
        {
            get
            {
                return _channels[(int)index];
            }
        }

        public AO_Channel this[int ChannelNumber]
        {
            get
            {
                if (ChannelNumber < 1 || ChannelNumber > 2)
                    throw new ArgumentException();

                return _channels[ChannelNumber - 1];
            }
        }

        public IEnumerator<AO_Channel> GetEnumerator()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                yield return _channels[index];
            }
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                yield return _channels[index];
            }
        }

        public void SetVoltage_to_DefCh(BOX_AnalogOutChannelsEnum boxOut, double voltage)
        {
            var OutputNumber = (int)boxOut + 1;

            if ((OutputNumber <= 8) && (OutputNumber >= 1))
            {
                _channels[1].OutputNumber = boxOut;
                _channels[0].OutputNumber = _channels[0].OutputNumber;
                _channels[1].Enabled = true;
                LatchEnable.Pulse();
                _channels[1].Voltage = voltage;
                _channels[1].OutputOFF();
            }
            else if ((OutputNumber <= 16) && (OutputNumber >= 9))
            {
                _channels[0].OutputNumber = boxOut;
                _channels[1].OutputNumber = _channels[1].OutputNumber;
                _channels[0].Enabled = true;
                LatchEnable.Pulse();
                _channels[0].Voltage = voltage;
                _channels[0].OutputOFF();
            }
        }
    }
}
