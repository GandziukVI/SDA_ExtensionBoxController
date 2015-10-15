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

        public Keithley26xxChannelBase(IDeviceIO Driver, string channelIdentifier)
        {
            _driver = Driver;
            _SetBeeperEnabled(true);

            ChannelIdentifier = channelIdentifier;

            _LoadDeviceFunctions();
        }

        private void _LoadDeviceFunctions()
        {
            var scriptFormat = string.Concat(
                "reset()\n",
                "DeviceFunctionsChannel{0} = nil\n",
                
                "loadandrunscript DeviceFunctionsChannel{0}\n",
                
                "result = nil\n",

                "current_NAvg = nil\n",
                "function MeasureVoltage(nAvg)\n",
                    "_nAvg = tonumber(nAvg)\n",
                    "if _nAvg ~= current_NAvg then\n",
                        "current_NAvg = _nAvg",
                        
                        "if _nAvg < 1 then\n",
                            "_nAvg = 1\n",
                        "elseif _nAvg > 9999 then\n",
                            "_nAvg = 9999\n",
                        "end\n",
                        
                        "smu{0}.measure.count = _nAvg\n",
                    "end\n",
                    
                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.v(smu{0}.nvbuffer1)\n",

                   "loopIterator = 1\n",
                   "result = 0.0\n",
                   "while smu{0}.nvbuffer1[loopIterator] do\n",
                       "result = result + smu{0}.nvbuffer1[loopIterator]\n",
                       "loopIterator = loopIterator + 1\n",
                    "end\n",

                    "result = result / _nAvg\n",

                    "print(result)",
                "end\n",
                
                "function MeasureCurrent(nAvg)\n",
                    "_nAvg = tonumber(nAvg)\n",
                    "if _nAvg ~= current_NAvg then\n",
                        "current_NAvg = _nAvg",
                        
                        "if _nAvg < 1 then\n",
                            "_nAvg = 1\n",
                        "elseif _nAvg > 9999 then\n",
                            "_nAvg = 9999\n",
                        "end\n",
                        
                        "smu{0}.measure.count = _nAvg\n",
                    "end\n",
                    
                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.i(smu{0}.nvbuffer1)\n",

                   "loopIterator = 1\n",
                   "result = 0.0\n",
                   "while smu{0}.nvbuffer1[loopIterator] do\n",
                       "result = result + smu{0}.nvbuffer1[loopIterator]\n",
                       "loopIterator = loopIterator + 1\n",
                    "end\n",

                    "result = result / _nAvg\n",

                    "print(result)",
                "end\n",
                
                "function MeasureResistance(nAvg)\n",
                    "_nAvg = tonumber(nAvg)\n",
                    "if _nAvg ~= current_NAvg then\n",
                        "current_NAvg = _nAvg",

                        "if _nAvg < 1 then\n",
                            "_nAvg = 1\n",
                        "elseif _nAvg > 9999 then\n",
                            "_nAvg = 9999\n",
                        "end\n",

                        "smu{0}.measure.count = _nAvg\n",
                    "end\n",
                    
                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.r(smu{0}.nvbuffer1)\n",

                   "loopIterator = 1\n",
                   "result = 0.0\n",
                   "while smu{0}.nvbuffer1[loopIterator] do\n",
                       "result = result + smu{0}.nvbuffer1[loopIterator]\n",
                       "loopIterator = loopIterator + 1\n",
                    "end\n",

                    "result = result / _nAvg\n",

                    "print(result)",
                "end\n",
                
                "function MeasureConductance(nAvg)\n",
                    "_nAvg = tonumber(nAvg)\n",
                    "if _nAvg ~= current_NAvg then\n",
                        "current_NAvg = _nAvg",

                        "if _nAvg < 1 then\n",
                            "_nAvg = 1\n",
                        "elseif _nAvg > 9999 then\n",
                            "_nAvg = 9999\n",
                        "end\n",

                        "smu{0}.measure.count = _nAvg\n",
                    "end\n",
                    
                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.r(smu{0}.nvbuffer1)\n",

                   "loopIterator = 1\n",
                   "result = 0.0\n",
                   "while smu{0}.nvbuffer1[loopIterator] do\n",
                       "result = result + smu{0}.nvbuffer1[loopIterator]\n",
                       "loopIterator = loopIterator + 1\n",
                    "end\n",

                    "result = 1.0 / (result / _nAvg)\n",

                    "print(result)",
                "end\n");

            _driver.SendCommandRequest(string.Format(scriptFormat, ChannelIdentifier));
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
                return MeasureVoltage();
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
                return MeasureCurrent();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double Resistance
        {
            get { return MeasureResistance(); }
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
                return _currentAveraging;
            }
            set
            {
                SetAveraging(value);
            }
        }

        private double _NPLC = 1.0;
        public double NPLC
        {
            get
            {
                return _NPLC;
            }
            set
            {
                _NPLC = value;
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

        private int _currentAveraging = 10;
        public void SetAveraging(int avg)
        {
            _currentAveraging = avg;
        }

        public void SetNPLC(double val)
        {
            throw new NotImplementedException();
        }

        public double MeasureVoltage()
        {
            var responce = _driver.RequestQuery(string.Format("MeasureVoltage({0})", _currentAveraging));

            var result = 0.0;
            var success = double.TryParse(responce, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

            if (success)
                return result;
            else
                throw new Exception("Can't read voltage!");
        }

        public double MeasureCurrent()
        {
            var responce = _driver.RequestQuery(string.Format("MeasureCurrent({0})", _currentAveraging));

            var result = 0.0;
            var success = double.TryParse(responce, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

            if (success)
                return result;
            else
                throw new Exception("Can't read current!");
        }

        public double MeasureResistance()
        {
            var responce = _driver.RequestQuery(string.Format("MeasureResistance({0})", _currentAveraging));

            var result = 0.0;
            var success = double.TryParse(responce, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

            if (success)
                return result;
            else
                throw new Exception("Can't read resistance!");
        }
    }
}
