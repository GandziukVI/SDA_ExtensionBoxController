using System;
using System.Collections.Generic;
using System.Text;

using Ivi.Driver.Interop;
using Agilent.AgilentU254x.Interop;
using System.Threading;
using System.Collections;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Agilent_ExtensionBox
{
    public class BoxController : IDisposable
    {
        private AgilentU254x _Driver;

        AgilentU254xDigitalChannel[] _ChannelArray;

        public AgilentU254x Driver { get { return _Driver; } }

        private static readonly string Options = "Simulate=false, Cache=false, QueryInstrStatus=false";

        private string _ResourceName = "";

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

        private readonly AquisitionRouter _router = new AquisitionRouter();

        #region Agilent device initialization and closure

        private static object initLock = new object();

        /// <summary>
        /// Initializes the Agilent instrument
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public bool Init(string resourceName)
        {
            lock (initLock)
            {
                _ResourceName = resourceName;

                if (IsInistialized)
                    return true;

                try
                {
                    if (_Driver == null)
                        _Driver = new AgilentU254x();
                    else
                    {
                        Close();
                        _Driver = new AgilentU254x();
                    }

                    _Driver.Initialize(resourceName, false, true, Options);
                    _Driver.DriverOperation.QueryInstrumentStatus = false;
                    _Driver.System.TimeoutMilliseconds = 5000;

                    _ChannelArray = new AgilentU254xDigitalChannel[] {
                    _Driver.Digital.Channels.get_Item("DIOA"),
                    _Driver.Digital.Channels.get_Item("DIOB"),
                    _Driver.Digital.Channels.get_Item("DIOC"),
                    _Driver.Digital.Channels.get_Item("DIOD") };

                    for (int i = 0; i < _ChannelArray.Length; i++)
                        if (_ChannelArray[i].Direction != AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut)
                            _ChannelArray[i].Direction = AgilentU254xDigitalChannelDirectionEnum.AgilentU254xDigitalChannelDirectionOut;

                    Reset_Digital();

                    _AI_ChannelCollection = new AI_Channels(_Driver);
                    _AO_ChannelCollection = new AO_Channels(_Driver);

                    _IsInitialized = true;

                    return true;
                }
                catch { return false; }
            }
        }

        private static object closeLock = new object();

        /// <summary>
        /// Closes the Agilent instrument driver
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            lock (closeLock)
            {
                if (!IsInistialized)
                    return true;

                try
                {
                    if (_AcquisitionInProgress == true)
                        _Driver.AnalogIn.Acquisition.Stop();

                    _Driver.Close();
                    _IsInitialized = false;
                    return true;
                }
                catch { return false; }
            }
        }

        #endregion

        private static object resetDigitalLock = new object();
        public void Reset_Digital()
        {
            lock (resetDigitalLock)
            {
                _Driver.Digital.WriteByte("DIOA", 0x00);
                _Driver.Digital.WriteByte("DIOB", 0x00);
                _Driver.Digital.WriteByte("DIOC", 0x00);
                _Driver.Digital.WriteByte("DIOD", 0x00);
            }
        }

        #region Acquisition control

        private static object voltageMeasurementAllChannelsLock = new object();
        public double[] VoltageMeasurement_AllChannels(int AveragingNumber)
        {
            lock (voltageMeasurementAllChannelsLock)
            {
                var result = new double[] { 0.0, 0.0, 0.0, 0.0 };
                var singleReading = new double[] { 0.0, 0.0, 0.0, 0.0 };

                for (int i = 0; i < AveragingNumber; i++)
                {
                    try
                    {
                        _Driver.AnalogIn.Measurement.ReadMultiple("AIn1,AIn2,AIn3,AIn4", ref singleReading);

                        for (int j = 0; j < result.Length; j++)
                            result[j] += singleReading[j];
                    }
                    catch
                    {
                        if (i >= 0)
                            --i;
                    }
                }

                for (int i = 0; i < result.Length; i++)
                    result[i] /= AveragingNumber;

                return result;
            }
        }

        private static object disableAllChannelsForContiniousAcquisitionLock = new object();
        private void _DisableAllChannelsForContiniousAcquisition()
        {
            lock (disableAllChannelsForContiniousAcquisitionLock)
            {
                _AI_ChannelCollection[AnalogInChannelsEnum.AIn1].Enabled = false;
                _AI_ChannelCollection[AnalogInChannelsEnum.AIn2].Enabled = false;
                _AI_ChannelCollection[AnalogInChannelsEnum.AIn3].Enabled = false;
                _AI_ChannelCollection[AnalogInChannelsEnum.AIn4].Enabled = false;
            }
        }

        private static object configureAI_ChannelsLock = new object();
        public void ConfigureAI_Channels(params AI_ChannelConfig[] ChannelsConfig)
        {
            lock (configureAI_ChannelsLock)
            {
                _DisableAllChannelsForContiniousAcquisition();

                if (ChannelsConfig.Length < 1 || ChannelsConfig.Length > 4)
                    throw new ArgumentException("The requested number of channels is not supported");

                for (int i = 0; i < ChannelsConfig.Length; i++)
                {
                    _AI_ChannelCollection[ChannelsConfig[i].ChannelName].Enabled = ChannelsConfig[i].Enabled;
                    _AI_ChannelCollection[ChannelsConfig[i].ChannelName].Mode = ChannelsConfig[i].Mode;
                    _AI_ChannelCollection[ChannelsConfig[i].ChannelName].Polarity = ChannelsConfig[i].Polarity;
                    _AI_ChannelCollection[ChannelsConfig[i].ChannelName].Range = ChannelsConfig[i].Range;
                }
            }
        }

        private bool _AcquisitionInProgress = false;
        public bool AcquisitionInProgress
        {
            get { return _AcquisitionInProgress; }
            set { _AcquisitionInProgress = value; }
        }

        public delegate void CallAsync(ref short[] data);

        private static object startAnalogAcquisitionLock = new object();
        [HandleProcessCorruptedStateExceptions]
        public bool StartAnalogAcquisition(int SampleRate)
        {
            lock (startAnalogAcquisitionLock)
            {
                var results = new short[SampleRate];

                _Driver.AnalogIn.MultiScan.Configure(SampleRate, -1);

                _Driver.AnalogIn.Acquisition.BufferSize = SampleRate;
                _Driver.AnalogIn.Acquisition.Start();

                _router.Frequency = SampleRate;

                foreach (var item in _AI_ChannelCollection)
                {
                    if (item.IsEnabled)
                    {
                        item.SampleRate = SampleRate;
                        _router.Subscribe(item);
                    }
                }

                while (_AcquisitionInProgress)
                {
                    while (true)
                    {
                        if (_AcquisitionInProgress == false)
                            break;
                        try
                        {
                            var dataReady = (_Driver.AnalogIn.Acquisition.BufferStatus == AgilentU254xBufferStatusEnum.AgilentU254xBufferStatusDataReady);
                            if (dataReady == true)
                                break;
                        }
                        catch
                        {
                            StopAnalogAcquisition();
                            return false;
                        }
                    }

                    try
                    {
                        _Driver.AnalogIn.Acquisition.Fetch(ref results);
                        if (results.Length > 0)
                            _router.AddDataInvoke(ref results);
                    }
                    catch
                    {
                        StopAnalogAcquisition();
                        return false;
                    }
                }

                try
                {
                    _Driver.AnalogIn.Acquisition.Stop();
                }
                catch { return false; }

                return true;
            }
        }

        private static object startBufferedAnalogAcquisitionLock = new object();
        public void StartBufferedAnalogAcquisition(int SampleRate, int BufferSize = 65536)
        {
            lock (startBufferedAnalogAcquisitionLock)
            {
                if (BufferSize > SampleRate)
                    BufferSize = SampleRate;
                if (SampleRate % BufferSize != 0)
                    throw new ArgumentException();

                var results = new short[SampleRate];
                var temp = new short[BufferSize];

                _Driver.AnalogIn.MultiScan.Configure(SampleRate, -1);
                _Driver.AnalogIn.Acquisition.BufferSize = BufferSize;
                _Driver.AnalogIn.Acquisition.Start();

                _router.Frequency = SampleRate;

                foreach (var item in _AI_ChannelCollection)
                {
                    if (item.IsEnabled)
                    {
                        item.SampleRate = SampleRate;
                        _router.Subscribe(item);
                    }
                }

                var nSamples = (int)(SampleRate / BufferSize);
                var counter = 0;
                var startIndex = 0;

                while (_AcquisitionInProgress)
                {
                    while (true)
                    {
                        if (_AcquisitionInProgress == false)
                            break;
                        try
                        {
                            var dataReady = (_Driver.AnalogIn.Acquisition.BufferStatus == AgilentU254xBufferStatusEnum.AgilentU254xBufferStatusDataReady);
                            if (dataReady == true)
                                ++counter;
                            if (counter == nSamples - 1)
                                break;

                            _Driver.AnalogIn.Acquisition.Fetch(ref temp);
                            startIndex = counter * temp.Length;
                            Array.Copy(temp, 0, results, startIndex, temp.Length);
                        }
                        catch { return; }
                    }

                    try
                    {
                        _router.AddDataInvoke(ref results);
                    }
                    catch { }

                    counter = 0;
                    startIndex = 0;
                }

                try
                {
                    _Driver.AnalogIn.Acquisition.Stop();
                }
                catch { return; }
            }
        }

        public void StopAnalogAcquisition()
        {
            if (_AcquisitionInProgress == true)
            {
                _AcquisitionInProgress = false;
                _Driver.AnalogIn.Acquisition.Stop();
            }
        }

        private static object acquireSingleShotLock = new object();
        public bool AcquireSingleShot(int SampleRate)
        {
            lock (acquireSingleShotLock)
            {
                try
                {
                    var results = new short[SampleRate];

                    _Driver.AnalogIn.MultiScan.SampleRate = SampleRate;
                    _Driver.AnalogIn.MultiScan.NumberOfScans = SampleRate;

                    _Driver.AnalogIn.Acquisition.BufferSize = SampleRate;
                    _Driver.AnalogIn.Acquisition.Start();

                    while (!(_Driver.AnalogIn.Acquisition.Completed == true)) ;

                    _Driver.AnalogIn.Acquisition.Fetch(ref results);

                    _router.Frequency = SampleRate;

                    foreach (var item in _AI_ChannelCollection)
                    {
                        if (item.IsEnabled)
                        {
                            item.SampleRate = SampleRate;
                            _router.Subscribe(item);
                        }
                    }

                    _router.AddData(ref results);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        #endregion

        public void Dispose()
        {
            if (IsInistialized)
            {
                if (AcquisitionInProgress)
                    StopAnalogAcquisition();

                Reset_Digital();

                _DisableAllChannelsForContiniousAcquisition();

                _Driver.Status.Clear();
                _Driver.Utility.Reset();

                Close();
            }

            int i = _ChannelArray.Length - 1;

            for (; i >= 0; )
            {
                Marshal.ReleaseComObject(_ChannelArray[i]);
                --i;
            }

            Marshal.ReleaseComObject(_Driver);
        }
    }
}
