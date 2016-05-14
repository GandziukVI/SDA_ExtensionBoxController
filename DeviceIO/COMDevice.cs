using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

namespace DeviceIO
{
    class COMDevice : IDeviceIO, IDisposable
    {
        private SerialPort _COMPort;
        private bool _dataReady = false;
        private string _dataReading;

        public COMDevice(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, string returnToken = "\n")
        {
            _COMPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

            _COMPort.NewLine = returnToken;
            _COMPort.RtsEnable = true;
            _COMPort.DtrEnable = true;

            _COMPort.ReadTimeout = SerialPort.InfiniteTimeout;
            _COMPort.WriteTimeout = SerialPort.InfiniteTimeout;

            _COMPort.DataReceived += _COMPort_DataReceived;

            _COMPort.Open();

            if (!_COMPort.IsOpen)
                throw new Exception("Can't connect to the COM port!");
        }

        ~COMDevice()
        {
            Dispose();
        }

        private void _COMPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            using(var port = sender as SerialPort)
            {
                _dataReading += port.ReadExisting();

                if (_dataReading.Contains(_COMPort.NewLine))
                    _dataReady = true;
            }
        }

        public void SendCommandRequest(string request)
        {
            try
            {
                _dataReady = false;

                request = request.EndsWith("\n") ? request : request + "\n";

                var strBytes = Encoding.ASCII.GetBytes(request);
                _COMPort.Write(strBytes, 0, strBytes.Length);
            }
            catch
            {
                throw new Exception("Error while communicating with COM port!");
            }
        }

        public string ReceiveDeviceAnswer()
        {
            while (!_dataReady) ;
            
            var _dataReadingCopy = string.Copy(_dataReading);
            _dataReading = string.Empty;

            return _dataReadingCopy.TrimEnd("\r\n".ToCharArray());
        }

        public string RequestQuery(string query)
        {
            SendCommandRequest(query);
            return ReceiveDeviceAnswer();
        }

        public void Dispose()
        {
            if(_COMPort != null)
            {
                if (_COMPort.IsOpen)
                {
                    _COMPort.Close();
                    _COMPort.Dispose();
                }
                else
                    _COMPort.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
