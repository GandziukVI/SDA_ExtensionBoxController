using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class DigitalChannels
    {
        private DigitalChannel[] _channels;
        public DigitalChannels(AgilentU254xClass Driver)
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
    }
}
