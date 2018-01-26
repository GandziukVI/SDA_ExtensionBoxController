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
        void Start(object StartInfo);
        void Stop();
        void SaveToFile(string FileName);

        event EventHandler<ExpDataArrivedEventArgs> DataArrived;
        event EventHandler<StatusEventArgs> Status;
        event EventHandler<ProgressEventArgs> Progress;
        event EventHandler<StartedEventArgs> ExpStarted;
        event EventHandler<FinishedEventArgs> ExpFinished;
    }
}
