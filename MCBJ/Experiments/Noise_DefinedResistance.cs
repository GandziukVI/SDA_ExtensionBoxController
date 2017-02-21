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

using ChannelSwitchLibrary;

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using MotionManager;

using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.SpectralMeasurements;
using D3Helper;
using System.IO;

using MCBJ.Experiments.DataLog;

namespace MCBJ.Experiments
{
    public class Noise_DefinedResistance : ExperimentBase
    {
        BoxController boxController;
        ChannelSwitch channelSwitch;

        IMotionController1D motor;

        Stopwatch stabilityStopwatch;

        bool connectionEstablished = false;

        bool isDCMode = false;
        bool isACMode = false;

        private readonly double ConductanceQuantum = 0.0000774809173;

        private StreamWriter TT_StreamWriter;

        public Noise_DefinedResistance(string SDA_ConnectionString, IMotionController1D Motor)
            : base()
        {
            boxController = new BoxController();

            var boxInit = boxController.Init(SDA_ConnectionString);

            if (!boxInit)
                throw new Exception("Cannot connect the box.");

            channelSwitch = new ChannelSwitch();

            channelSwitch.Connecting += channelSwitch_Connecting;
            channelSwitch.ConnectionEstablished += channelSwitch_ConnectionEstablished;
            channelSwitch.ConnectionLost += channelSwitch_ConnectionLost;

            channelSwitch.Initialize();
            while (!connectionEstablished) ;

            motor = Motor;

            stabilityStopwatch = new Stopwatch();

            this.DataArrived += Noise_DefinedResistance_DataArrived;
        }

        void channelSwitch_Connecting(object sender, EventArgs e)
        {
            onStatusChanged(new StatusEventArgs("Connecting to the voltages controller module..."));
        }

        void channelSwitch_ConnectionEstablished(object sender, EventArgs e)
        {
            connectionEstablished = true;
            onStatusChanged(new StatusEventArgs("Connection to the voltages controller module is established."));
        }

