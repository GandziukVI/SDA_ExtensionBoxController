using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    public class TT_SmartModeInfo
    {
        public double ZeroPos { get; set; }
        public double MinPos { get; set; }
        public double MaxPos { get; set; }
        public double SetPos { get; set; }
        public double DestPos { get; set; }
        public double PointsPerMilimeter { get; set; }
        public int nCycles { get; set; }
        public double MinConductance { get; set; }
        public double MaxConductance { get; set; }
    }
}
