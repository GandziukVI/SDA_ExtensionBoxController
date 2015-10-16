using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley26xx
{
    public class Keithley2601B : Keithley26xxChannelBase
    {
        public Keithley2601B()
            : base()
        {
            minVoltageVal = -40.0;
            maxVoltageVal = 40.0;

            minCurrentVal = -3.0;
            maxCurrentVal = 3.0;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 40.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 1.0;
        }
    }

    [NumberOfChannels(2)]
    public class Keithley2602B : Keithley26xxChannelBase
    {
        public Keithley2602B()
            : base()
        {
            minVoltageVal = -40.0;
            maxVoltageVal = 40.0;

            minCurrentVal = -3.0;
            maxCurrentVal = 3.0;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 40.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 1.0;
        }
    }

    [NumberOfChannels(2)]
    public class Keithley2604B : Keithley26xxChannelBase
    {
        public Keithley2604B()
            : base()
        {
            minVoltageVal = -40.0;
            maxVoltageVal = 40.0;

            minCurrentVal = -3.0;
            maxCurrentVal = 3.0;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 40.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 1.0;
        }
    }

    public class Keithley2611B : Keithley26xxChannelBase
    {
        public Keithley2611B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;

            minCurrentVal = -1.5;
            maxCurrentVal = 1.5;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 20.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 0.1;
        }
    }

    [NumberOfChannels(2)]
    public class Keithley2612B : Keithley26xxChannelBase
    {
        public Keithley2612B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;

            minCurrentVal = -1.5;
            maxCurrentVal = 1.5;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 20.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 0.1;
        }
    }

    [NumberOfChannels(2)]
    public class Keithley2614B : Keithley26xxChannelBase
    {
        public Keithley2614B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;

            minCurrentVal = -1.5;
            maxCurrentVal = 1.5;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 20.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 0.1;
        }
    }

    [NumberOfChannels(2)]
    public class Keithley2634B : Keithley26xxChannelBase
    {
        public Keithley2634B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;

            minCurrentVal = -1.5;
            maxCurrentVal = 1.5;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 20.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 0.1;
        }
    }

    public class Keithley2635B : Keithley26xxChannelBase
    {
        public Keithley2635B()
            : base()
        {
            minVoltageVal = -200.0;
            maxVoltageVal = 200.0;

            minCurrentVal = -1.5;
            maxCurrentVal = 1.5;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 20.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 0.1;
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

            minCurrentVal = -1.5;
            maxCurrentVal = 1.5;

            _minVoltageCompliance = 0.0;
            _maxVoltageCompliance = 20.0;

            _minCurrentCompliance = 0.0;
            _maxCurrentCompliance = 0.1;
        }
    }
}
