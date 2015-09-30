using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;

namespace Keithley24xx
{
    public class Keithley24xxChannel<T> : ISourceMeterUnit
        where T : Keithley24xxBase
    {
        private SupportedModels _SMU_Model;

        public Keithley24xxChannel()
        {
            if (typeof(T) == typeof(Keithley2400))
                _SMU_Model = SupportedModels.Keithley2400;
            else if (typeof(T) == typeof(Keithley2410))
                _SMU_Model = SupportedModels.Keithley2410;
            else if (typeof(T) == typeof(Keithley2420))
                _SMU_Model = SupportedModels.Keithley2420;
            else if (typeof(T) == typeof(Keithley2430))
                _SMU_Model = SupportedModels.Keithley2430;
            else if (typeof(T) == typeof(Keithley2440))
                _SMU_Model = SupportedModels.Keithley2440;
            else
                throw new ArgumentException("Non-supported model!");
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

        public int Averaging
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

        public double NPLC
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

        public void SwitchON()
        {
            throw new NotImplementedException();
        }

        public void SwitchOFF()
        {
            throw new NotImplementedException();
        }

        public void SetCompliance(SourceMode sourceMode, double compliance)
        {
            throw new NotImplementedException();
        }

        public void SetSourceDelay(double delay)
        {
            throw new NotImplementedException();
        }

        public void SetSourceVoltage(double val)
        {
            throw new NotImplementedException();
        }

        public void SetSourceCurrent(double val)
        {
            throw new NotImplementedException();
        }

        public void SetAveraging(int avg)
        {
            throw new NotImplementedException();
        }

        public void SetNPLC(double val)
        {
            throw new NotImplementedException();
        }

        public double MeasureVoltage()
        {
            throw new NotImplementedException();
        }

        public double MeasureCurrent()
        {
            throw new NotImplementedException();
        }

        public double MeasureResistance()
        {
            throw new NotImplementedException();
        }
    }
}
