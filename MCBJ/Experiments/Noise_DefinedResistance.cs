using ExperimentController;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System.Globalization;

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using MotionManager;

using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.SpectralMeasurements;
using D3Helper;
using System.IO;

using MCBJ.Experiments.DataHandling;
using IneltaMotorPotentiometer;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Runtime.ExceptionServices;

namespace MCBJ.Experiments
{
    public class Noise_DefinedResistance : ExperimentBase
    {
        BoxController boxController;
        BS350_MotorPotentiometer VdsMotorPotentiometer;

        IMotionController1D motor;

        Stopwatch stabilityStopwatch;
        Stopwatch accuracyStopWatch;

        bool isDCMode = false;
        bool isACMode = false;
        bool isDCOscilloscopeMode = false;

        private readonly double ConductanceQuantum = 0.0000774809173;

        private AutoResetEvent TT_AutoresetEvent = new AutoResetEvent(false);
        private FileStream TT_Stream;

        private Noise_DefinedResistanceInfo experimentSettings;

        private Point[] amplifierNoise;
        private Point[] frequencyResponce;

        private SpectralAnalysis.TwoPartsFFT twoPartsFFT;

        public Noise_DefinedResistance(string SDA_ConnectionString, IMotionController1D Motor, Point[] AmplifierNoise, Point[] FrequencyResponce)
            : base()
        {
            //boxController = new BoxController();

            //var boxInit = boxController.Init(SDA_ConnectionString);

            //if (!boxInit)
            //    throw new Exception("Cannot connect the box.");

            motor = Motor;

            amplifierNoise = AmplifierNoise;
            frequencyResponce = FrequencyResponce;
            twoPartsFFT = new SpectralAnalysis.TwoPartsFFT();

            stabilityStopwatch = new Stopwatch();
            accuracyStopWatch = new Stopwatch();

            this.DataArrived += Noise_DefinedResistance_DataArrived;
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
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},  
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vm_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},
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

