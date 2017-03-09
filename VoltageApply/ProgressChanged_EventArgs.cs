using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoltageApply
{
    public class ProgressChanged_EventArgs : EventArgs
    {
        public int Progress { get; private set; }

        public ProgressChanged_EventArgs(int progress)
            : base()
        {
            Progress = progress;
        }
    }
}
