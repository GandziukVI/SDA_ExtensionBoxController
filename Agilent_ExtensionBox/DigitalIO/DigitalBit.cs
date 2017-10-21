using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{
    public class DigitalBit : IDisposable
    {
        private int _bitNumber;
        private DigitalChannel _channel;
        public DigitalBit(DigitalChannel channel, int BitNumber)
        {
            _channel = channel;
            _bitNumber = BitNumber;
        }

        public bool Value
        {
            get { 
                var res = false;
                _channel.ReadBit(_bitNumber, out  res);
                return res;
            }
        }

        public void Set()
        {
            _channel.WriteBit(_bitNumber, true);
        }

        public void Reset()
        {
            _channel.WriteBit(_bitNumber, false);
        }

        public void Pulse()
        {
            Set();
            System.Threading.Thread.Sleep(100);
            Reset();
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
