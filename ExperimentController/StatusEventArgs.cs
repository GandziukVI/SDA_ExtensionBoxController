using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentController
{
    public class StatusEventArgs : EventArgs
    {
        public string StatusMessage { get; private set; }
        public StatusEventArgs(string StatusMsg)
            :base()
        {
            StatusMessage = StatusMsg;
        }
    }
}
