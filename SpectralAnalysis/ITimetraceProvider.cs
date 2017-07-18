using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpectralAnalysis
{
    interface ITimetraceProvider<T>
        where T: IEnumerable
    {
        T[] GetLastTraceData();

        event EventHandler<TraceDataArrived_EventArgs<T>> TraceDataArrived;
    }
}
