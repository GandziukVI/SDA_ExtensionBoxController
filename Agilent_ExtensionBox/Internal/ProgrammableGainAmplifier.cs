using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public enum PGA_GainsEnum
    {
        gain1 = 0x01,
        gain10 = 0x02,
        gain100 = 0x03
    }
    public class ProgrammableGainAmplifier
    {
        private DigitalChannel _channel;
        public ProgrammableGainAmplifier(DigitalChannel ControlChannel)
        {
            _channel = ControlChannel;
        }

        public PGA_GainsEnum Gain { get; private set; }

        public void SetAmplification(PGA_GainsEnum gain)
        {
            if (gain < PGA_GainsEnum.gain1 || gain > PGA_GainsEnum.gain100)
                throw new ArgumentException("Gain out of range");
            Gain = gain;

        }
    }
}
