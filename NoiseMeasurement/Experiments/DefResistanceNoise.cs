using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ExperimentController;
using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments.Analysis.Dsp;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Windows.Threading;
using D3Helper;

namespace NoiseMeasurement.Experiments
{
    public class DefResistanceNoise : ExperimentBase
    {
        private static int averagingCounter = 0;
        private BoxController b;

        public override void ToDo(object Arg)
        {
            b = new BoxController();
            b.Init("USB0::0x0957::0x1718::TW54334510::INSTR");

            var _ch = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25}
            };

            b.ConfigureAI_Channels(_ch);

            var freq = 500000;
            var updNumber = 1;
            var avgNumber = 100;

            double[] autoPSDLowFreq;
            double[] autoPSDHighFreq;

            Point[] noisePSD = new Point[] { };

            if (freq % 2 != 0)
                throw new ArgumentException("The frequency should be an even number!");

            b.AcquisitionInProgress = true;
            b.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady += DefResistanceNoise_DataReady;

            var sb = new StringBuilder();

            double dtLowFreq = 0.0, dtHighFreq = 0.0;
            double dfLowFreq = 1.0, dfHighFreq = 0.0;
            double equivalentNoiseBandwidthLowFreq, equivalentNoiseBandwidthHighFreq;
            double coherentGainLowFreq, coherentGainHighFreq;

            Parallel.Invoke(
                () =>
                {
                    b.StartAnalogAcquisition(freq);
                    IsRunning = false;
                },
                () =>
                {
                    while (true)
                    {
                        if (!IsRunning)
                        {
                            b.AcquisitionInProgress = false;
                            break;
                        }
                        if (averagingCounter >= avgNumber)
                        {
                            b.AcquisitionInProgress = false;
                            break;
                        }

                        Point[] timeTrace;
                        var dataReadingSuccess = b.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out timeTrace);

                        if (dataReadingSuccess)
                        {
                            var query = from val in timeTrace
                                        select val.Y;

                            var counter = 0;
                            var traceData = new double[timeTrace.Length];
                            foreach (var item in query)
                            {
                                traceData[counter] = item;
                                ++counter;
                            }

                            var unit = new System.Text.StringBuilder("V", 256);
                            var sw = ScaledWindow.CreateRectangularWindow();

                            // Calculation of the low-frequency part of the spectrum

                            sw.Apply(traceData, out equivalentNoiseBandwidthLowFreq, out coherentGainLowFreq);

                            dtLowFreq = 1.0 / (double)freq;

                            autoPSDLowFreq = Measurements.AutoPowerSpectrum(traceData, dtLowFreq, out dfLowFreq);
                            var singlePSDLowFreq = Measurements.SpectrumUnitConversion(autoPSDLowFreq, SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, dfLowFreq, equivalentNoiseBandwidthLowFreq, coherentGainLowFreq, unit);

                            // Calculation of the hugh-frequency part of the spectrum

                            var selection64Hz = PointSelector.SelectPoints(ref traceData, 64);

                            sw.Apply(selection64Hz, out equivalentNoiseBandwidthHighFreq, out coherentGainHighFreq);

                            dtHighFreq = 64.0 * 1.0 / (double)freq;

                            autoPSDHighFreq = Measurements.AutoPowerSpectrum(selection64Hz, dtHighFreq, out dfHighFreq);
                            var singlePSDHighFreq = Measurements.SpectrumUnitConversion(autoPSDHighFreq, SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, dfHighFreq, equivalentNoiseBandwidthHighFreq, coherentGainHighFreq, unit);

                            var lowFreqSpectrum = singlePSDLowFreq.Select((value, index) => new Point((index + 1) * dfLowFreq, value)).Where(value => value.X <= 1064);
                            var highFreqSpectrum = singlePSDLowFreq.Select((value, index) => new Point((index + 1) * dfHighFreq, value)).Where(value => value.X > 1064);

                            noisePSD = new Point[lowFreqSpectrum.Count() + highFreqSpectrum.Count()];

                            counter = 0;
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

                            //for (int i = 0; i < singlePSDLowFreq.Length; i++)
                            //    noisePSD[i] += singlePSDLowFreq[i];

                            if (averagingCounter % updNumber == 0)
                            {
                                sb = new StringBuilder();

                                for (int i = 0; i < noisePSD.Length; i++)
                                    sb.AppendFormat("{0}\t{1}\r\n", (noisePSD[i].X).ToString(NumberFormatInfo.InvariantInfo), (noisePSD[i].Y / (double)averagingCounter).ToString(NumberFormatInfo.InvariantInfo));

                                onDataArrived(new ExpDataArrivedEventArgs(sb.ToString()));
                            }
                        }
                    }

                    //sb = new StringBuilder();

                    //for (int i = 0; i < noisePSD.Length; i++)
                    //    sb.AppendFormat("{0}\t{1}\r\n", (noisePSD[i].X).ToString(NumberFormatInfo.InvariantInfo), (noisePSD[i].Y / (double)averagingCounter).ToString(NumberFormatInfo.InvariantInfo));

                    //onDataArrived(new ExpDataArrivedEventArgs(sb.ToString()));
                });

            b.Close();
        }

        void DefResistanceNoise_DataReady(object sender, EventArgs e)
        {
            Interlocked.Increment(ref averagingCounter);
        }

        public override void Dispose()
        {
            if (b != null)
                while (IsRunning == true)
                    b.AcquisitionInProgress = false;

            base.Dispose();
        }
    }
}
