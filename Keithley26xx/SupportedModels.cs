using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley26xx
{
    //public class 

    public class Keithley2611B : Keithley26xxChannelBase
    {
        public Keithley2611B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;
        }
    }

    public class Keithley2612B : Keithley26xxChannelBase
    {
        public Keithley2612B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;
        }
    }

    public class Keithley2614B : Keithley26xxChannelBase
    {
        public Keithley2614B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;
        }
    }

    public class Keithley2634B : Keithley26xxChannelBase
    {
        public Keithley2634B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;
        }
    }

    public class Keithley2635B : Keithley26xxChannelBase
    {
        public Keithley2635B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;
        }
    }

    [NumberOfChannels(2)]
    public class Keithley2636B : Keithley26xxChannelBase
    {
        public Keithley2636B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;
        }
    }
}
