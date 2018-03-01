using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;

namespace IneltaMotorPotentiometer
{
    public class BS350_MotorPotentiometer
    {
        private BoxController _boxController;
        private BOX_AnalogOutChannelsEnum _controlChannel;

        public BS350_MotorPotentiometer(BoxController boxController, BOX_AnalogOutChannelsEnum controlChannel)
        {
            _boxController = boxController;
            _controlChannel = controlChannel;
        }

        private readonly double stopMotorVoltage = 0.0;
        private readonly double minMotorVoltage = 0.7;
        private readonly double maxMotorVoltage = 6.0;

        public void StartMotion(byte speed, MotionDirection direction)
        {
            var voltageToSet = minMotorVoltage + (maxMotorVoltage - minMotorVoltage) / 255.0 * speed;

            switch (direction)
            {
                case MotionDirection.cw:
                    _boxController.AO_ChannelCollection.ApplyVoltageToChannel(_controlChannel, voltageToSet);
                    break;
                case MotionDirection.ccw:
                    _boxController.AO_ChannelCollection.ApplyVoltageToChannel(_controlChannel, -voltageToSet);
                    break;
            }
        }

        public void StopMotion()
        {
            _boxController.AO_ChannelCollection.ApplyVoltageToChannel(_controlChannel, stopMotorVoltage);
        }
    }
}
