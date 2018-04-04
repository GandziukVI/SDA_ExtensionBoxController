using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ExperimentController;
using Agilent_ExtensionBox;
using IneltaMotorPotentiometer;
using SpectralAnalysis;
using Agilent_ExtensionBox.IO;
using System.Threading;
using Agilent_ExtensionBox.Internal;
using System.Globalization;
using FET_Characterization.Experiments.DataHandling;
using System.IO.MemoryMappedFiles;
using System.Security.AccessControl;
using System.Runtime.ExceptionServices;
using System.Windows.Threading;

namespace FET_Characterization.Experiments
{
    class FET_Noise_Experiment : ExperimentBase
    {
        BoxController boxController;

        BS350_MotorPotentiometer VdsMotorPotentiometer;
        BS350_MotorPotentiometer VgMotorPotentiometer;

        Stopwatch stabilityStopwatch;
        Stopwatch accuracyStopWatch;

        bool isDCMode = false;
        bool isACMode = false;

        FileStream TT_Stream;
        AutoResetEvent TT_AutoresetEvent = new AutoResetEvent(false);

        FETUI.NoiseFETSettingsControlModel experimentSettings;

        Point[] amplifierNoise;
        Point[] frequencyResponse;

        TwoPartsFFT twoPartsFFT;

        public FET_Noise_Experiment(string SDA_ConnectionString, Point[] AmplifierNoise, Point[] FrequencyResponse)
            : base()
        {
            amplifierNoise = AmplifierNoise;
            frequencyResponse = FrequencyResponse;
            twoPartsFFT = new TwoPartsFFT();

            stabilityStopwatch = new Stopwatch();
            accuracyStopWatch = new Stopwatch();

            DataArrived += FET_Noise_Experiment_DataArrived;
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

        AI_ChannelConfig[] setDCConf(double Vs, double Vm, double Vg)
        {
            var Vs_Range = setRangeForGivenVoltage(Vs);
            var Vm_Range = setRangeForGivenVoltage(Vm);
            var Vg_Range = setRangeForGivenVoltage(Vg);

            var config = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},  
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vm_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vg_Range},   // Vg
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range}    // Vs
            };

            return config;
        }

        AI_ChannelConfig[] setACConf(double Vs)
        {
            var Vs_Range = setRangeForGivenVoltage(Vs);

            var config = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},    // Vs
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range}
            };

            return config;
        }

        double[] confAIChannelsForDC_Measurement()
        {
            try
            {
                var init_conf = setDCConf(9.99, 9.99, 9.99);
                boxController.ConfigureAI_Channels(init_conf);
                var voltages = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);
                var real_conf = setDCConf(voltages[3], voltages[1], voltages[2]);
                boxController.ConfigureAI_Channels(real_conf);

                isDCMode = true;
                isACMode = false;

                return voltages;
            }
            catch
            {
                throw;
            }
        }

        void confAIChannelsForDC_Measurement(double Vs, double Vm, double Vg)
        {
            if (!isDCMode)
            {
                var voltages = new double[] { Vs, Vm, Vg };
                var real_conf = setDCConf(voltages[3], voltages[1], voltages[2]);
                boxController.ConfigureAI_Channels(real_conf);

                isDCMode = true;
                isACMode = false;
            }
        }

        bool confAIChannelsForAC_Measurement()
        {
            var result = false;

            if (!isACMode)
            {
                var init_conf = setACConf(9.99);
                boxController.ConfigureAI_Channels(init_conf);

                // Erasing the data queue

                Point[] temp;
                while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.IsEmpty)
                    boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out temp);

                // Acquiring single shot with AC data
                int i = 0;
                while (true)
                {
                    result = boxController.AcquireSingleShot(1000);
                    if (result == true)
                        break;
                    ++i;
                    if (i >= 10)
                        throw new Exception();
                }

                var maxAcquiredVoltage = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.Last().Max(p => p.Y);

                // Configuring the channels to measure noise

                var real_conf = setACConf(maxAcquiredVoltage);
                boxController.ConfigureAI_Channels(real_conf);

                while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.IsEmpty)
                    boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out temp);

                isACMode = true;
                isDCMode = false;
            }

            return result;
        }

        byte minSpeed = 10;
        byte maxSpeed = 255;

        int averagingNumberFast;

        int estimationCollectionSize = 25;
        LinkedList<Point> estimationList = new LinkedList<Point>();

        private void preciseSetVoltage(BS350_MotorPotentiometer motorPotentiometer, int voltNumber, double voltage, double voltageDeviation, int averagingNumber)
        {
            var voltSign = voltage < 0 ? -1.0 : 1.0;
            var voltSet = Math.Abs(voltage);

            Func<double, double, double> intervalCoarse = (varVoltVal, varVoltDev) =>
            {
                var absVoltVal = Math.Abs(varVoltVal);
                var absDeviationVal = Math.Abs(varVoltDev);

                if (absVoltVal > absDeviationVal)
                    return voltSign * Math.Abs(varVoltVal - varVoltDev);
                else
                    return voltSign * varVoltDev;
            };

            Func<double, double, double, double> multFactor = (varVoltDest, varVoltCurr, varVoltDev) =>
            {
                var absVoltVal = Math.Abs(varVoltDest);
                var absDeviationVal = Math.Abs(varVoltDev);

                var divider = 1.0;

                if (absVoltVal > absDeviationVal)
                    divider = voltSet - intervalCoarse(voltSet, varVoltDev);
                else
                    divider = Math.Abs(voltSet + intervalCoarse(voltSet, varVoltDev));

                return (1.0 - Math.Tanh(-1.0 * Math.Abs(voltSet - varVoltCurr) / divider * Math.PI + Math.PI)) / 2.0;
            };

            var isFirstMeasurement = true;
            var voltageFirstReading = 0.0;

            Func<double, bool> setFirstVoltageReading = (reading) =>
            {
                if (isFirstMeasurement)
                {
                    isFirstMeasurement = false;
                    voltageFirstReading = reading;
                    return true;
                }
                else
                    return false;
            };

            var resetFirstReading = new Action(() => { isFirstMeasurement = true; });

            Func<double, double, double> getDifferenceSign = (value1, value2) =>
            {
                if (value1 - value2 < 0)
                    return -1.0;
                else
                    return 1.0;
            };

            byte PotentiometerSpeed = 0;
            byte PotentiometerMinSpeed = 0;
            byte PotentiometerMaxSpeed = 255;

            Func<Func<double, byte>, bool> seekVoltFunc = (speedFunc) =>
            {
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumber);
                var currentVoltage = voltages[voltNumber];
                setFirstVoltageReading(currentVoltage);
                var firstVoltageReadingSign = getDifferenceSign(voltage, voltageFirstReading);

                while (true)
                {

                    voltages = boxController.VoltageMeasurement_AllChannels(averagingNumber);
                    currentVoltage = voltages[voltNumber];

                    var currentVoltageReadingSign = getDifferenceSign(voltage, currentVoltage);
                    PotentiometerSpeed = speedFunc(currentVoltage);

                    onStatusChanged(new StatusEventArgs(string.Format("Vs = {0} (=> {1} V), Vm = {2}, Speed = {3}", voltages[voltNumber].ToString("0.0000", NumberFormatInfo.InvariantInfo), voltage.ToString("0.0000", NumberFormatInfo.InvariantInfo), voltages[1].ToString("0.0000", NumberFormatInfo.InvariantInfo), PotentiometerSpeed)));

                    if (Math.Abs(voltage - currentVoltage) <= Math.Abs(voltageDeviation))
                    {
                        motorPotentiometer.StopMotion();
                        resetFirstReading.Invoke();
                        return true;
                    }

                    if (firstVoltageReadingSign != currentVoltageReadingSign)
                    {
                        motorPotentiometer.StopMotion();
                        break;
                    }
                    else
                    {
                        if (Math.Abs(currentVoltage) > Math.Abs(voltage))
                            motorPotentiometer.StartMotion(PotentiometerSpeed, MotionDirection.cw);
                        else
                            motorPotentiometer.StartMotion(PotentiometerSpeed, MotionDirection.ccw);
                    }
                }

                resetFirstReading.Invoke();

                return false;
            };

            Func<bool> superCoarseStep = () =>
            {
                Func<double, byte> currSpeedFunc = (currentVoltage) =>
                {
                    return PotentiometerMaxSpeed;
                };

                seekVoltFunc(currSpeedFunc);

                return true;
            };

            Func<bool> coarseStep = () =>
            {
                Func<double, byte> currSpeedFunction = (currentVoltage) =>
                {
                    return (byte)(PotentiometerMinSpeed + (PotentiometerMaxSpeed - PotentiometerMinSpeed) * multFactor(voltage, currentVoltage, voltageDeviation));
                };

                seekVoltFunc(currSpeedFunction);

                return true;
            };

            Func<bool> fineStep = () =>
            {
                Func<double, byte> currSpeedFunc = (currentVoltage) =>
                {
                    return PotentiometerMinSpeed;
                };

                return seekVoltFunc(currSpeedFunc);
            };

            Func<bool> checkStep = () =>
            {
                var stopwatch = new Stopwatch();

                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumber);
                var currentVoltage = voltages[voltNumber];

                stopwatch.Start();
                var estimationCollection = new List<double>();
                do
                {
                    fineStep();
                    stopwatch.Reset();
                    var counter = 0;
                    estimationCollection.Clear();

                    while (stopwatch.ElapsedMilliseconds <= 500 && counter < 25)
                    {
                        voltages = boxController.VoltageMeasurement_AllChannels(averagingNumber);
                        currentVoltage = voltages[voltNumber];
                        estimationCollection.Add(currentVoltage);

                        ++counter;
                    }
                } while (!(Math.Abs(voltage - estimationCollection.Average()) <= Math.Abs(voltageDeviation)));

                stopwatch.Stop();

                return true;
            };

            var setter = new AdvancedValueSetter();

            setter.RegisterStep(ref superCoarseStep);
            setter.RegisterStep(ref coarseStep);
            setter.RegisterStep(ref fineStep);
            setter.RegisterStep(ref checkStep);

            setter.SetValue();
        }

        //void setVoltage(BS350_MotorPotentiometer motorPotentiometer, int voltNumber, double voltage, double voltageDeviation)
        //{
        //    var voltSign = voltage < 0 ? -1.0 : 1.0;
        //    var voltSet = Math.Abs(voltage);

        //    Func<double, double, double> intervalCoarse = (varVoltVal, varVoltDev) =>
        //        {
        //            var absVoltVal = Math.Abs(varVoltVal);
        //            var absDeviationVal = Math.Abs(varVoltDev);

        //            if (absVoltVal > absDeviationVal)
        //                return voltSign * Math.Abs(varVoltVal - varVoltDev);
        //            else
        //                return voltSign * varVoltDev;
        //        };

        //    Func<double, double, double, double> multFactor = (varVoltDest, varVoltCurr, varVoltDev) =>
        //        {
        //            var absVoltVal = Math.Abs(varVoltDest);
        //            var absDeviationVal = Math.Abs(varVoltDev);

        //            var divider = 1.0;

        //            if (absVoltVal > absDeviationVal)
        //                divider = voltSet - intervalCoarse(voltSet, varVoltDev);
        //            else
        //                divider = Math.Abs(voltSet + intervalCoarse(voltSet, varVoltDev));

        //            return (1.0 - Math.Tanh(-1.0 * Math.Abs(voltSet - varVoltCurr) / divider * Math.PI + Math.PI)) / 2.0;
        //        };

        //    var voltageCurr = 0.0;
        //    var voltagePrev = 0.0;

        //    averagingNumberFast = experimentSettings.NAveragesFast;

        //    var isFirstMeasurement = true;
        //    var firstVoltageReading = 0.0;

        //    var proximityFactor = 5.0;
        //    Func<double, double, bool> isInCloseProximity = (voltDest, voltCurr) =>
        //        {
        //            return Math.Abs(voltDest - voltCurr) <= proximityFactor * voltageDeviation;
        //        };

        //    var currIter = 0;
        //    Nullable<bool> directionChangeIndicator = null;

        //    accuracyStopWatch.Start();

        //    while (true)
        //    {
        //        var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

        //        voltageCurr = Math.Abs(voltages[voltNumber]);

        //        if (isFirstMeasurement)
        //        {
        //            firstVoltageReading = voltageCurr;
        //            isFirstMeasurement = false;
        //        }

        //        var speedCorrectionFactor = 1.0;
        //        var speed = (byte)(minSpeed + (maxSpeed - minSpeed) * multFactor(voltSet, voltageCurr, speedCorrectionFactor * voltageDeviation));
                
        //        Func<bool> voltSetSuccess = () =>
        //            {
        //                return (voltageCurr >= Math.Abs(voltSet - voltageDeviation)) && (voltageCurr <= Math.Abs(voltSet + voltageDeviation));
        //            };

        //        onStatusChanged(new StatusEventArgs(string.Format("Voltage value to set -> {0}, current voltage -> {1}, speed -> {2}", (voltSet * voltSign).ToString("0.0000", NumberFormatInfo.InvariantInfo), (voltageCurr * voltSign).ToString("0.0000", NumberFormatInfo.InvariantInfo), speed.ToString(NumberFormatInfo.InvariantInfo))));
        //        onProgressChanged(new ProgressEventArgs(100.0 * (1.0 - Math.Abs(voltSet - voltageCurr) / Math.Abs(voltSet - firstVoltageReading))));

        //        if (voltSetSuccess())
        //        {
        //            motorPotentiometer.StopMotion();
        //            accuracyStopWatch.Stop();
        //            break;
        //        }
        //        else
        //        {
        //            if (isInCloseProximity(voltSet, voltageCurr) && currIter != 0)
        //            {
        //                estimationList.Clear();

        //                while (isInCloseProximity(voltSet, voltageCurr) || estimationList.Count <= 10)
        //                {
        //                    accuracyStopWatch.Restart();

        //                    voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);
        //                    voltageCurr = Math.Abs(voltages[voltNumber]);
        //                    estimationList.AddLast(new Point(accuracyStopWatch.ElapsedMilliseconds, voltagePrev - voltageCurr));
        //                    voltagePrev = voltageCurr;

        //                    if (estimationList.Count > estimationCollectionSize)
        //                        estimationList.RemoveFirst();
        //                }

        //                motorPotentiometer.StopMotion();

        //                var condition = voltageCurr > voltSet;

        //                var condEstimationList = condition ? estimationList.Where(x => x.Y < 0) :
        //                    estimationList.Where(x => x.Y >= 0);

        //                if (condEstimationList.Count() > 0)
        //                {
        //                    var timeAVG = condEstimationList.Select(x => x.X).Average();
        //                    var voltAVG = condEstimationList.Select(x => x.Y).Average();

        //                    var voltPerMilisecond = timeAVG != 0 ? voltAVG / timeAVG : voltAVG;
        //                    var stepTime = (int)(Math.Abs((voltSet - voltageCurr) / voltPerMilisecond));

        //                    if (voltageCurr > voltSet)
        //                    {
        //                        motorPotentiometer.StartMotion(speed, MotionDirection.cw);
        //                        Thread.Sleep(stepTime);
        //                        motorPotentiometer.StopMotion();
        //                    }
        //                    else
        //                    {
        //                        motorPotentiometer.StartMotion(speed, MotionDirection.ccw);
        //                        Thread.Sleep(stepTime);
        //                        motorPotentiometer.StopMotion();
        //                    }
        //                }

        //                if (voltSetSuccess())
        //                {
        //                    motorPotentiometer.StopMotion();
        //                    accuracyStopWatch.Stop();
        //                    break;
        //                }
        //            }
                    
        //            if (voltageCurr > voltSet)
        //            {
        //                motorPotentiometer.StartMotion(speed, MotionDirection.cw);

        //                if (directionChangeIndicator == null)
        //                    directionChangeIndicator = true;
        //                else if (directionChangeIndicator == false)
        //                {
        //                    speedCorrectionFactor *= 1.1;
        //                    directionChangeIndicator = true;
        //                    ++currIter;
        //                }
        //            }
        //            else
        //            {
        //                motorPotentiometer.StartMotion(speed, MotionDirection.ccw);
        //                if (directionChangeIndicator == null)
        //                    directionChangeIndicator = false;
        //                else if (directionChangeIndicator == true)
        //                {
        //                    speedCorrectionFactor *= 1.1;
        //                    directionChangeIndicator = false;
        //                    ++currIter;
        //                }
        //            }

        //            if (currIter >= 99)
        //            {
        //                motorPotentiometer.StopMotion();
        //                break;
        //            }
        //        }

        //        voltagePrev = voltageCurr;
        //    }

        //    motorPotentiometer.StopMotion();
        //}

        //void setVoltage(BS350_MotorPotentiometer motorPotentiometer, int voltNum, double voltage, double voltageDev)
        //{
        //    var voltageSign = voltage < 0 ? -1.0 : 1.0;

        //    voltage = Math.Abs(voltage);
        //    var intervalCoarse = voltage * (1.0 - 1.0 / Math.Sqrt(2.0));

        //    double voltageCurr = 0.0,
        //        voltagePrev = 0.0,
        //        factorCoarse = 0.0;

        //    accuracyStopWatch.Start();

        //    averagingNumberFast = experimentSettings.NAveragesFast;

        //    var isFirstMeasurement = true;
        //    var firstVoltageReading = 0.0;

        //    while (true)
        //    {
        //        var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

        //        voltageCurr = Math.Abs(voltages[voltNum]);

        //        if (isFirstMeasurement)
        //        {
        //            firstVoltageReading = voltageCurr;
        //            isFirstMeasurement = false;
        //        }

        //        onStatusChanged(new StatusEventArgs(string.Format("Voltage value to set -> {0}, current voltage -> {1}", (voltage * voltageSign).ToString("0.0000", NumberFormatInfo.InvariantInfo), (voltageCurr * voltageSign).ToString("0.0000", NumberFormatInfo.InvariantInfo))));
        //        onProgressChanged(new ProgressEventArgs(100.0 * (1.0 - Math.Abs(voltage - voltageCurr) / Math.Abs(voltage - firstVoltageReading))));

        //        var speed = minSpeed;
        //        try
        //        {
        //            if (Math.Abs(voltage - voltageCurr) <= 0.05)
        //            {
        //                factorCoarse = (1.0 - Math.Tanh(-1.0 * Math.Abs(voltage - voltageCurr) / intervalCoarse * Math.PI + Math.PI)) / 2.0;
        //                speed = (byte)(minSpeed + (maxSpeed - minSpeed) * factorCoarse);
        //            }
        //            else
        //                speed = maxSpeed;
        //        }
        //        catch { speed = minSpeed; }

        //        if ((voltageCurr >= Math.Abs(voltage - voltageDev)) &&
        //            (voltageCurr <= Math.Abs(voltage + voltageDev)))
        //        {
        //            motorPotentiometer.StopMotion();
        //            accuracyStopWatch.Stop();
        //            break;
        //        }
        //        else
        //        {
        //            // Implementing voltage set with enchansed accuracy
        //            if (estimationList.Count > estimationCollectionSize)
        //                estimationList.RemoveFirst();

        //            estimationList.AddLast(new Point(accuracyStopWatch.ElapsedMilliseconds, Math.Abs(voltagePrev - voltageCurr)));

        //            var timeAVG = estimationList.Select(val => val.X).Average();
        //            var voltAVG = estimationList.Select(val => val.Y).Average();

        //            var voltPerMilisecond = timeAVG != 0 ? voltAVG / timeAVG : voltAVG;

        //            var stepTime = (int)(Math.Abs(voltage - voltageCurr) / voltPerMilisecond);

        //            if (voltageCurr > voltage)
        //            {
        //                if (voltageDev >= 0.006 || voltageCurr - voltage > 2.0 * voltageDev)
        //                {
        //                    averagingNumberFast = 2;
        //                    motorPotentiometer.StartMotion(speed, MotionDirection.cw);
        //                }
        //                else
        //                {
        //                    motorPotentiometer.StartMotion(speed, MotionDirection.cw);
        //                    Thread.Sleep(stepTime);
        //                    motorPotentiometer.StopMotion();
        //                    averagingNumberFast = 25;
        //                }
        //            }
        //            else
        //            {
        //                if (voltageDev >= 0.006 || voltage - voltageCurr > 2.0 * voltageDev)
        //                {
        //                    averagingNumberFast = 2;
        //                    motorPotentiometer.StartMotion(speed, MotionDirection.ccw);
        //                }
        //                else
        //                {
        //                    motorPotentiometer.StartMotion(speed, MotionDirection.ccw);
        //                    Thread.Sleep(stepTime);
        //                    motorPotentiometer.StopMotion();
        //                    averagingNumberFast = 25;
        //                }
        //            }

        //            accuracyStopWatch.Restart();
        //        }

        //        voltagePrev = voltageCurr;
        //    }

        //    motorPotentiometer.StopMotion();
        //}

        void SetDrainSourceVoltage(double voltage, double voltageDev)
        {
            var voltages = confAIChannelsForDC_Measurement();
            confAIChannelsForDC_Measurement(voltage, voltages[1], voltages[2]);

            preciseSetVoltage(VdsMotorPotentiometer, 3, voltage, voltageDev, experimentSettings.NAveragesFast);
        }

        void SetGateVoltage(double voltage, double voltageDev)
        {
            var voltages = confAIChannelsForDC_Measurement();
            confAIChannelsForDC_Measurement(voltages[3], voltages[1], voltage);

            preciseSetVoltage(VdsMotorPotentiometer, 3, voltage, voltageDev, experimentSettings.NAveragesFast);
        }

        static string TTSaveFileName = "TT.dat";
        string NoiseSpectrumFinal = string.Empty;

        bool acquisitionIsRunning = false;
        bool acquisitionIsSuccessful = false;

        static int averagingCounter = 0;

        bool measureNoiseSpectra(

            int samplingFrequency,
            int nDataSamples,
            int nAverages,
            int updateNumber,
            double kAmpl

            )
        {
            var noisePSD = new Point[] { };

            Interlocked.Exchange(ref averagingCounter, 0);

            if (samplingFrequency % 2 != 0)
                throw new ArgumentException("The frequency should be an even number!");

            foreach (var item in boxController.AI_ChannelCollection)
                if (item.IsEnabled)
                    item.Parameters.SetParams(FilterCutOffFrequencies.Freq_120kHz, FilterGain.gain1, PGA_GainsEnum.gain1);

            boxController.AcquisitionInProgress = true;

            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady -= FET_Noise_DataReady;
            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady += FET_Noise_DataReady;

            Point[] temp;
            while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.IsEmpty)
                boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out temp);

            acquisitionIsRunning = true;
            boxController.AcquisitionInProgress = true;

            var acquisitionTaskResult = new Task(new Action(() => { }));

            Parallel.Invoke(
                () =>
                {
                    while (true)
                    {
                        if (!acquisitionIsRunning)
                        {
                            boxController.AcquisitionInProgress = false;
                            averagingCounter = 0;
                            break;
                        }
                        if (averagingCounter >= nAverages)
                        {
                            boxController.AcquisitionInProgress = false;
                            averagingCounter = 0;
                            break;
                        }
                        if (acquisitionTaskResult != null)
                        {
                            var taskStatus = acquisitionTaskResult.Status;
                            var taskCompleted = acquisitionTaskResult.IsCompleted;

                            if ((taskCompleted == true) ||
                                (taskStatus == TaskStatus.Canceled) ||
                                (taskStatus == TaskStatus.Faulted))
                            {
                                averagingCounter = 0;
                                break;
                            }
                        }

                        Point[] timeTrace = new Point[] { };
                        var dataReadingSuccess = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out timeTrace);

                        if (dataReadingSuccess)
                        {
                            var TTVoltageValues = (from item in timeTrace
                                                   select item.Y).ToArray();
                            
                            var singleNoiseSpectrum = twoPartsFFT.GetTwoPartsFFT(TTVoltageValues, samplingFrequency, 1, 1.0, 1.0, 1600, 102400, 8, 6400, 10, 10);

                            if (noisePSD == null || noisePSD.Length == 0)
                            {
                                noisePSD = new Point[singleNoiseSpectrum.Length];
                                for (int i = 0; i != singleNoiseSpectrum.Length; )
                                {
                                    noisePSD[i] = new Point(singleNoiseSpectrum[i].X, 0.0);
                                    ++i;
                                }
                            }

                            for (int i = 0; i != noisePSD.Length; )
                            {
                                noisePSD[i].Y += singleNoiseSpectrum[i].Y;
                                ++i;
                            }

                            if (averagingCounter % updateNumber == 0)
                            {
                                var dividedSpectrum = (from item in noisePSD
                                                       select new Point(item.X, item.Y / averagingCounter));

                                var normalizedSpectrum = (from item in dividedSpectrum
                                                          select new Point(item.X, item.Y / (kAmpl * kAmpl))).ToArray();

                                // Calibration here
                                var calibratedSpectrum = twoPartsFFT.GetCalibratedSpectrum(ref normalizedSpectrum, ref amplifierNoise, ref frequencyResponse);

                                var finalSpectrum = from divSpecItem in dividedSpectrum
                                                    join calSpecItem in calibratedSpectrum on divSpecItem.X equals calSpecItem.X
                                                    select string.Format("{0}\t{1}\t{2}", divSpecItem.X.ToString(NumberFormatInfo.InvariantInfo), calSpecItem.Y.ToString(NumberFormatInfo.InvariantInfo), divSpecItem.Y.ToString(NumberFormatInfo.InvariantInfo));

                                // Sending the calculated spectrum data
                                onDataArrived(new ExpDataArrivedEventArgs(string.Format("NS{0}", string.Join("\r\n", finalSpectrum))));
                                onProgressChanged(new ProgressEventArgs((double)averagingCounter / (double)nAverages * 100.0));
                            }

                            if (experimentSettings.RecordTimeTraces == true)
                            {
                                TT_AutoresetEvent.Reset();

                                var n = experimentSettings.SamplingFrequency / experimentSettings.RecordingFrequency;
                                var timeTraceSelection = timeTrace
                                    .Where((value, index) => index % n == 0)
                                    .Select(value => string.Format("{0}\t{1}\r\n", value.X.ToString(NumberFormatInfo.InvariantInfo), (value.Y / kAmpl).ToString(NumberFormatInfo.InvariantInfo)))
                                    .ToArray();

                                var ttMemorySize = 0;
                                for (int i = 0; i != timeTraceSelection.Length; )
                                {
                                    ttMemorySize += ASCIIEncoding.ASCII.GetByteCount(timeTraceSelection[i]);
                                    ++i;
                                }

                                int offset = 0;
                                int count = 0;
                                byte[] buf;

                                var security = new MemoryMappedFileSecurity();
                                security.AddAccessRule(new AccessRule<MemoryMappedFileRights>("everyone", MemoryMappedFileRights.FullControl, AccessControlType.Allow));

                                using (var mmf = MemoryMappedFile.CreateNew(
                                    @"TTMappedFile",
                                    ttMemorySize,
                                    MemoryMappedFileAccess.ReadWrite,
                                    MemoryMappedFileOptions.DelayAllocatePages,
                                    security,
                                    HandleInheritability.Inheritable))
                                {
                                    using (var stream = mmf.CreateViewStream(0, ttMemorySize))
                                    {
                                        for (int i = 0; i != timeTraceSelection.Length; )
                                        {
                                            buf = ASCIIEncoding.ASCII.GetBytes(timeTraceSelection[i]);
                                            count = ASCIIEncoding.ASCII.GetByteCount(timeTraceSelection[i]);

                                            stream.Write(buf, offset, count);
                                            ++i;
                                        }
                                    }
                                    onDataArrived(new ExpDataArrivedEventArgs(string.Format("TT {0}", ttMemorySize.ToString(NumberFormatInfo.InvariantInfo))));
                                    TT_AutoresetEvent.WaitOne();
                                }
                            }
                        }
                    }
                },
                async () =>
                {
                    acquisitionTaskResult = Task.Factory.StartNew(() =>
                    {
                        acquisitionIsSuccessful = boxController.StartAnalogAcquisition(samplingFrequency);
                    });

                    await acquisitionTaskResult;
                });

            acquisitionTaskResult.Wait();

            return acquisitionIsSuccessful;
        }

        [HandleProcessCorruptedStateExceptions]
        public override void ToDo(object Arg)
        {
            onExpStarted(new StartedEventArgs());
            onStatusChanged(new StatusEventArgs("Measurement started."));
            onProgressChanged(new ProgressEventArgs(0.0));

            experimentSettings = (FETUI.NoiseFETSettingsControlModel)Arg;

            #region Writing data to log files

            var noiseMeasLog = new NoiseMeasurementDataLog();

            var logFileName = string.Join("\\", experimentSettings.FilePath, "Noise", noiseMeasLog.DataLogFileName);
            var logFileNameNewFormat = string.Join("\\", experimentSettings.FilePath, "Noise", noiseMeasLog.DataLogFileNameNewFormat);
            var logFileCaptureName = string.Join("\\", experimentSettings.FilePath, "Time traces", "MeasurDataCapture.dat");

            var mode = FileMode.OpenOrCreate;
            var access = FileAccess.Write;

            createFileWithHeader(logFileName, ref mode, ref access, NoiseMeasurementDataLog.DataHeader, NoiseMeasurementDataLog.DataSubHeader);
            createFileWithHeader(logFileNameNewFormat, ref mode, ref access, NoiseMeasurementDataLog.DataHeaderNewFormat, NoiseMeasurementDataLog.DataSubHeaderNewFormat);

            if (experimentSettings.RecordTimeTraces == true)
                createFileWithHeader(logFileCaptureName, ref mode, ref access, NoiseMeasurementDataLog.DataHeader, NoiseMeasurementDataLog.DataSubHeader);

            #endregion

            double[] outerLoopCollection;
            double[] innerLoopCollection;

            if (experimentSettings.IsOutputCurveMode == true)
            {
                outerLoopCollection = experimentSettings.GateVoltageCollection;
                innerLoopCollection = experimentSettings.DSVoltageCollection;
            }
            else if (experimentSettings.IsTransferCurveMode == true)
            {
                outerLoopCollection = experimentSettings.DSVoltageCollection;
                innerLoopCollection = experimentSettings.GateVoltageCollection;
            }
            else
            {
                outerLoopCollection = new double[] { 0.0 };
                innerLoopCollection = new double[] { 0.0 };
            }

            for (int i = 0; i < outerLoopCollection.Length; i++)
            {
                var outerLoopVoltage = outerLoopCollection[i];
                if (!IsRunning)
                    break;
                for (int j = 0; j < innerLoopCollection.Length; j++)
                {
                    var innerLoopVoltage = innerLoopCollection[j];
                    if (!IsRunning)
                        break;

                    #region Recording time trace FileStream settings

                    if (TT_Stream != null)
                        TT_Stream.Close();

                    if (experimentSettings.RecordTimeTraces == true)
                    {
                        TTSaveFileName = GetFileNameWithIncrement(string.Join("\\", experimentSettings.FilePath, "Time traces", experimentSettings.SaveFileName));
                        createFileWithHeader(TTSaveFileName, ref mode, ref access, "", "");
                        TT_Stream = new FileStream(TTSaveFileName, FileMode.Open, FileAccess.Write);
                    }

                    #endregion

                    try
                    {
                        using (boxController = new BoxController())
                        {
                            var boxInit = boxController.Init(experimentSettings.AgilentU2542AResName);

                            if (!boxInit)
                                throw new Exception("Cannot connect the box.");

                            // Implementing voltage control for automatic applying
                            // Gate and Drain-Source voltages, as well as for automatic
                            // disabling of the Vs DC measurement channel for the
                            // period of noise measurement

                            var VdsMotorOutChannel = BOX_AnalogOutChannelsEnum.BOX_AOut_01;
                            var VdsEnableChannel = BOX_AnalogOutChannelsEnum.BOX_AOut_09;

                            var VgMotorOutChannel = BOX_AnalogOutChannelsEnum.BOX_AOut_02;

                            // Enabling Vds DC measurement channel before measuring noise spectra
                            // for measuring sample characteristics before noise measurement
                            boxController.AO_ChannelCollection.ApplyVoltageToChannel(VdsEnableChannel, -6.2);
                            
                            if (experimentSettings.UseVoltageControl == true)
                            {
                                VdsMotorPotentiometer = new BS350_MotorPotentiometer(boxController, VdsMotorOutChannel);
                                VgMotorPotentiometer = new BS350_MotorPotentiometer(boxController, VgMotorOutChannel);

                                if (experimentSettings.IsOutputCurveMode == true)
                                {
                                    onStatusChanged(new StatusEventArgs(string.Format("Setting gate voltage V -> {0} V", outerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                                    SetGateVoltage(outerLoopVoltage, experimentSettings.VoltageDeviation.RealValue);
                                    onStatusChanged(new StatusEventArgs(string.Format("Setting drain-source voltage V -> {0} V", innerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                                    SetDrainSourceVoltage(innerLoopVoltage, experimentSettings.VoltageDeviation.RealValue);
                                }
                                else if (experimentSettings.IsTransferCurveMode == true)
                                {
                                    onStatusChanged(new StatusEventArgs(string.Format("Setting gate voltage V -> {0} V", innerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                                    SetGateVoltage(innerLoopVoltage, experimentSettings.VoltageDeviation.RealValue);
                                    onStatusChanged(new StatusEventArgs(string.Format("Setting drain-source voltage V -> {0} V", outerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                                    SetDrainSourceVoltage(outerLoopVoltage, experimentSettings.VoltageDeviation.RealValue);
                                }
                            }
                            else
                                IsRunning = false;
                            
                            onStatusChanged(new StatusEventArgs("Measuring sample characteristics before noise spectra measurement."));

                            confAIChannelsForDC_Measurement();
                            var voltagesBeforeNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);

                            // Disabling Vds DC measurement channel for measuring noise spectra
                            // to reduce the noise influence of the box controller on the measurement
                            boxController.AO_ChannelCollection.DisableAllVoltages();

                            var ACConfStatus = confAIChannelsForAC_Measurement();
                            if (ACConfStatus)
                            {
                                // Stabilization before noise spectra measurements
                                onProgressChanged(new ProgressEventArgs(0.0));
                                onStatusChanged(new StatusEventArgs("Waiting for stabilization..."));
                                Thread.Sleep((int)(experimentSettings.StabilizationTime * 1000));

                                // Measuring noise spectra
                                onStatusChanged(new StatusEventArgs("Measuring noise spectra & time traces."));
                                var noiseSpectraMeasurementState = measureNoiseSpectra(experimentSettings.SamplingFrequency, 1, experimentSettings.SpectraAveraging, experimentSettings.UpdateNumber, experimentSettings.KPreAmpl * experimentSettings.KAmpl);

                                // Enabling Vds DC measurement channel after measuring noise spectra
                                // for measuring sample characteristics after noise measurement
                                boxController.AO_ChannelCollection.ApplyVoltageToChannel(VdsEnableChannel, -6.2);

                                if (noiseSpectraMeasurementState)
                                {
                                    onStatusChanged(new StatusEventArgs("Measuring sample characteristics after noise spectra measurement."));

                                    confAIChannelsForDC_Measurement();
                                    var voltagesAfterNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);

                                    // Saving to log file all the parameters of the measurement

                                    var fileName = string.Join("\\", experimentSettings.FilePath, "Noise", experimentSettings.SaveFileName);
                                    var dataFileName = GetFileNameWithIncrement(fileName);

                                    SaveToFile(dataFileName);

                                    noiseMeasLog.SampleVoltage = voltagesAfterNoiseMeasurement[3];
                                    noiseMeasLog.SampleCurrent = (voltagesAfterNoiseMeasurement[1] - voltagesAfterNoiseMeasurement[3]) / experimentSettings.LoadResistance;
                                    noiseMeasLog.FileName = (new FileInfo(dataFileName)).Name;
                                    noiseMeasLog.Rload = experimentSettings.LoadResistance;
                                    noiseMeasLog.Uwhole = voltagesAfterNoiseMeasurement[1];
                                    noiseMeasLog.URload = voltagesAfterNoiseMeasurement[1] - voltagesAfterNoiseMeasurement[3];
                                    noiseMeasLog.U0sample = voltagesBeforeNoiseMeasurement[3];
                                    noiseMeasLog.U0whole = voltagesBeforeNoiseMeasurement[1];
                                    noiseMeasLog.U0Rload = voltagesBeforeNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[3];
                                    noiseMeasLog.U0Gate = voltagesBeforeNoiseMeasurement[2];
                                    noiseMeasLog.R0sample = noiseMeasLog.U0sample / (noiseMeasLog.U0Rload / noiseMeasLog.Rload);
                                    noiseMeasLog.REsample = noiseMeasLog.SampleVoltage / (noiseMeasLog.URload / noiseMeasLog.Rload);
                                    noiseMeasLog.EquivalentResistance = 1.0 / (1.0 / experimentSettings.LoadResistance + 1.0 / noiseMeasLog.REsample + 1.0 / experimentSettings.AmpInputResistance);
                                    noiseMeasLog.Temperature0 = experimentSettings.Temperature0;
                                    noiseMeasLog.TemperatureE = experimentSettings.TemperatureE;
                                    noiseMeasLog.kAmpl = experimentSettings.KAmpl;
                                    noiseMeasLog.NAver = experimentSettings.SpectraAveraging;
                                    noiseMeasLog.Vg = voltagesAfterNoiseMeasurement[2];

                                    SaveDataToLog(logFileName, noiseMeasLog.ToString());
                                    SaveDataToLog(logFileNameNewFormat, noiseMeasLog.ToStringNewFormat());

                                    if (experimentSettings.RecordTimeTraces == true)
                                        SaveDataToLog(logFileCaptureName, noiseMeasLog.ToString());
                                }
                                else
                                {
                                    if (experimentSettings.RecordTimeTraces == true)
                                    {
                                        if (TT_Stream != null)
                                            TT_Stream.Dispose();

                                        File.Delete(TTSaveFileName);
                                    }
                                    if (j > 0)
                                        --j;
                                }

                                // Disabling Vds DC measurement channel
                                boxController.AO_ChannelCollection.DisableAllVoltages();
                            }
                            else
                            {
                                if (experimentSettings.RecordTimeTraces == true)
                                {
                                    if (TT_Stream != null)
                                        TT_Stream.Dispose();

                                    File.Delete(TTSaveFileName);
                                }
                                if (j > 0)
                                    --j;
                            }
                        }
                    }
                    catch
                    {
                        if (boxController != null)
                            boxController.Dispose();
                        if (experimentSettings.RecordTimeTraces == true)
                        {
                            if (TT_Stream != null)
                                TT_Stream.Dispose();

                            File.Delete(TTSaveFileName);
                        }
                        if (j > 0)
                            --j;
                    }
                }
            }

            onExpFinished(new FinishedEventArgs());
            onStatusChanged(new StatusEventArgs("The measurement is done!"));            
        }

        #region File operations

        void createFileWithHeader(string FileName, ref FileMode mode, ref FileAccess access, string Header = "", string Subheader = "")
        {
            access = FileAccess.Write;
            mode = FileMode.OpenOrCreate;

            var info = new FileInfo(FileName);

            var dirName = info.DirectoryName;

            if (!Directory.Exists(dirName))
                Directory.CreateDirectory(dirName);

            if (!File.Exists(FileName))
            {
                using (var sw = new StreamWriter(new FileStream(FileName, mode, access)))
                {
                    sw.Write(Header);
                    sw.Write(Subheader);
                }

                mode = FileMode.Append;
            }
            else
                mode = FileMode.OpenOrCreate;
        }

        private static int FileCounter;
        private string GetFileNameWithIncrement(string FileName)
        {
            string result;
            FileCounter = 0;

            while (true)
            {
                result = FileName.Insert(FileName.LastIndexOf('.'), String.Format("_{0}{1}{2}", (FileCounter / 100) % 10, (FileCounter / 10) % 10, FileCounter % 10));

                if (!File.Exists(result))
                    break;
                ++FileCounter;
            }

            return result;
        }

        private async Task WriteData(byte[] __ToWrite, string fileName, FileMode mode, FileAccess access)
        {
            using (var fs = new FileStream(fileName, mode, access, FileShare.None, 4098, true))
            {
                await (fs.WriteAsync(__ToWrite, 0, __ToWrite.Length));
            }
        }

        public override async void SaveToFile(string FileName)
        {
            var mode = FileMode.OpenOrCreate;
            var access = FileAccess.Write;

            //createFileWithHeader(FileName, ref mode, ref access, SingleNoiseMeasurement.DataHeader, SingleNoiseMeasurement.DataSubHeader);
            createFileWithHeader(FileName, ref mode, ref access, "", "");

            var toWrite = Encoding.ASCII.GetBytes(NoiseSpectrumFinal);
            await WriteData(toWrite, FileName, mode, access);
        }

        private async void SaveDataToLog(string DataLogFileName, string LogData)
        {
            var mode = FileMode.OpenOrCreate;
            var access = FileAccess.Write;

            if (File.Exists(DataLogFileName))
                mode = FileMode.Append;

            var toWrite = Encoding.ASCII.GetBytes(LogData);
            await WriteData(toWrite, DataLogFileName, mode, access);
        }

        #endregion

        private void FET_Noise_DataReady(object sender, EventArgs e)
        {
            Interlocked.Increment(ref averagingCounter);
        }

        StringBuilder dataBuilder = new StringBuilder();
        void FET_Noise_Experiment_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("NS"))
            {
                NoiseSpectrumFinal = e.Data.Substring(2);
            }
            else if (e.Data.StartsWith("TT") && experimentSettings.RecordTimeTraces == true)
            {
                var streamSize = int.Parse(e.Data.Substring(3));
                try
                {
                    using (var mmf = MemoryMappedFile.OpenExisting(@"TTMappedFile", MemoryMappedFileRights.ReadWrite, HandleInheritability.Inheritable))
                    {
                        using (var mmfStream = mmf.CreateViewStream(0, streamSize, MemoryMappedFileAccess.Read))
                        {
                            byte[] toRead = new byte[streamSize];
                            mmfStream.Read(toRead, 0, streamSize);

                            TT_Stream.Write(toRead, 0, toRead.Length);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    TT_AutoresetEvent.Set();
                }
            }
        }

        public override void Stop()
        {
            if (TT_Stream != null)
                TT_Stream.Close();

            File.Delete(TTSaveFileName);

            onStatusChanged(new StatusEventArgs("Measurement is aborted."));
            onProgressChanged(new ProgressEventArgs(0.0));
            Dispose();
        }

        public override void Dispose()
        {
            if (IsRunning)
            {
                IsRunning = false;

                DataArrived -= FET_Noise_Experiment_DataArrived;

                if (boxController != null)
                    boxController.Close();
            }

            base.Dispose();
        }
    }
}
