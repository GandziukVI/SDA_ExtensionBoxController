using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Agilent.AgilentU254x.Interop;
using System.Collections;
using System.Threading;
using Agilent_ExtensionBox.Internal;
using System.Windows;
using System.Runtime.InteropServices;

namespace Agilent_ExtensionBox.IO
{
    public class AI_Channel : IObserver<Point>, IDisposable
    {
        private AnalogInChannelsEnum _channelName;
        private AgilentU254x _driver;
        private AgilentU254xAnalogInChannel _channel;
        private ChannelModeSwitch _modeSwitch;

        public ConcurrentQueue<Point[]> ChannelData { get; private set; }
        private Point[] _currentChannelData;
        private int _iterator = 0;

        private int _sampleRate;
        public int SampleRate
        {
            get
            {
                return _sampleRate;
            }
            set
            {
                _currentChannelData = new Point[value];
                _sampleRate = value;
            }
        }

        public AI_Channel(AnalogInChannelsEnum channelName, ref AgilentU254x Driver, ChannelModeSwitch ModeSwitch, Filter ChannelFilter, ProgrammableGainAmplifier ChannelPGA, AnalogInLatch CommonLatch)
        {
            _channelName = channelName;
            _driver = Driver;
            _modeSwitch = ModeSwitch;

            Parameters = new ChannelParams(_channelName, ChannelFilter, ChannelPGA, CommonLatch);
            InitDriverChannel(_channelName, out _channel);

            ChannelData = new ConcurrentQueue<Point[]>();
        }

        private static object initDriverChannelLock = new object();
        private void InitDriverChannel(AnalogInChannelsEnum ChannelName, out AgilentU254xAnalogInChannel channel)
        {
            lock (initDriverChannelLock)
            {
                switch (ChannelName)
                {
                    case AnalogInChannelsEnum.AIn1:
                        channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN1);
                        break;
                    case AnalogInChannelsEnum.AIn2:
                        channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN2);
                        break;
                    case AnalogInChannelsEnum.AIn3:
                        channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN3);
                        break;
                    case AnalogInChannelsEnum.AIn4:
                        channel = _driver.AnalogIn.Channels.get_Item(ChannelNamesEnum.AIN4);
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
        }

        private static object enabledLock = new object();

        private bool _IsEnabled = false;
        public bool IsEnabled { get { return _IsEnabled; } }

        public bool Enabled
        {
            get
            {
                lock (enabledLock)
                {
                    _IsEnabled = _channel.Enabled;
                    return _IsEnabled;
                }
            }
            set
            {
                lock (enabledLock)
                {
                    _IsEnabled = value;
                    _channel.Enabled = value;
                }
            }
        }

        private static object polarityLock = new object();
        public PolarityEnum Polarity
        {
            get
            {
                lock (polarityLock)
                {
                    var currPolarity = _channel.Polarity;
                    switch (currPolarity)
                    {
                        case AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar:
                            {
                                //Marshal.ReleaseComObject(currPolarity);
                                return PolarityEnum.Polarity_Bipolar;
                            }                            
                        case AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityUnipolar:
                            {
                                //Marshal.ReleaseComObject(currPolarity);
                                return PolarityEnum.Polarity_Unipolar;
                            }                            
                        default:
                            {
                                //Marshal.ReleaseComObject(currPolarity);
                                return PolarityEnum.Polarity_Bipolar;
                            }                            
                    }
                }
            }
            set
            {
                lock (polarityLock)
                {
                    switch (value)
                    {
                        case PolarityEnum.Polarity_Bipolar:
                            {
                                var selectedPol = AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityBipolar;
                                _channel.Polarity = selectedPol;
                                //Marshal.ReleaseComObject(selectedPol);
                                break;
                            }                            
                        case PolarityEnum.Polarity_Unipolar:
                            {
                                var selectedPol = AgilentU254xAnalogPolarityEnum.AgilentU254xAnalogPolarityUnipolar;
                                _channel.Polarity = selectedPol;
                                //Marshal.ReleaseComObject(selectedPol);
                                break;
                            }
                    }
                }
            }
        }

        private static object rangeLock = new object();

        private RangesEnum _Range;
        public RangesEnum Range
        {
            get { return _Range; }
            set
            {
                lock (rangeLock)
                {
                    var r = AvailableRanges.FromRangeEnum(value);
                    _channel.Range = r;
                    _Range = value;
                }
            }
        }

        private static object modeLock = new object();

        private ChannelModeEnum _Mode;
        public ChannelModeEnum Mode
        {
            get { return _Mode; }
            set
            {
                lock (modeLock)
                {
                    _modeSwitch.SetChannelMode(_channelName, value);
                    _Mode = value;
                }
            }
        }

        public ChannelParams Parameters
        {
            get;
            private set;
        }

        public event EventHandler DataReady;

        private void On_DataReady()
        {
            var handler = DataReady;
            if (handler != null)
                handler(this, new EventArgs());
        }

        public void OnCompleted()
        {
            _iterator = 0;
            ChannelData.Enqueue(_currentChannelData);
            On_DataReady();
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnNext(Point value)
        {
            _currentChannelData[_iterator] = value;
            ++_iterator;
        }

        public void Dispose()
        {
            Marshal.ReleaseComObject(_channel);
        }
    }
}
