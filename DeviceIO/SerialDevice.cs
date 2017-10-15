using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;
using System.Threading;
using System.Diagnostics;

namespace DeviceIO
{
    public class SerialDevice : IDeviceIO
    {
        private SerialPort _COMPort;
        private string _returnToken;
        private string _termChars = "\f\r\n\0";

        private string _dataReading;
        private ConcurrentQueue<string> _dataQueue;

        private const int _freqCriticalLimit = 20;

        public SerialDevice(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, string returnToken = "\n")
        {
            _dataReading = string.Empty;
            _dataQueue = new ConcurrentQueue<string>();

            _COMPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

            _returnToken = returnToken;

            _COMPort.NewLine = _returnToken;
            _COMPort.RtsEnable = true;
            _COMPort.DtrEnable = true;

            _COMPort.ReadTimeout = SerialPort.InfiniteTimeout;
            _COMPort.WriteTimeout = SerialPort.InfiniteTimeout;

            _COMPort.Open();

            if (!_COMPort.IsOpen)
                _COMPort.Open();
            if (!_COMPort.IsOpen)
                throw new Exception("Can't connect to the COM port!");
        }

        ~SerialDevice()
        {
            Dispose();
        }

        private static object sendCommandRequestLock = new object();
        public void SendCommandRequest(string request)
        {
            lock (requestQueryLock)
            {
                lock (receiveDeviceAnswerLock)
                {
                    lock (sendCommandRequestLock)
                    {
                        if (_COMPort != null && _COMPort.IsOpen == true)
                        {
                            request = request.EndsWith("\n") ? request : string.Format("{0}\n", request);

                            var strBytes = Encoding.ASCII.GetBytes(request);
                            _COMPort.Write(strBytes, 0, strBytes.Length);
                        }
                        else
                            throw new Exception("Serial port is closed!");
                    }
                }
            }
        }

        private static object receiveDeviceAnswerLock = new object();
        public string ReceiveDeviceAnswer()
        {
            lock (sendCommandRequestLock)
            {
                lock (requestQueryLock)
                {
                    lock (receiveDeviceAnswerLock)
                    {
                        if (_COMPort != null && _COMPort.IsOpen == true)
                        {
                            StringBuilder resultBuilder = new StringBuilder();
                            string reading;

                            while (true)
                            {
                                while (_COMPort.BytesToRead == 0) ;
                                reading = _COMPort.ReadExisting();
                                resultBuilder.Append(reading);
                                if (_termChars.Contains(reading[reading.Length - 1]))
                                    break;
                            }

                            return resultBuilder.ToString();
                        }
                        else
                            throw new Exception("Serial port is closed!");
                    }
                }
            }
        }

        private static object requestQueryLock = new object();
        public string RequestQuery(string query)
        {
            lock (sendCommandRequestLock)
            {
                lock (receiveDeviceAnswerLock)
                {
                    lock (requestQueryLock)
                    {
                        if (_COMPort != null && _COMPort.IsOpen == true)
                        {
                            while (_COMPort.BytesToRead > 0)
                            {
                                _COMPort.ReadExisting();
                                Thread.Sleep(_freqCriticalLimit);
                            }

                            query = query.EndsWith("\n") ? query : string.Format("{0}\n", query);

                            var strBytes = Encoding.ASCII.GetBytes(query);
                            _COMPort.Write(strBytes, 0, strBytes.Length);

                            StringBuilder resultBuilder = new StringBuilder();
                            string reading;

                            while (true)
                            {
                                while (_COMPort.BytesToRead == 0) ;
                                reading = _COMPort.ReadExisting();
                                resultBuilder.Append(reading);
                                if (_termChars.Contains(reading[reading.Length - 1]))
                                    break;
                            }

                            return resultBuilder.ToString();
                        }
                        else
                            throw new Exception("Serial port is closed!");
                    }
                }
            }
        }

        private static object disposeLock = new object();
        public void Dispose()
        {
            lock (disposeLock)
            {
                if (_COMPort != null)
                {
                    if (_COMPort.IsOpen)
                    {
                        _COMPort.Close();
                        _COMPort.Dispose();
                    }
                    else
                        _COMPort.Dispose();
                }
            }
        }
    }
}
