using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public interface ISourceMeterUnit
    {
        void SetCompliance(SourceMode sourceMode, double compliance);
    }
}
