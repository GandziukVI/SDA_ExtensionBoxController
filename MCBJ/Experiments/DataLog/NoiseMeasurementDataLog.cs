using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments.DataLog
{
    public class NoiseMeasurementDataLog
    {
        public double SampleVoltage { get; set; }
        public double SampleCurrent { get; set; }
        public double EquivalentResistance { get; set; }
        public string FileName { get; set; }
        public double Rload { get; set; }
        public double Uwhole { get; set; }
        public double URload { get; set; }
        public double U0sample { get; set; }
        public double U0whole { get; set; }
        public double U0Rload { get; set; }
        public double U0Gate { get; set; }
        public double R0sample { get; set; }
        public double REsample { get; set; }
        public double Temperature0 { get; set; }
        public double TemperatureE { get; set; }
        public double kAmpl { get; set; }
        public double NAver { get; set; }
        public double Vg { get; set; }

        public string DataHeader
        {
            get { return "U\\-(sample)\tCurrent\tR\\-(Eq)\tFilename\tR\\-(load)\tU\\-(Whole)\tU\\-(Rload)\tU\\-(0sample)\tU\\-(0Whole)\tU\\-(0Rload)\tU\\-(0Gate)\tR\\-(0sample)\tR\\-(Esample)\tTemperature\\-(0)\tTemperature\\-(E)\tk\\-(ampl)\tN\\-(aver)\tV\\-(g)"; }
        }

        public string DataSubHeader
        {
            get { return "V\tA\t\\g(W)\t\t\\g(W)\tV\tV\tV\t\\g(W)\t\\g(W)\tK\tK\t\t\tV"; }
        }

        public string DataLogFileName
        {
            get { return "MeasurData.dat"; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            
            sb.AppendFormat(

                "{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}\t{16}\t{17}\n",

                SampleVoltage.ToString(NumberFormatInfo.InvariantInfo),
                SampleCurrent.ToString(NumberFormatInfo.InvariantInfo),
                EquivalentResistance.ToString(NumberFormatInfo.InvariantInfo),
                FileName.ToString(NumberFormatInfo.InvariantInfo),
                Rload.ToString(NumberFormatInfo.InvariantInfo),
                Uwhole.ToString(NumberFormatInfo.InvariantInfo),
                URload.ToString(NumberFormatInfo.InvariantInfo),
                U0sample.ToString(NumberFormatInfo.InvariantInfo),
                U0whole.ToString(NumberFormatInfo.InvariantInfo),
                U0Rload.ToString(NumberFormatInfo.InvariantInfo),
                U0Gate.ToString(NumberFormatInfo.InvariantInfo),
                R0sample.ToString(NumberFormatInfo.InvariantInfo),
                REsample.ToString(NumberFormatInfo.InvariantInfo),
                Temperature0.ToString(NumberFormatInfo.InvariantInfo),
                TemperatureE.ToString(NumberFormatInfo.InvariantInfo),
                kAmpl.ToString(NumberFormatInfo.InvariantInfo),
                NAver.ToString(NumberFormatInfo.InvariantInfo),
                Vg.ToString(NumberFormatInfo.InvariantInfo)

                );

            return sb.ToString();
        }
    }
}
