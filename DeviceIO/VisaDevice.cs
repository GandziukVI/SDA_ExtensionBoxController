using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.VisaNS;

namespace DeviceIO
{
    public class VisaDevice : IDeviceIO
    {
        private MessageBasedSession mbSession;

        public string VisaID { get; private set; }

        public VisaDevice(string resourceString)
        {
            VisaID = resourceString;
            mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(resourceString);
            mbSession.Timeout = int.MaxValue;
        }

        private static object sendCommandRequestLocker = new object();
        public void SendCommandRequest(string request)
        {
            lock (sendCommandRequestLocker)
            {
                var _Request = request.EndsWith("\n") ?
                        Encoding.ASCII.GetBytes(request) :
                        Encoding.ASCII.GetBytes(request + "\n");

                mbSession.Write(_Request);
            }
        }

        private static object receiveDeviceAnswerLocker = new object();
        public string ReceiveDeviceAnswer()
        {
            lock (receiveDeviceAnswerLocker)
            {
                return mbSession.ReadString().TrimEnd("\r\n".ToCharArray());
            }
        }

        private static object requestQueryLocker = new object();
        public string RequestQuery(string query)
        {
            lock (sendCommandRequestLocker) lock (receiveDeviceAnswerLocker) lock (requestQueryLocker)
            {
                var _Query = query.EndsWith("\n") ? Encoding.ASCII.GetBytes(query) : Encoding.ASCII.GetBytes(query + "\n");

                return Encoding.ASCII.GetString(mbSession.Query(_Query)).TrimEnd("\r\n".ToCharArray());
            }
        }

        private static object disposeLocker = new object();
        public void Dispose()
        {
            lock (disposeLocker)
            {
                if (mbSession != null)
                    mbSession.Dispose();

                GC.Collect();
            }
        }
    }
}
