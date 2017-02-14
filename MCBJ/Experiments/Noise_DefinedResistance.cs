using ExperimentController;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ChannelSwitchLibrary;

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.Threading;

namespace MCBJ.Experiments
{
    public class Noise_DefinedResistance : ExperimentBase
    {
        BoxController boxController;
        ChannelSwitch channelSwitch;

        Stopwatch stabilityStopwatch;

        bool connectionEstablished = false;
        bool decreasing = false;

        public Noise_DefinedResistance()
            : base()
        {
            boxController = new BoxController();
            channelSwitch = new ChannelSwitch();

            channelSwitch.ConnectionEstablished += channelSwitch_ConnectionEstablished;
            channelSwitch.ConnectionLost += channelSwitch_ConnectionLost;

            stabilityStopwatch = new Stopwatch();
        }

        void channelSwitch_ConnectionLost(object sender, EventArgs e)
        {
            connectionEstablished = false;
        }

        void channelSwitch_ConnectionEstablished(object sender, EventArgs e)
        {
            connectionEstablished = true;
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

        int averagingNumberFast = 1;
        int averagingNumberSlow = 100;

        short stopSpeed = 0;
        short minSpeed = 128;
        short maxSpeed = 255;

        short channelIdentifyer = 1;

        void confAIChannelsForDC_Measurement()
        {
            var init_conf = setDCConf(9.99, 9.99);
            boxController.ConfigureAI_Channels(init_conf);
            Thread.Sleep(1000);
            var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberSlow);
            var real_conf = setDCConf(voltages[0], voltages[1]);
            boxController.ConfigureAI_Channels(real_conf);
        }

        int inRangeCounter;
        int outsiderCounter;

        void setDrainVoltage(double drainVoltage, double voltageDev, double stabilizationTime)
        {
            drainVoltage = Math.Abs(drainVoltage);

            confAIChannelsForDC_Measurement();
            channelSwitch.Initialize();

            while (!connectionEstablished) ;

            if (channelSwitch.Initialized == true)
            {
                while (true)
                {
                    var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

                    var drainVoltageCurr = Math.Abs(voltages[0]);

                    var speed = minSpeed;
                    try
                    {
                        var k = (drainVoltageCurr >= 1.0) ? Math.Log10(drainVoltageCurr) : drainVoltageCurr;
                        var x = Math.Abs(drainVoltageCurr - drainVoltage);

                        var factor = (1.0 - Math.Tanh((-1.0 * x + Math.PI / k) * k)) / 2.0;

                        speed = (short)(minSpeed + (maxSpeed - minSpeed) * factor);
                    }
                    catch { speed = minSpeed; }

                    if ((drainVoltageCurr >= drainVoltage - (drainVoltage * voltageDev / 100.0)) &&
                        (drainVoltageCurr <= drainVoltage + (drainVoltage * voltageDev / 100.0)))
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
                            //if (!decreasing)
                                channelSwitch.MoveMotor(channelIdentifyer, speed);

                            //decreasing = true;
                        }
                        else
                        {
                            //if (decreasing)
                                channelSwitch.MoveMotor(channelIdentifyer, (short)(-1.0 * speed));

                            //decreasing = false;
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

                channelSwitch.Exit();
            }
            else
                throw new Exception("Can't connect to the channel switch.");
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

        public override void ToDo(object Arg)
        {
            var firstIdentifyer = 0x0957;
            var secondIdentifyer = 0x1718;
            var visaBuilder = new StringBuilder();

            visaBuilder.AppendFormat("USB0::{0}::{1}::TW54334510::INSTR", firstIdentifyer.ToString(NumberFormatInfo.InvariantInfo), secondIdentifyer.ToString(NumberFormatInfo.InvariantInfo));

            var boxConnection = boxController.Init(visaBuilder.ToString());

            if (boxConnection == true)
                setDrainVoltage(0.05, 1.0, 15);
            else
                throw new Exception("Cannot establisch the connection to the box.");
        }
    }
}
