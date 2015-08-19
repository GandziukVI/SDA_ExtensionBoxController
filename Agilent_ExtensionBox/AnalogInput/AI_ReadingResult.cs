using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AI_ReadingResult
    {
        public AnalogInChannelsEnum ChannelName { get; private set; }
        public LinkedList<double> Readings { get; private set; }

        public AI_ReadingResult(AnalogInChannelsEnum ChannelID)
        {
            ChannelName = ChannelID;
            Readings = new LinkedList<double>();
        }
    }
}
