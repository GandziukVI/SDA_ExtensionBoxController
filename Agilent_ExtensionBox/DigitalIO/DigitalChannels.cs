using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class DigitalChannels : IEnumerable<DigitalChannel>
    {
        private DigitalChannel[] _channels;
        public DigitalChannels(AgilentU254x Driver)
        {
            _channels = new DigitalChannel[4]
            {
                new DigitalChannel(DigitalChannelsEnum.DIOA, Driver),
                new DigitalChannel(DigitalChannelsEnum.DIOB, Driver),
                new DigitalChannel(DigitalChannelsEnum.DIOC, Driver),
                new DigitalChannel(DigitalChannelsEnum.DIOD, Driver),
            };
        }

        public DigitalChannel this[DigitalChannelsEnum index]
        {
            get
            {
                return _channels[(int)index];
            }
        }

        public DigitalChannel  this[int ChannelNumber]
        {
            get
            {
                if (ChannelNumber < 1 || ChannelNumber > 4)
                    throw new ArgumentException();

                return _channels[ChannelNumber - 1];
            }
        }

        public IEnumerator<DigitalChannel> GetEnumerator()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                yield return _channels[index];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int index = 0; index < _channels.Length; index++)
            {
                yield return _channels[index];
            }
        }
    }
}
