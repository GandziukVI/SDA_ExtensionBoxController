using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments.DataHandling
{
    public class SingleNoiseMeasurement
    {
        public static string DataHeader
        {
            get { return "Frequency\tSv\n"; }
        }

        public static string DataSubHeader
        {
            get { return "Hz\tV^2/Hz\n"; }
        }

        public string DataFileName { get; set; }
    }
}