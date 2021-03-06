﻿using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public class AnalogInLatch : ILatch, IDisposable
    {
        private DigitalBit _Selector_ADC_A0;
        private DigitalBit _Selector_ADC_A1;
        private DigitalBit _LatchPulseBit;

        public AnalogInLatch(DigitalBit Selector_ADC_A0, DigitalBit Selector_ADC_A1, DigitalBit LatchPulseBit)
        {
            if ((Selector_ADC_A0 == null) || (Selector_ADC_A1 == null) || (LatchPulseBit == null))
                throw new ArgumentException();
            _Selector_ADC_A0 = Selector_ADC_A0;
            _Selector_ADC_A1 = Selector_ADC_A1;
            _LatchPulseBit = LatchPulseBit;
        }

        public void PulseLatchForChannel(Enum ChannelName)
        {
            var channelName = (AnalogInChannelsEnum)ChannelName;
            switch (channelName)
            {
                case AnalogInChannelsEnum.AIn1:
                    {
                        _Selector_ADC_A0.Reset();
                        _Selector_ADC_A1.Reset();
                    }
                    break;
                case AnalogInChannelsEnum.AIn2:
                    {
                        _Selector_ADC_A0.Set();
                        _Selector_ADC_A1.Reset();
                    }
                    break;
                case AnalogInChannelsEnum.AIn3:
                    {
                        _Selector_ADC_A0.Reset();
                        _Selector_ADC_A1.Set();
                    }
                    break;
                case AnalogInChannelsEnum.AIn4:
                    {
                        _Selector_ADC_A0.Set();
                        _Selector_ADC_A1.Set();
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
            _LatchPulseBit.Pulse();
        }

        public void Dispose()
        {
            _LatchPulseBit.Dispose();
            _Selector_ADC_A1.Dispose();
            _Selector_ADC_A0.Dispose();
        }
    }
}
