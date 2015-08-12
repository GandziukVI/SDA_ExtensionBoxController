using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public class AO_ChannelModeSwitch
    {
        private DigitalBit _Selector_A0;
        private DigitalBit _Selector_A1;
        private DigitalBit _Selector_A2;
        private DigitalBit _Enable;

        public AO_ChannelModeSwitch(DigitalBit Enable, DigitalBit Selector_A0, DigitalBit Selector_A1, DigitalBit Selector_A2)
        {
            if ((Enable == null) || (Selector_A0 == null) || (Selector_A1 == null) || (Selector_A2 == null))
                throw new ArgumentNullException();

            _Enable = Enable;
            _Selector_A0 = Selector_A0;
            _Selector_A1 = Selector_A1;
            _Selector_A2 = Selector_A2;
        }

        public void SetActiveChennel(BOX_AnalogOutChannelsEnum Channel, bool Enabled = true)
        {
            switch (Channel)
            {
                case BOX_AnalogOutChannelsEnum.BOX_AOut_1: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_2: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_3: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_4: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_5: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_6: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_7: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_8: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_9: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_10: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_11: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_12: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Reset(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_13: { _Selector_A0.Reset(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_14: { _Selector_A0.Set(); _Selector_A1.Set(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_15: { _Selector_A0.Set(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                case BOX_AnalogOutChannelsEnum.BOX_AOut_16: { _Selector_A0.Reset(); _Selector_A1.Reset(); _Selector_A2.Set(); } break;
                

                default:
                    break;
            }

            if (Enabled)
                _Enable.Set();
            else
                _Enable.Reset();
        }
    }
}
