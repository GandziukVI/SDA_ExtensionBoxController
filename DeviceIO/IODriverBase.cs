using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public abstract class IODriverBase: IDeviceIO, IDisposable
    {
        abstract public void SendCommandRequest(string request);

        abstract public string ReceiveDeviceAnswer();

        abstract public string RequestQuery(string query);

        abstract public void Dispose();
    }
}
