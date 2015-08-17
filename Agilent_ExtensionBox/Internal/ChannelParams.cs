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
        public AnalogLatch CommonLatch { get; private set; }
        private AnalogInChannelsEnum _channelName;

        public ChannelParams(AnalogInChannelsEnum channelName, Filter channelFilter, ProgrammableGainAmplifier channelPGA, AnalogLatch channelLatch)
        {
            _channelName = channelName;
            ChannelFilter = channelFilter;
            ChannelPGA = channelPGA;
            CommonLatch = channelLatch;
        }

        public void SetParams(FilterCutOffFrequencies cutoff, FilterGain filter_gain, PGA_GainsEnum pga_gain)
        {
            ChannelFilter.SetCutOffFrequencyAndGain(_channelName, cutoff, filter_gain);
            ChannelPGA.SetAmplification(pga_gain);
            CommonLatch.PulseLatchForChannel(_channelName);
        }


    }
}
