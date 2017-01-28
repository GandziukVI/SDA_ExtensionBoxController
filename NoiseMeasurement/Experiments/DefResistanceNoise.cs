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

namespace NoiseMeasurement.Experiments
{
    public class DefResistanceNoise : ExperimentBase
    {
        private static int averagingCounter = 0;

        public override void ToDo(object Arg)
        {
            BoxController b = new BoxController();
            b.Init("USB0::0x0957::0x1718::TW54334510::INSTR");

            var _ch = new AI_ChannelConfig[4]
            {
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
                new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25}
            };

            b.ConfigureAI_Channels(_ch);

            var freq = 200000;
            var updNumber = 2;
            var avgNumber = 10;

            double[] autoPSD;
            double[] noisePSD = new double[freq / 2];

            if (freq % 2 != 0)
                throw new ArgumentException("The frequency should be an even number!");

            b.AcquisitionInProgress = true;
            b.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].DataReady += DefResistanceNoise_DataReady;

            var sb = new StringBuilder();

            double dt = 0.0, df = 1.0, equivalentNoiseBandwidth, coherentGain;

            Parallel.Invoke(
                () =>
                {
                    b.StartAnalogAcquisition(freq);
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

                            sw.Apply(traceData, out equivalentNoiseBandwidth, out coherentGain);

                            dt = 1.0 / (double)freq;

                            autoPSD = Measurements.AutoPowerSpectrum(traceData, dt, out df);
                            var singlePSD = Measurements.SpectrumUnitConversion(autoPSD, SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, df, equivalentNoiseBandwidth, coherentGain, unit);

                            for (int i = 0; i < singlePSD.Length; i++)
                                noisePSD[i] += singlePSD[i];

                            if (averagingCounter % updNumber == 0)
                            {
                                sb = new StringBuilder();

                                for (int i = 0; i < noisePSD.Length; i++)
                                    sb.AppendFormat("{0}\t{1}\r\n", ((i + 1) * df).ToString(NumberFormatInfo.InvariantInfo), (noisePSD[i] / (double)averagingCounter).ToString(NumberFormatInfo.InvariantInfo));

                                onDataArrived(new ExpDataArrivedEventArgs(sb.ToString()));
                            }
                        }
                    }

                    sb = new StringBuilder();

                    for (int i = 0; i < noisePSD.Length; i++)
                        sb.AppendFormat("{0}\t{1}\r\n", ((i + 1) * df).ToString(NumberFormatInfo.InvariantInfo), (noisePSD[i] / (double)averagingCounter).ToString(NumberFormatInfo.InvariantInfo));

                    onDataArrived(new ExpDataArrivedEventArgs(sb.ToString()));
                });

            b.Close();
        }

        void DefResistanceNoise_DataReady(object sender, EventArgs e)
        {
            Interlocked.Increment(ref averagingCounter);
        }
    }
}
