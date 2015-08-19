using Agilent.AgilentU254x.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AO_Channels : IEnumerable<AO_Channel>
    {
        private AO_Channel[] _channels;
        public AO_Channels(AgilentU254xClass Driver)
        {
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
    }
}
