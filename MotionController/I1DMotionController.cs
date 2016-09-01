using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotionController
{
    public interface I1DMotionController : IDisposable
    {
        void Enable();
        void Disable();

        void StartMotion();
        void StopMotion();

        double GetCurrentPosition();
        double GetSpeed();
        
        void SetSpeed(double Speed);
        void SetDestination(double Destination);
        void SetPosition(double Position);

        double Position { get; set; }
        double Speed { get; set; }
    }
}
