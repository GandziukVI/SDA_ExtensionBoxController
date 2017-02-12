using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentController
{
    public class ProgressEventArgs : EventArgs
    {
        public double Progress { get; private set; }
        public ProgressEventArgs(double progress)
            : base()
        {
            Progress = progress;
        }
    }
}
