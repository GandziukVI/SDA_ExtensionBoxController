using System;
using System.Collections.Generic;
using System.Text;
using Agilent.AgilentU254x.Interop;
using System.Collections;
using System.Threading;

namespace Agilent_ExtensionBox
{
    public enum MeasuringMode { AC_Mode, DC_Mode }

    public class AI_Channel
    {
        private int _channelNumber;
        private AgilentU254xClass Driver;
        private const string AIn1 = "AIn1";
        private const string AIn2 = "AIn2";
        private const string AIn3 = "AIn3";
        private const string AIn4 = "AIn4";


        public AI_Channel(int ChannelNumber, AgilentU254xClass Controller)
        {
            _channelNumber = ChannelNumber;
            Driver = Controller;
            //ChannelNumber = __ChannelNumber;
            //Controller = __Controller;

           // _ChannelSettings = new AI_ChannelParams(__ChannelNumber, __Controller);
        }

        #region Analog input channel functionality implementation

        private void _Set_ChannelEnabled(bool Enabled)
        {
            switch (_channelNumber)
            {
                case 1:
                    Driver.AnalogIn.Channels.get_Item(AIn1).Enabled = Enabled; break;
                case 2:
                    Driver.AnalogIn.Channels.get_Item(AIn2).Enabled = Enabled; break;
                case 3:
                    Driver.AnalogIn.Channels.get_Item(AIn3).Enabled = Enabled; break;
                case 4:
                    Driver.AnalogIn.Channels.get_Item(AIn4).Enabled = Enabled; break;
                default:
                    break;
            }
        }

        private void _Set_ChannelPolarity(AgilentU254xAnalogPolarityEnum Polarity)
        {
            switch (_channelNumber)
            {
                case 1:
                    Driver.AnalogIn.Channels.get_Item(AIn1).Polarity = Polarity; break;
                case 2:
                    Driver.AnalogIn.Channels.get_Item(AIn2).Polarity = Polarity; break;
                case 3:
                    Driver.AnalogIn.Channels.get_Item(AIn3).Polarity = Polarity; break;
                case 4:
                    Driver.AnalogIn.Channels.get_Item(AIn4).Polarity = Polarity; break;
                default:
                    break;
            }
        }

        private void _Set_ChannelRange(Ranges range)
        {
            var r = AvailableRanges.FromRangeEnum(range);

            switch (_channelNumber)
            {
                case 1:
                    Driver.AnalogIn.Channels.get_Item(AIn1).Range = r; break;
                case 2:
                    Driver.AnalogIn.Channels.get_Item(AIn2).Range = r; break;
                case 3:
                    Driver.AnalogIn.Channels.get_Item(AIn3).Range = r; break;
                case 4:
                    Driver.AnalogIn.Channels.get_Item(AIn4).Range = r; break;
                default:
                    break;
            }
        }

        #endregion

        private bool _Enabled;
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                _Set_ChannelEnabled(value);
                _Enabled = value;
            }
        }


        private AgilentU254xAnalogPolarityEnum _Polarity;
        public AgilentU254xAnalogPolarityEnum Polarity
        {
            get { return _Polarity; }
            set
            {
                _Set_ChannelPolarity(value);
                _Polarity = value;
            }
        }

        private Ranges _Range;
        public Ranges Range
        {
            get { return _Range; }
            set
            {
                _Set_ChannelRange(value);
                _Range = value;
            }
        }

        private SDA_Bit ChannelSelector_A0;
        private SDA_Bit ChannelSelector_A1;

        private SDA_Bit Relay_SetReset;
        private SDA_Bit Relay_Pulse;

        private void _SelectChannel()
        {
            switch (_channelNumber)
            {
                case 1:
                    {
                        //ChannelSelector_A0.Set_ToZero();
                        //ChannelSelector_A1.Set_ToZero();
                        Driver.Digital.WriteByte("DIOB", 0x00);
                    } break;
                case 2:
                    {
                        //ChannelSelector_A0.Set_ToOne();
                        //ChannelSelector_A1.Set_ToZero();
                        Driver.Digital.WriteByte("DIOB", 0x01);
                    } break;
                case 3:
                    {
                        //ChannelSelector_A0.Set_ToZero();
                        //ChannelSelector_A1.Set_ToOne();
                        Driver.Digital.WriteByte("DIOB", 0x02);
                    } break;
                case 4:
                    {
                        //ChannelSelector_A0.Set_ToOne();
                        //ChannelSelector_A1.Set_ToOne();
                        Driver.Digital.WriteByte("DIOB", 0x03);
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

            Driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionIn;
            Driver.Digital.ReadByte(ChannelID, ref wasWritten);
            Driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

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
                        Driver.Digital.WriteByte("DIOC", 0x00);
                    } break;
                case 10:
                    {
                        Driver.Digital.WriteByte("DIOC", 0x01);
                    } break;
                case 100:
                    {
                        Driver.Digital.WriteByte("DIOC", 0x02);
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

            Driver.Digital.WriteByte("DIOA", _GetIntFromBitArray(ToWrite));
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

            Driver.Digital.WriteByte("DIOA", _GetIntFromBitArray(ToWrite));
        }

        public void Set_ChannelParametersToLatch(int Frequency, int FilterGain, int PGA_Gain)
        {
            _Set_PGA_Gain(PGA_Gain);
            _Set_Filter_Gain(FilterGain);
            _Set_Filter_Frequency(Frequency);

            _SelectChannel();

            var ToWrite = _GetPreviouslyWrittenValue("DIOD");

            ToWrite[2] = true;
            Driver.Digital.WriteByte("DIOD", _GetIntFromBitArray(ToWrite));
            Thread.Sleep(100);
            ToWrite[2] = false;
            Driver.Digital.WriteByte("DIOD", _GetIntFromBitArray(ToWrite));
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
}
