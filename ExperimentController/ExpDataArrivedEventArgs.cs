using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentController
{
    public class ExpDataArrivedEventArgs : EventArgs
    {
        public string Data { get; private set; }
        public ExpDataArrivedEventArgs(string DataString)
            : base()
        {
            Data = DataString;
        }
    }
}
