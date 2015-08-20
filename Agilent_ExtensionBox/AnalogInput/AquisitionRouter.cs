using System;
using System.Collections.Generic;
using System.Text;
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

        private List<IObserver<Point>> _channels;

        public IDisposable Subscribe(IObserver<Point> observer)
        {
            if (!_channels.Contains(observer))
                _channels.Add(observer);
            return new Unsubscriber(_channels, observer);
        }

        public int Frequency { get; set; }

        public void AddData(double[] data)
        {
            double time = 0;
            double timeQuant = 1.0 / Frequency;
            for (int i = 0, j = 0; i + j < data.Length; i += _channels.Count, time += timeQuant)
            {
                for (j = 0; j < _channels.Count; j++)
                {
                    _channels[j].OnNext(new Point(time, data[i + j]));
                }

            }
            for (int i = 0; i < _channels.Count; i++)
            {
                _channels[i].OnCompleted();
            }
        }
    }


}
