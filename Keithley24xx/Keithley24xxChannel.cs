using System;
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

        public Keithley24xxChannel(IDeviceIO Driver)
        {
            _device = new T();
            Initialize(Driver);
        }

        public void Initialize(IDeviceIO Driver)
        {
            _device.Initialize(Driver);
        }

        public void Initialize(IDeviceIO Driver, string channelID = "Not supported for Keithley24xx series!")
        {
            _device.Initialize(Driver);
        }

        public ShapeMode SMU_ShapeMode
        {
            get { return _device.SMU_ShapeMode; }
            set { _device.SMU_ShapeMode = value; }
        }

        public SourceMode SMU_SourceMode
        {
            get { return _device.SMU_SourceMode; }
            set { _device.SMU_SourceMode = value; }
        }

        public double Voltage
        {
            get { return _device.Voltage; }
            set { _device.Voltage = value; }
        }

        public double Current
        {
            get { return _device.Current; }
            set { _device.Current = value; }
        }

        public double Resistance
        {
            get { return _device.Resistance; }
        }

        public double Compliance
        {
            get { return _device.Compliance; }
            set { _device.Compliance = value; }
        }

        public double PulseWidth
        {
            get { return _device.PulseWidth; }
            set { _device.PulseWidth = value; }
        }

        public double PulseDelay
        {
            get { return _device.PulseDelay; }
            set { _device.PulseDelay = value; }
        }

        public int Averaging
        {
            get { return _device.Averaging; }
            set { _device.Averaging = value; }
        }

        public double NPLC
        {
            get { return _device.NPLC; }
            set { _device.NPLC = value; }
        }

        public void SwitchON()
        {
            _device.SwitchON();
        }

        public void SwitchOFF()
        {
            _device.SwitchOFF();
        }

        public void SetCompliance(SourceMode sourceMode, double compliance)
        {
            _device.SetCompliance(sourceMode, compliance);
        }

        public void SetSourceDelay(double delay)
        {
            _device.SetSourceDelay(delay);
        }

        public void SetSourceVoltage(double val)
        {
            _device.SetSourceVoltage(val);
        }

        public void SetSourceCurrent(double val)
        {
            _device.SetSourceCurrent(val);
        }

        public void SetAveraging(int avg)
        {
            _device.SetAveraging(avg);
        }

        public void SetNPLC(double val)
        {
            _device.SetNPLC(val);
        }

        public double MeasureVoltage()
        {
            return _device.MeasureVoltage();
        }

        public double MeasureCurrent()
        {
            return _device.MeasureCurrent();
        }

        public double MeasureResistance()
        {
            return _device.MeasureResistance();
        }
    }
}
