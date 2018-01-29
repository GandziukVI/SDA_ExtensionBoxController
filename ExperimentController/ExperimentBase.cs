using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExperimentController
{
    public class ExperimentBase : IExperiment
    {
        protected Thread experimentThread;

        private bool isRunning = false;
        public bool IsRunning
        {
            get { return isRunning; }
            set { isRunning = value; }
        }

        public event EventHandler<ExpDataArrivedEventArgs> DataArrived;
        protected void onDataArrived(ExpDataArrivedEventArgs e)
        {
            if (DataArrived != null)
                DataArrived(this, e);
        }
        
        public event EventHandler<StatusEventArgs> Status;
        protected void onStatusChanged(StatusEventArgs e)
        {
            if (Status != null)
                Status(this, e);
        }

        public event EventHandler<ProgressEventArgs> Progress;
        protected void onProgressChanged(ProgressEventArgs e)
        {
            if (Progress != null)
                Progress(this, e);
        }

        public event EventHandler<StartedEventArgs> ExpStarted;
        protected void onExpStarted(StartedEventArgs e)
        {
            if (ExpStarted != null)
                ExpStarted(this, e);
        }

        public event EventHandler<FinishedEventArgs> ExpFinished;
        protected void onExpFinished(FinishedEventArgs e)
        {
            if (ExpFinished != null)
                ExpFinished(this, e);
        }

        public virtual void ToDo(object Arg)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            Dispose();

            var expThreadInfo = new ParameterizedThreadStart(ToDo);
            experimentThread = new Thread(expThreadInfo);
            experimentThread.Priority = ThreadPriority.AboveNormal;

            IsRunning = true;
            experimentThread.Start();
        }

        public void Start(object StartInfo)
        {
            Dispose();

            var expThreadInfo = new ParameterizedThreadStart(ToDo);
            experimentThread = new Thread(expThreadInfo);
            experimentThread.Priority = ThreadPriority.AboveNormal;

            IsRunning = true;
            experimentThread.Start(StartInfo);
        }

        public virtual void Stop()
        {
            if (experimentThread != null)
                if (experimentThread.IsAlive)
                    Dispose();
        }

        public virtual void SaveToFile(string FileName)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispose()
        {
            if (experimentThread != null)
            {
                IsRunning = false;
                if (experimentThread.IsAlive)
                {
                    Thread.Sleep(100);
                    experimentThread.Abort();
                    Thread.Sleep(500);
                    if (experimentThread.IsAlive)
                        experimentThread.Join();
                }
            }
        }
    }
}
