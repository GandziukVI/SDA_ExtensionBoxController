using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments;
using NationalInstruments.NI4882;
using NationalInstruments.VisaNS;
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Tdms;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using System.IO;
using System.Globalization;
using ExperimentController;
using NoiseMeasurement.Experiments;

namespace NoiseMeasurement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var a = new DefResistanceNoise() as IExperiment;
            a.Start();

            //BoxController b = new BoxController();
            //b.Init("USB0::0x0957::0x1718::TW54334510::INSTR");

            //var _ch = new AI_ChannelConfig[4]
            //{
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn1, Enabled = true, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn2, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn3, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25},
            //    new AI_ChannelConfig(){ ChannelName = AnalogInChannelsEnum.AIn4, Enabled = false, Mode = ChannelModeEnum.DC, Polarity = PolarityEnum.Polarity_Bipolar, Range = RangesEnum.Range_1_25}
            //};

            //b.ConfigureAI_Channels(_ch);

            //var freq = 499712;

            //b.AcquireSingleShot(freq);

            //Point[] res;
            //b.AI_ChannelCollection[AnalogInChannelsEnum.AIn1].ChannelData.TryDequeue(out res);

            //var query = from val in res
            //            select val.Y;
            
            //var counter = 0;
            //var spectrum = new double[res.Length];
            //foreach (var item in query)
            //{
            //    spectrum[counter] = item;
            //    ++counter;
            //}

            //StringBuilder unit = new System.Text.StringBuilder("V", 256);

            //double dt, df, equivalentNoiseBandwidth, coherentGain;
            //ScaledWindow sw = ScaledWindow.CreateRectangularWindow();
            //sw.Apply(spectrum, out equivalentNoiseBandwidth, out coherentGain);

            //dt = 1.0 / (double)freq;

            //var autoPsd = Measurements.AutoPowerSpectrum(spectrum, dt, out df);
            //var specConversion = Measurements.SpectrumUnitConversion(autoPsd, SpectrumType.Power, ScalingMode.Linear, DisplayUnits.VoltsPeakSquaredPerHZ, df, equivalentNoiseBandwidth, coherentGain, unit);

            //counter = 0;
            //using (var fs = new FileStream("F:\\Temp.txt", FileMode.Create, FileAccess.Write))
            //{
            //    using (var sWriter = new StreamWriter(fs))
            //    {
            //        foreach (var item in specConversion)
            //        {
            //            sWriter.Write(string.Format("{0}\t{1}\r\n", counter.ToString(NumberFormatInfo.InvariantInfo), item.ToString(NumberFormatInfo.InvariantInfo)));
            //            ++counter;
            //        }
            //    }
            //}

            //b.Close();
        }
    }
}
