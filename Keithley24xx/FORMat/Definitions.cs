using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley24xx.FORMat
{
    public enum StatusRegisterDataFormats
    {
        ASCii,
        HEXadecimal,
        OCTal,
        BINary
    }

    public enum OutputDataFormats
    {
        ASCii,
        REAL,
        Format_32,
        SREal
    }

    public enum BorderTypes
    {
        NORMal,
        SWAPed
    }

    public enum Elements
    {
        VOLTage,
        CURRent,
        RESistance,
        TIME,
        STATus,
        All
    }
}
