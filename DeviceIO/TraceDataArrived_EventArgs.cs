using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public class TraceDataArrived_EventArgs : EventArgs
    {
        public TraceData DataPoint { get; private set; }

        public TraceDataArrived_EventArgs(TraceData TracePoint)
            : base()
        {
            DataPoint = TracePoint;
        }
    }
}
