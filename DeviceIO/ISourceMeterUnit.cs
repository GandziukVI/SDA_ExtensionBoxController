using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public interface ISourceMeterUnit
    {
        void SwitchON();
        void SwitchOFF();
        void SetCompliance(SourceMode sourceMode, double compliance);
        void SetSourceDelay(double delay);
        void SetSourceVoltage(double val);
        void SetSourceCurrent(double val);
        double MeasureVoltage();
        double MeasureCurrent();
        double MeasureResistance();
    }
}
