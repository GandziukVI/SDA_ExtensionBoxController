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

        private DigitalChannels _DigitalChannels;

        private DigitalBit _Selector_A0;
        private DigitalBit _Selector_A1;
        private DigitalBit _Selector_A2;
        private DigitalBit _Enable;

        public AO_Channel(AnalogOutChannelsEnum Channel, AgilentU254xClass Driver)
        {
            switch (Channel)
            {
                case AnalogOutChannelsEnum.AOut1: _channelName = ChannelNamesEnum.AOUT1; break;
                case AnalogOutChannelsEnum.AOut2: _channelName = ChannelNamesEnum.AOUT2; break;

                default:
                    throw new ArgumentException();
            }
            
            _driver = Driver;
            _DigitalChannels = new DigitalChannels(_driver);

            switch (Channel)
            {
                case AnalogOutChannelsEnum.AOut1:
                    {
                        _Selector_A0 = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 4);
                        _Selector_A1 = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 5);
                        _Selector_A2 = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 6);

                        _Enable = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 7);
                    } break;
                case AnalogOutChannelsEnum.AOut2:
                    {
                        _Selector_A0 = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 0);
                        _Selector_A1 = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 1);
                        _Selector_A2 = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 2);

                        _Enable = new DigitalBit(_DigitalChannels[DigitalChannelsEnum.DIOA], 3);
                    } break;
                default:
                    break;
            }

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
            
            if (Enabled)
                _Enable.Set();
            else
                _Enable.Reset();
        }

        private void _Set_ActiveChennel(BOX_AnalogOutChannelsEnum Channel)
        {
            switch (Channel)
            {
                case BOX_AnalogOutChannelsEnum.BOX_AOut_01: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_02: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_03: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_04: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_05: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_06: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_07: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_08: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_09: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_10: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_11: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_12: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_13: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_14: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_15: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_16: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;


                default:
                    throw new ArgumentException();
            }
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

        private BOX_AnalogOutChannelsEnum _OutputNumber = BOX_AnalogOutChannelsEnum.BOX_AOut_01;
        public BOX_AnalogOutChannelsEnum OutputNumber
        {
            get { return _OutputNumber; }
            set
            {
                _Set_ActiveChennel(value);
                _OutputNumber = value;
            }
        }
    }
}
