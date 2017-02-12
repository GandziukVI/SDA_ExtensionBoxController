using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceMeterUnit
{
    public enum SMUSourceMode
    {
        Voltage,
        Current,
        ModeNotSet
    }

    public enum SenseMode
    {
        Voltage,
        Current,
        Resistance,
        ModeNotSet
    }

    public enum OhmsMode
    {
        Auto,
        Manual,
        ModeNotSet
    }

    public enum AutoZeroMode
    {
        ON,
        OFF
    }

    public enum ShapeMode
    {
        DC,
        Pulse,
        ModeNotSet
    }

    public class IV_Data
    {
        static char[] _delimeters = { '\r', '\n' };
        static char[] _separators = { ' ', '\t', ',' };

        private double _Time;

        public IV_Data()
        {
            _Time = 0.0;
            _Voltage = 0.0;
            _Current = 0.0;
        }

        public IV_Data(double time, double voltage, double current)
        {
            _Time = time;
            _Voltage = voltage;
            _Current = current;
        }

        public IV_Data(string InputData)
        {
            var data = InputData.TrimEnd(_delimeters).Split(_separators, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 3)
                throw new ArgumentException("The input data has wrong format!");

            var successTime = double.TryParse(data[0], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _Time);
            var successVoltage = double.TryParse(data[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _Voltage);
            var successCurrent = double.TryParse(data[2], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _Current);

            if (!(successTime && successVoltage && successCurrent))
                throw new Exception("Canno't interpert input data!");
        }

        public double Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        private double _Voltage;

        public double Voltage
        {
            get { return _Voltage; }
            set { _Voltage = value; }
        }

        private double _Current;

        public double Current
        {
            get { return _Current; }
            set { _Current = value; }
        }

        public override string ToString()
        {
            return string.Join("\t", Voltage.ToString(NumberFormatInfo.InvariantInfo), Current.ToString(NumberFormatInfo.InvariantInfo));
        }

        public static IV_Data[] FromString(string DataString)
        {
            var query = (from item in DataString.Split(_delimeters, StringSplitOptions.RemoveEmptyEntries)
                        select new IV_Data(item)).ToArray();                        

            return query;
        }
    }

    public class TraceData
    {
        char[] _delimeters = { '\r', '\n' };
        char[] _separators = { ' ' };


        private double _Time;
        public double Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        private double _Value;
        public double Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public TraceData()
        {
            _Time = 0.0;
            _Value = 0.0;
        }

        public TraceData(double time, double val)
        {
            _Time = time;
            _Value = val;
        }

        public TraceData(string InputData)
        {
            var data = InputData.TrimEnd(_delimeters).Split(_separators, StringSplitOptions.RemoveEmptyEntries);
            if (data.Length != 2)
                throw new ArgumentException("The input data has wrong format!");

            var successTime = double.TryParse(data[0], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _Time);
            var successValue = double.TryParse(data[1], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out _Value);

            if (!(successTime && successValue))
                throw new Exception("Canno't interpert input data!");
        }
    }
}
