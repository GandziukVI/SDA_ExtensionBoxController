using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Agilent_ExtensionBox;
using IneltaMotorPotentiometer;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using System.Windows;
using System.Threading;

namespace VoltageApply
{
    public class ApplyVoltageController
    {
        public event EventHandler<ProgressChanged_EventArgs> progressChanged;
        public void onProgressChanged(object sender, ProgressChanged_EventArgs e)
        {
            var handler = progressChanged;
            if (handler != null)
                handler(sender, e);
        }

        Stopwatch accuracyStopWatch;

        BoxController boxController;

        BS350_MotorPotentiometer VdsMotorPotentiometer;
        BS350_MotorPotentiometer VgMotorPotentiometer;

        public ApplyVoltageController()
        {
            accuracyStopWatch = new Stopwatch();
            boxController = new BoxController();
        }

        public bool Init()
        {
            try
            {
                var firstIdentifyer = 0x0957;
                var secondIdentifyer = 0x1718;
                var visaBuilder = new StringBuilder();

                visaBuilder.AppendFormat("USB0::{0}::{1}::TW54334510::INSTR",
                    firstIdentifyer.ToString(NumberFormatInfo.InvariantInfo),
                    secondIdentifyer.ToString(NumberFormatInfo.InvariantInfo));

                var initSuccess = boxController.Init(visaBuilder.ToString());

                if (initSuccess == true)
                {
                    VdsMotorPotentiometer = new BS350_MotorPotentiometer(boxController, Agilent_ExtensionBox.IO.BOX_AnalogOutChannelsEnum.BOX_AOut_02);
                    VgMotorPotentiometer = new BS350_MotorPotentiometer(boxController, Agilent_ExtensionBox.IO.BOX_AnalogOutChannelsEnum.BOX_AOut_09);
                }

                return initSuccess;
            }
            catch { return false; }
        }

        public bool Close()
        {
            try
            {
                boxController.Close();
                return true;
            }
            catch { return false; }
        }

        RangesEnum setRangeForGivenVoltage(double Voltage)
        {
            RangesEnum range;

            if (Math.Abs(Voltage) < 1.25)
                range = RangesEnum.Range_1_25;
            else if (Math.Abs(Voltage) >= 1.25 && Math.Abs(Voltage) < 2.5)
                range = RangesEnum.Range_2_5;
            else if (Math.Abs(Voltage) >= 2.5 && Math.Abs(Voltage) < 5.0)
                range = RangesEnum.Range_5;
            else if (Math.Abs(Voltage) >= 5.0 && Math.Abs(Voltage) < 10)
                range = RangesEnum.Range_10;
            else
                throw new ArgumentException("The drain voltage is out of range.");

            return range;
        }

        AI_ChannelConfig[] setDCConf(double Vs, double Vm)
        {
            var Vs_Range = setRangeForGivenVoltage(Vs);
            var Vm_Range = setRangeForGivenVoltage(Vm);

            var config = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},   // Vs
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vm_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range}
            };

            return config;
        }

        AI_ChannelConfig[] setACConf(double Vs)
        {
            var Vs_Range = setRangeForGivenVoltage(Vs);

            var config = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},   // Vs
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range}
            };

            return config;
        }

        bool isDCMode = false;
        bool isACMode = false;

        int averagingNumberFast = 2;
        int averagingNumberSlow = 100;

        byte minSpeed = 0;
        byte maxSpeed = 255;

        void confAIChannelsForDC_Measurement()
        {
            if (!isDCMode)
            {
                var init_conf = setDCConf(9.99, 9.99);
                boxController.ConfigureAI_Channels(init_conf);
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberSlow);
                var real_conf = setDCConf(voltages[0], voltages[1]);
                boxController.ConfigureAI_Channels(real_conf);

                isDCMode = true;
                isACMode = false;
            }
        }

        private int estimationCollectionSize = 25;
        private LinkedList<Point> estimationList = new LinkedList<Point>();

        private void setVoltage(BS350_MotorPotentiometer motorPotentiometer, int voltNum, double voltage, double voltageDev)
        {
            voltage = Math.Abs(voltage);
            var intervalCoarse = voltage * (1.0 - 1.0 / Math.Sqrt(2.0));

            double drainVoltageCurr = 0.0,
                drainVoltagePrev = 0.0,
                factorCoarse = 0.0;

            confAIChannelsForDC_Measurement();

            accuracyStopWatch.Start();

            while (true)
            {
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

                drainVoltageCurr = Math.Abs(voltages[voltNum]);

                var lowerVal = Math.Min(drainVoltageCurr, voltage);
                var higherVal = Math.Max(drainVoltageCurr, voltage);

                onProgressChanged(this, new ProgressChanged_EventArgs((int)(lowerVal / higherVal * 100.0)));

                var speed = minSpeed;
                try
                {
                    factorCoarse = (1.0 - Math.Tanh(-1.0 * Math.Abs(voltage - drainVoltageCurr) / intervalCoarse * Math.PI + Math.PI)) / 2.0;
                    speed = (byte)(minSpeed + (maxSpeed - minSpeed) * factorCoarse);
                }
                catch { speed = minSpeed; }

                if ((drainVoltageCurr >= Math.Abs(voltage - voltageDev)) &&
                    (drainVoltageCurr <= Math.Abs(voltage + voltageDev)))
                {
                    motorPotentiometer.StopMotion();
                    accuracyStopWatch.Stop();
                    break;
                }
                else
                {
                    // Implementing voltage set with enchansed accuracy
                    if (estimationList.Count > estimationCollectionSize)
                        estimationList.RemoveFirst();

                    estimationList.AddLast(new Point(accuracyStopWatch.ElapsedMilliseconds, Math.Abs(drainVoltagePrev - drainVoltageCurr)));

                    var timeAVG = estimationList.Select(val => val.X).Average();
                    var voltAVG = estimationList.Select(val => val.Y).Average();

                    var voltPerMilisecond = timeAVG != 0 ? voltAVG / timeAVG : voltAVG;

                    var stepTime = (int)(Math.Abs(voltage - drainVoltageCurr) / voltPerMilisecond);

                    if (drainVoltageCurr > voltage)
                    {
                        if (voltageDev >= 0.006 || drainVoltageCurr - voltage > 2.0 * voltageDev)
                        {
                            averagingNumberFast = 2;
                            motorPotentiometer.StartMotion(speed, MotionDirection.cw);
                        }
                        else
                        {
                            motorPotentiometer.StartMotion(speed, MotionDirection.cw);
                            Thread.Sleep(stepTime);
                            motorPotentiometer.StopMotion();
                            averagingNumberFast = 25;
                        }
                    }
                    else
                    {
                        if (voltageDev >= 0.006 || voltage - drainVoltageCurr > 2.0 * voltageDev)
                        {
                            averagingNumberFast = 2;
                            motorPotentiometer.StartMotion(speed, MotionDirection.ccw);
                        }
                        else
                        {
                            motorPotentiometer.StartMotion(speed, MotionDirection.ccw);
                            Thread.Sleep(stepTime);
                            motorPotentiometer.StopMotion();
                            averagingNumberFast = 25;
                        }
                    }

                    accuracyStopWatch.Restart();
                }

                drainVoltagePrev = drainVoltageCurr;
            }

            motorPotentiometer.StopMotion();
        }

        public void SetDrainSourceVoltage(double voltage, double voltageDev)
        {
            setVoltage(VdsMotorPotentiometer, 0, voltage, voltageDev);
        }

        public void SetGateVoltage(double voltage, double voltageDev)
        {
            setVoltage(VgMotorPotentiometer, 2, voltage, voltageDev);
        }
    }
}
