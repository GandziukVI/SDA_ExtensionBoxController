using DeviceIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keithley26xx
{
    public enum Keithley26xxB_Channels
    {
        Channel_A = 0,
        Channel_B = 1
    }

    public class Keithley26xxB<T> : IEnumerable<ISourceMeterUnit>
        where T : Keithley26xxChannelBase, new()
    {
        private IDeviceIO _driver;
        private int _numberOfChannels;
        public ISourceMeterUnit[] ChannelCollection { get; private set; }

        public Keithley26xxB(IDeviceIO Driver)
        {
            _driver = Driver;
            var attr = (NumberOfChannelsAttribute)typeof(T).GetCustomAttributes(typeof(NumberOfChannelsAttribute), true).FirstOrDefault();
            _numberOfChannels = attr.NumberOfChannels;

            if (_numberOfChannels > 2)
                throw new ArgumentException("The amount of channels can't more than two for supported models!");

            ChannelCollection = new ISourceMeterUnit[_numberOfChannels];

            for (int i = 0; i < _numberOfChannels; i++)
            {
                ChannelCollection[i] = new T();

                if (i == 0)
                    ChannelCollection[i].Initialize(Driver, "a");
                else if (i == 1)
                    ChannelCollection[i].Initialize(Driver, "b");
            }
        }

        public ISourceMeterUnit this[Keithley26xxB_Channels index]
        {
            get
            {
                try
                {
                    return ChannelCollection[(int)index];
                }
                catch(IndexOutOfRangeException)
                {
                    throw new ArgumentException("The channel is not supported for current model!");
                }
            }
        }

        public ISourceMeterUnit this[int index]
        {
            get
            {
                try
                {
                    return ChannelCollection[index];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new ArgumentException("The channel is not supported for current model!");
                }
            }
        }

        public IEnumerator<ISourceMeterUnit> GetEnumerator()
        {
            for (int index = 0; index < ChannelCollection.Length; index++)
            {
                yield return ChannelCollection[index];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            for (int index = 0; index < ChannelCollection.Length; index++)
            {
                yield return ChannelCollection[index];
            }
        }
    }
}
