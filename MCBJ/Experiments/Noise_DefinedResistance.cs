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

namespace MCBJ.Experiments
{
    public class Noise_DefinedResistance : ExperimentBase
    {
        BoxController boxController;
        ChannelSwitch channelSwitch;

        Stopwatch stabilityStopwatch;

        public Noise_DefinedResistance()
            : base()
        {
            boxController = new BoxController();
            channelSwitch = new ChannelSwitch();

            stabilityStopwatch = new Stopwatch();
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

        int averagingNumberFast = 5;
        int averagingNumberSlow = 100;

        short stopSpeed = 0;
        short minSpeed = 25;
        short maxSpeed = 255;

        void confAIChannelsForDC_Measurement()
        {
            var init_conf = setDCConf(9.99, 9.99);
            boxController.ConfigureAI_Channels(init_conf);

            var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberSlow);
            var real_conf = setDCConf(voltages[0], voltages[1]);
            boxController.ConfigureAI_Channels(real_conf);
        }

        int inRangeCounter;
        int outsiderCounter;

        void setDrainVoltage(double drainVoltage, double voltageDev, double stabilizationTime)
        {
            channelSwitch.Initialize();

            if (channelSwitch.Initialized == true)
            {
                confAIChannelsForDC_Measurement();

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
                        channelSwitch.MoveMotor(0, stopSpeed);

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
                            channelSwitch.MoveMotor(0, 255);
                        else
                            channelSwitch.MoveMotor(0, -255);

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
            // Needs to be reimplemented

            var init_conf = setDCConf(9.99, 9.99);
            boxController.ConfigureAI_Channels(init_conf);

            var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberSlow);
            var real_conf = setDCConf(voltages[0], voltages[1]);
            boxController.ConfigureAI_Channels(real_conf);
        }

        public override void ToDo(object Arg)
        {

        }
    }
}
