﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        static AgilentU254xAnalogInChannel analogInChannel1;
        static AgilentU254xAnalogInChannel analogInChannel2;
        static AgilentU254xAnalogInChannel analogInChannel3;
        static AgilentU254xAnalogInChannel analogInChannel4;

        static readonly string ResourceID = "USB0::2391::5912::TW54334510::INSTR";
        static readonly string Options = "Simulate=false, Cache=false, QueryInstrStatus=false";

        /// <summary>
        /// Gets the amount of memory, used by the proces
        /// </summary>
        /// <param name="proc">Process to be analyzed</param>
        /// <returns></returns>
        static int getMemoryUsage(ref Process proc)
        {
            PerformanceCounter PC = new PerformanceCounter();
            PC.CategoryName = "Process";
            PC.CounterName = "Working Set - Private";
            PC.InstanceName = proc.ProcessName;

            PC.Close();
            PC.Dispose();

            return ((int)PC.NextValue()) / 1024;
        }

        static void Main(string[] args)
        {
            Process currProcess = Process.GetCurrentProcess();

            // Printing initial memory, occupied by the process
            int memsizeInit = getMemoryUsage(ref currProcess);
            Console.WriteLine(string.Format("Initial memory occupied by the process is {0}\r\n", memsizeInit));

            #region Initializing COM objects

            driver = new AgilentU254x();
            driver.Initialize(ResourceID, false, true);

            driver.DriverOperation.QueryInstrumentStatus = false;
            driver.DriverOperation.Cache = false;
            driver.DriverOperation.RecordCoercions = false;
            driver.DriverOperation.InterchangeCheck = false;
            driver.System.TimeoutMilliseconds = 5000;

            analogInChannel1 = driver.AnalogIn.Channels.get_Item("AIn1");
            analogInChannel2 = driver.AnalogIn.Channels.get_Item("AIn2");
            analogInChannel3 = driver.AnalogIn.Channels.get_Item("AIn3");
            analogInChannel4 = driver.AnalogIn.Channels.get_Item("AIn4");

            analogInChannel1.Configure(AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar, 1.25, true);
            analogInChannel2.Configure(AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar, 1.25, false);
            analogInChannel3.Configure(AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar, 1.25, false);
            analogInChannel4.Configure(AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar, 1.25, false);

            #endregion

            #region Test Acquisition

            int samplingFrequency = 500000;

            driver.AnalogIn.MultiScan.Configure(samplingFrequency, -1);

            driver.AnalogIn.Acquisition.BufferSize = samplingFrequency;

            bool acquisitionInProgress = true;

            driver.AnalogIn.Acquisition.Start();

            var acquisitionTask = Task.Factory.StartNew(new Action(() =>
            {
                int readingsCounter = 0;
                short[] buf = { 0 };
                while (acquisitionInProgress)
                {
                    if (driver.AnalogIn.Acquisition.BufferStatus == AgilentU254xBufferStatusEnum.AgilentU254xBufferStatusDataReady)
                    {
                        driver.AnalogIn.Acquisition.Fetch(ref buf);
                        Console.WriteLine(string.Format("Number of readings with sampling frequency {0} is {1}.", samplingFrequency, readingsCounter + 1));
                        ++readingsCounter;
                    }
                }
            }));

            int numOfSeconds = 10;
            Thread.Sleep(numOfSeconds * 1000);

            driver.AnalogIn.Acquisition.Stop();
            acquisitionInProgress = false;

            while (!acquisitionTask.IsCompleted) ;

            #endregion

            #region Releasing COM objects

            if (analogInChannel4 != null)
            {
                while (Marshal.ReleaseComObject(analogInChannel4) > 0) ;
                analogInChannel4 = null;
            }

            if (analogInChannel3 != null)
            {
                while (Marshal.ReleaseComObject(analogInChannel3) > 0) ;
                analogInChannel3 = null;
            }

            if (analogInChannel2 != null)
            {
                while (Marshal.ReleaseComObject(analogInChannel2) > 0) ;
                analogInChannel2 = null;
            }

            if (analogInChannel1 != null)
            {
                while (Marshal.ReleaseComObject(analogInChannel1) > 0) ;
                analogInChannel1 = null;
            }

            if (driver != null)
            {
                while (Marshal.ReleaseComObject(driver) > 0) ;
                driver = null;
            }

            #endregion

            // Printing memory, occupied by the process after data readings
            Console.WriteLine();

            int memsizeEnd = getMemoryUsage(ref currProcess);
            Console.WriteLine(string.Format("Memory occupied by the process after data readings is {0}\r\n", memsizeEnd));
            Console.ReadKey();
        }
    }
}