        void channelSwitch_ConnectionLost(object sender, EventArgs e)
        {
            connectionEstablished = false;
            onStatusChanged(new StatusEventArgs("Connection to the voltages controller module is lost. Trying to reconnect..."));
            try
            {
                channelSwitch.Initialize();
                while (!connectionEstablished) ;
            }
            catch
            {
                onStatusChanged(new StatusEventArgs("Connection to the voltages controller module is failed."));
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

        int minStepTime = 10;
        int maxStepTime = 500;

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

        void confAIChannelsForAC_Measurement()
        {
            if (!isACMode)
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

                isACMode = true;
                isDCMode = false;
            }
        }

        void setVoltage(double voltage, double voltageDev, short channelIdentifyer = 1)
        {
            if (channelIdentifyer < 0 || channelIdentifyer > 2)
                throw new ArgumentException("Channel number has incorrect value.");

            voltage = Math.Abs(voltage);
            var intervalCoarse = voltage * (1.0 - 1.0 / Math.Sqrt(2.0));
            var intervalFine = 0.0;

            double drainVoltageCurr,
                factorCoarse = 0.0,
                factorFine = 0.0;

            confAIChannelsForDC_Measurement();

            while (!connectionEstablished) ;

            while (true)
            {
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

                onStatusChanged(new StatusEventArgs(string.Format("Vs = {0} (=> {1} V), Vm = {2}", voltages[0].ToString("0.0000", NumberFormatInfo.InvariantInfo), voltage.ToString("0.0000", NumberFormatInfo.InvariantInfo), voltages[1].ToString("0.0000", NumberFormatInfo.InvariantInfo))));

                drainVoltageCurr = Math.Abs(voltages[0]);

                var speed = minSpeed;
                try
                {
                    factorCoarse = (1.0 - Math.Tanh(-1.0 * Math.Abs(voltage - drainVoltageCurr) / intervalCoarse * Math.PI + Math.PI)) / 2.0;
                    speed = (short)(minSpeed + (maxSpeed - minSpeed) * factorCoarse);
                }
                catch { speed = minSpeed; }

                if ((drainVoltageCurr >= Math.Abs(voltage - voltageDev)) &&
                    (drainVoltageCurr <= Math.Abs(voltage + voltageDev)))
                {
                    channelSwitch.MoveMotor(channelIdentifyer, stopSpeed);
                    break;
                }
                else
                {
                    intervalFine = voltage * (1.0 - 1.0 / Math.Sqrt(2.0)) * 2.0 * voltageDev;
                    factorFine = (1.0 - Math.Tanh(-1.0 * Math.Abs(voltage - drainVoltageCurr) / intervalFine * Math.PI + Math.PI)) / 2.0;

                    var stepTime = (int)(minStepTime + (maxStepTime - minStepTime) * factorFine);

                    if (drainVoltageCurr > voltage)
                    {
                        if (voltageDev >= 0.006)
                        {
                            averagingNumberFast = 2;
                            channelSwitch.MoveMotor(channelIdentifyer, speed);
                        }
                        else if (drainVoltageCurr - voltage > 2.0 * voltageDev)
                        {
                            averagingNumberFast = 2;
                            channelSwitch.MoveMotor(channelIdentifyer, speed);
                        }
                        else
                        {
                            channelSwitch.MoveMotor(channelIdentifyer, speed);
                            Thread.Sleep((int)(stepTime));
                            channelSwitch.MoveMotor(channelIdentifyer, stopSpeed);
                            averagingNumberFast = 25;
                        }
                    }
                    else
                    {
                        if (voltageDev >= 0.006)
                        {
                            averagingNumberFast = 2;
                            channelSwitch.MoveMotor(channelIdentifyer, (short)(-1.0 * speed));
                        }
                        else if (voltage - drainVoltageCurr > 2.0 * voltageDev)
                        {
                            averagingNumberFast = 2;
                            channelSwitch.MoveMotor(channelIdentifyer, (short)(-1.0 * speed));
                        }
                        else
                        {
                            channelSwitch.MoveMotor(channelIdentifyer, (short)(-1.0 * speed));
                            Thread.Sleep((int)(stepTime));
                            channelSwitch.MoveMotor(channelIdentifyer, stopSpeed);
                            averagingNumberFast = 25;
                        }
                    }
                }
            }

            channelSwitch.MoveMotor(channelIdentifyer, 0);
        }

        void setDrainVoltage(double drainVoltage, double voltageDev)
        {
            setVoltage(drainVoltage, voltageDev, 1);
        }

        double measureResistance(

           double LoadResistance = 5000.0,
           int nAveraging = 100,
           double SetVoltage = 0.02,
           double VoltageDeviation = 0.001,
           double VoltageTreshold = 0.05

           )
        {
            confAIChannelsForDC_Measurement();

            var voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);

            if (voltages[0] > VoltageTreshold)
            {
                onStatusChanged(new StatusEventArgs("Treshold voltage value is reached. Going down to set voltage value."));

                if (motor.IsEnabled == true)
                    motor.Enabled = false;

                setDrainVoltage(SetVoltage, VoltageDeviation);

                voltages = boxController.VoltageMeasurement_AllChannels(nAveraging);
            }

            var Vs = voltages[0];
            var Vm = voltages[1];

            var Is = (Vm - Vs) / LoadResistance;

            var res = Is != 0 ? Vs / Is : 0.0;

            return Math.Abs(res);
        }

