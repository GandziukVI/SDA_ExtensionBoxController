using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley24xx
{
    public enum MATH_StateEnum
    {
        ON,
        OFF
    }

    public class Keithley2400 : Keithley24xxChannelBase
    {
        public Keithley2400()
            : base()
        {
            //Complaince settings
            _minCurrentCompliance = 0.000000001;
            _maxCurrentCompliance = 1.05;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 210.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 200.0 };
            _CurrentRanges = new double[] { 0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0 };
        }

        public override ShapeMode SMU_ShapeMode
        {
            get
            {
                return base.SMU_ShapeMode;
            }
            set
            {
                if (value != ShapeMode.Pulse)
                    base.SMU_ShapeMode = value;
                else
                    throw new Exception("Not supported for current Keithley model!");
            }
        }

        public override double PulseWidth
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }

        public override double PulseDelay
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }
    }

    public class Keithley2400_LV : Keithley24xxChannelBase
    {
        public Keithley2400_LV()
            : base()
        {
            //Complaince settings
            _minCurrentCompliance = 0.000000001;
            _maxCurrentCompliance = 1.05;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 210.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 200.0 };
            _CurrentRanges = new double[] { 0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0 };
        }

        public override ShapeMode SMU_ShapeMode
        {
            get
            {
                return base.SMU_ShapeMode;
            }
            set
            {
                if (value != ShapeMode.Pulse)
                    base.SMU_ShapeMode = value;
                else
                    throw new Exception("Not supported for current Keithley model!");
            }
        }

        public override double PulseWidth
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }

        public override double PulseDelay
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }
    }

    public class Keithley2401 : Keithley24xxChannelBase
    {
        public Keithley2401()
            : base()
        {
            //Complaince settings
            _minCurrentCompliance = 0.000000001;
            _maxCurrentCompliance = 1.05;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 210.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 200.0 };
            _CurrentRanges = new double[] { 0.000001, 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0 };
        }

        public override ShapeMode SMU_ShapeMode
        {
            get
            {
                return base.SMU_ShapeMode;
            }
            set
            {
                if (value != ShapeMode.Pulse)
                    base.SMU_ShapeMode = value;
                else
                    throw new Exception("Not supported for current Keithley model!");
            }
        }

        public override double PulseWidth
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }

        public override double PulseDelay
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }
    }

    public class Keithley2410 : Keithley24xxChannelBase
    {
        public Keithley2410()
            : base()
        {
            //Compliance settings
            _minCurrentCompliance = 0.000000001;
            _maxCurrentCompliance = 1.05;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 1100.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 1000.0 };
            _CurrentRanges = new double[] { 0.000001, 0.00001, 0.0001, 0.001, 0.02, 0.1, 1.0 };
        }

        public override ShapeMode SMU_ShapeMode
        {
            get
            {
                return base.SMU_ShapeMode;
            }
            set
            {
                if (value != ShapeMode.Pulse)
                    base.SMU_ShapeMode = value;
                else
                    throw new Exception("Not supported for current Keithley model!");
            }
        }

        public override double PulseWidth
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }

        public override double PulseDelay
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }
    }

    public class Keithley2420 : Keithley24xxChannelBase
    {
        public Keithley2420()
            : base()
        {
            //Compliance settings
            _minCurrentCompliance = 0.00000001;
            _maxCurrentCompliance = 3.15;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 63.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 60.0 };
            _CurrentRanges = new double[] { 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0, 3.0 };
        }

        public override ShapeMode SMU_ShapeMode
        {
            get
            {
                return base.SMU_ShapeMode;
            }
            set
            {
                if (value != ShapeMode.Pulse)
                    base.SMU_ShapeMode = value;
                else
                    throw new Exception("Not supported for current Keithley model!");
            }
        }

        public override double PulseWidth
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }

        public override double PulseDelay
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }
    }

    public class Keithley2425 : Keithley24xxChannelBase
    {
        public Keithley2425()
            : base()
        {
            //Compliance settings
            _minCurrentCompliance = 0.00000001;
            _maxCurrentCompliance = 3.15;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 105.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 100.0 };
            _CurrentRanges = new double[] { 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0, 3.0 };
        }
    }

    public class Keithley2430 : Keithley24xxChannelBase
    {
        public Keithley2430()
            : base()
        {
            //Compliance settings
            _minCurrentCompliance = 0.00000001;
            _maxCurrentCompliance = 3.15;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 105.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 20.0, 100.0 };
            _CurrentRanges = new double[] { 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0, 3.0, 10.0 };
        }
    }

    public class Keithley2440 : Keithley24xxChannelBase
    {
        public Keithley2440()
            : base()
        {
            //Compliance settings
            _minCurrentCompliance = 0.00000001;
            _maxCurrentCompliance = 5.25;

            _minVoltageCompliance = 0.0002;
            _maxVoltageCompliance = 42.0;

            //Source range settings
            _VoltageRanges = new double[] { 0.0002, 2.0, 10.0, 40.0 };
            _CurrentRanges = new double[] { 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0, 5.0 };
        }

        public override ShapeMode SMU_ShapeMode
        {
            get
            {
                return base.SMU_ShapeMode;
            }
            set
            {
                if (value != ShapeMode.Pulse)
                    base.SMU_ShapeMode = value;
                else
                    throw new Exception("Not supported for current Keithley model!");
            }
        }

        public override double PulseWidth
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }

        public override double PulseDelay
        {
            get { throw new Exception("Not supported for current Keithley model!"); }
            set { throw new Exception("Not supported for current Keithley model!"); }
        }
    }
}
