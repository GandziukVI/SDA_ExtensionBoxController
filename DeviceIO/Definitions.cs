using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public enum SourceMode
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
}
