using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public enum FilterCutOffFrequencies:int
    {
        Freq_0kHz = 0x00,
        Freq_10kHz = 0x01,
        Freq_20kHz = 0x02,
        Freq_30kHz = 0x03,
        Freq_40kHz = 0x04,
        Freq_50kHz = 0x05,
        Freq_60kHz = 0x06,
        Freq_70kHz = 0x07,
        Freq_80kHz = 0x08,
        Freq_90kHz = 0x09,
        Freq_100kHz = 0x0a,
        Freq_110kHz = 0x0b,
        Freq_120kHz = 0x0c,
        Freq_130kHz = 0x0d,
        Freq_140kHz = 0x0e,
        Freq_150kHz = 0x0f
    }

    public enum FilterGain
    {
        gain1,
        gain2,
        gain3,
        gain4,
        gain5,
        gain6,
        gain7,
        gain8,
        gain9,
        gain10,
        gain11,
        gain12,
        gain13,
        gain14,
        gain15,
        gain16
    }
    public class Filter
    {
        private DigitalBit _F0;
        private DigitalBit _F1;
        private DigitalBit _F2;
        private DigitalBit _F3;

        private DigitalBit _G0;
        private DigitalBit _G1;
        private DigitalBit _G2;
        private DigitalBit _G3;

        private Latch _letch;

        public Filter(DigitalBit F0,DigitalBit F1,DigitalBit F2,DigitalBit F3,DigitalBit G0,DigitalBit G1,DigitalBit G2,DigitalBit G3, Latch letch)
        {
            if ((F0 == null) || (F1 == null) || (F2 == null) || (F3 == null) || (G0 == null) || (G1 == null) || (G2 == null) || (G3 == null)||(letch == null ))
                throw new ArgumentNullException();
            _F0 = F0;
            _F1 = F1;
            _F2 = F2;
            _F3 = F3;

            _G0 = G0;
            _G1 = G1;
            _G2 = G2;
            _G3 = G3;

            _letch = letch;
        }

        public void SetCutOffFrequency(AnalogInChannelsEnum channelName, FilterCutOffFrequencies cutoff, FilterGain gain)
        {
            switch (cutoff)
            {
                case FilterCutOffFrequencies.Freq_0kHz:
                    { }
                    break;
                case FilterCutOffFrequencies.Freq_10kHz:
                    break;
                case FilterCutOffFrequencies.Freq_20kHz:
                    break;
                case FilterCutOffFrequencies.Freq_30kHz:
                    break;
                case FilterCutOffFrequencies.Freq_40kHz:
                    break;
                case FilterCutOffFrequencies.Freq_50kHz:
                    break;
                case FilterCutOffFrequencies.Freq_60kHz:
                    break;
                case FilterCutOffFrequencies.Freq_70kHz:
                    break;
                case FilterCutOffFrequencies.Freq_80kHz:
                    break;
                case FilterCutOffFrequencies.Freq_90kHz:
                    break;
                case FilterCutOffFrequencies.Freq_100kHz:
                    break;
                case FilterCutOffFrequencies.Freq_110kHz:
                    break;
                case FilterCutOffFrequencies.Freq_120kHz:
                    break;
                case FilterCutOffFrequencies.Freq_130kHz:
                    break;
                case FilterCutOffFrequencies.Freq_140kHz:
                    break;
                case FilterCutOffFrequencies.Freq_150kHz:
                    break;
                default:
                    throw new ArgumentException();
            }
            switch (gain)
            {
                case FilterGain.gain1:
                    break;
                case FilterGain.gain2:
                    break;
                case FilterGain.gain3:
                    break;
                case FilterGain.gain4:
                    break;
                case FilterGain.gain5:
                    break;
                case FilterGain.gain6:
                    break;
                case FilterGain.gain7:
                    break;
                case FilterGain.gain8:
                    break;
                case FilterGain.gain9:
                    break;
                case FilterGain.gain10:
                    break;
                case FilterGain.gain11:
                    break;
                case FilterGain.gain12:
                    break;
                case FilterGain.gain13:
                    break;
                case FilterGain.gain14:
                    break;
                case FilterGain.gain15:
                    break;
                case FilterGain.gain16:
                    break;
                default:
                    break;
            }

        }
    }
}
