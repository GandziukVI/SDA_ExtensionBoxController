using Agilent_ExtensionBox.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.Internal
{
    public class DCMotor
    {
        private AO_Channel _channel;
        private const double MaxVoltage = 9;
        private const double MinVoltage = 2;
        public DCMotor(AO_Channel Channel)
        {
            if (Channel == null)
                throw new ArgumentNullException();
            _channel = Channel;
        }



    }
}
