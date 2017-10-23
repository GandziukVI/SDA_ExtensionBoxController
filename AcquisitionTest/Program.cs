using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using Ivi.Driver.Interop;
using Agilent.AgilentU254x.Interop;

namespace AcquisitionTest
{
    class Program
    {
        static AgilentU254x driver;
        static AgilentU254xAnalogInChannel analogInChannel;

        static readonly string ResourceID = "USB0::2391::5912::TW54334510::INSTR";
        static readonly string Options = "Simulate=false, Cache=false, QueryInstrStatus=false";

        static void Main(string[] args)
        {
            #region Initializing COM objects

            driver = new AgilentU254x();
            driver.Initialize(ResourceID, false, true, Options);

            driver.DriverOperation.QueryInstrumentStatus = false;
            driver.DriverOperation.Cache = false;
            driver.DriverOperation.RecordCoercions = false;
            driver.DriverOperation.InterchangeCheck = false;
            driver.System.TimeoutMilliseconds = 5000;

            analogInChannel = driver.AnalogIn.Channels.get_Item("AIn1");
            analogInChannel.Configure(AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar, 1.25, true);

            driver.AnalogIn.MultiScan.Configure(262144, -1);

            driver.AnalogIn.Acquisition.BufferSize = 262144;
            driver.AnalogIn.Acquisition.Start();

            Thread.Sleep(10000);

            driver.AnalogIn.Acquisition.Stop();

            short[] buf = new short[262144];
            while (driver.AnalogIn.Acquisition.BufferStatus == AgilentU254xBufferStatusEnum.AgilentU254xBufferStatusDataReady)
                driver.AnalogIn.Acquisition.Fetch(ref buf);

            #endregion

            #region Releasing COM objects

            if (analogInChannel != null)
            {
                while (Marshal.ReleaseComObject(analogInChannel) > 0) ;
                analogInChannel = null;
            }

            if(driver != null)
            {
                while (Marshal.ReleaseComObject(driver) > 0) ;
                driver = null;
            }

            #endregion
        }
    }
}
