using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley2430Namespace
{
    public class Keithley2430
    {
        public IDeviceIO Driver { get; private set; }
        public Keithley2430Channel SMU_Channel { get; private set; }

        public Keithley2430(string resourceName)
        {
            Driver = new VisaDevice(resourceName);
            SMU_Channel = new Keithley2430Channel();
            SMU_Channel.Initialize(Driver);
            //SMU_Channel = new Keithley2430Channel(Driver);
        }
    }
}
