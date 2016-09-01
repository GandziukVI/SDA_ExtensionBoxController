using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;
using SMU;

using System.Globalization;
using System.Diagnostics;
using System.Threading;

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
        public string ChannelIdentifier { get; protected set; }

        private IDeviceIO _driver;
        private Keithley26xxB_Display _display;

        private Stopwatch _stopWatch;

        private void _LoadDeviceFunctions()
        {
            var formatString = "loadscript {0}\n{1}\nendscript\n{0}.run()\n";
            switch (ChannelIdentifier)
            {
                case "a":
                    _driver.SendCommandRequest(string.Format(formatString, "DeviceFunctionsChannelA", Properties.Resources.DeviceFunctionsChannelA));
                    break;
                case "b":
                    _driver.SendCommandRequest(string.Format(formatString, "DeviceFunctionsChannelB", Properties.Resources.DeviceFunctionsChannelB));
                    break;
            }
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

            _stopWatch = new Stopwatch();
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
                        return _currentCurrentCompliance;
                    case SourceMode.Current:
                        return _currentVoltageCompliance;
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
            _currentNPLC = val;
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

        private char[] _delim = { '\n' };

        public IV_Data[] LinearVoltageSweep(double start, double stop, int numPoints)
        {
            _driver.SendCommandRequest(string.Format("DCSweepVLinear_smu{0}({1}, {2}, {3}, {4}, {5})",
                ChannelIdentifier,
                start.ToString(NumberFormatInfo.InvariantInfo),
                stop.ToString(NumberFormatInfo.InvariantInfo),
                numPoints.ToString(NumberFormatInfo.InvariantInfo),
                Compliance.ToString(NumberFormatInfo.InvariantInfo),
                NPLC.ToString(NumberFormatInfo.InvariantInfo)));

            var result = new IV_Data[numPoints];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] LinearCurrentSweep(double start, double stop, int numPoints)
        {
            _driver.SendCommandRequest(string.Format("DCSweepILinear_smu{0}({1}, {2}, {3}, {4}, {5})",
                ChannelIdentifier,
                start.ToString(NumberFormatInfo.InvariantInfo),
                stop.ToString(NumberFormatInfo.InvariantInfo),
                numPoints.ToString(NumberFormatInfo.InvariantInfo),
                Compliance.ToString(NumberFormatInfo.InvariantInfo),
                NPLC.ToString(NumberFormatInfo.InvariantInfo)));

            var result = new IV_Data[numPoints];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] LogarithmicVoltageSweep(double start, double stop, int numPoints)
        {
            _driver.SendCommandRequest(string.Format("DCSweepVLog_smu{0}({1}, {2}, {3}, {4}, {5})",
                ChannelIdentifier,
                start.ToString(NumberFormatInfo.InvariantInfo),
                stop.ToString(NumberFormatInfo.InvariantInfo),
                numPoints.ToString(NumberFormatInfo.InvariantInfo),
                Compliance.ToString(NumberFormatInfo.InvariantInfo),
                NPLC.ToString(NumberFormatInfo.InvariantInfo)));

            var result = new IV_Data[numPoints];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] LogarithmicCurrentSweep(double start, double stop, int numPoints)
        {
            _driver.SendCommandRequest(string.Format("DCSweepILog_smu{0}({1}, {2}, {3}, {4}, {5})",
                ChannelIdentifier,
                start.ToString(NumberFormatInfo.InvariantInfo),
                stop.ToString(NumberFormatInfo.InvariantInfo),
                numPoints.ToString(NumberFormatInfo.InvariantInfo),
                Compliance.ToString(NumberFormatInfo.InvariantInfo),
                NPLC.ToString(NumberFormatInfo.InvariantInfo)));

            var result = new IV_Data[numPoints];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] ListVoltageSweep(double[] sweepList)
        {
            var stringList = "{";
            for (int i = 0; i < sweepList.Length - 1; i++)
            {
                stringList += sweepList[i].ToString(NumberFormatInfo.InvariantInfo) + ", ";
            }
            stringList += sweepList[sweepList.Length - 1] + "}";

            _driver.SendCommandRequest(string.Format("DCSweepVList_smu{0}({1}, {2}, {3}, {4})",
                 ChannelIdentifier,
                 stringList,
                 sweepList.Length.ToString(NumberFormatInfo.InvariantInfo),
                 Compliance.ToString(NumberFormatInfo.InvariantInfo),
                 NPLC.ToString(NumberFormatInfo.InvariantInfo)));

            var result = new IV_Data[sweepList.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] ListCurrentSweep(double[] sweepList)
        {
            var stringList = "{";
            for (int i = 0; i < sweepList.Length - 1; i++)
            {
                stringList += sweepList[i].ToString(NumberFormatInfo.InvariantInfo) + ", ";
            }
            stringList += sweepList[sweepList.Length - 1] + "}";

            _driver.SendCommandRequest(string.Format("DCSweepIList_smu{0}({1}, {2}, {3}, {4})",
                 ChannelIdentifier,
                 stringList,
                 sweepList.Length.ToString(NumberFormatInfo.InvariantInfo),
                 Compliance.ToString(NumberFormatInfo.InvariantInfo),
                 NPLC.ToString(NumberFormatInfo.InvariantInfo)));

            var result = new IV_Data[sweepList.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] PulsedLinearVoltageSweep(double start, double stop, int numPoints, double pulseWidth, double pulsePeriod, bool remoteSense = false)
        {
            var _senseMode = (remoteSense == true) ? "true" : "false";

            _driver.SendCommandRequest(string.Format("PulsedSweepVSingle_smu{0}({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                ChannelIdentifier,
                start.ToString(NumberFormatInfo.InvariantInfo),
                stop.ToString(NumberFormatInfo.InvariantInfo),
                numPoints.ToString(NumberFormatInfo.InvariantInfo),
                pulseWidth.ToString(NumberFormatInfo.InvariantInfo),
                pulsePeriod.ToString(NumberFormatInfo.InvariantInfo),
                Compliance.ToString(NumberFormatInfo.InvariantInfo),
                NPLC.ToString(NumberFormatInfo.InvariantInfo),
                _senseMode));

            var result = new IV_Data[numPoints];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }

        public IV_Data[] PulsedLinearCurrentSweep(double start, double stop, int numPoints, double pulseWidth, double pulsePeriod, bool remoteSense = false)
        {
            var _senseMode = (remoteSense == true) ? "true" : "false";

            _driver.SendCommandRequest(string.Format("PulsedSweepISingle_smu{0}({1}, {2}, {3}, {4}, {5}, {6}, {7}, {8})",
                ChannelIdentifier,
                start.ToString(NumberFormatInfo.InvariantInfo),
                stop.ToString(NumberFormatInfo.InvariantInfo),
                numPoints.ToString(NumberFormatInfo.InvariantInfo),
                pulseWidth.ToString(NumberFormatInfo.InvariantInfo),
                pulsePeriod.ToString(NumberFormatInfo.InvariantInfo),
                Compliance.ToString(NumberFormatInfo.InvariantInfo),
                NPLC.ToString(NumberFormatInfo.InvariantInfo),
                _senseMode));

            var result = new IV_Data[numPoints];

            for (int i = 0; i < result.Length; i++)
                result[i] = new IV_Data(_driver.ReceiveDeviceAnswer());

            return result;
        }


        public event EventHandler<TraceDataArrived_EventArgs> TraceDataArrived;

        private void _OnTraceDataArrived(TraceDataArrived_EventArgs e)
        {
            if (TraceDataArrived != null)
                TraceDataArrived(this, e);
        }

        private bool VoltageTrace_InProgress = false;
        public void StartVoltageTrace(double srcCurr, double srcLimitV, double devNPLC)
        {
            VoltageTrace_InProgress = true;

            SetCompliance(SourceMode.Current, srcLimitV);
            SetNPLC(devNPLC);
            SetSourceCurrent(srcCurr);
            SwitchON();
            _stopWatch.Start();

            var reading = Task.Run(() =>
            {
                while (VoltageTrace_InProgress == true)
                    _OnTraceDataArrived(new TraceDataArrived_EventArgs(new TraceData((double)(_stopWatch.ElapsedMilliseconds) / 1000.0, MeasureVoltage())));
            });
        }

        private bool CurrentTrace_InProgress = false;
        public void StartCurrentTrace(double srcVolt, double srcLimitI, double devNPLC)
        {
            CurrentTrace_InProgress = true;

            SetCompliance(SourceMode.Voltage, srcLimitI);
            SetNPLC(devNPLC);
            SetSourceVoltage(srcVolt);
            SwitchON();
            _stopWatch.Start();

            var reading = Task.Run(() =>
            {
                while (CurrentTrace_InProgress == true)
                    _OnTraceDataArrived(new TraceDataArrived_EventArgs(new TraceData((double)(_stopWatch.ElapsedMilliseconds) / 1000.0, MeasureCurrent())));
            });
        }

        public void StopVoltageTrace()
        {
            VoltageTrace_InProgress = false;

            SwitchOFF();
            _stopWatch.Reset();
        }

        public void StopCurrentTrace()
        {
            CurrentTrace_InProgress = false;

            SwitchOFF();
            _stopWatch.Reset();
        }

        public void Reset()
        {
            _driver.SendCommandRequest(string.Format("smu{0}.reset()", ChannelIdentifier));
        }

        public void Dispose()
        {
            if (_driver != null)
                _driver.Dispose();
        }
    }
}
