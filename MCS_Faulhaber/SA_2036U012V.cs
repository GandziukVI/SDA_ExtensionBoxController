using DeviceIO;
using MotionManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCS_Faulhaber
{
    public class SA_2036U012V : MotionController1DBase
    {
        public SA_2036U012V() { }
        public SA_2036U012V(IDeviceIO Driver)
        {
            Initialize(Driver);
        }

        private double gear = 1526.0;
        public double Gear
        {
            get { return gear; }
            set { gear = value; }
        }

        public readonly double StepsPerRevolution = 3000.0;
        public readonly double MilimetersPerRevolution = 0.5;

        public override void SetPosition(double Position)
        {
            if (driver != null)
            {
                // Conversion of the position in mm to motor units
                var motorPosition = (int)(Gear * StepsPerRevolution * Position / MilimetersPerRevolution);
                // Loading the position to the controller and starting the motion
                driver.RequestQuery(string.Format("la{0}", Convert.ToString(motorPosition, NumberFormatInfo.InvariantInfo)));
                driver.RequestQuery("np");
                driver.RequestQuery("m");
                while (!driver.ReceiveDeviceAnswer().Contains('p')) ;
            }
            else
                throw new Exception("The connection failed! Check the device IO driver.");
        }

        public override void SetPositionAsync(double Position)
        {
            if (driver != null)
            {
                // Conversion of the position in mm to motor units<wpfTool:DoubleUpDown Grid.Row="14" Grid.Column="1" Value="{Binding MotorMaxPos, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"/>velocdcdc
                var motorPosition = (int)(Gear * StepsPerRevolution * Position / MilimetersPerRevolution);
                // Loading the position to the controller and starting the motion
                driver.RequestQuery(string.Format("la{0}", Convert.ToString(motorPosition, NumberFormatInfo.InvariantInfo)));
                driver.RequestQuery("np");
                driver.RequestQuery("m");
            }
            else
                throw new Exception("The connection failed! Check the device IO driver.");
        }

        public override double GetPosition()
        {
            try
            {
                var controllerResponce = driver.RequestQuery("pos").TrimEnd("\r\r".ToCharArray());

                int motorPosition;
                var conversionSuccess = int.TryParse(controllerResponce, out motorPosition);
                if (conversionSuccess)
                    return (double)motorPosition / Gear / StepsPerRevolution * MilimetersPerRevolution;
                else
                    throw new Exception("Error while reading the motor position!");
            }
            catch { return 0.0; }
        }

        private double currentVelosity;
        public override void SetVelosity(double Velosity)
        {
            var speedRPM = (int)(Velosity / (1.0 / gear * MilimetersPerRevolution));
            
            if (speedRPM < 150)
                speedRPM = 150;
            else if (speedRPM > 15000)
                speedRPM = 15000;

            currentVelosity = speedRPM;

            driver.RequestQuery(string.Format("SP{0}", speedRPM));
        }

        public override double GetVelosity()
        {
            if (!double.IsNaN(currentVelosity))
                return currentVelosity * MilimetersPerRevolution;
            else
                return 0.0;
        }

        public override bool Enable()
        {
            var enableBase = base.Enable();
            driver.RequestQuery("en");
            return enableBase;
        }

        public override bool Disable()
        {
            var disableBase = base.Disable();
            driver.RequestQuery("di");
            return disableBase;
        }
    }
}
