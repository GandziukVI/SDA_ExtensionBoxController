using Agilent.AgilentU254x.Interop;
using Agilent_ExtensionBox.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class AI_ChannelConfig
    {
        public AnalogInChannelsEnum ChannelName { get; set; }
        public bool Enabled { get; set; }
        public ChannelModeEnum Mode { get; set; }
        public AgilentU254xAnalogPolarityEnum Polarity { get; set; }
        public Ranges Range { get; set; }
    }
}
