using System;
using System.Collections.Generic;
using System.Text;
using Agilent_ExtensionBox.IO;

namespace Agilent_ExtensionBox.Internal
{
    public class ChannelParams
    {
        public Filter ChannelFilter { get; private set; }
        public ProgrammableGainAmplifier ChannelPGA { get; private set; }
        public AnalogInLatch CommonLatch { get; private set; }
        private AnalogInChannelsEnum _channelName;

        public ChannelParams(AnalogInChannelsEnum channelName, Filter channelFilter, ProgrammableGainAmplifier channelPGA, AnalogInLatch channelLatch)
        {
            _channelName = channelName;
            ChannelFilter = channelFilter;
            ChannelPGA = channelPGA;
            CommonLatch = channelLatch;
        }

        public void SetParams(FilterCutOffFrequencies cutoff, FilterGain filter_gain, PGA_GainsEnum pga_gain)
        {
            ChannelFilter.SetCutOffFrequencyAndGain(_channelName, cutoff, filter_gain);
            CommonLatch.PulseLatchForChannel(_channelName);
            ChannelPGA.SetAmplification(pga_gain);
        }

        public void SetCutoffFrequency(FilterCutOffFrequencies cutoff)
        {
            SetParams(cutoff, ChannelFilter.Gain, ChannelPGA.Gain);
        }

        public void SetFilter_Gain(FilterGain gain)
        {
            SetParams(ChannelFilter.CutoffFrequency, gain, ChannelPGA.Gain);
        }

        public void SetPGA_Gain(PGA_GainsEnum gain)
        {
            SetParams(ChannelFilter.CutoffFrequency, ChannelFilter.Gain, gain);
        }
    }
}
