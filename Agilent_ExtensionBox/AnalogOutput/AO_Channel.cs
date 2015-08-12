using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AO_Channel
    {
        private string _channelName;
        private AgilentU254xClass _driver;


        public AO_Channel(AnalogOutChannelsEnum Channel, AgilentU254xClass Driver)
        {
            switch (Channel)
            {
                case AnalogOutChannelsEnum.AOut1: _channelName = "AOn1"; break;
                case AnalogOutChannelsEnum.AOut2: _channelName = "AOn2"; break;

                default:
                    throw new ArgumentException();
            }
            _driver = Driver;
        }


    }
}
