using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Agilent_ExtensionBox.IO
{
    class AquisitionRouter:IObservable<Point>
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

        private List<IObserver<Point>> channels;

        public IDisposable Subscribe(IObserver<Point> observer)
        {
            if (!channels.Contains(observer))
                channels.Add(observer);
            return new Unsubscriber(channels, observer);
        }

        public int Frequency { get; set; }

        public void AddData(double[] data)
        {
            double time = 0;
            double timeQuant = 1 / Frequency;
            for (int i = 0, j = 0; i + j < data.Length; i += channels.Count, time += timeQuant)
            {
                for (j = 0; j < channels.Count; j++)
                {
                    channels[j].OnNext(new Point(time, data[i + j]));
                }
                
            }
            for (int i = 0; i < channels.Count; i++)
            {
                channels[j].OnCompleted();
            }
        }
       



    }

    
}