        void setJunctionResistance(

            double ScanningVoltage,
            double VoltageDeviation,
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
            var voltageTreshold = VoltageTreshold;

            var inRangeCounter = 0;
            var outsiderCounter = 0;

            var setResistance = 1.0 / (setCond * ConductanceQuantum);

            onStatusChanged(new StatusEventArgs(string.Format("Reaching resistance of {0}.", setResistance.ToString(NumberFormatInfo.InvariantInfo))));
            onProgressChanged(new ProgressEventArgs(0.0));

            motor.Enabled = true;
            motor.Velosity = maxSpeed;

            var interval = setCond * (1.0 - 1.0 / Math.Sqrt(2.0));

            // Resistance stabilization

            while (true)
            {
                if (!IsRunning)
                    break;

                var currResistance = measureResistance(loadResistance, nAverages, setVolt, voltDev, voltageTreshold);
                var scaledConductance = (1.0 / currResistance) / ConductanceQuantum;

                onStatusChanged(new StatusEventArgs(string.Format("Gset = {0} G0, G = {1} G0, Rset = {2}, R = {3}",
                    setCond.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                    scaledConductance.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                    setResistance.ToString("0.0000", NumberFormatInfo.InvariantInfo),
                    currResistance.ToString("0.0000", NumberFormatInfo.InvariantInfo))));

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
        }

        private static string TTSaveFileName = "TT.dat";
        private string NoiseSpectrumFinal = string.Empty;


        private static int averagingCounter = 0;
        void measureNoiseSpectra(

            int samplingFrequency,
            int nAverages,
            int updateNumber,
            double kAmpl

            )
        {
            confAIChannelsForAC_Measurement();

            double[] autoPSDLowFreq;
            double[] autoPSDHighFreq;

            Point[] noisePSD = new Point[] { };

            if (samplingFrequency % 2 != 0)
                throw new ArgumentException("The frequency should be an even number!");

            boxController.AcquisitionInProgress = true;
            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady += DefResistanceNoise_DataReady;

            var sb = new StringBuilder();

            double dtLowFreq = 0.0, dtHighFreq = 0.0;
            double dfLowFreq = 1.0, dfHighFreq = 0.0;
            double equivalentNoiseBandwidthLowFreq, equivalentNoiseBandwidthHighFreq;
            double coherentGainLowFreq, coherentGainHighFreq;

            Parallel.Invoke(
                () =>
                {
                    boxController.StartAnalogAcquisition(samplingFrequency);
                    IsRunning = false;
                },
                () =>
                {
                    while (true)
                    {
                        if (!IsRunning)
                        {
                            boxController.AcquisitionInProgress = false;
                            break;
                        }
                        if (averagingCounter >= nAverages)
                        {
                            boxController.AcquisitionInProgress = false;
                            break;
                        }

                        Point[] timeTrace;
                        var dataReadingSuccess = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out timeTrace);

                        if (dataReadingSuccess)
                        {
                            sb = new StringBuilder();
                            sb.Append("TT");
                            foreach (var item in timeTrace)
                                sb.AppendFormat("{0}\t{1}\r\n", item.X.ToString(NumberFormatInfo.InvariantInfo), (item.Y / kAmpl).ToString(NumberFormatInfo.InvariantInfo));

                            // First sending the time trace data before FFT
                            onDataArrived(new ExpDataArrivedEventArgs(sb.ToString()));

                            var traceData = (from val in timeTrace
                                             select val.Y).ToArray();

                            var unit = new System.Text.StringBuilder("V", 256);
                            var sw = ScaledWindow.CreateRectangularWindow();

                            // Calculation of the low-frequency part of the spectrum

                            sw.Apply(traceData, out equivalentNoiseBandwidthLowFreq, out coherentGainLowFreq);

                            dtLowFreq = 1.0 / (double)samplingFrequency;

                            autoPSDLowFreq = Measurements.AutoPowerSpectrum(traceData, dtLowFreq, out dfLowFreq);
                            var singlePSDLowFreq = Measurements.SpectrumUnitConversion(autoPSDLowFreq, SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, dfLowFreq, equivalentNoiseBandwidthLowFreq, coherentGainLowFreq, unit);

                            // Calculation of the hugh-frequency part of the spectrum

                            var selection64Hz = PointSelector.SelectPoints(ref traceData, 64);

                            sw.Apply(selection64Hz, out equivalentNoiseBandwidthHighFreq, out coherentGainHighFreq);

                            dtHighFreq = 64.0 * 1.0 / (double)samplingFrequency;

                            autoPSDHighFreq = Measurements.AutoPowerSpectrum(selection64Hz, dtHighFreq, out dfHighFreq);
                            var singlePSDHighFreq = Measurements.SpectrumUnitConversion(autoPSDHighFreq, SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, dfHighFreq, equivalentNoiseBandwidthHighFreq, coherentGainHighFreq, unit);

                            var lowFreqSpectrum = singlePSDLowFreq.Select((value, index) => new Point((index + 1) * dfLowFreq, value)).Where(value => value.X <= 1064);
                            var highFreqSpectrum = singlePSDLowFreq.Select((value, index) => new Point((index + 1) * dfHighFreq, value)).Where(value => value.X > 1064);

                            noisePSD = new Point[lowFreqSpectrum.Count() + highFreqSpectrum.Count()];

                            var counter = 0;
                            foreach (var item in lowFreqSpectrum)
                            {
                                noisePSD[counter].X = item.X;
                                noisePSD[counter].Y += item.Y;

                                ++counter;
                            }
                            foreach (var item in highFreqSpectrum)
                            {
                                noisePSD[counter].X = item.X;
                                noisePSD[counter].Y += item.Y;

                                ++counter;
                            }

                            if (averagingCounter % updateNumber == 0)
                            {
                                sb = new StringBuilder();
                                sb.Append("NS");

                                for (int i = 0; i < noisePSD.Length; i++)
                                    sb.AppendFormat("{0}\t{1}\r\n", (noisePSD[i].X).ToString(NumberFormatInfo.InvariantInfo), (noisePSD[i].Y / (double)averagingCounter / (kAmpl * kAmpl)).ToString(NumberFormatInfo.InvariantInfo));

                                // Seinding the calculated spectrum data
                                onDataArrived(new ExpDataArrivedEventArgs(sb.ToString()));
                                onProgressChanged(new ProgressEventArgs((double)averagingCounter / (double)nAverages * 100.0));
                            }
                        }
                    }
                });
        }

