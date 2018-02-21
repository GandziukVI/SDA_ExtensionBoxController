using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DeviceIO;
using System.Globalization;

namespace Keithley24xx.DISPlay
{
    public class DISPlay
    {
        protected IDeviceIO _driver;
        protected string SubsystemIdentifier { get; private set; }

        public DISPlay(ref IDeviceIO Driver)
        {
            _driver = Driver;
            SubsystemIdentifier = ":DISP";

            WINDow_01 = new DisplayWindow1(ref Driver);
            WINDow_02 = new DisplayWindow2(ref Driver);
        }

        /// <summary>
        /// Turn on or turn off front panel display.
        /// Query state of display.
        /// </summary>
        public bool Enabled
        {
            get
            {
                var response = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":ENAB?"));

                if (response == "ON")
                    return true;
                else if (response == "OFF")
                    return false;
                else
                    throw new Exception("The device answer is not corect!");
            }
            set
            {
                var toSet = value ? "ON" : "OFF";
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", SubsystemIdentifier, ":ENAB", toSet));
            }
        }

        /// <summary>
        /// Return to source-measure display state.
        /// </summary>
        public void SetSMU_DisplayState()
        {
            _driver.SendCommandRequest(string.Format("{0}{1}", SubsystemIdentifier, ":CND"));
        }

        /// <summary>
        /// Path to locate message to top display
        /// </summary>
        public DisplayWindow1 WINDow_01 { get; private set; }

        /// <summary>
        /// Path to locate message to bottom display
        /// </summary>
        public DisplayWindow2 WINDow_02 { get; private set; }

        /// <summary>
        /// Specify display resolution (4 to 7).
        /// Query display resolution
        /// </summary>
        public int Digits
        {
            get
            {
                var response = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":DIG?"));
                var result = 4;
                var success = int.TryParse(response, out result);

                if (success)
                    return result;
                else
                    throw new Exception("Can't acquire the display resolution!");
            }
            set
            {
                var toSet = 4;
                if (value < 4)
                    toSet = 4;
                else if (value > 7)
                    toSet = 7;
                
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", SubsystemIdentifier, ":DIG", toSet.ToString(NumberFormatInfo.InvariantInfo)));
            }
        }
    }

    public interface IDisplayWindow
    {
        string WindowIdentifier { get; }
    }

    public class DisplayWindow1 : DISPlay, IDisplayWindow
    {
        public DisplayWindow1(ref IDeviceIO Driver)
            : base(ref Driver)
        {
            TEXT = new WindowText(ref Driver, this);
        }

        public string WindowIdentifier
        {
            get { return string.Format("{0}{1}", SubsystemIdentifier, ":WINDow1"); }
        }

        public WindowText TEXT { get; private set; }

        /// <summary>
        /// Query data on bottom portion of display.
        /// </summary>
        public string Data
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", WindowIdentifier, ":DATA?"));
            }
        }

        /// <summary>
        /// Query attributes of message characters:
        ///     blinking (1) or not blinking (0).
        /// </summary>
        public string Attributes
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", WindowIdentifier, ":ATTR?"));
            }
        }
    }

    public class DisplayWindow2 : DISPlay, IDisplayWindow
    {
        public DisplayWindow2(ref IDeviceIO Driver)
            : base(ref Driver) 
        {
            TEXT = new WindowText(ref Driver, this);
        }

        public string WindowIdentifier
        {
            get { return string.Format("{0}{1}", SubsystemIdentifier, ":WINDow2"); }
        }

        public WindowText TEXT { get; private set; }

        public string Data
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", WindowIdentifier, ":DATA?"));
            }
        }

        public string Attributes
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", WindowIdentifier, ":ATTR?"));
            }
        }
    }

    /// <summary>
    /// Control user test message
    /// </summary>
    public class WindowText
    {
        private string _textIdentifier;
        private IDeviceIO _driver;

        public WindowText(ref IDeviceIO Driver, IDisplayWindow window)
        {
            _driver = Driver;
            _textIdentifier = string.Format("{0}{1}", window.WindowIdentifier, ":TEXT");
        }

        /// <summary>
        /// Define ASCII message “a” (up to 20 characters).
        /// Query text message.
        /// </summary>
        public string DATA
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", _textIdentifier, ":DATA?"));
            }
            set
            {
                var toSet = value.Length > 20 ? value.Substring(0, 20) : value;
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", _textIdentifier, ":DATA", toSet));
            }
        }

        /// <summary>
        /// Enable or disable message mode.
        /// Query text message state.
        /// </summary>
        public bool State
        {
            get
            {
                var response = _driver.RequestQuery(string.Format("{0}{1}", _textIdentifier, ":STAT?"));

                if (response == "ON")
                    return true;
                else if (response == "OFF")
                    return false;
                else
                    throw new Exception("The device answer is not corect!");
            }
            set
            {
                var toSet = value ? "ON" : "OFF";
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", _textIdentifier, ":STAT", toSet));
            }
        }
    }
}
