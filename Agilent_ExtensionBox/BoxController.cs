﻿using System;
using System.Collections.Generic;
using System.Text;

using Ivi.Driver.Interop;
using Agilent.AgilentU254x.Interop;
using System.Threading;
using System.Collections;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;

namespace Agilent_ExtensionBox
{
    public class BoxController
    {
        private readonly AgilentU254xClass _Driver = new AgilentU254xClass();
        public AgilentU254xClass Driver { get { return _Driver; } }

        private static readonly string Options = "Simulate=false, Cache=false, QueryInstrStatus=true";

        private static bool _IsInitialized = false;
        public static bool IsInistialized
        {
            get { return _IsInitialized; }
        }

        private AI_Channels _AI_ChannelCollection;
        public AI_Channels AI_ChannelCollection
        {
            get { return _AI_ChannelCollection; }
        }

        private AO_Channels _AO_ChannelCollection;
        public AO_Channels AO_ChannelCollection
        {
            get { return _AO_ChannelCollection; }
        }

        #region Agilent device initialization and closure

        /// <summary>
        /// Initializes the Agilent instrument
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public bool Init(string resourceName)
        {
            if (IsInistialized)
                return true;

            try
            {
                _Driver.Initialize(resourceName, false, true, Options);

                var _ChannelArray = new AgilentU254xDigitalChannel[] {
                    _Driver.Digital.Channels.get_Item("DIOA"),
                    _Driver.Digital.Channels.get_Item("DIOB"),
                    _Driver.Digital.Channels.get_Item("DIOC"),
                    _Driver.Digital.Channels.get_Item("DIOD") };

                foreach (var ch in _ChannelArray)
                    if (ch.Direction != AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut)
                        ch.Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

                _AI_ChannelCollection = new AI_Channels(_Driver);
                _AO_ChannelCollection = new AO_Channels(_Driver);

                _IsInitialized = true;

                return true;
            }
            catch { return false; }
        }

        /// <summary>
        /// Closes the Agilent instrument driver
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            if (!IsInistialized)
                return true;

            try
            {
                _Driver.Close();
                _IsInitialized = false;
                return true;
            }
            catch { return false; }
        }

        #endregion

        public void Reset_Digital()
        {
            _Driver.Digital.WriteByte("DIOA", 0);
            _Driver.Digital.WriteByte("DIOB", 0);
            _Driver.Digital.WriteByte("DIOC", 0);
            _Driver.Digital.WriteByte("DIOD", 0);
        }

        #region Acquisition control

        public double[] VoltageMeasurement_AllChannels(int AveragingNumber)
        {
            var result = new double[] { 0.0, 0.0, 0.0, 0.0 };
            var singleReading = new double[] { 0.0, 0.0, 0.0, 0.0 };

            for (int i = 0; i < AveragingNumber; i++)
            {
                _Driver.AnalogIn.Measurement.ReadMultiple("AIn1,AIn2,AIn3,AIn4", ref singleReading);

                for (int j = 0; j < result.Length; j++)
                    result[j] += singleReading[j];
            }

            for (int i = 0; i < result.Length; i++)
                result[i] /= AveragingNumber;

            return result;
        }

        private void _DisableAllChannelsForContiniousAcquisition()
        {
            _AI_ChannelCollection[AnalogInChannelsEnum.AIn1].Enabled = false;
            _AI_ChannelCollection[AnalogInChannelsEnum.AIn2].Enabled = false;
            _AI_ChannelCollection[AnalogInChannelsEnum.AIn3].Enabled = false;
            _AI_ChannelCollection[AnalogInChannelsEnum.AIn4].Enabled = false;
        }

        public void StartAnalogAcquisition(params AnalogInChannelsEnum[] WorkingChannels)
        {
            _DisableAllChannelsForContiniousAcquisition();

            if (WorkingChannels.Length < 1 || WorkingChannels.Length > 4)
                throw new ArgumentException("The requested number of channels is not supported");

            foreach (var item in WorkingChannels)
            {
                _AI_ChannelCollection[item].Enabled = true;
            }
        }

        #endregion
    }
}
