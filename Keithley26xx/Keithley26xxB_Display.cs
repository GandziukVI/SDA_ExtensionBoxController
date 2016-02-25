using DeviceIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley26xx
{
    public enum Keithley26xxBAvailableChannels
    {
        SMUA = 0,
        SMUB = 1,
        SMUA_SMUB = 2,
        USER = 3
    }

    public enum Keithley26xxBLimitFunctions
    {
        LIMIT_IV = 0,
        LIMIT_P = 1
    }

    public enum Keithley26xxBMeasureFunctions
    {
        MEASURE_DCAMPS = 0,
        MEASURE_DCVOLTS = 1,
        MEASURE_OHMS = 2,
        MEASURE_WATTS = 3,
    }

    public class Keithley26xxB_Display
    {
        protected IDeviceIO _driver;
        protected static string _channelID;

        public Keithley26xxB_Display(ref IDeviceIO Driver, string ChannelID)
        {
            _driver = Driver;
            _channelID = ChannelID;

            smuX = new SmuX(ref Driver, ChannelID);
        }

        /// <summary>
        /// This function switches to the user screen and then clears the display.
        /// The display.clear(), display.setcursor(), and display.settext() functions are overlapped
        /// commands. That is, the script does not wait for one of these commands to complete. These functions do not
        /// immediately update the display. For performance considerations, they update the physical display as soon as
        /// processing time becomes available.
        /// </summary>
        public void Clear()
        {
            _driver.SendCommandRequest("display.clear()");
        }

        /// <summary>
        /// This function switches the display to the user screen (the text set by display.settext()), and then returns
        /// values to indicate the cursor's row and column position and cursor style.
        /// Columns are numbered from left to right on the display.
        /// </summary>
        /// <returns>row, column, style = display.getcursor()</returns>
        public double[] GetCursor()
        {
            var result = new double[3];
            _driver.SendCommandRequest("cursorRow, cursorColumn, cursorStyle = display.getcursor()");
            var responce = _driver.RequestQuery("print(cursorRow, cursorColumn, cursorStyle)").Split("\r\n\t, ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var _isRowReadingSucceed = double.TryParse(responce[0], NumberStyles.Float, CultureInfo.InvariantCulture, out result[0]);
            var _isColumnReadingSucceed = double.TryParse(responce[1], NumberStyles.Float, CultureInfo.InvariantCulture, out result[1]);
            var _isStyleReadingSucceed = double.TryParse(responce[2], NumberStyles.Float, CultureInfo.InvariantCulture, out result[2]);

            if (_isRowReadingSucceed && _isColumnReadingSucceed && _isStyleReadingSucceed)
                return result;
            else
                throw new Exception("The reading from device was incorrect!");
        }

        /// <summary>
        /// This attribute contains the selected display screen.
        /// </summary>
        public Keithley26xxBAvailableChannels Screen
        {
            get
            {
                _driver.SendCommandRequest("displayID = display.screen");
                var responce = _driver.RequestQuery("print(displayID)");

                var doubleResponce = 0.0;
                var success = double.TryParse(responce, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out doubleResponce);
                
                if (success)
                {
                    if (0 == (int)doubleResponce)
                        return Keithley26xxBAvailableChannels.SMUA;
                    else if (1 == (int)doubleResponce)
                        return Keithley26xxBAvailableChannels.SMUB;
                    else if (2 == (int)doubleResponce)
                        return Keithley26xxBAvailableChannels.SMUA_SMUB;
                    else if (3 == (int)doubleResponce)
                        return Keithley26xxBAvailableChannels.USER;
                    else
                        throw new Exception("Can't read current screen!");
                }
                else
                {
                    if (responce == "0" || responce == "display.SMUA")
                        return Keithley26xxBAvailableChannels.SMUA;
                    else if (responce == "1" || responce == "display.SMUB")
                        return Keithley26xxBAvailableChannels.SMUB;
                    else if (responce == "2" || responce == "display.SMUA_SMUB")
                        return Keithley26xxBAvailableChannels.SMUA_SMUB;
                    else if (responce == "3" || responce == "display.USER")
                        return Keithley26xxBAvailableChannels.USER;
                    else
                        throw new Exception("Can't read current screen!");
                }
            }
            set
            {
                _driver.SendCommandRequest(string.Format("display.screen = {0}", (int)value));
            }
        }

        public void SetText(string Text)
        {
            _driver.SendCommandRequest(string.Format("display.settext({0})", Text));
        }

        public SmuX smuX { get; private set; }
    }

    public class SmuX
    {
        public SmuX(ref IDeviceIO Driver, string ChannelID)
        {
            limit = new Limit(ref Driver, ChannelID);
            measure = new Measure(ref Driver, ChannelID);
        }

        public Limit limit { get; private set; }
        public Measure measure { get; private set; }
    }

    public class Limit
    {
        private IDeviceIO _driver;
        private string _channelID;

        public Limit(ref IDeviceIO Driver, string ChannelID)
        {
            _driver = Driver;
            _channelID = ChannelID;
        }

        public Keithley26xxBLimitFunctions func
        {
            get
            {
                _driver.SendCommandRequest(string.Format("display{0}LimitFunc = display.smu{0}.limit.func", _channelID));
                var responce = _driver.RequestQuery(string.Format("print(display{0}LimitFunc)", _channelID));

                var doubleResponce = 0.0;
                var success = double.TryParse(responce, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out doubleResponce);

                if (success)
                {
                    if (0 == (int)doubleResponce)
                        return Keithley26xxBLimitFunctions.LIMIT_IV;
                    else if (1 == (int)doubleResponce)
                        return Keithley26xxBLimitFunctions.LIMIT_P;
                    else
                        throw new Exception(string.Format("Can't read smu{0} limit function", _channelID));
                }
                else
                {
                    if (responce == "0" || responce == "display.LIMIT_IV")
                        return Keithley26xxBLimitFunctions.LIMIT_IV;
                    else if (responce == "1" || responce == "display.LIMIT_P")
                        return Keithley26xxBLimitFunctions.LIMIT_P;
                    else
                        throw new Exception(string.Format("Can't read smu{0} limit function", _channelID));
                }
            }
            set
            {
                _driver.SendCommandRequest(string.Format("display.smu{0}.limit.func = {1}", _channelID, (int)value));
            }
        }
    }

    public class Measure
    {
        private IDeviceIO _driver;
        private string _channelID;

        public Measure(ref IDeviceIO Driver, string ChannelID)
        {
            _driver = Driver;
            _channelID = ChannelID;
        }

        public Keithley26xxBMeasureFunctions func
        {
            get
            {
                _driver.SendCommandRequest(string.Format("smu{0}MeasureFunc = display.smu{0}.measure.func", _channelID));
                var responce = _driver.RequestQuery(string.Format("print(smu{0}MeasureFunc)", _channelID));

                var doubleResponce = 0.0;
                var success = double.TryParse(responce, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out doubleResponce);

                if (success)
                {
                    if (0 == (int)doubleResponce)
                        return Keithley26xxBMeasureFunctions.MEASURE_DCAMPS;
                    else if (1 == (int)doubleResponce)
                        return Keithley26xxBMeasureFunctions.MEASURE_DCVOLTS;
                    else if (2 == (int)doubleResponce)
                        return Keithley26xxBMeasureFunctions.MEASURE_OHMS;
                    else if (3 == (int)doubleResponce)
                        return Keithley26xxBMeasureFunctions.MEASURE_WATTS;
                    else
                        throw new Exception(string.Format("Can't read smu{0} measure function!", _channelID));
                }
                else
                {
                    if (responce == "0" || responce == "display.MEASURE_DCAMPS")
                        return Keithley26xxBMeasureFunctions.MEASURE_DCAMPS;
                    else if (responce == "1" || responce == "display.MEASURE_DCVOLTS")
                        return Keithley26xxBMeasureFunctions.MEASURE_DCVOLTS;
                    else if (responce == "2" || responce == "MEASURE_OHMS")
                        return Keithley26xxBMeasureFunctions.MEASURE_OHMS;
                    else if (responce == "3" || responce == "display.MEASURE_WATTS")
                        return Keithley26xxBMeasureFunctions.MEASURE_WATTS;
                    else
                        throw new Exception(string.Format("Can't read smu{0} measure function!", _channelID));
                }
            }
            set
            {
                _driver.SendCommandRequest(string.Format("display.smu{0}.measure.func = {1}", _channelID, (int)value));
            }
        }
    }
}
