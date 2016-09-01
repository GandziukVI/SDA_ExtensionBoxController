using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Measurements
{
    public interface IMeasurement : IDisposable
    {
        void StartMeasurement();
        bool SetMeasurement(params object[] data);
        void DoMeasurements();
        void StopMeasurement();
    }
}
