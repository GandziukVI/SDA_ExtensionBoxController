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

    public class Keithley2400 : Keithley24xxBase
    {
        public Keithley2400()
            : base()
        {
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

    public class Keithley2410 : Keithley24xxBase
    {
        public Keithley2410()
            : base()
        {
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

    public class Keithley2420 : Keithley24xxBase
    {
        public Keithley2420()
            : base()
        {
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

    public class Keithley2430 : Keithley24xxBase
    {
        public Keithley2430()
            : base()
        {
        }
    }

    public class Keithley2440 : Keithley24xxBase
    {
        public Keithley2440()
            : base()
        {
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
