using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SourceMeterUnit
{
    public static class SourceMeterUnitExtensions
    {
        static char[] _delimeters = { '\r', '\n' };
        static char[] _separators = { ' ', '\t', ',' };

        public static IV_Data[] FromString(this string DataString)
        {
            var query = (from item in DataString.Split(_delimeters, StringSplitOptions.RemoveEmptyEntries)
                         select new IV_Data(item)).ToArray();

            return query;
        }

        public static string ToString(this IV_Data[] ivData)
        {
            var sb = new StringBuilder();

            foreach (var ivPoint in ivData)
                sb.AppendFormat("{0}\t{1}\t{2}\r\n", ivPoint.Time, ivPoint.Voltage, ivPoint.Current);

            return sb.ToString();
        }
    }
}
