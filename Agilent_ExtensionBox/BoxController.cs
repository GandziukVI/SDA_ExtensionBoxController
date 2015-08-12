using System;
using System.Collections.Generic;
using System.Text;

using Ivi.Driver.Interop;
using Agilent.AgilentU254x.Interop;
using System.Threading;
using System.Collections;
using Agilent_ExtensionBox.IO;

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

        private AI_Channels _ChannelCollection;
        public AI_Channels ChannelCollection
        {
            get { return _ChannelCollection; }
        }

        public BoxController()
        {
            _ChannelCollection = new AI_Channels(this);
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



        #endregion
    }
}
