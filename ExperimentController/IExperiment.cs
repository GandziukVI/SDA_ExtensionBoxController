using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentController
{
    public interface IExperiment : IDisposable
    {
        bool IsRunning { get; set; }
        void ToDo(object Arg);
        void Start();
        void Stop();

        event EventHandler<ExpDataArrivedEventArgs> DataArrived;
    }
}
