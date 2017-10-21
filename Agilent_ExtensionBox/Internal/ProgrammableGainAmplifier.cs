using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public enum PGA_GainsEnum : int
    {
        gain1 = 0x00,
        gain10 = 0x01,
        gain100 = 0x02
    }

    public class ProgrammableGainAmplifier : IDisposable
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
            _channel.WriteByte((int)gain);
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
