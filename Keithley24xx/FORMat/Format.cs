using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;

namespace Keithley24xx.FORMat
{
    public class FORMat
    {
        protected IDeviceIO _driver;
        protected string SubsystemIdentifier { get; private set; }

        public FORMat(ref IDeviceIO Driver)
        {
            _driver = Driver;
            SubsystemIdentifier = ":FORM";

            ELEM = new ELEMents(ref Driver);
        }

        /// <summary>
        /// Select data format for reading status event registers
        /// (ASCii, HEXadecimal, OCTal or BINary).
        /// </summary>
        public StatusRegisterDataFormats StatusRegisterDataFormat
        {
            get
            {
                var response = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":SREG?")).Substring(0, 3).ToLower();

                if (response == "asc")
                    return StatusRegisterDataFormats.ASCii;
                else if (response == "hex")
                    return StatusRegisterDataFormats.HEXadecimal;
                else if (response == "oct")
                    return StatusRegisterDataFormats.OCTal;
                else if (response == "bin")
                    return StatusRegisterDataFormats.BINary;
                else
                    throw new Exception("Unable to acquire data format!");
            }
            set
            {
                var toSet = "ASC";
                switch (value)
                {
                    case StatusRegisterDataFormats.ASCii:
                        toSet = "ASC";
                        break;
                    case StatusRegisterDataFormats.HEXadecimal:
                        toSet = "HEX";
                        break;
                    case StatusRegisterDataFormats.OCTal:
                        toSet = "OCT";
                        break;
                    case StatusRegisterDataFormats.BINary:
                        toSet = "BIN";
                        break;
                }

                _driver.SendCommandRequest(string.Format("{0}{1} {2}", SubsystemIdentifier, ":SREG", toSet));
            }
        }

        /// <summary>
        /// Specify data format (ASCii, REAL, 32 or SREal).
        /// Query data format.
        /// </summary>
        public OutputDataFormats DataFormat
        {
            get
            {
                var response = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":DATA?")).ToLower();

                if ("ASCii".ToLower().Contains(response))
                    return OutputDataFormats.ASCii;
                else if ("REAL".ToLower().Contains(response))
                    return OutputDataFormats.REAL;
                else if ("32".ToLower().Contains(response))
                    return OutputDataFormats.Format_32;
                else if ("SREal".ToLower().Contains(response))
                    return OutputDataFormats.SREal;
                else
                    throw new Exception("Unable to acquire data format!");
            }
            set
            {
                var toSet = "ASC";
                switch (value)
                {
                    case OutputDataFormats.ASCii:
                        toSet = "ASC";
                        break;
                    case OutputDataFormats.REAL:
                        toSet = "REAL";
                        break;
                    case OutputDataFormats.Format_32:
                        toSet = "32";
                        break;
                    case OutputDataFormats.SREal:
                        toSet = "SRE";
                        break;
                }

                _driver.SendCommandRequest(string.Format("{0}{1} {2}", SubsystemIdentifier, ":DATA", toSet));
            }
        }

        /// <summary>
        /// Specify byte order (NORMal or SWAPped).
        /// Query byte order.
        /// </summary>
        public BorderTypes BorderType
        {
            get
            {
                var response = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":BORD?")).ToLower();

                if ("NORMal".ToLower().Contains(response))
                    return BorderTypes.NORMal;
                else if ("SWAPed".ToLower().Contains(response))
                    return BorderTypes.SWAPed;
                else
                    throw new Exception("Can't acquire border type from the device!");
            }
            set
            {
                var toSet = "NORM";
                switch (value)
                {
                    case BorderTypes.NORMal:
                        toSet = "NORM";
                        break;
                    case BorderTypes.SWAPed:
                        toSet = "SWAP";
                        break;
                }

                _driver.SendCommandRequest(string.Format("{0}{1} {2}", SubsystemIdentifier, ":BORD", toSet));
            }
        }

        public ELEMents ELEM { get; private set; }
    }

    public class ELEMents : FORMat
    {
        private string CommandIdentifier;

        public ELEMents(ref IDeviceIO Driver)
            : base(ref Driver)
        {
            CommandIdentifier = string.Format("{0}{1}", SubsystemIdentifier, ":ELEM");
        }

        /// <summary>
        /// Specify data elements (VOLTage, CURRent,
        /// RESistance, TIME, and STATus).
        /// </summary>
        /// <param name="elems"></param>
        public void SetSenseElements(params Elements[] elems)
        {
            var elemsList = string.Empty;

            for (int i = 0; i < elems.Length; i++)
            {
                var temp = string.Empty;

                switch (elems[i])
                {
                    case Elements.VOLTage:
                        temp = "VOLT";
                        break;
                    case Elements.CURRent:
                        temp = "CURR";
                        break;
                    case Elements.RESistance:
                        temp = "RES";
                        break;
                    case Elements.TIME:
                        temp = "TIME";
                        break;
                    case Elements.STATus:
                        temp = "STAT";
                        break;
                    case Elements.All:
                        temp = "All";
                        break;
                }

                elemsList += (i == elems.Length - 1) ? temp + " " : temp;
            }

            _driver.SendCommandRequest(string.Format("{0} {1}", CommandIdentifier, elemsList));
        }

        public string GetSenseElements()
        {
            return _driver.RequestQuery(string.Format("{0}{1}", CommandIdentifier, "?"));
        }
    }
}
