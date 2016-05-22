using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.VisaNS;

namespace DeviceIO
{
    public class VisaDevice : IDeviceIO, IDisposable
    {
        private MessageBasedSession mbSession;

        public string VisaID { get; private set; }

        public VisaDevice(string resourceString)
        {
            VisaID = resourceString;
            mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(resourceString);
            mbSession.Timeout = int.MaxValue;
        }

        public void SendCommandRequest(string request)
        {
            var _Request = request.EndsWith("\n") ?
                    Encoding.ASCII.GetBytes(request) :
                    Encoding.ASCII.GetBytes(request + "\n");

            mbSession.Write(_Request);
        }

        public string ReceiveDeviceAnswer()
        {
            return mbSession.ReadString().TrimEnd("\r\n".ToCharArray());
        }

        public string RequestQuery(string query)
        {
            var _Query = query.EndsWith("\n") ? Encoding.ASCII.GetBytes(query) : Encoding.ASCII.GetBytes(query + "\n");

            return Encoding.ASCII.GetString(mbSession.Query(_Query)).TrimEnd("\r\n".ToCharArray());
        }

        public void Dispose()
        {
            if (mbSession != null)
                mbSession.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
