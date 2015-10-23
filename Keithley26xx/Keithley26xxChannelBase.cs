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

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class NumberOfChannelsAttribute : Attribute
    {
        public int NumberOfChannels { get; protected set; }

        public NumberOfChannelsAttribute(int numberOfChannels)
            : base()
        {
            NumberOfChannels = numberOfChannels;
        }
    }

    [NumberOfChannels(1)]
    public class Keithley26xxChannelBase : ISourceMeterUnit
    {
        private int _numberOfChannels = 1;
        public int NumberOfChannels
        {
            get { return _numberOfChannels; }
            set
            {
                if (value > 2)
                    throw new ArgumentException("Unavailable number of channels!");

                _numberOfChannels = value;
            }
        }

        public string ChannelIdentifier { get; protected set; }

        private IDeviceIO _driver;
        private Keithley26xxB_Display _display;

        private void _LoadDeviceFunctions()
        {
            var scriptFormat = string.Concat(
                "reset()\n",
                "DeviceFunctionsChannel{0} = nil\n",

                "loadscript DeviceFunctionsChannel{0}\n",

                "result_{0} = nil\n",

                "currentNAvg_smu_{0} = smu{0}.measure.count\n",
                "currentNPLC_smu_{0} = smu{0}.measure.nplc\n",
                "currentDelay_smu_{0} = smu{0}.measure.delay\n",

                "currentCurrentLimit_smu_{0} = smu{0}.source.limiti\n",
                "currentVoltageLimit_smu_{0} = smu{0}.source.limitv\n",
                "currentOutputFunc_smu_{0} = smu{0}.source.func\n",

                "currentMeasureCurrent_smu{0}_AutorangeState = smu{0}.measure.autorangei\n",
                "currentMeasureVoltage_smu{0}_AutorangeState = smu{0}.measure.autorangev\n",

                "function SetVoltage_smu{0}(srcVolt, srcLimitI)\n",
                    "_srcVolt = tonumber(srcVolt)\n",
                    "_srcLimitI = tonumber(srcLimitI)\n",

                    "if currentOutputFunc_smu_{0} ~= smu{0}.OUTPUT_DCVOLTS then\n",
                        "currentOutputFunc_smu_{0} = smu{0}.OUTPUT_DCVOLTS\n",
                        "smu{0}.source.func = smu{0}.OUTPUT_DCVOLTS\n",
                    "end\n",

                    "if currentCurrentLimit_smu_{0} ~= _srcLimitI then\n",
                        "currentCurrentLimit_smu_{0} = _srcLimitI\n",
                        "smu{0}.source.limiti = _srcLimitI\n",
                    "end\n",

                    "smu{0}.source.levelv = _srcVolt\n",
                "end\n",

                "function MeasureVoltage_smu{0}(devAvg, devNPLC, devDelay)\n",
                    "_devAvg = tonumber(devAvg)\n",
                    "_devNPLC = tonumber(devNPLC)\n",
                    "_devDelay = tonumber(devDelay)\n",

                    "if currentNPLC_smu_{0} ~= _devNPLC then\n",
                        "currentNPLC_smu_{0} = _devNPLC\n",

                        "if _devNPLC < 0.01 then\n",
                            "_devNPLC = 0.01\n",
                        "elseif _devNPLC > 25.0 then\n",
                            "_devNPLC = 25.0\n",
                        "end\n",

                        "smu{0}.measure.nplc = _devNPLC\n",
                    "end\n",

                    "if currentDelay_smu_{0} ~= _devDelay then\n",
                        "currentDelay_smu_{0} = _devDelay\n",

                        "if _devDelay == 0.0 then\n",
                            "smu{0}.measure.delay = smu{0}.DELAY_AUTO\n",
                        "else smu{0}.measure.delay = _devDelay\n",
                        "end\n",
                    "end\n",

                    "if _devAvg ~= currentNAvg_smu_{0} then\n",
                        "currentNAvg_smu_{0} = _devAvg\n",

                        "if  _devAvg < 1 then\n",
                            "_devAvg = 1\n",
                        "elseif  _devAvg > 9999 then\n",
                            "_devAvg = 9999\n",
                        "end\n",

                        "smu{0}.measure.count = _devAvg\n",
                    "end\n",

                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.v(smu{0}.nvbuffer1)\n",

                    "result_{0} = 0.0\n",
                    "for loopIterator = 1, smu{0}.nvbuffer1.n do\n",
                        "result_{0} = result_{0} + smu{0}.nvbuffer1[loopIterator]\n",
                    "end\n",

                    "result_{0} = result_{0} / _devAvg\n",

                    "print(result_{0})\n",
                "end\n",

                "function SetCurrent_smu{0}(srcCurr, srcLimitV)\n",
                    "_srcCurr = tonumber(srcCurr)\n",
                    "_srcLimitV = tonumber(srcLimitV)\n",

                    "if currentOutputFunc_smu_{0} ~= smu{0}.OUTPUT_DCAMPS then\n",
                        "currentOutputFunc_smu_{0} = smu{0}.OUTPUT_DCAMPS\n",
                        "smu{0}.source.func = smu{0}.OUTPUT_DCAMPS\n",
                    "end\n",

                    "if currentVoltageLimit_smu_{0} ~= _srcLimitV then\n",
                        "currentVoltageLimit_smu_{0} = _srcLimitV\n",
                        "smu{0}.source.limitv = _srcLimitV\n",
                    "end\n",

                    "smu{0}.source.leveli = _srcCurr\n",
                "end\n",

                "function MeasureCurrent_smu{0}(devAvg, devNPLC, devDelay)\n",
                    "_devAvg = tonumber(devAvg)\n",
                    "_devNPLC = tonumber(devNPLC)\n",
                    "_devDelay = tonumber(devDelay)\n",

                    "if currentNPLC_smu_{0} ~= _devNPLC then\n",
                        "currentNPLC_smu_{0} = _devNPLC\n",

                        "if _devNPLC < 0.01 then\n",
                            "_devNPLC = 0.01\n",
                        "elseif _devNPLC > 25.0 then\n",
                            "_devNPLC = 25.0\n",
                        "end\n",

                        "smu{0}.measure.nplc = _devNPLC\n",
                    "end\n",

                    "if currentDelay_smu_{0} ~= _devDelay then\n",
                        "currentDelay_smu_{0} = _devDelay\n",

                        "if _devDelay == 0.0 then\n",
                            "smu{0}.measure.delay = smu{0}.DELAY_AUTO\n",
                        "else smu{0}.measure.delay = _devDelay\n",
                        "end\n",
                    "end\n",

                    "if _devAvg ~= currentNAvg_smu_{0} then\n",
                        "currentNAvg_smu_{0} = _devAvg\n",

                        "if  _devAvg < 1 then\n",
                            "_devAvg = 1\n",
                        "elseif  _devAvg > 9999 then\n",
                            "_devAvg = 9999\n",
                        "end\n",

                        "smu{0}.measure.count = _devAvg\n",
                    "end\n",

                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.i(smu{0}.nvbuffer1)\n",

                    "result_{0} = 0.0\n",
                    "for loopIterator = 1, smu{0}.nvbuffer1.n do\n",
                        "result_{0} = result_{0} + smu{0}.nvbuffer1[loopIterator]\n",
                    "end\n",

                    "result_{0} = result_{0} / _devAvg\n",

                    "print(result_{0})\n",
                "end\n",

                "function MeasureResistance_smu{0}(devAvg, devNPLC, devDelay)\n",
                    "_devAvg = tonumber(devAvg)\n",
                    "_devNPLC = tonumber(devNPLC)\n",
                    "_devDelay = tonumber(devDelay)\n",

                    "if currentNPLC_smu_{0} ~= _devNPLC then\n",
                        "currentNPLC_smu_{0} = _devNPLC\n",

                        "if _devNPLC < 0.01 then\n",
                            "_devNPLC = 0.01\n",
                        "elseif _devNPLC > 25.0 then\n",
                            "_devNPLC = 25.0\n",
                        "end\n",

                        "smu{0}.measure.nplc = _devNPLC\n",
                    "end\n",

                    "if currentDelay_smu_{0} ~= _devDelay then\n",
                        "currentDelay_smu_{0} = _devDelay\n",

                        "if _devDelay == 0.0 then\n",
                            "smu{0}.measure.delay = smu{0}.DELAY_AUTO\n",
                        "else smu{0}.measure.delay = _devDelay\n",
                        "end\n",
                    "end\n",

                    "if _devAvg ~= currentNAvg_smu_{0} then\n",
                        "currentNAvg_smu_{0} = _devAvg\n",

                        "if  _devAvg < 1 then\n",
                            "_devAvg = 1\n",
                        "elseif  _devAvg > 9999 then\n",
                            "_devAvg = 9999\n",
                        "end\n",

                        "smu{0}.measure.count = _devAvg\n",
                    "end\n",

                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.r(smu{0}.nvbuffer1)\n",

                    "result_{0} = 0.0\n",
                    "for loopIterator = 1, smu{0}.nvbuffer1.n do\n",
                        "result_{0} = result_{0} + smu{0}.nvbuffer1[loopIterator]\n",
                    "end\n",

                    "result_{0} = result_{0} / _devAvg\n",

                    "print(result_{0})\n",
                "end\n",

                "function MeasureConductance_smu{0}(devAvg, devNPLC, devDelay)\n",
                    "_devAvg = tonumber(devAvg)\n",
                    "_devNPLC = tonumber(devNPLC)\n",
                    "_devDelay = tonumber(devDelay)\n",

                    "if currentNPLC_smu_{0} ~= _devNPLC then\n",
                        "currentNPLC_smu_{0} = _devNPLC\n",

                        "if _devNPLC < 0.01 then\n",
                            "_devNPLC = 0.01\n",
                        "elseif _devNPLC > 25.0 then\n",
                            "_devNPLC = 25.0\n",
                        "end\n",

                        "smu{0}.measure.nplc = _devNPLC\n",
                    "end\n",

                    "if currentDelay_smu_{0} ~= _devDelay then\n",
                        "currentDelay_smu_{0} = _devDelay\n",

                        "if _devDelay == 0.0 then\n",
                            "smu{0}.measure.delay = smu{0}.DELAY_AUTO\n",
                        "else smu{0}.measure.delay = _devDelay\n",
                        "end\n",
                    "end\n",

                    "if _devAvg ~= currentNAvg_smu_{0} then\n",
                        "currentNAvg_smu_{0} = _devAvg\n",

                        "if  _devAvg < 1 then\n",
                            "_devAvg = 1\n",
                        "elseif  _devAvg > 9999 then\n",
                            "_devAvg = 9999\n",
                        "end\n",

                        "smu{0}.measure.count = _devAvg\n",
                    "end\n",

                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.measure.r(smu{0}.nvbuffer1)\n",

                    "result_{0} = 0.0\n",
                    "for loopIterator = 1, smu{0}.nvbuffer1.n do\n",
                        "result_{0} = result_{0} + smu{0}.nvbuffer1[loopIterator]\n",
                    "end\n",

                    "result_{0} = 1.0 / (result_{0} / _devAvg)\n",

                    "print(result_{0})\n",
                "end\n",

                "function DCSweepVLinear_smu{0}(start, stop, numPoints, limitI, nplc)\n",
                    "reset()\n",

                    "smu{0}.reset()\n",
                    "smu{0}.source.func = smu{0}.OUTPUT_DCVOLTS\n",
                    "smu{0}.source.limiti = limitI\n",
                    "smu{0}.measure.nplc = nplc\n",
                    "smu{0}.measure.delay = smua.DELAY_AUTO\n",

                    "smu{0}.nvbuffer1.clear()\n",
                    "smu{0}.nvbuffer1.collecttimestamps = 1\n",
                    "smu{0}.nvbuffer2.clear()\n",
                    "smu{0}.nvbuffer2.collecttimestamps	= 1\n",

                    "smu{0}.trigger.source.linearv(start, stop, numPoints)\n",
                    "smu{0}.trigger.source.limiti = limitI\n",
                    "smu{0}.trigger.measure.action = smu{0}.ENABLE\n",
                    "smu{0}.trigger.measure.iv(smu{0}.nvbuffer1, smu{0}.nvbuffer2)\n",
                    "smu{0}.trigger.endpulse.action		= smu{0}.SOURCE_HOLD\n",
                    "smu{0}.trigger.endsweep.action		= smu{0}.SOURCE_IDLE\n",
                    "smu{0}.trigger.count = numPoints\n",
                    "smu{0}.trigger.source.action = smu{0}.ENABLE\n",

                    "smu{0}.source.output = smua.OUTPUT_ON\n",
                    "smu{0}.trigger.initiate()\n",
                    "waitcomplete()\n",
                    "smu{0}.source.output = smua.OUTPUT_OFF\n",

                    "result = \"\"",
                    "for x=1, smu{0}.nvbuffer1.n do\n",
                        "result = result .. smu{0}.nvbuffer1.timestamps[x] .. \" \" .. smu{0}.nvbuffer2[x] .. \" \" .. smu{0}.nvbuffer1[x] .. \"\\n\"\n",
                    "end\n",

                    "print(result)\n",
                "end\n",

                "endscript\n",

                "DeviceFunctionsChannel{0}.run()\n");

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
            Initialize(Driver, "a");
        }

        public void Initialize(IDeviceIO Driver, string channelID)
        {
            _driver = Driver;
            ChannelIdentifier = channelID;

            _display = new Keithley26xxB_Display(ref Driver, ChannelIdentifier);
            _SetBeeperEnabled(true);

            _currentMeasureFunction = Keithley26xxBMeasureFunctions.MEASURE_DCAMPS;
            _display.smuX.measure.func = Keithley26xxBMeasureFunctions.MEASURE_DCAMPS;

            _LoadDeviceFunctions();
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

        private SourceMode _currentSourceMode = SourceMode.ModeNotSet;
        public SourceMode SMU_SourceMode
        {
            get
            {
                return _currentSourceMode;
            }
            set
            {
                _currentSourceMode = value;
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
                SetSourceVoltage(value);
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
                SetSourceCurrent(value);
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
                switch (_currentSourceMode)
                {
                    case SourceMode.Voltage:
                        return _currentVoltageCompliance;
                    case SourceMode.Current:
                        return _currentCurrentCompliance;
                    default:
                        return double.NaN;
                }
            }
            set { SetCompliance(_currentSourceMode, value); }
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

        private double _currentNPLC = 1.0;
        public double NPLC
        {
            get
            {
                return _currentNPLC;
            }
            set
            {
                _currentNPLC = value;
            }
        }

        private bool _outpOn = false;
        public void SwitchON()
        {
            if (_outpOn == false)
            {
                _outpOn = true;
                _driver.SendCommandRequest(string.Format("smu{0}.source.output = smu{0}.OUTPUT_ON", ChannelIdentifier));
                Beep(0.5, BeeperFrequencyEnum._2400_Hz);
            }
        }

        public void SwitchOFF()
        {
            if (_outpOn == true)
            {
                _outpOn = false;
                _driver.SendCommandRequest(string.Format("smu{0}.source.output = smu{0}.OUTPUT_OFF", ChannelIdentifier));
                Beep(0.5, BeeperFrequencyEnum._2400_Hz);
            }
        }

        private double _currentVoltageCompliance;
        protected double _minVoltageCompliance;
        protected double _maxVoltageCompliance;

        private double _currentCurrentCompliance;
        protected double _minCurrentCompliance;
        protected double _maxCurrentCompliance;

        public void SetCompliance(SourceMode sourceMode, double compliance)
        {
            switch (sourceMode)
            {
                case SourceMode.Voltage:
                    {
                        var _compliance = compliance;
                        if (compliance < _minCurrentCompliance)
                            _compliance = _minCurrentCompliance;
                        else if (compliance > _maxCurrentCompliance)
                            _compliance = _maxCurrentCompliance;

                        if (_compliance != _currentCurrentCompliance)
                            _currentCurrentCompliance = _compliance;
                    } break;
                case SourceMode.Current:
                    {
                        var _compliance = compliance;
                        if (compliance < _minVoltageCompliance)
                            _compliance = _minVoltageCompliance;
                        else if (compliance > _maxVoltageCompliance)
                            _compliance = _maxVoltageCompliance;

                        if (_compliance != _currentVoltageCompliance)
                            _currentVoltageCompliance = _compliance;
                    } break;
                default:
                    break;
            }
        }

        private double _currentDelay = 0.0;
        public void SetSourceDelay(double delay)
        {
            _currentDelay = delay;
        }

        private Keithley26xxBLimitFunctions _currentLimitFunction;

        protected double minVoltageVal;
        protected double maxVoltageVal;

        public void SetSourceVoltage(double val)
        {
            if (_currentLimitFunction != Keithley26xxBLimitFunctions.LIMIT_IV)
            {
                _currentLimitFunction = Keithley26xxBLimitFunctions.LIMIT_IV;
                _display.smuX.limit.func = Keithley26xxBLimitFunctions.LIMIT_IV;
            }

            if (SMU_SourceMode != SourceMode.Voltage)
                SMU_SourceMode = SourceMode.Voltage;

            var toSet = val;
            if (val < minVoltageVal)
                toSet = minVoltageVal;
            else if (val > maxVoltageVal)
                toSet = maxVoltageVal;

            _driver.SendCommandRequest(string.Format("SetVoltage_smu{0}({1}, {2})", ChannelIdentifier, toSet.ToString(CultureInfo.InvariantCulture), _currentCurrentCompliance.ToString(CultureInfo.InvariantCulture)));
        }

        protected double minCurrentVal;
        protected double maxCurrentVal;
        public void SetSourceCurrent(double val)
        {
            if (_currentLimitFunction != Keithley26xxBLimitFunctions.LIMIT_IV)
            {
                _currentLimitFunction = Keithley26xxBLimitFunctions.LIMIT_IV;
                _display.smuX.limit.func = Keithley26xxBLimitFunctions.LIMIT_IV;
            }

            if (SMU_SourceMode != SourceMode.Current)
                SMU_SourceMode = SourceMode.Current;

            var toSet = val;
            if (val < minCurrentVal)
                toSet = minCurrentVal;
            else if (val > maxCurrentVal)
                toSet = maxCurrentVal;

            _driver.SendCommandRequest(string.Format("SetCurrent_smu{0}({1}, {2})", ChannelIdentifier, toSet.ToString(CultureInfo.InvariantCulture), _currentVoltageCompliance.ToString(CultureInfo.InvariantCulture)));
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

        private Keithley26xxBMeasureFunctions _currentMeasureFunction;

        public double MeasureVoltage()
        {
            if (_currentMeasureFunction != Keithley26xxBMeasureFunctions.MEASURE_DCVOLTS)
            {
                _currentMeasureFunction = Keithley26xxBMeasureFunctions.MEASURE_DCVOLTS;
                _display.smuX.measure.func = Keithley26xxBMeasureFunctions.MEASURE_DCVOLTS;
            }

            var responce = _driver.RequestQuery(string.Format("MeasureVoltage_smu{0}({1}, {2}, {3})", ChannelIdentifier, _currentAveraging, _currentNPLC.ToString(CultureInfo.InvariantCulture), _currentDelay.ToString(CultureInfo.InvariantCulture)));

            var result = 0.0;
            var success = double.TryParse(responce, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

            if (success)
                return result;
            else
                throw new Exception("Can't read voltage!");
        }

        public double MeasureCurrent()
        {
            if (_currentMeasureFunction != Keithley26xxBMeasureFunctions.MEASURE_DCAMPS)
            {
                _currentMeasureFunction = Keithley26xxBMeasureFunctions.MEASURE_DCAMPS;
                _display.smuX.measure.func = Keithley26xxBMeasureFunctions.MEASURE_DCAMPS;
            }

            var responce = _driver.RequestQuery(string.Format("MeasureCurrent_smu{0}({1}, {2}, {3})", ChannelIdentifier, _currentAveraging, _currentNPLC.ToString(CultureInfo.InvariantCulture), _currentDelay.ToString(CultureInfo.InvariantCulture)));

            var result = 0.0;
            var success = double.TryParse(responce, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

            if (success)
                return result;
            else
                throw new Exception("Can't read current!");
        }

        public double MeasureResistance()
        {
            if (_currentMeasureFunction != Keithley26xxBMeasureFunctions.MEASURE_OHMS)
            {
                _currentMeasureFunction = Keithley26xxBMeasureFunctions.MEASURE_OHMS;
                _display.smuX.measure.func = Keithley26xxBMeasureFunctions.MEASURE_OHMS;
            }

            var responce = _driver.RequestQuery(string.Format("MeasureResistance_smu{0}({1}, {2}, {3})", ChannelIdentifier, _currentAveraging, _currentNPLC.ToString(CultureInfo.InvariantCulture), _currentDelay.ToString(CultureInfo.InvariantCulture)));

            var result = 0.0;
            var success = double.TryParse(responce, NumberStyles.Float, CultureInfo.InvariantCulture, out result);

            if (success)
                return result;
            else
                throw new Exception("Can't read resistance!");
        }
    }
}