        AI_ChannelConfig[] setDCOscilloscopeConf(double Vs)
        {
            var Vs_Range = setRangeForGivenVoltage(Vs);

            var config = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = false, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},  
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},   // Vm
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.AC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = Vs_Range}    // Vs
            };

            return config;
        }

        int averagingNumberFast = 2;
        int averagingNumberSlow = 100;

        byte minSpeed = 10;
        byte maxSpeed = 255;

        void confAIChannelsForDC_Measurement()
        {
            if (!isDCMode)
            {
                var init_conf = setDCConf(9.99, 9.99);
                boxController.ConfigureAI_Channels(init_conf);
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberSlow);
                var real_conf = setDCConf(voltages[3], voltages[1]);
                boxController.ConfigureAI_Channels(real_conf);

                isDCMode = true;
                isACMode = false;
                isDCOscilloscopeMode = false;
            }
        }

        private bool confAIChannelsForAC_Measurement()
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

                result = boxController.AcquireSingleShot(1000);

                if (result == true)
                {
                    var maxAcquiredVoltage = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.Last().Max(p => p.Y);

                    // Configuring the channels to measure noise

                    var real_conf = setACConf(maxAcquiredVoltage);
                    boxController.ConfigureAI_Channels(real_conf);

                    while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.IsEmpty)
                        boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out temp);

                    isACMode = true;
                    isDCMode = false;
                    isDCOscilloscopeMode = false;
                }
            }

            return result;
        }

        private bool confAIChannelsForDCStabilization()
        {
            var result = false;

            if (!isDCOscilloscopeMode)
            {
                var init_conf = setDCOscilloscopeConf(9.99);
                boxController.ConfigureAI_Channels(init_conf);

                // Erasing the data queue

                Point[] temp;
                while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.IsEmpty)
                    boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.TryDequeue(out temp);

                // Acquiring single shot with AC data

                result = boxController.AcquireSingleShot(1000);

                if (result == true)
                {
                    var averagedVoltage = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.Last().Average(p => p.Y);

                    // Configuring the channels to measure dc shift

                    var real_conf = setDCOscilloscopeConf(averagedVoltage);
                    boxController.ConfigureAI_Channels(real_conf);

                    while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.IsEmpty)
                        boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.TryDequeue(out temp);

                    isDCOscilloscopeMode = true;
                    isACMode = false;
                    isDCMode = false;
                }
            }

            return result;
        }

        void PerformDCStabilization()
        {
            confAIChannelsForDCStabilization();

            double averagedVoltage = double.MaxValue;
            Point[] temp;

            while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.IsEmpty)
                boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.TryDequeue(out temp);

            while (!(averagedVoltage <= 0.1))
            {
                // Acquiring single shot with AC data

                while (!boxController.AcquireSingleShot(1000)) ;
                averagedVoltage = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.Last().Average(p => p.Y);
            }

            while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.IsEmpty)
                boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn4].ChannelData.TryDequeue(out temp);
        }

        // For the step time estimation to increase voltage set accuracy
        private int estimationCollectionSize = 25;
        private LinkedList<Point> estimationList = new LinkedList<Point>();
        void setDrainVoltage(double voltage, double voltageDev)
        {
            voltage = Math.Abs(voltage);
            var intervalCoarse = voltage * (1.0 - 1.0 / Math.Sqrt(2.0));

            averagingNumberFast = experimentSettings.NAveragesFast;

            double drainVoltageCurr = 0.0,
                drainVoltagePrev = 0.0,
                factorCoarse = 0.0;

            confAIChannelsForDC_Measurement();

            accuracyStopWatch.Start();

            while (true)
            {
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

                onStatusChanged(new StatusEventArgs(string.Format("Vs = {0} (=> {1} V), Vm = {2}", voltages[3].ToString("0.0000", NumberFormatInfo.InvariantInfo), voltage.ToString("0.0000", NumberFormatInfo.InvariantInfo), voltages[1].ToString("0.0000", NumberFormatInfo.InvariantInfo))));

                drainVoltageCurr = Math.Abs(voltages[3]);

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
                    VdsMotorPotentiometer.StopMotion();
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
                            VdsMotorPotentiometer.StartMotion(speed, MotionDirection.cw);
                        }
                        else
                        {
                            VdsMotorPotentiometer.StartMotion(speed, MotionDirection.cw);
                            Thread.Sleep(stepTime);
                            VdsMotorPotentiometer.StopMotion();
                            averagingNumberFast = 25;
                        }
                    }
                    else
                    {
                        if (voltageDev >= 0.006 || voltage - drainVoltageCurr > 2.0 * voltageDev)
                        {
                            averagingNumberFast = 2;
                            VdsMotorPotentiometer.StartMotion(speed, MotionDirection.ccw);
                        }
                        else
                        {
                            VdsMotorPotentiometer.StartMotion(speed, MotionDirection.ccw);
                            Thread.Sleep(stepTime);
                            VdsMotorPotentiometer.StopMotion();
                            averagingNumberFast = 25;
                        }
                    }

                    accuracyStopWatch.Restart();
                }

                drainVoltagePrev = drainVoltageCurr;
            }

            VdsMotorPotentiometer.StopMotion();
        }

        double measureResistance(

           double LoadResistance = 5000.0,
           int nAveraging = 100,
           double SetVoltage = 0.02,
           double VoltageDeviation = 0.001,
           double MinVoltageTreshold = 0.025,
           double VoltageTreshold = 0.05

           )
        {
            confAIChannelsForDC_Measurement();

            var voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);

            if (Math.Abs(voltages[3]) > VoltageTreshold)
            {
                onStatusChanged(new StatusEventArgs("Treshold voltage value is reached. Setting drain voltage..."));

                var motorWasEnabled = motor.IsEnabled;

                if (motorWasEnabled == true)
                    motor.Enabled = false;

                setDrainVoltage(SetVoltage, VoltageDeviation);

                if (motorWasEnabled == true)
                    motor.Enabled = true;

                voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);
            }
            else if (Math.Abs(voltages[3]) < MinVoltageTreshold)
            {
                onStatusChanged(new StatusEventArgs("Minimum voltage treshold value is reached. Setting drain voltage..."));

                var motorWasEnabled = motor.IsEnabled;

                if (motorWasEnabled == true)
                    motor.Enabled = false;

                setDrainVoltage(SetVoltage, VoltageDeviation);

                if (motorWasEnabled == true)
                    motor.Enabled = true;

                voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);
            }

            var Vs = voltages[3];
            var Vm = voltages[1];

            var Is = (Vm - Vs) / LoadResistance;

            var res = Is != 0 ? Vs / Is : 0.0;

            return Math.Abs(res);
        }

        bool setJunctionResistance(

            double ScanningVoltage,
            double VoltageDeviation,
            double MinVoltageTreshold,
            double VoltageTreshold,
            double SetConductance,
            double ConductanceDeviation,
            double StabilizationTime,
            double MotionMinSpeed,
            double MotionMaxSpeed,
            double MotionMinPos,
            double MotionMaxPos,
            int NAverages,
            double LoadResistance

            )
        {
            var setVolt = ScanningVoltage;
            var voltDev = VoltageDeviation;
            var setCond = SetConductance;
            var condDev = ConductanceDeviation;
            var stabilizationTime = StabilizationTime;

            var minSpeed = MotionMinSpeed;
            var maxSpeed = MotionMaxSpeed;

            var minPos = MotionMinPos;
            var maxPos = MotionMaxPos;

            var nAverages = NAverages;
            var loadResistance = LoadResistance;
            var minVoltageTreshold = MinVoltageTreshold;
            var voltageTreshold = VoltageTreshold;

            var inRangeCounter = 0;
            var outsiderCounter = 0;

            var setResistance = 1.0 / (setCond * ConductanceQuantum);

            onProgressChanged(new ProgressEventArgs(0.0));

            motor.Enabled = true;
            motor.Velosity = maxSpeed;

            var interval = setCond * (1.0 - 1.0 / Math.Sqrt(2.0));

            // Resistance stabilization

            while (true)
            {
                if (!IsRunning)
                {
                    motor.Enabled = true;
                    return false;
                }

                var currResistance = measureResistance(loadResistance, nAverages, setVolt, voltDev, minVoltageTreshold, voltageTreshold);
                var scaledConductance = (1.0 / currResistance) / ConductanceQuantum;

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

                    var motorPosition = motor.Position;

                    onStatusChanged(new StatusEventArgs(string.Format("Reaching: G = {0} G0 ( => {1} G0), R = {2} Ohm ( => {3} Ohm). Current motor pos. is {4} [mm]",
                            scaledConductance.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                            setCond.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                            currResistance.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                            setResistance.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                            motorPosition.ToString("0.0000", NumberFormatInfo.InvariantInfo)
                        )));

                    if (scaledConductance < setCond && motorPosition == minPos)
                    {
                        onStatusChanged(new StatusEventArgs("The sample is broken."));

                        motor.Position = minPos;
                        motor.Enabled = false;
                        return false;
                    }
                    else if (scaledConductance > setCond && motorPosition == maxPos)
                    {
                        onStatusChanged(new StatusEventArgs("Unable to reach desired conductance."));

                        motor.Position = minPos;
                        motor.Enabled = false;
                        return false;
                    }
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
                            motor.Disable();
                            return true;
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
        }



        private static string TTSaveFileName = "TT.dat";
        private string NoiseSpectrumFinal = string.Empty;

        private bool acquisitionIsRunning = false;
        private bool acquisitionIsSuccessful = false;

        private static int averagingCounter = 0;

        private bool measureNoiseSpectra(

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
                    item.Parameters.SetParams(FilterCutOffFrequencies.Freq_150kHz, FilterGain.gain1, PGA_GainsEnum.gain1);

            boxController.AcquisitionInProgress = true;

            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady -= DefResistanceNoise_DataReady;
            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady += DefResistanceNoise_DataReady;

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

                            var singleNoiseSpectrum = twoPartsFFT.GetTwoPartsFFT(TTVoltageValues, samplingFrequency = experimentSettings.SamplingFrequency, experimentSettings.NSubSamples);

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
                                                       select new Point(item.X, item.Y / averagingCounter)).ToArray();

                                var calibratedSpectrum = twoPartsFFT.GetCalibratedSpecteum(ref dividedSpectrum, ref amplifierNoise, ref frequencyResponce);

                                var finalSpectrum = from divSpecItem in dividedSpectrum
                                                    join calSpecItem in calibratedSpectrum on divSpecItem.X equals calSpecItem.X
                                                    select string.Format("{0}\t{1}\t{2}", divSpecItem.X.ToString(NumberFormatInfo.InvariantInfo), (calSpecItem.Y / (kAmpl * kAmpl)).ToString(NumberFormatInfo.InvariantInfo), divSpecItem.Y.ToString(NumberFormatInfo.InvariantInfo));

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
            onStatusChanged(new StatusEventArgs("Measurement started."));
            onProgressChanged(new ProgressEventArgs(0.0));

            experimentSettings = (Noise_DefinedResistanceInfo)Arg;

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

            //confAIChannelsForDC_Measurement();

            var resistanceStabilizationState = false;

            for (int i = 0; i < experimentSettings.SetConductanceCollection.Length; i++)
            {
                var conductance = experimentSettings.SetConductanceCollection[i];
                if (!IsRunning)
                    break;
                for (int j = 0; j < experimentSettings.ScanningVoltageCollection.Length; )
                {
                    var voltage = experimentSettings.ScanningVoltageCollection[j];
                    if (!IsRunning)
                        break;

                    #region Saving time traces to files

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
                            var initResult = boxController.Init(experimentSettings.AgilentU2542AResName);
                            if (!initResult)
                                throw new Exception("Cannot connect to the box");

                            VdsMotorPotentiometer = new BS350_MotorPotentiometer(boxController, BOX_AnalogOutChannelsEnum.BOX_AOut_02);

                            onStatusChanged(new StatusEventArgs(string.Format("Setting sample voltage V -> {0} V", voltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));

                            setDrainVoltage(voltage, experimentSettings.VoltageDeviation);

                            onStatusChanged(new StatusEventArgs(string.Format("Reaching resistance value R -> {0}", (1.0 / conductance).ToString("0.0000", NumberFormatInfo.InvariantInfo))));

                            resistanceStabilizationState = setJunctionResistance(
                                voltage,
                                experimentSettings.VoltageDeviation,
                                experimentSettings.MinVoltageTreshold,
                                experimentSettings.VoltageTreshold,
                                conductance,
                                experimentSettings.ConductanceDeviation,
                                experimentSettings.StabilizationTime,
                                experimentSettings.MotionMinSpeed,
                                experimentSettings.MotionMaxSpeed,
                                experimentSettings.MotorMinPos,
                                experimentSettings.MotorMaxPos,
                                experimentSettings.NAveragesFast,
                                experimentSettings.LoadResistance);

                            if (resistanceStabilizationState == false)
                            {
                                IsRunning = false;
                                break;
                            }

                            setDrainVoltage(voltage, experimentSettings.VoltageDeviation);

                            onStatusChanged(new StatusEventArgs("Measuring sample characteristics before noise spectra measurement."));

                            confAIChannelsForDC_Measurement();
                            var voltagesBeforeNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);

                            motor.Enabled = false;

                            var ACConfStatus = confAIChannelsForAC_Measurement();

                            if (ACConfStatus == true)
                            {
                                Thread.Sleep(15000);

                                onStatusChanged(new StatusEventArgs("Measuring noise spectra & time traces."));

                                var noiseSpectraMeasurementState = measureNoiseSpectra(experimentSettings.SamplingFrequency, experimentSettings.NSubSamples, experimentSettings.SpectraAveraging, experimentSettings.UpdateNumber, experimentSettings.KPreAmpl * experimentSettings.KAmpl);

                                if (noiseSpectraMeasurementState)
                                {
                                    onStatusChanged(new StatusEventArgs("Measuring sample characteristics after noise spectra measurement."));

                                    confAIChannelsForDC_Measurement();
                                    var voltagesAfterNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);

                                    //Saving to log file all the parameters of the measurement

                                    var fileName = string.Join("\\", experimentSettings.FilePath, "Noise", experimentSettings.SaveFileName);
                                    var dataFileName = GetFileNameWithIncrement(fileName);

                                    SaveToFile(dataFileName);

                                    noiseMeasLog.SampleVoltage = voltagesAfterNoiseMeasurement[3];
                                    noiseMeasLog.SampleCurrent = (voltagesAfterNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[3]) / experimentSettings.LoadResistance;
                                    noiseMeasLog.FileName = (new FileInfo(dataFileName)).Name;
                                    noiseMeasLog.Rload = experimentSettings.LoadResistance;
                                    noiseMeasLog.Uwhole = voltagesAfterNoiseMeasurement[1];
                                    noiseMeasLog.URload = voltagesAfterNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[3];
                                    noiseMeasLog.U0sample = voltagesBeforeNoiseMeasurement[3];
                                    noiseMeasLog.U0whole = voltagesBeforeNoiseMeasurement[1];
                                    noiseMeasLog.U0Rload = voltagesBeforeNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[3];
                                    noiseMeasLog.U0Gate = voltagesBeforeNoiseMeasurement[2];
                                    noiseMeasLog.R0sample = noiseMeasLog.U0sample / (noiseMeasLog.U0Rload / noiseMeasLog.Rload);
                                    noiseMeasLog.REsample = noiseMeasLog.SampleVoltage / (noiseMeasLog.URload / noiseMeasLog.Rload);
                                    noiseMeasLog.EquivalentResistance = 1.0 / (1.0 / experimentSettings.LoadResistance + 1.0 / noiseMeasLog.REsample);
                                    noiseMeasLog.Temperature0 = experimentSettings.Temperature0;
                                    noiseMeasLog.TemperatureE = experimentSettings.TemperatureE;
                                    noiseMeasLog.kAmpl = experimentSettings.KAmpl;
                                    noiseMeasLog.NAver = experimentSettings.SpectraAveraging;
                                    noiseMeasLog.Vg = voltagesAfterNoiseMeasurement[2];

                                    SaveDataToLog(logFileName, noiseMeasLog.ToString());
                                    SaveDataToLog(logFileNameNewFormat, noiseMeasLog.ToStringNewFormat());

                                    if (experimentSettings.RecordTimeTraces == true)
                                    {
                                        SaveDataToLog(logFileCaptureName, noiseMeasLog.ToString());
                                        if (TT_Stream != null)
                                            TT_Stream.Dispose();
                                    }
                                }
                                else
                                {
                                    if (experimentSettings.RecordTimeTraces == true)
                                    {
                                        if (TT_Stream != null)
                                            TT_Stream.Dispose();

                                        File.Delete(TTSaveFileName);
                                    }
                                    --j;
                                }
                            }
                            else
                            {
                                if (experimentSettings.RecordTimeTraces == true)
                                {
                                    if (TT_Stream != null)
                                        TT_Stream.Dispose();

                                    File.Delete(TTSaveFileName);
                                }

                                --j;
                            }
                        }

                        ++j;
                    }
                    catch
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

            //motor.Enabled = true;
            //motor.Position = experimentSettings.MotorMinPos;

            if (motor != null)
                motor.Disable();

            onStatusChanged(new StatusEventArgs("The measurement is done!"));

            Dispose();
        }

        private void DefResistanceNoise_DataReady(object sender, EventArgs e)
        {
            Interlocked.Increment(ref averagingCounter);
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

        private StringBuilder dataBuilder = new StringBuilder();
        void Noise_DefinedResistance_DataArrived(object sender, ExpDataArrivedEventArgs e)
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
                            byte[] toWrite = new byte[streamSize];
                            mmfStream.Read(toWrite, 0, streamSize);

                            TT_Stream.Write(toWrite, 0, toWrite.Length);
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

        #endregion

        public override void Stop()
        {
            if (TT_Stream != null)
                TT_Stream.Dispose();

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

                this.DataArrived -= Noise_DefinedResistance_DataArrived;

                if (motor != null)
                    motor.Dispose();

                base.Dispose();
            }
        }
    }
}
