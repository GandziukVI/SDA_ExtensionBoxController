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

    public class MultichannelKeithley<T> where T : ISourceMeterUnit, new()
    {
        private IDeviceIO _driver;
        public ISourceMeterUnit[] Channels { get; private set; }

        public MultichannelKeithley(IDeviceIO Driver)
        {
            _driver = Driver;
            //var chN = 
            var type = typeof(T);
            var attr = (KeithleyAttribute)type.GetCustomAttributes(typeof(KeithleyAttribute), false).FirstOrDefault();

            Channels = new ISourceMeterUnit[attr.ChannelNumber];
            //Channel.Initialize(Driver);
        }
    }
}
