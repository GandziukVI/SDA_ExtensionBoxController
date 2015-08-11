using System;
using System.Collections.Generic;
using System.Text;
using Agilent.AgilentU254x.Interop;

namespace Agilent_ExtensionBox
{
    public enum MeasuringMode { AC_Mode, DC_Mode }

    public class AI_Channel
    {
        private int ChannelNumber;
        private BoxController Controller;

        public AI_Channel(int __ChannelNumber, BoxController __Controller)
        {
            ChannelNumber = __ChannelNumber;
            Controller = __Controller;

            _ChannelSettings = new AI_ChannelParams(__ChannelNumber, __Controller);
        }

        #region Analog input channel functionality implementation

        private void _Set_ChannelEnabled(bool Enabled)
        {
            switch (ChannelNumber)
            {
                case 1:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn1").Enabled = Enabled; break;
                case 2:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn2").Enabled = Enabled; break;
                case 3:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn3").Enabled = Enabled; break;
                case 4:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn4").Enabled = Enabled; break;
                default:
                    break;
            }
        }

        private void _Set_ChannelPolarity(AgilentU254xAnalogPolarityEnum Polarity)
        {
            switch (ChannelNumber)
            {
                case 1:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn1").Polarity = Polarity; break;
                case 2:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn2").Polarity = Polarity; break;
                case 3:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn3").Polarity = Polarity; break;
                case 4:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn4").Polarity = Polarity; break;
                default:
                    break;
            }
        }

        private void _Set_ChannelRange(double Range)
        {
            if (Array.IndexOf(DefinitionsAndConstants._AvailableRanges, Range) == -1)
                Range = DefinitionsAndConstants._GetClosestValueInArray(DefinitionsAndConstants._AvailableRanges, Range);

            switch (ChannelNumber)
            {
                case 1:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn1").Range = Range; break;
                case 2:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn2").Range = Range; break;
                case 3:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn3").Range = Range; break;
                case 4:
                    Controller.Driver.AnalogIn.Channels.get_Item("AIn4").Range = Range; break;
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

        private double _Range;
        public double Range
        {
            get { return _Range; }
            set
            {
                _Set_ChannelRange(value);
                _Range = value;
            }
        }

        private AI_ChannelParams _ChannelSettings;
        public AI_ChannelParams ChannelSettings
        {
            get { return _ChannelSettings; }
        }
    }
}
