using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceIO
{
    public interface IDeviceIO : IDisposable
    {
        void SendCommandRequest(string request);
        string ReceiveDeviceAnswer();
        string RequestQuery(string query);
    }
}
