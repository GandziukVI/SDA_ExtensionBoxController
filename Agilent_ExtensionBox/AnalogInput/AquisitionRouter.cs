using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Agilent_ExtensionBox.AnalogInput
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
        
        public void AddData(double[] data)
        {

        }
       



    }

    
}