        public override void ToDo(object Arg)
        {
            var settings = (Noise_DefinedResistanceInfo)Arg;

            foreach (var conductance in settings.SetConductanceCollection)
            {
                foreach (var voltage in settings.ScanningVoltageCollection)
                {
                    if (TT_StreamWriter != null)
                        TT_StreamWriter.Close();

                    TTSaveFileName = GetFileNameWithIncrement(string.Join("\\", settings.FilePath, "Time traces", settings.SaveFileName));

                    var mode = FileMode.OpenOrCreate;
                    var access = FileAccess.Write;

                    var info = new FileInfo(TTSaveFileName);

                    var dirName = info.DirectoryName;
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);

                    if (File.Exists(TTSaveFileName))
                    {
                        File.Create(TTSaveFileName);
                        mode = FileMode.Append;
                    }

                    TT_StreamWriter = new StreamWriter(new FileStream(TTSaveFileName, mode, access));

                    setDrainVoltage(voltage, settings.VoltageDeviation);

                    setJunctionResistance(
                        voltage,
                        settings.VoltageDeviation,
                        settings.VoltageTreshold,
                        conductance,
                        settings.ConductanceDeviation,
                        settings.StabilizationTime,
                        settings.MotionMinSpeed,
                        settings.MotionMaxSpeed,
                        settings.MotorMinPos,
                        settings.MotorMaxPos,
                        settings.NAveragesFast,
                        settings.LoadResistance);

                    setDrainVoltage(voltage, settings.VoltageDeviation);

                    confAIChannelsForDC_Measurement();
                    var voltagesBeforeNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(settings.NAveragesSlow);

                    confAIChannelsForAC_Measurement();

                    foreach (var item in boxController.AI_ChannelCollection)
                        if (item.IsEnabled)
                            item.Parameters.SetParams(FilterCutOffFrequencies.Freq_150kHz, FilterGain.gain1, PGA_GainsEnum.gain1);

                    measureNoiseSpectra(settings.SamplingFrequency, settings.SpectraAveraging, settings.UpdateNumber, settings.KPreAmpl * settings.KAmpl);

                    confAIChannelsForDC_Measurement();
                    var voltagesAfterNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(settings.NAveragesSlow);

                    // Saving to log file all the parameters of the measurement

                    var fileName = string.Join("\\", settings.FilePath, "Noise", settings.SaveFileName);
                    var dataFileName = GetFileNameWithIncrement(fileName);

                    SaveToFile(dataFileName);

                    var noiseMeasLog = new NoiseMeasurementDataLog();

                    noiseMeasLog.SampleVoltage = voltagesAfterNoiseMeasurement[0];
                    noiseMeasLog.SampleCurrent = (voltagesAfterNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[0]) / settings.LoadResistance;
                    noiseMeasLog.FileName = dataFileName;
                    noiseMeasLog.Rload = settings.LoadResistance;
                    noiseMeasLog.Uwhole = voltagesAfterNoiseMeasurement[1];
                    noiseMeasLog.URload = voltagesAfterNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[0];
                    noiseMeasLog.U0sample = voltagesBeforeNoiseMeasurement[0];
                    noiseMeasLog.U0whole = voltagesBeforeNoiseMeasurement[1];
                    noiseMeasLog.U0Rload = voltagesBeforeNoiseMeasurement[1] - voltagesBeforeNoiseMeasurement[0];
                    noiseMeasLog.U0Gate = voltagesBeforeNoiseMeasurement[2];
                    noiseMeasLog.R0sample = noiseMeasLog.U0sample / (noiseMeasLog.U0Rload / noiseMeasLog.Rload);
                    noiseMeasLog.REsample = noiseMeasLog.URload / (noiseMeasLog.URload / noiseMeasLog.Rload);
                    noiseMeasLog.Temperature0 = settings.Temperature0;
                    noiseMeasLog.TemperatureE = settings.TemperatureE;
                    noiseMeasLog.kAmpl = settings.KAmpl;
                    noiseMeasLog.NAver = settings.SpectraAveraging;
                    noiseMeasLog.Vg = voltagesAfterNoiseMeasurement[2];

                    var logFileName = string.Join("\\", settings.FilePath, "Noise", noiseMeasLog.DataLogFileName);
                    var logFileCaptureName = string.Join("\\", settings.FilePath, "Time traces", "MeasurDataCapture.dat");

                    SaveDataToLog(logFileName, noiseMeasLog.ToString());
                    SaveDataToLog(logFileCaptureName, noiseMeasLog.ToString());
                }
            }

