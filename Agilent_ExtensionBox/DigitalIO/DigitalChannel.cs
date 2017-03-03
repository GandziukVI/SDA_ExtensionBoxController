using Agilent.AgilentU254x.Interop;
using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.IO
{


    public class DigitalChannel
    {
        public int Width { get { return _bitArray.Length; } }
        private DigitalBit[] _bitArray;
        private AgilentU254x _driver;
        public AgilentU254x Driver { get { return _driver; } }
        private string _channelName;


        public DigitalChannel(DigitalChannelsEnum Channel, AgilentU254x Driver)
        {
            int width = 0;
            switch (Channel)
            {
                case DigitalChannelsEnum.DIOA:
                    {
                        _channelName = ChannelNamesEnum.DIOA;
                        width = 8;
                    } break;
                case DigitalChannelsEnum.DIOB:
                    {
                        _channelName = ChannelNamesEnum.DIOB;
                        width = 8;
                    } break;
                case DigitalChannelsEnum.DIOC:
                    {
                        _channelName = ChannelNamesEnum.DIOC;
                        width = 4;
                    } break;
                case DigitalChannelsEnum.DIOD:
                    {
                        _channelName = ChannelNamesEnum.DIOD;
                        width = 4;
                    } break;
                default:
                    throw new ArgumentException();
            }
            _driver = Driver;
            Driver.Digital.Channels.get_Item(_channelName).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

            _bitArray = new DigitalBit[width];
            for (int i = 0; i < width; i++)
            {
                _bitArray[i] = new DigitalBit(this, i);
            }
        }


        public void ReadByte(ref int result)
        {
            _driver.Digital.Channels.get_Item(_channelName).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionIn;
            _driver.Digital.ReadByte(_channelName, ref result);
            _driver.Digital.Channels.get_Item(_channelName).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;
        }

        public void WriteByte(int value)
        {
            _driver.Digital.WriteByte(_channelName, value);
        }

        public void ReadBit(int _bitNumber, out bool res)
        {
            int val = 0;
            res = false;
            ReadByte(ref val);
            res = ((0x01 << _bitNumber) & val) > 0;
            WriteByte(val);
        }

        public void WriteBit(int _bitNumber, bool p)
        {
            int val = 0;
            ReadByte(ref val);
            if (p)
                val |= 0x01 << _bitNumber;
            else
                val &= ~(0x01 << _bitNumber);
            WriteByte(val);
        }
    }
}
