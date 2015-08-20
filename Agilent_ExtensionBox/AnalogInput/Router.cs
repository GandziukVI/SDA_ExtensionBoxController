using System;
using System.Collections.Generic;
using System.Text;

namespace Agilent_ExtensionBox.AnalogInput
{
    class Router:IObservable<double>
    {
        public IDisposable Subscribe(IObserver<double> observer)
        {
            throw new NotImplementedException();
            observer.OnNext()
        }


        public void smth()
        {
            Router r = new Router();
            r.Subscribe()

        }
    }

    
}
