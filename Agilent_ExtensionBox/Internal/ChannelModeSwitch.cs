
using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public enum ChannelModeEnum
    {
        AC,
        DC
    }

    public class ChannelModeSwitch
    {
        private DigitalBit _pulseBit;
        private DigitalBit _setResetBit;
        private DigitalBit _SelectorA0;
        private DigitalBit _SelectorA1;

        

        public ChannelModeSwitch(DigitalBit PulseBit, DigitalBit SetResetBit, DigitalBit SelectorA0, DigitalBit SelectorA1)
        {
            if ((PulseBit == null) || (SetResetBit == null) && (SelectorA0 == null) || (SelectorA1 == null))
                throw new ArgumentNullException();

            _pulseBit = PulseBit;
            _setResetBit = SetResetBit;

            _SelectorA0 = SelectorA0;
            _SelectorA1 = SelectorA1;
        }

        public void SetChannelMode(AnalogInChannelsEnum channel, ChannelModeEnum mode)
        {

            switch (channel)
            {
                case AnalogInChannelsEnum.AIn1:
                    {
                        _SelectorA0.Reset();
                        _SelectorA1.Reset();
                    } break;
                case AnalogInChannelsEnum.AIn2:
                    {
                        _SelectorA0.Set();
                        _SelectorA1.Reset();
                    } break;
                case AnalogInChannelsEnum.AIn3:
                    {
                        _SelectorA0.Reset();
                        _SelectorA1.Set();
                    } break;
                case AnalogInChannelsEnum.AIn4:
                    {
                        _SelectorA0.Set();
                        _SelectorA1.Set();
                    } break;
                default:
                    throw new ArgumentException();
            }
            switch (mode)
            {
                case ChannelModeEnum.AC:
                    _setResetBit.Reset();
                    break;
                case ChannelModeEnum.DC:
                    _setResetBit.Set();
                    break;
                default:
                    throw new ArgumentException();
            }

            Pulse();
        }

        private void Pulse()
        {
            _pulseBit.Pulse();
        }
    }
}
