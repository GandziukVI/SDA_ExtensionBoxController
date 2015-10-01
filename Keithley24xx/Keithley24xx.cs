using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley24xx
{
    public class Keithley24xx<T> where T : ISourceMeterUnit, new()
    {
        private IDeviceIO _driver;
        public ISourceMeterUnit Channel { get; private set; }

        public Keithley24xx(IDeviceIO Driver)
        {
            _driver = Driver;
            Channel = new T();
            Channel.Initialize(Driver);
        }
    }
}
