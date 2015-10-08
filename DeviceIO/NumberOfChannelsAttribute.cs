using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class NumberOfChannelsAttribute
    {
        public int NumberOfChannels { get; private set; }

        public NumberOfChannelsAttribute(int numberOfChannels)
        {
            NumberOfChannels = numberOfChannels;
        }
    }
}
