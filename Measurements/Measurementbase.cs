using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace Measurements
{
    public class Measurementbase : IMeasurement
    {
        public Thread MeasurementThread { get; private set; }

        public Measurementbase()
        {
            var threadStart = new ThreadStart(DoMeasurements);
            MeasurementThread = new Thread(threadStart);
        }

        public void StartMeasurement()
        {
            if (!SetMeasurement())
                throw new Exception("En error occured during setting the measurement");

            MeasurementThread.Start();
        }

        public bool SetMeasurement(params object[] data)
        {
            throw new NotImplementedException();
        }

        public virtual void StopMeasurement()
        {
            throw new NotImplementedException();
        }

        public virtual void DoMeasurements()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            while (MeasurementThread.IsAlive)
            {
                MeasurementThread.Abort();
                Thread.Sleep(100);
            }
        }
    }
}
