using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments.DataHandling
{
    public class SingleNoiseMeasurement
    {
        public static string DataHeader
        {
            get { return "Frequency\tSv\n"; }
        }

        public static string DataSubHeader
        {
            get { return "Hz\tV^2\n"; }
        }

        public string DataFileName { get; set; }
    }
}
