using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionManager
{
    public interface IMotionController1D : IDisposable
    {
        void Initialize(IDeviceIO Driver);
        void SetPosition(double Position);
        void SetPositionAsync(double Position);
        double GetPosition();
        void SetVelosity(double Velosity);
        double GetVelosity();
        bool Enable();
        bool Disable();


        double Position { get; set; }
        double PositionAsync { set; }
        double Velosity { get; set; }
        bool Enabled { get; set; }
        bool IsEnabled { get; }
    }
}
