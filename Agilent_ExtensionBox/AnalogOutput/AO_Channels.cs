using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AO_Channels
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
    }
}
