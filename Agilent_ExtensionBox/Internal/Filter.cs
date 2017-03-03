using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public enum FilterCutOffFrequencies : int
    {
        Freq_0kHz = 0,
        Freq_10kHz = 10,
        Freq_20kHz = 20,
        Freq_30kHz = 30,
        Freq_40kHz = 40,
        Freq_50kHz = 50,
        Freq_60kHz = 60,
        Freq_70kHz = 70,
        Freq_80kHz = 80,
        Freq_90kHz = 90,
        Freq_100kHz = 100,
        Freq_110kHz = 110,
        Freq_120kHz = 120,
        Freq_130kHz = 130,
        Freq_140kHz = 140,
        Freq_150kHz = 150
    }

    public enum FilterGain : int
    {
        gain1 = 1,
        gain2 = 2,
        gain3 = 3,
        gain4 = 4,
        gain5 = 5,
        gain6 = 6,
        gain7 = 7,
        gain8 = 8,
        gain9 = 9,
        gain10 = 10,
        gain11 = 11,
        gain12 = 12,
        gain13 = 13,
        gain14 = 14,
        gain15 = 15,
        gain16 = 16
    }

    public class Filter
    {
        #region Added here (From Sergii)

        private byte[] _bitmask = new byte[] { 1, 2, 4, 8 };
        
        private DigitalBit FilterGain_Bit0, FilterGain_Bit1, FilterGain_Bit2, FilterGain_Bit3;
        private DigitalBit Frequency_Bit0, Frequency_Bit1, Frequency_Bit2, Frequency_Bit3;
        private DigitalBit HOLD_CS;
       
        private DigitalBit[] FrequencyBits, GainBits;

        int[] CutOffFrequencies = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };

        #endregion

        private DigitalChannel _channel;

        public Filter(DigitalChannel controlChannel)
        {
            #region Added here (From Sergii)

            FilterGain_Bit0 = new DigitalBit(controlChannel, 4);
            FilterGain_Bit1 = new DigitalBit(controlChannel, 5);
            FilterGain_Bit2 = new DigitalBit(controlChannel, 6);
            FilterGain_Bit3 = new DigitalBit(controlChannel, 7);

            Frequency_Bit0 = new DigitalBit(controlChannel, 0);
            Frequency_Bit1 = new DigitalBit(controlChannel, 1);
            Frequency_Bit2 = new DigitalBit(controlChannel, 2);
            Frequency_Bit3 = new DigitalBit(controlChannel, 3);

            FrequencyBits = new DigitalBit[] { Frequency_Bit0, Frequency_Bit1, Frequency_Bit2, Frequency_Bit3 };
            GainBits = new DigitalBit[] { FilterGain_Bit0, FilterGain_Bit1, FilterGain_Bit2, FilterGain_Bit3 };

            #endregion

            if ((controlChannel == null))
                throw new ArgumentNullException();
            if (controlChannel.Width < 8)
                throw new ArgumentException("Too narrow channel width");
            _channel = controlChannel;
        }

        #region Added here from Sergii code

        private FilterCutOffFrequencies freq;
        public FilterCutOffFrequencies Freq
        {
            get { return freq; }
            set
            {
                freq = value;
                value = (FilterCutOffFrequencies)Array.IndexOf(CutOffFrequencies, (int)freq);

                for (int i = 0; i < _bitmask.Length; i++)
                {
                    if ((((int)value & _bitmask[i]) == _bitmask[i]))
                        FrequencyBits[i].Set();
                    else
                        FrequencyBits[i].Reset();
                }
            }
        }

        private FilterGain fGain;
        public FilterGain FGain
        {
            get { return fGain; }
            set 
            {
                fGain = value;

                if (((int)value > 16) || ((int)value < 1))
                    throw new Exception(string.Format("Wring gain passed to filter {0}", (int)value));

                var localval = (int)value - 1;

                for (int i = 0; i < _bitmask.Length; i++)
                {
                    if (((localval & _bitmask[i]) == _bitmask[i]))
                        GainBits[i].Set();
                    else
                        GainBits[i].Reset();
                }
            }
        }

        #endregion

        public FilterCutOffFrequencies CutoffFrequency { get; private set; }

        public FilterGain Gain { get; private set; }

        public void SetCutOffFrequencyAndGain(AnalogInChannelsEnum channelName, FilterCutOffFrequencies cutoff, FilterGain gain)
        {
            if (cutoff < FilterCutOffFrequencies.Freq_0kHz || cutoff > FilterCutOffFrequencies.Freq_150kHz)
                throw new ArgumentException("Frequency out of range");
            if (gain < FilterGain.gain1 || gain > FilterGain.gain16)
                throw new ArgumentException("Gain out of range");
            var valForLatch = (int)cutoff | (int)gain;
            
            _channel.WriteByte(valForLatch);
           
            CutoffFrequency = cutoff;
            Gain = gain;
        }
    }
}
