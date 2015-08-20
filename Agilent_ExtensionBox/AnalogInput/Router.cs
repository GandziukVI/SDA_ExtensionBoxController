using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Agilent_ExtensionBox.IO
{
    class Router:IObservable<Point>
    {
        private List<IObserver<Point>> observers;

        public IDisposable Subscribe(IObserver<Point> observer)
        {
            if (!observers.Contains(observer))
                observers.Add(observer);
            return new Unsubscriber(observers, observer);
        }
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
       
    }

    
}
