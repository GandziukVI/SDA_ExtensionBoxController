using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;

using Keithley24xx.CALCulate.Commands;

namespace Keithley24xx.CALCulate
{
    public class CALCulate
    {
        protected IDeviceIO _driver;
        protected string SubsystemIdentifier { get; private set; }

        public CALCulate(ref IDeviceIO Driver)
        {
            _driver = Driver;
            SubsystemIdentifier = ":CALC";

            MATH = new MATH_Command(ref Driver);
        }

        public MATH_Command MATH { get; private set; }

        /// <summary>
        /// Enable or disable math expression.
        /// Query state of math expression. 
        /// </summary>
        public MATH_StateEnum MATH_State
        {
            get
            {
                var responce = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":STAT?"));

                if (responce == "ON")
                    return MATH_StateEnum.ON;
                else if (responce == "OFF")
                    return MATH_StateEnum.OFF;
                else
                    throw new Exception("Math state not supported!");
            }
            set
            {
                var toSet = "OFF";
                switch (value)
                {
                    case MATH_StateEnum.ON:
                        toSet = "ON";
                        break;
                    case MATH_StateEnum.OFF:
                        toSet = "OFF";
                        break;
                }

                _driver.SendCommandRequest(string.Format("{0}{1} {2}", SubsystemIdentifier, ":STAT", toSet));
            }
        }

    }
}
