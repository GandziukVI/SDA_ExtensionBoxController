using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public interface ISourceMeterUnit : IDisposable
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

        IV_Data[] LinearVoltageSweep(double start, double stop, int numPoints);
        IV_Data[] LinearCurrentSweep(double start, double stop, int numPoints);

        IV_Data[] LogarithmicVoltageSweep(double start, double stop, int numPoints);
        IV_Data[] LogarithmicCurrentSweep(double start, double stop, int numPoints);

        IV_Data[] ListVoltageSweep(double[] sweepList);
        IV_Data[] ListCurrentSweep(double[] sweepList);

        IV_Data[] PulsedLinearVoltageSweep(double start, double stop, int numPoints, double pulseWidth, double pulsePeriod, bool remoteSense);
        IV_Data[] PulsedLinearCurrentSweep(double start, double stop, int numPoints, double pulseWidth, double pulsePeriod, bool remoteSense);

        event EventHandler<TraceDataArrived_EventArgs> TraceDataArrived;

        void StartVoltageTrace(double srcCurr, double srcLimitV, double devNPLC);
        void StartCurrentTrace(double srcVolt, double srcLimitI, double devNPLC);

        void StopVoltageTrace();
        void StopCurrentTrace();

        void Reset();
    }
}
