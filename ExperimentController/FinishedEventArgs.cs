using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExperimentController
{
    public class FinishedEventArgs : EventArgs
    {
        private int statusCode = 0;
        public int StatusCode
        {
            get { return statusCode; }
            set { statusCode = value; }
        }

        public FinishedEventArgs(int status)
            : base()
        {
            statusCode = status;
        }
    }
}
