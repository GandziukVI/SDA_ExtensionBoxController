
using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public enum ChannelMode
    {
        AC,
        DC
    }

    public class AI_ChannelModeSwitch
    {
        private DigitalBit _pulseBit;
        private DigitalBit _SetResetBit;
        private DigitalBit _SelectorA0;
        private DigitalBit _SelectorA1;
        private object SyncRoot = new object();
        public AI_ChannelModeSwitch(DigitalBit PulseBit, DigitalBit SetResetBit, DigitalBit SelectorA0, DigitalBit SelectorA1)
        {
            if ((PulseBit == null) || (SetResetBit == null) && (SelectorA0 == null) || (SelectorA1 == null))
                throw new ArgumentNullException();
            _pulseBit = PulseBit;
            _SetResetBit = SetResetBit;
            _SelectorA0 = SelectorA0;
            _SelectorA1 = SelectorA1;
        }

        public void SetChannelMode(AnalogInChannelsEnum channel, ChannelMode mode)
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
                        _SelectorA0.Reset();
                        _SelectorA1.Reset();
                    } break;
                case AnalogInChannelsEnum.AIn3:
                    {
                        _SelectorA0.Reset();
                        _SelectorA1.Reset();
                    } break;
                case AnalogInChannelsEnum.AIn4:
                    {
                        _SelectorA0.Reset();
                        _SelectorA1.Reset();
                    } break;
                default:
                    throw new ArgumentException();
            }
            switch (mode)
            {
                case ChannelMode.AC:
                    _SetResetBit.Reset();
                    break;
                case ChannelMode.DC:
                    _SetResetBit.Set();
                    break;
                default:
                    throw new ArgumentException();
            }
            Pulse();

        }

        private void Pulse()
        {
            _pulseBit.Set();
            System.Threading.Thread.Sleep(100);
            _pulseBit.Reset();
        }
    }
}
