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

    public interface IWindow
    {
        string WindowIdentifier { get; }
    }

    public class Window1 : DISPlay, IWindow
    {
        public Window1(ref IDeviceIO Driver)
            : base(ref Driver)
        {
            TEXT = new Text(ref Driver, this);
        }

        public string WindowIdentifier
        {
            get { return string.Format("{0}{1}", SubsystemIdentifier, ":WINDow1"); }
        }

        public Text TEXT { get; private set; }
    }

    public class Window2 : DISPlay, IWindow
    {
        public Window2(ref IDeviceIO Driver)
            : base(ref Driver) 
        {

        }

        public string WindowIdentifier
        {
            get { return string.Format("{0}{1}", SubsystemIdentifier, ":WINDow2"); }
        }

        public Text TEXT { get; private set; }
    }

    public class Text
    {
        protected string TextIdentifier { get; private set; }
        private IWindow _window;

        private IDeviceIO _driver;

        public Text(ref IDeviceIO Driver, IWindow window)
        {
            _driver = Driver;
            _window = window;
            TextIdentifier = string.Format("{0}{1}", window.WindowIdentifier, ":TEXT");
        }

        public string DATA
        {
            get
            {
                return _driver.RequestQuery(string.Format("{0}{1}", TextIdentifier, ":DATA?"));
            }
            set
            {
                var toSet = value.Length > 20 ? value.Substring(0, 20) : value;
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", TextIdentifier, ":DATA", toSet));
            }
        }

        public bool State
        {
            get
            {
                var responce = _driver.RequestQuery(string.Format("{0}{1}", TextIdentifier, ":STAT?"));

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
                _driver.SendCommandRequest(string.Format("{0}{1} {2}", TextIdentifier, ":STAT", toSet));
            }
        }
    }
}
