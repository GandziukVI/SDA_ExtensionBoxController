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
        private IDeviceIO Transport{get;set;}
        public ISourceMeterUnit Channel{ get; private set; }

        public Keithley24xx(IDeviceIO transport)
        {
            Transport = transport;
            Channel = new T();
            Channel.Initialize(transport);
        }
    }
}
