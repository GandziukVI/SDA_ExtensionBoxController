using ExperimentController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Globalization;

using ChannelSwitchLibrary;

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using MotionManager;

namespace MCBJ.Experiments
{
    public class Noise_DefinedResistance : ExperimentBase
    {
        BoxController boxController;
        ChannelSwitch channelSwitch;

        IMotionController1D motor;

        Stopwatch stabilityStopwatch;

        bool connectionEstablished = false;

        private readonly double ConductanceQuantum = 0.0000774809173;

        public Noise_DefinedResistance(string SDA_ConnectionString, IMotionController1D Motor)
            : base()
        {
            boxController = new BoxController();

            var boxInit = boxController.Init(SDA_ConnectionString);

            if (!boxInit)
                throw new Exception("Cannot connect the box.");

            channelSwitch = new ChannelSwitch();

            channelSwitch.ConnectionEstablished += channelSwitch_ConnectionEstablished;
            channelSwitch.ConnectionLost += channelSwitch_ConnectionLost;

            channelSwitch.Initialize();
            while (!connectionEstablished) ;

            motor = Motor;

            stabilityStopwatch = new Stopwatch();
        }

        void channelSwitch_ConnectionEstablished(object sender, EventArgs e)
        {
            connectionEstablished = true;
        }

        void channelSwitch_ConnectionLost(object sender, EventArgs e)
        {
            connectionEstablished = false;
            try
            {
                channelSwitch.Initialize();
                while (!connectionEstablished) ;
            }
            catch
            {
                throw new Exception("Couldn't reestablisch the connection with channel switch.");
            }
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
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range}
            };

