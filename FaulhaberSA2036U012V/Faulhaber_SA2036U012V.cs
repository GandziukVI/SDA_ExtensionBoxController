using DeviceIO;
using MotionController;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaulhaberSA2036U012V
{
    public class Faulhaber_SA2036U012V : I1DMotionController
    {
        private IDeviceIO _driver;
        public Faulhaber_SA2036U012V(ref IDeviceIO Driver)
        {
            _driver = Driver;
        }

        private double _queryMotor(string query)
        {
            while (true)
            {
                var responce = _driver.RequestQuery(query);
                double result;
                if (!responce.Contains("OK"))
                    if (double.TryParse(responce, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out result))
                        return result;
            }
        }

        public void Enable()
        {
            _driver.SendCommandRequest("en");
        }

        public void Disable()
        {
            _driver.SendCommandRequest("di");
        }

        public void StartMotion()
        {
            _driver.SendCommandRequest("m");
        }

        public void StopMotion()
        {
            Disable();
        }

        public double GetCurrentPosition()
        {
            return _queryMotor("pos");
        }

        public double GetSpeed()
        {
            var rpmSpeed = _queryMotor("GV");
            var mmpm = rpmSpeed / 2.0;

            return mmpm;
        }

        public void SetSpeed(double Speed)
        {
            var rpmSpeed = (int)Math.Round(2.0 * Speed);
            _driver.SendCommandRequest(string.Format("V{0}", rpmSpeed.ToString(NumberFormatInfo.InvariantInfo)));
        }

        public void SetDestination(double Destination)
        {
            var destination = 
            throw new NotImplementedException();
        }

        public void SetPosition(double Position)
        {
            throw new NotImplementedException();
        }

        public double Position
        {
            get { return GetCurrentPosition(); }
            set { SetPosition(value); }
        }

        public double Speed
        {
            get { return GetSpeed(); }
            set { SetSpeed(value); }
        }

        public void Dispose()
        {
            _driver.Dispose();
        }
    }
}
