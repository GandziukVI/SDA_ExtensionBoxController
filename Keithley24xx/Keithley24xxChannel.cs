﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;

namespace Keithley24xx
{
    public class Keithley24xxChannel<T> : ISourceMeterUnit
        where T : Keithley24xxBase, new()
    {
        private ISourceMeterUnit _device;

        public Keithley24xxChannel(ref IDeviceIO Driver)
        {
            _device = new T();
            _device.Initialize(ref Driver);
        }

        public void Initialize(ref IDeviceIO driver)
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