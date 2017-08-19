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

        StreamWriter TT_StreamWriter;

        FET_NoiseModel experimentSettings;

        Point[] amplifierNoise;
        Point[] frequencyResponce;

        TwoPartsFFT twoPartsFFT;

        public FET_Noise_Experiment(string SDA_ConnectionString, Point[] AmplifierNoise, Point[] FrequencyResponce)
            : base()
        {
            boxController = new BoxController();

            var boxInit = boxController.Init(SDA_ConnectionString);

            if (!boxInit)
                throw new Exception("Cannot connect the box.");

            VdsMotorPotentiometer = new BS350_MotorPotentiometer(boxController, BOX_AnalogOutChannelsEnum.BOX_AOut_02);
            VgMotorPotentiometer = new BS350_MotorPotentiometer(boxController, BOX_AnalogOutChannelsEnum.BOX_AOut_09);

            amplifierNoise = AmplifierNoise;
            frequencyResponce = FrequencyResponce;
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
            if (!isDCMode)
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
            else
                return new double[] { };
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

        void confAIChannelsForAC_Measurement()
        {
            if (!isACMode)
            {
                var init_conf = setACConf(9.99);
                boxController.ConfigureAI_Channels(init_conf);

                // Erasing the data queue

                Point[] temp;
                while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.IsEmpty)
                    boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out temp);

                // Acquiring single shot with AC data

                boxController.AcquireSingleShot(1000);
                var maxAcquiredVoltage = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.Last().Max(p => p.Y);

                // Configuring the channels to measure noise

                var real_conf = setACConf(maxAcquiredVoltage);
                boxController.ConfigureAI_Channels(real_conf);

                while (!boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.IsEmpty)
                    boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out temp);

                isACMode = true;
                isDCMode = false;
            }
        }

        byte minSpeed = 10;
        byte maxSpeed = 255;

        int averagingNumberFast;

        int estimationCollectionSize = 25;
        LinkedList<Point> estimationList = new LinkedList<Point>();

        void setVoltage(BS350_MotorPotentiometer motorPotentiometer, int voltNum, double voltage, double voltageDev)
        {
            voltage = Math.Abs(voltage);
            var intervalCoarse = voltage * (1.0 - 1.0 / Math.Sqrt(2.0));

            double drainVoltageCurr = 0.0,
                drainVoltagePrev = 0.0,
                factorCoarse = 0.0;

            accuracyStopWatch.Start();

            averagingNumberFast = experimentSettings.NAveragesFast;

            while (true)
            {
                var voltages = boxController.VoltageMeasurement_AllChannels(averagingNumberFast);

                drainVoltageCurr = Math.Abs(voltages[voltNum]);

                var lowerVal = Math.Min(drainVoltageCurr, voltage);
                var higherVal = Math.Max(drainVoltageCurr, voltage);

                //onProgressChanged(this, new ProgressChanged_EventArgs((int)(lowerVal / higherVal * 100.0)));

                var speed = minSpeed;
                try
                {
                    if (Math.Abs(voltage - drainVoltageCurr) <= 0.05)
                    {
                        factorCoarse = (1.0 - Math.Tanh(-1.0 * Math.Abs(voltage - drainVoltageCurr) / intervalCoarse * Math.PI + Math.PI)) / 2.0;
                        speed = (byte)(minSpeed + (maxSpeed - minSpeed) * factorCoarse);
                    }
                    else
                        speed = maxSpeed;
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

        void SetDrainSourceVoltage(double voltage, double voltageDev)
        {
            var voltages = confAIChannelsForDC_Measurement();
            confAIChannelsForDC_Measurement(voltage, voltages[1], voltages[2]);

            setVoltage(VdsMotorPotentiometer, 3, voltage, voltageDev);
        }

        void SetGateVoltage(double voltage, double voltageDev)
        {
            var voltages = confAIChannelsForDC_Measurement();
            confAIChannelsForDC_Measurement(voltages[3], voltages[1], voltage);

            setVoltage(VgMotorPotentiometer, 2, voltage, voltageDev);
        }
        
        static string TTSaveFileName = "TT.dat";
        string NoiseSpectrumFinal = string.Empty;

        Point[] noisePSD = new Point[] { };

        bool acquisitionIsRunning = false;

        static int averagingCounter = 0;

        void measureNoiseSpectra(

            int samplingFrequency,
            int nDataSamples,
            int nAverages,
            int updateNumber,
            double kAmpl

            )
        {
            Interlocked.Exchange(ref averagingCounter, 0);

            if (samplingFrequency % 2 != 0)
                throw new ArgumentException("The frequency should be an even number!");

            foreach (var item in boxController.AI_ChannelCollection)
                if (item.IsEnabled)
                    item.Parameters.SetParams(FilterCutOffFrequencies.Freq_150kHz, FilterGain.gain1, PGA_GainsEnum.gain1);

            boxController.AcquisitionInProgress = true;

            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady -= FET_Noise_DataReady;
            boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady +=  FET_Noise_DataReady;

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

                        Point[] timeTrace = new Point[] { };
                        var dataReadingSuccess = boxController.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out timeTrace);

                        if (dataReadingSuccess)
                        {
                            var query = from item in timeTrace
                                        select string.Format("{0}\t{1}", item.X.ToString(NumberFormatInfo.InvariantInfo), (item.Y / kAmpl).ToString(NumberFormatInfo.InvariantInfo));

                            // First sending the time trace data before FFT
                            onDataArrived(new ExpDataArrivedEventArgs(string.Format("TT{0}", string.Join("\r\n", query))));

                            var TTVoltageValues = (from item in timeTrace
                                                   select item.Y).ToArray();

                            var singleNoiseSpectrum = twoPartsFFT.GetTwoPartsFFT(TTVoltageValues, samplingFrequency = experimentSettings.SamplingFrequency, experimentSettings.NSubSamples);

                            if (noisePSD == null || noisePSD.Length == 0)
                            {
                                noisePSD = new Point[singleNoiseSpectrum.Length];
                                for (int i = 0; i < singleNoiseSpectrum.Length; i++)
                                    noisePSD[i] = new Point(singleNoiseSpectrum[i].X, 0.0);
                            }

                            for (int i = 0; i < noisePSD.Length; i++)
                                noisePSD[i].Y += singleNoiseSpectrum[i].Y;

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
                        }
                    }
                },
                async () =>
                {
                    acquisitionTaskResult = Task.Factory.StartNew(() => { boxController.StartAnalogAcquisition(samplingFrequency); });
                    await acquisitionTaskResult;
                });

            acquisitionTaskResult.Wait();
        }

        public override void ToDo(object Arg)
        {
            onStatusChanged(new StatusEventArgs("Measurement started."));
            onProgressChanged(new ProgressEventArgs(0.0));

            experimentSettings = (FET_NoiseModel)Arg;

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

            confAIChannelsForDC_Measurement();

            double[] outerLoopCollection;
            double[] innerLoopCollection;

            if(experimentSettings.IsOutputCurveMode == true)
            {
                outerLoopCollection = experimentSettings.GateVoltageCollection;
                innerLoopCollection = experimentSettings.DSVoltageCollection;
            }
            else if(experimentSettings.IsTransferCurveMode == true)
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
                for (int j = 0; j < innerLoopCollection.Length;j++)
                {
                    var innerLoopVoltage = innerLoopCollection[i];
                    if (!IsRunning)
                        break;

                    #region Recording time trace FileStream settings

                    if (TT_StreamWriter != null)
                        TT_StreamWriter.Close();

                    if (experimentSettings.RecordTimeTraces == true)
                    {
                        TTSaveFileName = GetFileNameWithIncrement(string.Join("\\", experimentSettings.FilePath, "Time traces", experimentSettings.SaveFileName));
                        createFileWithHeader(TTSaveFileName, ref mode, ref access, "", "");// "Time\tVoltage\n", "s\tV\n");
                        TT_StreamWriter = new StreamWriter(new FileStream(TTSaveFileName, FileMode.Append, FileAccess.Write));
                    }

                    #endregion

                    if (experimentSettings.IsOutputCurveMode == true)
                    {
                        onStatusChanged(new StatusEventArgs(string.Format("Setting gate voltage V -> {0} V", outerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                        SetGateVoltage(outerLoopVoltage, experimentSettings.VoltageDeviation);
                        onStatusChanged(new StatusEventArgs(string.Format("Setting drain-source voltage V -> {0} V", innerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                        SetDrainSourceVoltage(innerLoopVoltage, experimentSettings.VoltageDeviation);
                    }
                    else if (experimentSettings.IsTransferCurveMode == true)
                    {
                        onStatusChanged(new StatusEventArgs(string.Format("Setting gate voltage V -> {0} V", innerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                        SetGateVoltage(innerLoopVoltage, experimentSettings.VoltageDeviation);
                        onStatusChanged(new StatusEventArgs(string.Format("Setting drain-source voltage V -> {0} V", outerLoopVoltage.ToString("0.0000", NumberFormatInfo.InvariantInfo))));
                        SetDrainSourceVoltage(outerLoopVoltage, experimentSettings.VoltageDeviation);
                    }

                    onStatusChanged(new StatusEventArgs("Measuring sample characteristics before noise spectra measurement."));

                    confAIChannelsForDC_Measurement();
                    var voltagesBeforeNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);

                    onStatusChanged(new StatusEventArgs("Measuring noise spectra & time traces."));


                    confAIChannelsForAC_Measurement();
                    Thread.Sleep((int)(experimentSettings.StabilizationTime * 1000));

                    measureNoiseSpectra(experimentSettings.SamplingFrequency, experimentSettings.NSubSamples, experimentSettings.SpectraAveraging, experimentSettings.UpdateNumber, experimentSettings.KPreAmpl * experimentSettings.KAmpl);

                    onStatusChanged(new StatusEventArgs("Measuring sample characteristics after noise spectra measurement."));

                    confAIChannelsForDC_Measurement();
                    var voltagesAfterNoiseMeasurement = boxController.VoltageMeasurement_AllChannels(experimentSettings.NAveragesSlow);

                    // Saving to log file all the parameters of the measurement

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
                        SaveDataToLog(logFileCaptureName, noiseMeasLog.ToString());
                }
            }

            if (boxController != null)
            {
                while (boxController.AcquisitionInProgress == true)
                {
                    boxController.AcquisitionInProgress = false;
                    acquisitionIsRunning = false;
                }

                boxController.Close();
            }

            onStatusChanged(new StatusEventArgs("The measurement is done!"));

            Dispose();
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

        //private StringBuilder dataBuilder = new StringBuilder();
        void Noise_DefinedResistance_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("TT"))
            {
                if (experimentSettings.RecordTimeTraces == true)
                {
                    if (experimentSettings.SamplingFrequency == experimentSettings.RecordingFrequency)
                        TT_StreamWriter.Write(e.Data.Substring(2));
                    else
                    {
                        if (dataBuilder != null)
                            dataBuilder.Clear();

                        var n = experimentSettings.SamplingFrequency / experimentSettings.RecordingFrequency;
                        var selectedData = string.Join
                            (
                                "\r\n",
                                e.Data.Substring(2).Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where((value, index) => index % n == 0)
                            );

                        TT_StreamWriter.Write(selectedData);
                    }
                }
            }
            else if (e.Data.StartsWith("NS"))
            {
                NoiseSpectrumFinal = e.Data.Substring(2);
            }
        }

        #endregion

        private void FET_Noise_DataReady(object sender, EventArgs e)
        {
            Interlocked.Increment(ref averagingCounter);
        }

        StringBuilder dataBuilder = new StringBuilder();
        void FET_Noise_Experiment_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            if (e.Data.StartsWith("TT"))
            {
                if (experimentSettings.RecordTimeTraces == true)
                {
                    if (experimentSettings.SamplingFrequency == experimentSettings.RecordingFrequency)
                        TT_StreamWriter.Write(e.Data.Substring(2));
                    else
                    {
                        if (dataBuilder != null)
                            dataBuilder.Clear();

                        var n = experimentSettings.SamplingFrequency / experimentSettings.RecordingFrequency;
                        var selectedData = string.Join
                            (
                                "\r\n",
                                e.Data.Substring(2).Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Where((value, index) => index % n == 0)
                            );

                        TT_StreamWriter.Write(selectedData);
                    }
                }
            }
            else if (e.Data.StartsWith("NS"))
            {
                NoiseSpectrumFinal = e.Data.Substring(2);
            }
        }

        public override void Stop()
        {
            if (TT_StreamWriter != null)
                TT_StreamWriter.Close();

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
