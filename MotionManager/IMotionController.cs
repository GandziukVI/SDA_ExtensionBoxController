using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionManager
{
    public interface IMotionController : IDisposable
    {
        void Initialize(IDeviceIO Driver);
        void SetPosition(double Position);

        double Position { get; set; }
    }
}
