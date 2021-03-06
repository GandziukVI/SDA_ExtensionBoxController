﻿using DeviceIO;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley24xx
{
    public class Keithley24xx<T>
        where T : Keithley24xxChannelBase, new()
    {
        private IDeviceIO _driver;
        public ISourceMeterUnit Channel { get; private set; }

        public Keithley24xx(IDeviceIO Driver)
        {
            _driver = Driver;
            Channel = new T();
            Channel.Initialize(_driver);
        }
    }
}
