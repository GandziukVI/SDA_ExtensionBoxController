using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public interface ILatch
    {
        void PulseLatchForChannel(Enum channelName);
    }
}
