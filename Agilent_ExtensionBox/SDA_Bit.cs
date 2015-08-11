using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using Ivi.Driver.Interop;
using Agilent.AgilentU254x.Interop;
using System.Threading;

namespace Agilent_ExtensionBox
{
    public class SDA_Bit
    {
        private string ChannelID;
        private int BitNumber;
        private BoxController Controller;

        public SDA_Bit(string __ChannelID, int __BitNumber, BoxController __Controller)
        {
            ChannelID = __ChannelID;
            BitNumber = __BitNumber;
            Controller = __Controller;
        }

        private int _GetIntFromBitArray(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            var array = new int[1];
            bitArray.CopyTo(array, 0);

            return array[0];
        }

        private BitArray _GetPreviouslyWrittenValue()
        {
            var wasWritten = 0x00;

            Controller.Driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionIn;
            Controller.Driver.Digital.ReadByte(ChannelID, ref wasWritten);
            Controller.Driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

            var result = new BitArray(new int[] { wasWritten });

            return result;
        }

        public void Set_ToOne()
        {
            var ToWrite = _GetPreviouslyWrittenValue();
            ToWrite[BitNumber] = true;
            Controller.Driver.Digital.WriteByte(ChannelID, _GetIntFromBitArray(ToWrite));
        }

        public void Set_ToZero()
        {
            var ToWrite = _GetPreviouslyWrittenValue();
            ToWrite[BitNumber] = false;
            Controller.Driver.Digital.WriteByte(ChannelID, _GetIntFromBitArray(ToWrite));
        }

        public void Pulse()
        {
            Set_ToOne();
            Thread.Sleep(100);
            Set_ToZero();
        }
    }
}
