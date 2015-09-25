using DeviceIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley2430
{
    public class Keithley2430Channel : ISourceMeterUnit
    {
        private IDeviceIO _driver;

        public Keithley2430Channel(IDeviceIO driver)
        {
            _driver = driver;
        }

        private int[] _VSourceProtectionLimits = new int[] { 10, 20, 30, 40, 50, 60, 70, 80, -1000 };
        private int _currentVoltageCompliance = 10;
        private double _currentCurrentCompliance = 0.001;

        public void SetCompliance(SourceMode sourceMode, double compliance)
        {
            switch (sourceMode)
            {
                case SourceMode.Voltage:
                    {
                        if (compliance != _currentCurrentCompliance)
                        {
                            _currentCurrentCompliance = compliance;
                            _driver.SendCommandRequest(string.Format(":SENS:CURR:PROT {0}", compliance.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                    } break;
                case SourceMode.Current:
                    {
                        var query = (from limit in _VSourceProtectionLimits
                                     where limit - Math.Abs(compliance) >= 0.0
                                     select new
                                     {
                                         limit,
                                         distance = limit - Math.Abs(compliance)
                                     }).OrderBy(p => p.distance).First().limit;

                        if (query != _currentVoltageCompliance)
                        {
                            _currentVoltageCompliance = query;
                            _driver.SendCommandRequest(string.Format(":SENS:VOLT:PROT {0}", query.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                    } break;
                default:
                    break;
            }
        }

        public void SetAutoZeroMode(AutoZeroMode mode)
        {
            switch (mode)
            {
                case AutoZeroMode.ON:
                    _driver.SendCommandRequest(":SYST:AZER ON");
                    break;
                case AutoZeroMode.OFF:
                    _driver.SendCommandRequest(":SYST:AZER OFF");
                    break;
                default:
                    break;
            }
        }

        public void SetSourceDelay(double delay)
        {
            if (delay >= 0.0)
                _driver.SendCommandRequest(string.Format(":SOUR:DEL {0}", delay.ToString(NumberFormatInfo.InvariantInfo)));
            else
                _driver.SendCommandRequest(":SOUR:DEL:AUTO ON");
        }

        private ShapeMode _currentShapeMode = ShapeMode.ModeNotSet;
        private void _SetShape(ShapeMode mode)
        {
            if (mode != _currentShapeMode)
            {
                _currentShapeMode = mode;

                switch (mode)
                {
                    case ShapeMode.DC:
                        _driver.SendCommandRequest(":SOUR:FUNC:SHAP DC");
                        break;
                    case ShapeMode.Pulse:
                        _driver.SendCommandRequest(":SOUR:FUNC:SHAP PULS");
                        break;
                    default:
                        break;
                }
            }
        }

        public ShapeMode SMU_ShapeMode
        {
            get { return _currentShapeMode; }
            set
            {
                _currentShapeMode = value;
                _SetShape(value);
            }
        }

        private SourceMode _currentSourceMode = SourceMode.ModeNotSet;
        private void _SetSourceMode(SourceMode mode)
        {
            if (mode != _currentSourceMode)
            {
                switch (mode)
                {
                    case SourceMode.Voltage:
                        {
                            _currentSourceMode = mode;
                            _driver.SendCommandRequest(":SOUR:FUNC:MODE VOLT");
                        } break;
                    case SourceMode.Current:
                        {
                            _currentSourceMode = mode;
                            _driver.SendCommandRequest(":SOUR:FUNC:MODE CURR");
                        } break;
                    default:
                        break;
                }
            }
        }

        public SourceMode SMU_SourceMode
        {
            get { return _currentSourceMode; }
            set { _SetSourceMode(value); }
        }

        private SourceMode _currentFixedSourceMode = SourceMode.ModeNotSet;
        private void _SetFixedSourceMode(SourceMode mode)
        {
            if (mode != _currentFixedSourceMode)
            {
                switch (mode)
                {
                    case SourceMode.Voltage:
                        {
                            _currentFixedSourceMode = mode;
                            _driver.SendCommandRequest(":SOUR:VOLT:MODE FIX");
                        } break;
                    case SourceMode.Current:
                        {
                            _currentFixedSourceMode = mode;
                            _driver.SendCommandRequest(":SOUR:CURR:MODE FIX");
                        } break;
                    default:
                        break;
                }
            }
        }

        private double[] _VoltageRanges = new double[] { 0.2, 2.0, 20.0, 100.0 };
        private double[] _CurrentRanges = new double[] { 0.00001, 0.0001, 0.001, 0.01, 0.1, 1.0, 3.0 };

        private double _currentSourceVoltageRange = 0.2;
        private double _currentSourceCurrentRange = 0.00001;

        private void _SetSourceRange(double val, SourceMode mode)
        {
            switch (mode)
            {
                case SourceMode.Voltage:
                    {
                        var query = (from range in _VoltageRanges
                                     where range - Math.Abs(val) > 0.0
                                     select new
                                     {
                                         range = range,
                                         distance = range - Math.Abs(val)
                                     }).OrderBy(p => p.distance).First().range;
                        if (query != _currentSourceVoltageRange)
                        {
                            _currentSourceVoltageRange = query;
                            _driver.SendCommandRequest(string.Format(":SOUR:VOLT:RANG {0}", query.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                    } break;
                case SourceMode.Current:
                    {
                        var query = (from range in _CurrentRanges
                                     where range - Math.Abs(val) > 0.0
                                     select new
                                     {
                                         range = range,
                                         distance = range - Math.Abs(val)
                                     }).OrderBy(p => p.distance).First().range;

                        if (query != _currentSourceCurrentRange)
                        {
                            _currentSourceCurrentRange = query;
                            _driver.SendCommandRequest(string.Format(":SOUR:CURR:RANG {0}", query.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                    } break;
                default:
                    break;
            }
        }

        private double _currentMeasurementVoltageRange = 0.2;
        private double _currentMeasurementCurrentRange = 0.00001;

        private void _SetMeasurementRange(double val, SenseMode mode)
        {
            switch (mode)
            {
                case SenseMode.Voltage:
                    {
                        var query = (from range in _VoltageRanges
                                     where range - Math.Abs(val) > 0.0
                                     select new
                                     {
                                         range = range,
                                         distance = range - Math.Abs(val)
                                     }).OrderBy(p => p.distance).First().range;

                        if (query != _currentMeasurementVoltageRange)
                        {
                            _currentMeasurementVoltageRange = query;
                            _driver.SendCommandRequest(string.Format(":SENS:VOLT:RANG {0}", query.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                    } break;
                case SenseMode.Current:
                    {
                        var query = (from range in _CurrentRanges
                                     where range - Math.Abs(val) > 0.0
                                     select new
                                     {
                                         range = range,
                                         distance = range - Math.Abs(val)
                                     }).OrderBy(p => p.distance).First().range;

                        if (query != _currentMeasurementCurrentRange)
                        {
                            _currentMeasurementCurrentRange = query;
                            _driver.SendCommandRequest(string.Format(":SENS:CURR:RANG {0}", query.ToString(NumberFormatInfo.InvariantInfo)));
                        }
                    } break;
                default:
                    break;
            }
        }

        bool _voltageAutorangeState = false;
        bool _currentAutorangeState = false;
        bool _resistanceAutorangeState = false;
        private void _SetMeasurementAutoRange(SenseMode mode, bool autorange)
        {
            if (autorange)
            {
                switch (mode)
                {
                    case SenseMode.Voltage:
                        {
                            if (autorange != _voltageAutorangeState)
                                _driver.SendCommandRequest(":SENS:VOLT:RANG:AUTO ON");
                            _voltageAutorangeState = true;
                        } break;
                    case SenseMode.Current:
                        {
                            if (autorange != _currentAutorangeState)
                                _driver.SendCommandRequest(":SENS:CURR:RANG:AUTO ON");
                            _currentAutorangeState = true;
                        } break;
                    case SenseMode.Resistance:
                        {
                            if (autorange != _resistanceAutorangeState)
                                _driver.SendCommandRequest("RES:RANG:AUTO ON");
                            _resistanceAutorangeState = true;
                        } break;
                    default:
                        break;
                }
            }
            else
            {
                switch (mode)
                {
                    case SenseMode.Voltage:
                        {
                            if (autorange != _voltageAutorangeState)
                                _driver.SendCommandRequest(":SENS:VOLT:RANG:AUTO OFF");
                            _voltageAutorangeState = false;
                        } break;
                    case SenseMode.Current:
                        {
                            if (autorange != _currentAutorangeState)
                                _driver.SendCommandRequest(":SENS:CURR:RANG:AUTO OFF");
                            _currentAutorangeState = false;
                        } break;
                    case SenseMode.Resistance:
                        {
                            if (autorange != _resistanceAutorangeState)
                                _driver.SendCommandRequest("RES:RANG:AUTO OFF");
                            _resistanceAutorangeState = false;
                        } break;
                    default:
                        break;
                }
            }
        }

        private void _SetSourceLevel(double val, SourceMode mode)
        {
            switch (mode)
            {
                case SourceMode.Voltage:
                    _driver.SendCommandRequest(string.Format(":SOUR:VOLT:LEV {0}", val.ToString(NumberFormatInfo.InvariantInfo)));
                    break;
                case SourceMode.Current:
                    _driver.SendCommandRequest(string.Format(":SOUR:CURR:LEV {0}", val.ToString(NumberFormatInfo.InvariantInfo)));
                    break;
                default:
                    break;
            }
        }

        private SenseMode _currentSenseMode = SenseMode.ModeNotSet;
        private void _SetSenseMode(SenseMode mode)
        {
            if (mode != _currentSenseMode)
            {
                _currentSenseMode = mode;

                switch (mode)
                {
                    case SenseMode.Voltage:
                        _driver.SendCommandRequest(":SENS:FUNC \"VOLT\"");
                        break;
                    case SenseMode.Current:
                        _driver.SendCommandRequest(":SENS:FUNC \"CURR\"");
                        break;
                    case SenseMode.Resistance:
                        _driver.SendCommandRequest("FUNC \"RES\"");
                        break;
                    default:
                        break;
                }
            }
        }

        private SenseMode _currentReadingFormatMode = SenseMode.ModeNotSet;
        private void _SetReadingFormat(SenseMode mode)
        {
            if (mode != _currentReadingFormatMode)
            {
                _currentReadingFormatMode = mode;

                switch (mode)
                {
                    case SenseMode.Voltage:
                        _driver.SendCommandRequest(":FORM:ELEM VOLT");
                        break;
                    case SenseMode.Current:
                        _driver.SendCommandRequest(":FORM:ELEM CURR");
                        break;
                    case SenseMode.Resistance:
                        _driver.SendCommandRequest(":FORM:ELEM RES");
                        break;
                    default:
                        break;
                }
            }
        }

        private bool _outpOn = false;

        public void SwitchON()
        {
            if (_outpOn == false)
            {
                _outpOn = true;
                _driver.SendCommandRequest(":OUTP ON");
            }
        }

        public void SwitchOFF()
        {
            if (_outpOn == true)
            {
                _outpOn = false;
                _driver.SendCommandRequest(":OUTP OFF");
            }
        }

        public void SetSourceVoltage(double val)
        {
            _SetSourceMode(SourceMode.Voltage);
            _SetFixedSourceMode(SourceMode.Voltage);
            _SetSourceRange(val, SourceMode.Voltage);
            _SetSourceLevel(val, SourceMode.Voltage);
        }

        public void SetSourceCurrent(double val)
        {
            _SetSourceMode(SourceMode.Current);
            _SetFixedSourceMode(SourceMode.Current);
            _SetSourceRange(val, SourceMode.Current);
            _SetSourceLevel(val, SourceMode.Current);
        }

        public double MeasureVoltage()
        {
            _SetReadingFormat(SenseMode.Voltage);
            _SetMeasurementAutoRange(SenseMode.Voltage, true);

            var res = 0.0;
            var success = double.TryParse(_driver.RequestQuery(":READ?"), out res);

            if (success)
            {
                //_SetMeasurementRange(res, SenseMode.Voltage);
                return res;
            }
            else
                return double.NaN;
        }

        public double MeasureCurrent()
        {
            _SetReadingFormat(SenseMode.Current);
            _SetMeasurementAutoRange(SenseMode.Current, true);

            var res = 0.0;
            var success = double.TryParse(_driver.RequestQuery(":READ?"), out res);

            if (success)
            {
                //_SetMeasurementRange(res, SenseMode.Current);
                return res;
            }
            else
                return double.NaN;
        }

        private OhmsMode _currentOhmsMode = OhmsMode.ModeNotSet;
        private void _SetResistanceMeasurementMode(OhmsMode mode)
        {
            if (mode != _currentOhmsMode)
            {
                _currentOhmsMode = mode;

                switch (mode)
                {
                    case OhmsMode.Auto:
                        _driver.SendCommandRequest("RES:MODE AUTO");
                        break;
                    case OhmsMode.Manual:
                        _driver.SendCommandRequest("RES:MODE MAN");
                        break;
                    default:
                        break;
                }
            }
        }

        public double MeasureResistance()
        {
            _SetSenseMode(SenseMode.Resistance);
            _SetResistanceMeasurementMode(OhmsMode.Auto);
            _SetMeasurementAutoRange(SenseMode.Resistance, true);
            _SetReadingFormat(SenseMode.Resistance);

            var res = 0.0;
            var success = double.TryParse(_driver.RequestQuery(":READ?"), out res);

            if (success)
                return res;
            else
                return double.NaN;
        }




        public double Voltage
        {
            get { return MeasureResistance(); }
            set { SetSourceVoltage(value); }
        }

        public double Current
        {
            get { return MeasureCurrent(); }
            set { SetSourceCurrent(value); }
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

        private double _PulseWidth = double.NaN;
        public double PulseWidth
        {
            get { return _PulseWidth; }
            set 
            {
                if (_PulseWidth != value)
                {
                    var val = 0.0;
                    if (value < 0.00015)
                        val = 0.00015;
                    else if (value > 0.005)
                        val = 0.005;

                    _PulseWidth = val;

                    _driver.SendCommandRequest(string.Format(":SOUR:WIDT {0}", val.ToString(NumberFormatInfo.InvariantInfo)));
                }
            }
        }

        private double _PulseDelay = double.NaN;
        public double PulseDelay
        {
            get { return _PulseDelay; }
            set
            {
                if (_PulseDelay != value)
                {
                    var val = 0.0;
                    if (value < 0.0)
                        val = 0.0;
                    else if (value > 9999.999)
                        val = 9999.999;

                    _PulseDelay = val;

                    _driver.SendCommandRequest(string.Format(":SOUR:DEL {0}", val.ToString(NumberFormatInfo.InvariantInfo)));
                }
            }
        }
    }
}
