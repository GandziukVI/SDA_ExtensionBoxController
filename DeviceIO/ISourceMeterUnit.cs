using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public interface ISourceMeterUnit
    {
        void Initialize(IDeviceIO Driver);
        void Initialize(IDeviceIO Driver, string channelID);

        ShapeMode SMU_ShapeMode { get; set; }
        SourceMode SMU_SourceMode { get; set; }

        double Voltage { get; set; }
        double Current { get; set; }
        double Resistance { get; }
        double Compliance { get; set; }

        double PulseWidth { get; set; }
        double PulseDelay { get; set; }

        int Averaging { get; set; }
        double NPLC { get; set; }

        void SwitchON();
        void SwitchOFF();
        void SetCompliance(SourceMode sourceMode, double compliance);
        void SetSourceDelay(double delay);
        void SetSourceVoltage(double val);
        void SetSourceCurrent(double val);
        void SetAveraging(int avg);
        void SetNPLC(double val);
        
        double MeasureVoltage();
        double MeasureCurrent();
        double MeasureResistance();

        ReturnValue[] LinearVoltageSweep(double start, double stop, int numPoints);
        ReturnValue[] LinearCurrentSweep(double start, double stop, int numPoints);

        ReturnValue[] LogarithmicVoltageSweep(double start, double stop, int numPoints);
        ReturnValue[] LogarithmicCurrentSweep(double start, double stop, int numPoints);

        ReturnValue[] ListVoltageSweep(double[] sweepList);
        ReturnValue[] ListCurrentSweep(double[] sweepList);

        ReturnValue[] PulsedLinearVoltageSweep(double start, double stop, int numPoints, double pulseWidth, double pulsePeriod, bool remoteSense);
        ReturnValue[] PulsedLinearCurrentSweep(double start, double stop, int numPoints, double pulseWidth, double pulsePeriod, bool remoteSense);
    }
}
