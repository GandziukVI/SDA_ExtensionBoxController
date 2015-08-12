using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Agilent.AgilentU254x.Interop;

namespace Agilent_ExtensionBox
{
    public class AI_ChannelParams
    {
        private BoxController Controller;

        private SDA_Bit ChannelSelector_A0;
        private SDA_Bit ChannelSelector_A1;

        private SDA_Bit Relay_SetReset;
        private SDA_Bit Relay_Pulse;

        private int ChannelNumber;

        public AI_ChannelParams(int __ChannelNumber, BoxController __Controller)
        {
            ChannelNumber = __ChannelNumber;
            Controller = __Controller;

            ChannelSelector_A0 = new SDA_Bit("DIOB", 0, __Controller);
            ChannelSelector_A1 = new SDA_Bit("DIOB", 1, __Controller);

            Relay_SetReset = new SDA_Bit("DIOD", 1, __Controller);
            Relay_Pulse = new SDA_Bit("DIOD", 0, __Controller);
        }

        private void _SelectChannel()
        {
            switch (ChannelNumber)
            {
                case 1:
                    {
                        //ChannelSelector_A0.Set_ToZero();
                        //ChannelSelector_A1.Set_ToZero();
                        Controller.Driver.Digital.WriteByte("DIOB", 0x00);
                    } break;
                case 2:
                    {
                        //ChannelSelector_A0.Set_ToOne();
                        //ChannelSelector_A1.Set_ToZero();
                        Controller.Driver.Digital.WriteByte("DIOB", 0x01);
                    } break;
                case 3:
                    {
                        //ChannelSelector_A0.Set_ToZero();
                        //ChannelSelector_A1.Set_ToOne();
                        Controller.Driver.Digital.WriteByte("DIOB", 0x02);
                    } break;
                case 4:
                    {
                        //ChannelSelector_A0.Set_ToOne();
                        //ChannelSelector_A1.Set_ToOne();
                        Controller.Driver.Digital.WriteByte("DIOB", 0x03);
                    } break;
                default:
                    break;
            }
        }

        private void _Set_To_AC()
        {
            _SelectChannel();
            Relay_SetReset.Set_ToZero();
            Relay_Pulse.Pulse();
        }

        private void _Set_To_DC()
        {
            _SelectChannel();
            Relay_SetReset.Set_ToOne();
            Relay_Pulse.Pulse();
        }

        private MeasuringMode _Mode;
        public MeasuringMode Mode
        {
            get { return _Mode; }
            set
            {
                switch (value)
                {
                    case MeasuringMode.AC_Mode:
                        _Set_To_AC();
                        break;
                    case MeasuringMode.DC_Mode:
                        _Set_To_DC();
                        break;
                }
                _Mode = value;
            }
        }

        private int _GetIntFromBitArray(BitArray bitArray)
        {
            if (bitArray.Length > 32)
                throw new ArgumentException("Argument length shall be at most 32 bits.");

            var array = new int[1];
            bitArray.CopyTo(array, 0);

            return array[0];
        }

        private BitArray _GetPreviouslyWrittenValue(string ChannelID)
        {
            var wasWritten = 0x00;

            Controller.Driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionIn;
            Controller.Driver.Digital.ReadByte(ChannelID, ref wasWritten);
            Controller.Driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

            var result = new BitArray(new int[] { wasWritten });

            return result;
        }

        private void _Set_PGA_Gain(int Gain)
        {
            if (Array.IndexOf(DefinitionsAndConstants._AvailableGains, Gain) == -1)
                Gain = DefinitionsAndConstants._GetClosestValueInArray(DefinitionsAndConstants._AvailableGains, Gain);

            switch (Gain)
            {
                case 1:
                    {
                        Controller.Driver.Digital.WriteByte("DIOC", 0x00);
                    } break;
                case 10:
                    {
                        Controller.Driver.Digital.WriteByte("DIOC", 0x01);
                    } break;
                case 100:
                    {
                        Controller.Driver.Digital.WriteByte("DIOC", 0x02);
                    } break;
            }
        }

        private void _Set_Filter_Gain(int Gain)
        {
            if ((Gain > 16) || (Gain < 1))
                throw new Exception(String.Format("Wring gain passed to filter {0}", Gain));

            var _Gain = --Gain;

            var ToWrite = _GetPreviouslyWrittenValue("DIOA");

            for (int i = 0; i < DefinitionsAndConstants._bitmask.Length; i++)
            {
                if (((_Gain & DefinitionsAndConstants._bitmask[i]) == DefinitionsAndConstants._bitmask[i]))
                    ToWrite[DefinitionsAndConstants._FilterGain_Bits[i]] = true;
                else
                    ToWrite[DefinitionsAndConstants._FilterGain_Bits[i]] = false;
            }

            Controller.Driver.Digital.WriteByte("DIOA", _GetIntFromBitArray(ToWrite));
        }

        private void _Set_Filter_Frequency(int Frequency)
        {
            if (Array.IndexOf(DefinitionsAndConstants._AvailableCutOffFrequencies, Frequency) == -1)
                Frequency = DefinitionsAndConstants._GetClosestValueInArray(DefinitionsAndConstants._AvailableCutOffFrequencies, Frequency);

            var value = Array.IndexOf(DefinitionsAndConstants._AvailableCutOffFrequencies, Frequency);

            var ToWrite = _GetPreviouslyWrittenValue("DIOA");

            for (int i = 0; i < DefinitionsAndConstants._bitmask.Length; i++)
            {
                if (((value & DefinitionsAndConstants._bitmask[i]) == DefinitionsAndConstants._bitmask[i]))
                    ToWrite[DefinitionsAndConstants._Frequency_Bits[i]] = true;
                else
                    ToWrite[DefinitionsAndConstants._Frequency_Bits[i]] = false;
            }

            Controller.Driver.Digital.WriteByte("DIOA", _GetIntFromBitArray(ToWrite));
        }

        public void Set_ChannelParametersToLatch(int Frequency, int FilterGain, int PGA_Gain)
        {
            _Set_PGA_Gain(PGA_Gain);
            _Set_Filter_Gain(FilterGain);
            _Set_Filter_Frequency(Frequency);

            _SelectChannel();

            var ToWrite = _GetPreviouslyWrittenValue("DIOD");

            ToWrite[2] = true;
            Controller.Driver.Digital.WriteByte("DIOD", _GetIntFromBitArray(ToWrite));
            Thread.Sleep(100);
            ToWrite[2] = false;
            Controller.Driver.Digital.WriteByte("DIOD", _GetIntFromBitArray(ToWrite));
        }

        private int _PGA_Gain = 1;
        public int PGA_Gain
        {
            get { return _PGA_Gain; }
            set
            {
                _Set_PGA_Gain(value);
                _PGA_Gain = value;
                Set_ChannelParametersToLatch(_Filter_Frequency, _Filter_Gain, _PGA_Gain);
            }
        }

        private int _Filter_Gain = 1;
        public int Filter_Gain
        {
            get { return _Filter_Gain; }
            set
            {
                _Set_Filter_Gain(value);
                _Filter_Gain = value;
                Set_ChannelParametersToLatch(_Filter_Frequency, _Filter_Gain, _PGA_Gain);
            }
        }

        private int _Filter_Frequency = 100;
        public int FilterFrequency
        {
            get { return _Filter_Frequency; }
            set
            {
                _Set_Filter_Frequency(value);
                _Filter_Frequency = value;
                Set_ChannelParametersToLatch(_Filter_Frequency, _Filter_Gain, _PGA_Gain);
            }
        }
    }
}
