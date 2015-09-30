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
        }

        /// <summary>
        /// Turn on or turn off front panel display.
        /// Query state of display.
        /// </summary>
        public bool Enabled
        {
            get
            {
                var responce = _driver.RequestQuery(string.Format("{0}{1}", SubsystemIdentifier, ":ENAB?"));

                if (responce == "ON")
                    return true;
                else if (responce == "OFF")
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
                var responce = _driver.RequestQuery(string.Format("{0}{1}", _textIdentifier, ":STAT?"));

                if (responce == "ON")
                    return true;
                else if (responce == "OFF")
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

    public class WindowData
    {
        private IDeviceIO _driver;
        public WindowData(ref IDeviceIO Driver, IDisplayWindow window)
        {
            _driver = Driver;

        }
    }
}
