using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;
using System.Globalization;

namespace Keithley26xx
{
    public enum BeeperFrequencyEnum
    {
        _2400_Hz = 2400
    }

    public class Keithley26xxChannelBase : ISourceMeterUnit
    {
        public string ChannelIdentifier { get; protected set; }

        private IDeviceIO _driver;

        public Keithley26xxChannelBase()
        {
            _SetBeeperEnabled(true);
        }

        #region Beeper functionality

        private bool _currentBeeperEnabledState = false;
        private void _SetBeeperEnabled(bool enabled)
        {
            if (enabled != _currentBeeperEnabledState)
            {
                if (enabled)
                    _driver.SendCommandRequest("beeper.enable = beeper.ON");
                else
                    _driver.SendCommandRequest("beeper.enable = beeper.OFF");

                _currentBeeperEnabledState = enabled;
            }
        }

        protected void Beep(double duration, BeeperFrequencyEnum frequency)
        {
            _driver.SendCommandRequest(string.Format("beeper.beep({0}, {1})", duration.ToString(NumberFormatInfo.InvariantInfo), (int)frequency));
        }

        #endregion

        public void Initialize(IDeviceIO Driver)
        {
            _driver = Driver;
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
