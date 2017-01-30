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
        private Thread experimentThread;

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

        public void Stop()
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

        public virtual void Dispose()
        {
            if (experimentThread != null)
                if (experimentThread.IsAlive)
                    Stop();
        }
    }
}
