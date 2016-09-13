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

        private bool enabled = false;
        public void Enable()
        {
            if (!enabled)
                _driver.SendCommandRequest("en");
            enabled = true;
        }

        public void Disable()
        {
            if (enabled)
                _driver.SendCommandRequest("di");
            enabled = false;
        }

        public void StartMotion()
        {
            Enable();
            _driver.SendCommandRequest("m");
        }

        public void StopMotion()
        {
            Disable();
        }

        public double GetCurrentPosition()
        {
            var motorPos = _queryMotor("pos");

            return motorPos / pulsesPerRevolution / gear / 2.0;
        }

        public double GetSpeed()
        {
            var rpmSpeed = _queryMotor("GV");
            var mmpm = rpmSpeed / 2.0 / gear;

            return mmpm;
        }

        public void SetSpeed(double Speed)
        {
            var rpmSpeed = (int)(Math.Round(2.0 * Speed) * gear);

            if (rpmSpeed < -30000)
                rpmSpeed = -30000;
            else if (rpmSpeed > 30000)
                rpmSpeed = 30000;

            _driver.SendCommandRequest(string.Format("V{0}", rpmSpeed.ToString(NumberFormatInfo.InvariantInfo)));
            //_driver.SendCommandRequest("nv");
            //while (!_driver.ReceiveDeviceAnswer().Contains('v')) ;
        }

        private bool onTarget = false;
        private double pulsesPerRevolution = 3000;
        private double gear = 1526.0;

        public void SetPosition(double Position)
        {
            onTarget = false;

            var destination = 2.0 * Position * pulsesPerRevolution * gear;
            _driver.SendCommandRequest(string.Format("la{0}", ((int)Math.Round(destination)).ToString(NumberFormatInfo.InvariantInfo)));
            _driver.SendCommandRequest("np");
            _driver.SendCommandRequest("m");
            while (!_driver.ReceiveDeviceAnswer().Contains('p')) ;

            onTarget = true;
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


        public bool OnTarget
        {
            get { return onTarget; }
        }
    }
}