            if (channelSwitch != null)
                if (channelSwitch.Initialized == true)
                    channelSwitch.Exit();

            if (motor != null)
            {
                motor.Disable();
                motor.Dispose();
            }

            if (boxController != null)
                while (IsRunning == true)
                    boxController.AcquisitionInProgress = false;

            onStatusChanged(new StatusEventArgs("The measurement is done!"));
        }

        private void DefResistanceNoise_DataReady(object sender, EventArgs e)
        {
            Interlocked.Increment(ref averagingCounter);
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

        async void Noise_DefinedResistance_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("TT"))
            {
                await TT_StreamWriter.WriteAsync(e.Data.Substring(2));
                //var mode = FileMode.OpenOrCreate;
                //var access = FileAccess.Write;

                //var info = new FileInfo(TTSaveFileName);

                //var dirName = info.DirectoryName;
                //if (!Directory.Exists(dirName))
                //    Directory.CreateDirectory(dirName);

                //if (File.Exists(TTSaveFileName))
                //{
                //    File.Create(TTSaveFileName);
                //    mode = FileMode.Append;
                //}

                //using(var sw = new StreamWriter(new FileStream(TTSaveFileName, mode, access)))
                //{
                //    sw.Write(e.Data.Substring(2));
                //}

                //var toWrite = Encoding.ASCII.GetBytes(e.Data.Substring(2));

                //await WriteData(toWrite, TTSaveFileName, mode, access);
            }
            else if (e.Data.StartsWith("NS"))
            {
                NoiseSpectrumFinal = e.Data.Substring(2);
            }
        }

        public override async void SaveToFile(string FileName)
        {
            var toWrite = Encoding.ASCII.GetBytes(NoiseSpectrumFinal);
            await WriteData(toWrite, FileName, FileMode.OpenOrCreate, FileAccess.Write);
        }

        private async void SaveDataToLog(string DataLogFileName, string LogData)
        {
            var mode = FileMode.Create;
            var access = FileAccess.Write;

            if (File.Exists(DataLogFileName))
                mode = FileMode.Append;

            var toWrite = Encoding.ASCII.GetBytes(LogData);
            await WriteData(toWrite, DataLogFileName, mode, access);
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

                if (boxController != null)
                    while (IsRunning == true)
                        boxController.AcquisitionInProgress = false;
            }

            base.Dispose();
        }
    }
}
