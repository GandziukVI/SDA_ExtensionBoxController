using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;

namespace MotionManager
{
    public class MotionController1DBase : IMotionController1D
    {
        private bool isEnabled = false;
        protected IDeviceIO driver;
        public virtual void Initialize(IDeviceIO Driver)
        {
            if (Driver != null)
                driver = Driver;
        }

        public virtual void SetPosition(double Position)
        {
            throw new NotImplementedException();
        }

        public virtual double GetPosition()
        {
            throw new NotImplementedException();
        }

        public virtual void SetVelosity(double Velosity)
        {
            throw new NotImplementedException();
        }

        public virtual double GetVelosity()
        {
            throw new NotImplementedException();
        }

        public virtual bool Enable()
        {
            isEnabled = true;
            return isEnabled;
        }

        public virtual bool Disable()
        {
            isEnabled = false;
            return isEnabled;
        }

        public double Position
        {
            get
            {
                return GetPosition();
            }
            set
            {
                SetPosition(value);
            }
        }

        public double Velosity
        {
            get
            {
                return GetVelosity();
            }
            set
            {
                SetVelosity(value);
            }
        }

        public bool Enabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (value)
                    Enable();
                else
                    Disable();
            }
        }

        public bool IsEnabled
        {
            get { return isEnabled; }
        }

        public void Dispose()
        {
            if (driver != null)
                driver.Dispose();
        }
    }
}
