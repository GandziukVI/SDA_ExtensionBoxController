using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SpectralAnalysis
{
    public class TwoPartsFFT
    {
        public Point[] GetTwoPartsFFT(double[] timeTrace, int samplingFrequency = 500000, int nDataSamples = 1, double kAmpl = 1.0, double lowFreqStartFreq = 1.0, double cutOffLowFreq = 1600, double cutOffHighFreq = 102400, int filterOrder = 8, double filterFrequency = 6400, int lowFreqPeriod = 10, int highFreqPeriod = 10)
        {
            Point[] autoPSDLowFreq = new Point[] { };
            Point[] autoPSDHighFreq = new Point[] { };

            if (filterFrequency == -1)
                filterFrequency = cutOffLowFreq;

            var sb = new StringBuilder();

            double dtLowFreq = 0.0, dtHighFreq = 0.0;
            double dfLowFreq = 0.0, dfHighFreq = 0.0;
            double equivalentNoiseBandwidthLowFreq, equivalentNoiseBandwidthHighFreq;
            double coherentGainLowFreq, coherentGainHighFreq;

            // Subsetting samples from the entire trace
            var timeTraceSelectionList = new LinkedList<double[]>();

            var range = (int)((timeTrace.Length) / nDataSamples);

            for (int i = 0; i != nDataSamples; )
            {
                var selection = timeTrace.Where((value, index) => index >= i * range && index < (i + 1) * range).Select(val => val);
                timeTraceSelectionList.AddLast(selection.ToArray());
                ++i;
            }

            var filter = new NationalInstruments.Analysis.Dsp.Filters.EllipticLowpassFilter(filterOrder, samplingFrequency, filterFrequency, 0.1, 100.0);

            var unit = new System.Text.StringBuilder("V", 256);

            var noisePSD = new Point[] { };

            // Calculating FFT in each sample
            foreach (var trace in timeTraceSelectionList)
            {
                // Calculation of the LOW-FREQUENCY part of the spectrum

                // Filtering data for low frequency selection

                var filteredData = filter.FilterData(trace);
                var sw = ScaledWindow.CreateRectangularWindow();

                // Selecting lower amount of data points to reduce the FFT noise

                var selectionLowFreq = filteredData.Where((value, index) => (index) % lowFreqPeriod == 0).ToArray();

                sw.Apply(selectionLowFreq, out equivalentNoiseBandwidthLowFreq, out coherentGainLowFreq);

                dtLowFreq = lowFreqPeriod * 1.0 / (double)samplingFrequency;

                var singlePSD_LOW_Freq = Measurements.AutoPowerSpectrum(selectionLowFreq, dtLowFreq, out dfLowFreq);

                autoPSDLowFreq = (singlePSD_LOW_Freq.Select((value, index) => new Point(index * dfLowFreq, value)).Where(p => p.X >= 1 && p.X <= cutOffLowFreq)).ToArray();

                // Calculation of the HIGH-FREQUENCY part of the spectrum

                dtHighFreq = 1.0 / (double)samplingFrequency;

                var highFreqSelectionRange = (int)((timeTrace.Length) / highFreqPeriod);

                LinkedList<double[]> selectionList = new LinkedList<double[]>();

                for (int i = 0; i != highFreqPeriod; )
                {
                    var arr = new double[highFreqSelectionRange];
                    Array.Copy(timeTrace, i * highFreqSelectionRange, arr, 0, highFreqSelectionRange);
                    selectionList.AddLast(arr);
                    ++i;
                }

                var cumulativePSD_HIGH_Freq = new double[] { };

                foreach (var selection in selectionList)
                {
                    sw.Apply(selection, out equivalentNoiseBandwidthHighFreq, out coherentGainHighFreq);
                    var singlePSD_HIGH_Freq = Measurements.AutoPowerSpectrum(selection, dtHighFreq, out dfHighFreq);

                    if (cumulativePSD_HIGH_Freq.Length == 0)
                        cumulativePSD_HIGH_Freq = Enumerable.Repeat(0.0, singlePSD_HIGH_Freq.Length).ToArray();

                    for (int i = 0; i != singlePSD_HIGH_Freq.Length; )
                    {
                        cumulativePSD_HIGH_Freq[i] += singlePSD_HIGH_Freq[i];
                        ++i;
                    }
                }

                autoPSDHighFreq = (cumulativePSD_HIGH_Freq
                    .Select((value, index) => new Point(index * dfHighFreq, value)))
                    .Where(p => p.X > cutOffLowFreq && p.X <= cutOffHighFreq).ToArray();

                if (noisePSD == null || noisePSD.Length == 0)
                    noisePSD = new Point[autoPSDLowFreq.Length + autoPSDHighFreq.Length];

                var counter = 0;
                for (int i = 0; i != autoPSDLowFreq.Length; )
                {
                    var item = autoPSDLowFreq[i];
                    noisePSD[counter].X = item.X;
                    noisePSD[counter].Y += item.Y;

                    ++counter;
                    ++i;
                }
                for (int i = 0; i != autoPSDHighFreq.Length; )
                {
                    var item = autoPSDHighFreq[i];
                    noisePSD[counter].X = item.X;
                    noisePSD[counter].Y += item.Y / ((double)(highFreqPeriod * highFreqPeriod));

                    ++counter;
                    ++i;
                }
            }

            return (from item in noisePSD
                    select new Point(item.X, item.Y / (kAmpl * kAmpl))).ToArray();
        }

        public Point[] GetCalibratedSpecteum(ref Point[] spectrumData, ref Point[] amplifierNoise, ref Point[] frequencyResponce, bool useInterpolation = false)
        {
            if (useInterpolation == false)
            {
                var query = (from spectrumPoint in spectrumData
                            join amplifierNoisePoint in amplifierNoise on spectrumPoint.X equals amplifierNoisePoint.X
                            join frequencyResponcePoint in frequencyResponce on spectrumPoint.X equals frequencyResponcePoint.X
                            select new Point(spectrumPoint.X, (spectrumPoint.Y - amplifierNoisePoint.Y) / (frequencyResponcePoint.Y * frequencyResponcePoint.Y))).ToArray();

                if (query.Length != spectrumData.Length)
                    throw new Exception("Noise spectrum and calibration data don't have the same format! Please check your measurement settings.");


                return query;
            }
            else
            {
                var xValuesAmpNoise = from item in amplifierNoise
                                      select item.X;
                var yValuesAmpNoise = from item in amplifierNoise
                                      select item.Y;

                var xValuesFrequencyResponce = from item in frequencyResponce
                                               select item.X;
                var yValuesFrequencyResponce = from item in frequencyResponce
                                               select item.Y;


                var interpolationSplineAmpNoise = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(xValuesAmpNoise, yValuesAmpNoise);
                var interpolationSplineFrequencyResponce = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(xValuesFrequencyResponce, yValuesFrequencyResponce);

                var query = from spectrumPoint in spectrumData
                            select new Point(spectrumPoint.X, (spectrumPoint.Y - interpolationSplineAmpNoise.Interpolate(spectrumPoint.X)) / (Math.Pow(interpolationSplineFrequencyResponce.Interpolate(spectrumPoint.X), 2)));


                return query.ToArray();
            }
        }
    }
}
