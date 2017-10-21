using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class DigitalChannels : IEnumerable<DigitalChannel>, IDisposable
    {
        private DigitalChannel[] _channels;
        public DigitalChannels(ref AgilentU254x Driver)
        {
            _channels = new DigitalChannel[4]
            {
                new DigitalChannel(DigitalChannelsEnum.DIOA, ref Driver),
                new DigitalChannel(DigitalChannelsEnum.DIOB, ref Driver),
                new DigitalChannel(DigitalChannelsEnum.DIOC, ref Driver),
                new DigitalChannel(DigitalChannelsEnum.DIOD, ref Driver),
            };
        }

        public DigitalChannel this[DigitalChannelsEnum index]
        {
            get
            {
                return _channels[(int)index];
            }
        }

        public DigitalChannel this[int ChannelNumber]
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

        public void Dispose()
        {
            int i = 0;
            int cnt = _channels.Length;
            
            for (; i != cnt; )
            {
                _channels[i].Dispose();
                ++i;
            }
        }
    }
}
