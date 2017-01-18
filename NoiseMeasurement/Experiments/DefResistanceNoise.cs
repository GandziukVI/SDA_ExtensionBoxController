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

namespace NoiseMeasurement.Experiments
{
    public class DefResistanceNoise : ExperimentBase
    {
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

            var freq = 499712;
            var avgNumber = 100;

            int averagingCounter = 0;

            double[] autoPSD;
            double[] noisePSD = new double[freq / 2];

            while (true)
            {
                if (!IsRunning)
                    break;
                if (averagingCounter >= avgNumber)
                    break;

                b.StartAnalogAcquisition(freq);

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

                    double dt, df, equivalentNoiseBandwidth, coherentGain;

                    var unit = new System.Text.StringBuilder("V", 256);
                    var sw = ScaledWindow.CreateRectangularWindow();

                    sw.Apply(traceData, out equivalentNoiseBandwidth, out coherentGain);

                    dt = 1.0 / (double)freq;

                    autoPSD = Measurements.AutoPowerSpectrum(traceData, dt, out df);
                    var singlePSD = Measurements.SpectrumUnitConversion(autoPSD,SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, df, equivalentNoiseBandwidth, coherentGain, unit);

                    for (int i = 0; i < singlePSD.Length; i++)
                        noisePSD[i] += singlePSD[i];

                    ++averagingCounter;
                }
            }



            b.Close();
        }
    }
}
