using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpectralAnalysis
{
    public class TraceDataArrived_EventArgs<T> : EventArgs
        where T: IEnumerable
    {
        public T[] TraceData { get; private set; }

        public TraceDataArrived_EventArgs(ref T[] traceData)
            : base()
        {
            TraceData = traceData;
        }
    }
}
