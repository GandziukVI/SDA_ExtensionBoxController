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
                case AnalogOutChannelsEnum.AOut1: _channelName = ChannelNames.AOUT1; break;
                case AnalogOutChannelsEnum.AOut2: _channelName = ChannelNames.AOUT2; break;

                default:
                    throw new ArgumentException();
            }
            _driver = Driver;

            _driver.AnalogOut.set_Polarity(_channelName, AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar);
        }

        private void _Set_DC_Voltage(double Voltage)
        {
            if (Voltage < -10.0)
                Voltage = -10.0;
            else if (Voltage > 10.0)
                Voltage = 10.0;

            _driver.AnalogOut.Generation.set_Voltage(_channelName, Voltage);
        }

        private void _Set_Enabled(bool Enabled)
        {
            _driver.AnalogOut.set_Enabled(_channelName, Enabled);
        }

        private double _Voltage = 0.0;
        public double Voltage
        {
            get { return _Voltage; }
            set
            {
                _Set_DC_Voltage(value);
                _Voltage = value;
            }
        }

        private bool _Enabled = false;
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                _Set_Enabled(value);
                _Enabled = value;
            }
        }
    }
}
