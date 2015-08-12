using System;
using System.Collections.Generic;
using System.Text;
using Agilent.AgilentU254x.Interop;
using System.Collections;
using System.Threading;

namespace Agilent_ExtensionBox.IO
{
    public enum MeasuringMode { AC_Mode, DC_Mode }

    public class AI_Channel
    {
        private int _channelNumber;
        private AgilentU254xClass _driver;
       

        public AI_Channel(int ChannelNumber, AgilentU254xClass Controller)
        {
            _channelNumber = ChannelNumber;
            _driver = Controller;
        }

        #region Analog input channel functionality implementation

        private void _Set_ChannelEnabled(bool Enabled)
        {
            switch (_channelNumber)
            {
                case 1:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN1).Enabled = Enabled; break;
                case 2:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN2).Enabled = Enabled; break;
                case 3:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN3).Enabled = Enabled; break;
                case 4:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN4).Enabled = Enabled; break;
                default:
                    break;
            }
        }

        private void _Set_ChannelPolarity(AgilentU254xAnalogPolarityEnum Polarity)
        {
            switch (_channelNumber)
            {
                case 1:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN1).Polarity = Polarity; break;
                case 2:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN2).Polarity = Polarity; break;
                case 3:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN3).Polarity = Polarity; break;
                case 4:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN4).Polarity = Polarity; break;
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
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN1).Range = r; break;
                case 2:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN2).Range = r; break;
                case 3:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN3).Range = r; break;
                case 4:
                    _driver.AnalogIn.Channels.get_Item(ChannelNames.AIN4).Range = r; break;
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

        private SDA_Bit Relay_SetReset;
        private SDA_Bit Relay_Pulse;

        private void _Select_AI_Channel()
        {
            switch (_channelNumber)
            {
                case 1: _driver.Digital.WriteByte(ChannelNames.DIOB, 0x00); break;
                case 2: _driver.Digital.WriteByte(ChannelNames.DIOB, 0x01); break;
                case 3: _driver.Digital.WriteByte(ChannelNames.DIOB, 0x02); break;
                case 4: _driver.Digital.WriteByte(ChannelNames.DIOB, 0x03); break;
                
                default:
                    break;
            }
        }

        private void _Set_To_AC()
        {
            _Select_AI_Channel();
            Relay_SetReset.Set_ToZero();
            Relay_Pulse.Pulse();
        }

        private void _Set_To_DC()
        {
            _Select_AI_Channel();
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

            _driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionIn;
            _driver.Digital.ReadByte(ChannelID, ref wasWritten);
            _driver.Digital.Channels.get_Item(ChannelID).Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

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
                        _driver.Digital.WriteByte(ChannelNames.DIOC, 0x00);
                    } break;
                case 10:
                    {
                        _driver.Digital.WriteByte(ChannelNames.DIOC, 0x01);
                    } break;
                case 100:
                    {
                        _driver.Digital.WriteByte(ChannelNames.DIOC, 0x02);
                    } break;
            }
        }

        private void _Set_Filter_Gain(int Gain)
        {
            if ((Gain > 16) || (Gain < 1))
                throw new Exception(String.Format("Wring gain passed to filter {0}", Gain));

            var _Gain = --Gain;

            var ToWrite = _GetPreviouslyWrittenValue(ChannelNames.DIOA);

            for (int i = 0; i < DefinitionsAndConstants._bitmask.Length; i++)
            {
                if (((_Gain & DefinitionsAndConstants._bitmask[i]) == DefinitionsAndConstants._bitmask[i]))
                    ToWrite[DefinitionsAndConstants._FilterGain_Bits[i]] = true;
                else
                    ToWrite[DefinitionsAndConstants._FilterGain_Bits[i]] = false;
            }

            _driver.Digital.WriteByte(ChannelNames.DIOA, _GetIntFromBitArray(ToWrite));
        }

        private void _Set_Filter_Frequency(int Frequency)
        {
            if (Array.IndexOf(DefinitionsAndConstants._AvailableCutOffFrequencies, Frequency) == -1)
                Frequency = DefinitionsAndConstants._GetClosestValueInArray(DefinitionsAndConstants._AvailableCutOffFrequencies, Frequency);

            var value = Array.IndexOf(DefinitionsAndConstants._AvailableCutOffFrequencies, Frequency);

            var ToWrite = _GetPreviouslyWrittenValue(ChannelNames.DIOA);

            for (int i = 0; i < DefinitionsAndConstants._bitmask.Length; i++)
            {
                if (((value & DefinitionsAndConstants._bitmask[i]) == DefinitionsAndConstants._bitmask[i]))
                    ToWrite[DefinitionsAndConstants._Frequency_Bits[i]] = true;
                else
                    ToWrite[DefinitionsAndConstants._Frequency_Bits[i]] = false;
            }

            _driver.Digital.WriteByte(ChannelNames.DIOA, _GetIntFromBitArray(ToWrite));
        }

        public void Set_ChannelParametersToLatch(int Frequency, int FilterGain, int PGA_Gain)
        {
            _Set_PGA_Gain(PGA_Gain);
            _Set_Filter_Gain(FilterGain);
            _Set_Filter_Frequency(Frequency);

            _Select_AI_Channel();

            var ToWrite = _GetPreviouslyWrittenValue(ChannelNames.DIOD);

            ToWrite[2] = true;
            _driver.Digital.WriteByte(ChannelNames.DIOD, _GetIntFromBitArray(ToWrite));
            Thread.Sleep(100);
            ToWrite[2] = false;
            _driver.Digital.WriteByte(ChannelNames.DIOD, _GetIntFromBitArray(ToWrite));
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
