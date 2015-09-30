using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;

namespace Keithley24xx
{
    public class Keithley24xxBase : ISourceMeterUnit
    {
        public void Initialize(IDeviceIO driver)
        {
            throw new NotImplementedException();
        }

        public ShapeMode SMU_ShapeMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public SourceMode SMU_SourceMode
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Voltage
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Current
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Resistance
        {
            get { throw new NotImplementedException(); }
        }

        public double Compliance
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double PulseWidth
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double PulseDelay
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual int Averaging
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual double NPLC
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public virtual void SwitchON()
        {
            throw new NotImplementedException();
        }

        public virtual void SwitchOFF()
        {
            throw new NotImplementedException();
        }

        public virtual void SetCompliance(SourceMode sourceMode, double compliance)
        {
            throw new NotImplementedException();
        }

        public virtual void SetSourceDelay(double delay)
        {
            throw new NotImplementedException();
        }

        public virtual void SetSourceVoltage(double val)
        {
            throw new NotImplementedException();
        }

        public virtual void SetSourceCurrent(double val)
        {
            throw new NotImplementedException();
        }

        public virtual void SetAveraging(int avg)
        {
            throw new NotImplementedException();
        }

        public virtual void SetNPLC(double val)
        {
            throw new NotImplementedException();
        }

        public virtual double MeasureVoltage()
        {
            throw new NotImplementedException();
        }

        public virtual double MeasureCurrent()
        {
            throw new NotImplementedException();
        }

        public virtual double MeasureResistance()
        {
            throw new NotImplementedException();
        }


       
    }
}
