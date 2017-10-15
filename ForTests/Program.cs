using DeviceIO;
using MCS_Faulhaber;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForTests
{
    class Program
    {
        static void Main(string[] args)
        {
            #region Testing of SerialDevice & Faulhaber SA2036U012V

            var driver = new SerialDevice("COM1", 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            var motor = new SA_2036U012V(driver);

            Console.WriteLine(string.Format("Motor Position is {0}", motor.Position));
            Console.WriteLine("Going to -0.2mm...");

            motor.Position = -0.2;

            Console.WriteLine(string.Format("Motor Position is {0}", motor.Position));
            Console.WriteLine("Going to 0mm...");

            motor.Position = 0.0;

            Console.WriteLine(string.Format("Motor Position is {0}", motor.Position));
            Console.WriteLine("Going to 0.2mm...");

            motor.Position = 0.2;

            Console.WriteLine(string.Format("Motor Position is {0}", motor.Position));
            Console.WriteLine("Going to 0mm...");

            motor.Position = 0.0;

            Console.WriteLine(string.Format("Motor Position is {0}", motor.Position));
            Console.WriteLine("Going to 0mm...");

            motor.Position = 0.0;
            Console.WriteLine(string.Format("Motor Position is {0}", motor.Position));

            Console.WriteLine();

            #endregion
        }
    }
}