            return config;
        }

        int averagingNumberFast = 2;
        int averagingNumberSlow = 100;

        short stopSpeed = 0;
        short minSpeed = 10;
        short maxSpeed = 255;

        short channelIdentifyer = 1;

        void confAIChannelsForDC_Measurement()
        {
            var init_conf = setDCConf(9.99, 9.99);
            boxController.ConfigureAI_Channels(init_conf);
            var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberSlow);
            var real_conf = setDCConf(voltages[0], voltages[1]);
            boxController.ConfigureAI_Channels(real_conf);
        }

        void confAIChannelsForAC_Measurement()
        {
            var init_conf = setACConf(4.99);
            boxController.ConfigureAI_Channels(init_conf);

            // Erasing the data queue

            Point[] temp;
            while (!boxController.AI_ChannelCollection[0].ChannelData.IsEmpty)
                boxController.AI_ChannelCollection[0].ChannelData.TryDequeue(out temp);

            // Acquiring single shot with AC data

            boxController.AcquireSingleShot(1000);
            var maxAcquiredVoltage = boxController.AI_ChannelCollection[0].ChannelData.Last().Max(p => p.Y);

            // Configuring the channels to measure noise

            var real_conf = setACConf(maxAcquiredVoltage);
            boxController.ConfigureAI_Channels(real_conf);
        }

        int inRangeCounter;
        int outsiderCounter;

        void setDrainVoltage(double drainVoltage, double voltageDev, double stabilizationTime)
        {
            drainVoltage = Math.Abs(drainVoltage);
            var interval = drainVoltage * (1.0 - 1.0 / Math.Sqrt(2.0));

            double drainVoltageCurr;

            confAIChannelsForDC_Measurement();

            //while (!connectionEstablished) ;

            while (true)
            {
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

                drainVoltageCurr = Math.Abs(voltages[0]);

                var speed = minSpeed;
                try
                {
                    var factor = (1.0 - Math.Tanh(-1.0 * Math.Abs(drainVoltage - drainVoltageCurr) / interval * Math.PI + Math.PI)) / 2.0;

                    speed = (short)(minSpeed + (maxSpeed - minSpeed) * factor);
                }
                catch { speed = minSpeed; }

                if ((drainVoltageCurr >= Math.Abs(drainVoltage - voltageDev)) &&
                    (drainVoltageCurr <= Math.Abs(drainVoltage + voltageDev)))
                {
                    channelSwitch.MoveMotor(channelIdentifyer, stopSpeed);

                    if (!stabilityStopwatch.IsRunning)
                    {
                        inRangeCounter = 0;
                        outsiderCounter = 0;

                        stabilityStopwatch.Start();

                        onStatusChanged(new StatusEventArgs("Stabilizing the specified resistance / conductance value."));
                    }

                    ++inRangeCounter;
                }
                else
                {
                    if (drainVoltageCurr > drainVoltage)
                    {
                        channelSwitch.MoveMotor(channelIdentifyer, speed);
                    }
                    else
                    {
                        channelSwitch.MoveMotor(channelIdentifyer, (short)(-1.0 * speed));
                    }


                    if (stabilityStopwatch.IsRunning == true)
                        ++outsiderCounter;
                }

                if (stabilityStopwatch.IsRunning)
                {
                    if (stabilityStopwatch.ElapsedMilliseconds > 0)
                        onProgressChanged(new ProgressEventArgs((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 / stabilizationTime * 100));

                    if ((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 >= stabilizationTime)
                    {
                        var divider = outsiderCounter > 0 ? (double)outsiderCounter : 1.0;
                        if (Math.Log10((double)inRangeCounter / divider) >= 1.0)
                        {
                            stabilityStopwatch.Stop();
                            break;
                        }
                        else
                        {
                            inRangeCounter = 0;
                            outsiderCounter = 0;

                            stabilityStopwatch.Restart();
                        }
                    }
                }
            }

            channelSwitch.MoveMotor(channelIdentifyer, 0);
        }

        double measureResistance(double LoadResistance = 5000.0, int nAveraging = 100, double SetVoltage = 0.02, double voltageTreshold = 0.05)
        {
            confAIChannelsForDC_Measurement();
            var voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);

            if (voltages[0] > voltageTreshold)
            {
                if (motor.IsEnabled == true)
                    motor.Enabled = false;

                setDrainVoltage(SetVoltage, 0.005, 5);
            }

            voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);

            var Vs = voltages[0];
            var Vm = voltages[1];

            var Is = (Vm - Vs) / LoadResistance;

            var res = Is != 0 ? Vs / Is : 0.0;

            return res;
        }

        void setJunctionResistance(Noise_DefinedResistanceInfo Arg)
        {
            var settings = Arg;

            var setVolt = settings.ScanningVoltage;
            var setCond = settings.SetConductance;
            var condDev = settings.Deviation;
            var stabilizationTime = settings.StabilizationTime;

            var minSpeed = settings.MinSpeed;
            var maxSpeed = settings.MaxSpeed;

            var minPos = settings.MotorMinPos;
            var maxPos = settings.MotorMaxPos;

            var nAverages = 100;
            var loadResistance = 5000.0;
            var voltageTreshold = 0.05;

            var fileName = string.Join("\\", settings.FilePath, settings.SaveFileName);

            var inRangeCounter = 0;
            var outsiderCounter = 0;

            onStatusChanged(new StatusEventArgs(string.Format("Reaching resistance of {0}.", settings.SetResistance.ToString(NumberFormatInfo.InvariantInfo))));
            onProgressChanged(new ProgressEventArgs(0.0));

            motor.Enabled = true;
            motor.Velosity = maxSpeed;

            onStatusChanged(new StatusEventArgs("Reaching the specified resistance / conductance value."));

            var interval = setCond * (1.0 - 1.0 / Math.Sqrt(2.0));

            // Resistance stabilization

            while (true)
            {
                if (!IsRunning)
                    break;

                var scaledConductance = (1.0 / measureResistance(loadResistance, nAverages, setVolt, voltageTreshold)) / ConductanceQuantum;

                var speed = minSpeed;
                try
                {
                    var factor = (1.0 - Math.Tanh(-1.0 * Math.Abs(scaledConductance - setCond) / interval * Math.PI + Math.PI)) / 2.0;
                    speed = minSpeed + (maxSpeed - minSpeed) * factor;
                }
                catch { speed = minSpeed; }

                motor.Velosity = speed;

                if ((scaledConductance >= setCond - (setCond * condDev / 100.0)) &&
                    (scaledConductance <= setCond + (setCond * condDev / 100.0)))
                {
                    if (motor.IsEnabled == true)
                        motor.Enabled = false;

                    if (!stabilityStopwatch.IsRunning)
                    {
                        inRangeCounter = 0;
                        outsiderCounter = 0;

                        stabilityStopwatch.Start();

                        onStatusChanged(new StatusEventArgs("Stabilizing the specified resistance / conductance value."));
                    }

                    ++inRangeCounter;
                }
                else
                {
                    if (motor.IsEnabled == false)
                        motor.Enabled = true;

                    if (scaledConductance > setCond)
                        motor.PositionAsync = maxPos;
                    else
                        motor.PositionAsync = minPos;

                    if (stabilityStopwatch.IsRunning == true)
                        ++outsiderCounter;
                }

                if (stabilityStopwatch.IsRunning)
                {
                    if (stabilityStopwatch.ElapsedMilliseconds > 0)
                        onProgressChanged(new ProgressEventArgs((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 / stabilizationTime * 100));

                    if ((double)stabilityStopwatch.ElapsedMilliseconds / 1000.0 >= stabilizationTime)
                    {
                        var divider = outsiderCounter > 0 ? (double)outsiderCounter : 1.0;
                        if (Math.Log10((double)inRangeCounter / divider) >= 1.0)
                        {
                            stabilityStopwatch.Stop();
                            break;
                        }
                        else
                        {
                            inRangeCounter = 0;
                            outsiderCounter = 0;

                            stabilityStopwatch.Restart();
                        }
                    }
                }
            }

            // Noise spectra measurement
        }

        public override void ToDo(object Arg)
        {
            setDrainVoltage(0.02, 0.003, 5);
            setDrainVoltage(0.05, 0.003, 5);
            setDrainVoltage(0.01, 0.003, 5);
            
            Dispose();
        }

        public override void Dispose()
        {
            if (IsRunning)
            {
                if (channelSwitch != null)
                    if (channelSwitch.Initialized == true)
                        channelSwitch.Exit();

                if (motor != null)
                    motor.Dispose();
            }

            base.Dispose();
        }
    }
}
