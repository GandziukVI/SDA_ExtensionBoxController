using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Agilent_ExtensionBox.IO
{
    public class AquisitionRouter : IObservable<Point>
    {
        private class Unsubscriber : IDisposable
        {
            private List<IObserver<Point>> _observers;
            private IObserver<Point> _observer;

            public Unsubscriber(List<IObserver<Point>> observers, IObserver<Point> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }

        private readonly List<IObserver<Point>> _channels = new List<IObserver<Point>>();

        public IDisposable Subscribe(IObserver<Point> observer)
        {
            if (!_channels.Contains(observer))
            {
                _channels.Add(observer);

                var _channel = observer as AI_Channel;
                var _range = AvailableRanges.FromRangeEnum(_channel.Range);

                switch (_channel.Polarity)
                {
                    case PolarityEnum.Polarity_Bipolar:
                        _Converters.Add(new Func<int, double>((x) => { return x * _range / 32768.0; }));
                        break;
                    case PolarityEnum.Polarity_Unipolar:
                        _Converters.Add(new Func<int, double>((x) => { return (x / 65536.0 + 0.5) * _range; }));
                        break;
                    default:
                        throw new ArgumentException();
                }
            }
            else
            {
                var index = _channels.IndexOf(observer);

                _Converters.RemoveAt(index);

                var _channel = observer as AI_Channel;
                var _range = AvailableRanges.FromRangeEnum(_channel.Range);

                switch (_channel.Polarity)
                {
                    case PolarityEnum.Polarity_Bipolar:
                        _Converters.Insert(index, new Func<int, double>((x) => { return x * _range / 32768.0; }));
                        break;
                    case PolarityEnum.Polarity_Unipolar:
                        _Converters.Insert(index, new Func<int, double>((x) => { return (x / 65536.0 + 0.5) * _range; }));
                        break;
                    default:
                        throw new ArgumentException();
                }
            }

            return new Unsubscriber(_channels, observer);
        }

        public int Frequency { get; set; }

        private readonly List<Func<int, double>> _Converters = new List<Func<int,double>>();

        private delegate void AddData_Async(ref short[] Data);
        public void AddDataInvoke(ref short[] Data)
        {
            var del = new AddData_Async(AddData);
            del.BeginInvoke(ref Data, null, null);
        }

        private readonly object dataLocker = new object();

        public void AddData(ref short[] data)
        {
            lock (dataLocker)
            {
                double time = 0.0;
                double timeQuant = 1.0 / Frequency;
                for (int i = 0, j = 0; i + j <= data.Length; i += _channels.Count, time += timeQuant)
                {
                    for (j = 0; j < _channels.Count; j++)
                    {
                        _channels[j].OnNext(new Point(time, _Converters[j](data[i + j])));
                    }

                }
                
                for (int i = 0; i < _channels.Count; i++)
                {
                    _channels[i].OnCompleted();
                }
            }
        }
    }
}
