using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley24xx
{
    public enum SupportedModels
    {
        Keithley2400,
        Keithley2410,
        Keithley2420,
        Keithley2430,
        Keithley2440,
    }

    public class Keithley2400 : Keithley24xxBase 
    {
    }
    
    public class Keithley2410 : Keithley24xxBase 
    { 
    }
    
    public class Keithley2420 : Keithley24xxBase 
    {
    }
    
    public class Keithley2430 : Keithley24xxBase 
    { 
    }
    
    public class Keithley2440 : Keithley24xxBase 
    {
    }

    public enum MATH_StateEnum
    {
        ON,
        OFF
    }
}
