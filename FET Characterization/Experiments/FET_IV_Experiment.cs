using ExperimentController;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments
{
    class IV_FET_Data
    {
        public double DrainVoltage { get; set; }
        public double DrainCurrent { get; set; }
        public double GateCurrent { get; set; }

        public IV_FET_Data(double drainVoltage, double drainCurrent, double gateCurrent)
        {
            DrainVoltage = drainVoltage;
            DrainCurrent = drainCurrent;
            GateCurrent = gateCurrent;
        }

        public override string ToString()
        {
            return string.Join("\t", DrainVoltage, DrainCurrent, GateCurrent);
        }
    }

    class IV_FET_DataContainer
    {
        public double GateVoltage { get; set; }
        public LinkedList<IV_FET_Data> IV_FET_CurveData { get; set; }

        public IV_FET_DataContainer(double gateVoltage)
        {
            GateVoltage = gateVoltage;
            IV_FET_CurveData = new LinkedList<IV_FET_Data>();
        }

        public IV_FET_DataContainer(double gateVoltage, LinkedList<IV_FET_Data> data)
        {
            GateVoltage = gateVoltage;
            IV_FET_CurveData = data;
        }
    }

    public class FET_IV_Experiment : ExperimentBase
    {
        private ISourceMeterUnit smuVds;
        private ISourceMeterUnit smuVg;

        private LinkedList<IV_FET_Data> singleIV_FET_CurveData;
        private LinkedList<IV_FET_DataContainer> iv_FET_CurveDataSet;

        public FET_IV_Experiment(ISourceMeterUnit SMU_Vds, ISourceMeterUnit SMU_Vg)
            : base()
        {
            smuVds = SMU_Vds;
            smuVg = SMU_Vg;
        }

        public override void ToDo(object Arg)
        {
            iv_FET_CurveDataSet = new LinkedList<IV_FET_DataContainer>();

            var settings = (FET_IVModel)Arg;

            #region Gate SMU settings

            smuVg.SourceMode = SMUSourceMode.Voltage;

            smuVg.Compliance = settings.Gate_Complaince;
            smuVg.Averaging = 1;
            smuVg.NPLC = 1.0;

            smuVg.Voltage = settings.VgStart;

            smuVg.SwitchON();

            #endregion

            #region Drain-Source SMU settings

            smuVds.SourceMode = settings.SMU_SourceMode;

            smuVds.Compliance = settings.DS_Complaince;
            smuVds.Averaging = 1;
            smuVds.NPLC = 1.0;

            switch (settings.SMU_SourceMode)
            {
                case SMUSourceMode.Voltage:
                    smuVds.Voltage = settings.VdsStart;
                    break;
                case SMUSourceMode.Current:
                    smuVds.Current = settings.VdsStart;
                    break;
                case SMUSourceMode.ModeNotSet:
                    break;
                default:
                    break;
            }

            smuVds.SwitchON();

            #endregion

            #region General settings

            var currentVg = settings.VgStart;
            var current_DS_value = settings.VdsStart;

            var dVg = (settings.VgStop - settings.VgStart) / (settings.N_VgStep - 1);
            var d_DS_value = (settings.VdsStop - settings.VdsStart) / (settings.N_VdsSweep - 1);

            #endregion

            if (settings.MeasureLeakage == true)
            {
                for (int i = 0; i < settings.N_VgStep; i++)
                {
                    onStatusChanged(new StatusEventArgs(string.Format("Measuring I-V Curve # {0} out of {1}", (i + 1).ToString(NumberFormatInfo.InvariantInfo), settings.N_VgStep)));
                    onDataArrived(new ExpDataArrivedEventArgs(string.Format("Vg = {0}", currentVg.ToString(NumberFormatInfo.InvariantInfo))));

                    singleIV_FET_CurveData = new LinkedList<IV_FET_Data>();

                    if (!IsRunning)
                        break;

                    for (int j = 0; j <= settings.N_VdsSweep; j++)
                    {
                        if (!IsRunning)
                            break;

                        switch (settings.SMU_SourceMode)
                        {
                            case SMUSourceMode.Voltage:
                                {
                                    smuVds.Voltage = current_DS_value;

                                    var drainVoltage = current_DS_value;
                                    var drainCurrent = smuVds.Current;
                                    var leakageCurrent = smuVg.Current;

                                    onDataArrived(new ExpDataArrivedEventArgs(string.Format(
                                        "{0}\t{1}\t{2}\r\n",
                                        drainVoltage.ToString(NumberFormatInfo.InvariantInfo),
                                        drainCurrent.ToString(NumberFormatInfo.InvariantInfo),
                                        leakageCurrent.ToString(NumberFormatInfo.InvariantInfo))));

                                    singleIV_FET_CurveData.AddLast(new IV_FET_Data(drainVoltage, drainCurrent, leakageCurrent));
                                } break;
                            case SMUSourceMode.Current:
                                {
                                    smuVds.Current = current_DS_value;

                                    var drainVoltage = smuVds.Voltage;
                                    var drainCurrent = current_DS_value;
                                    var leakageCurrent = smuVg.Current;

                                    onDataArrived(new ExpDataArrivedEventArgs(string.Format(
                                        "{0}\t{1}\t{2}\r\n",
                                        drainVoltage.ToString(NumberFormatInfo.InvariantInfo),
                                        drainCurrent.ToString(NumberFormatInfo.InvariantInfo),
                                        leakageCurrent.ToString(NumberFormatInfo.InvariantInfo))));

                                    singleIV_FET_CurveData.AddLast(new IV_FET_Data(drainVoltage, drainCurrent, leakageCurrent));
                                } break;
                            case SMUSourceMode.ModeNotSet:
                                throw new ArgumentException();
                            default:
                                throw new ArgumentException();
                        }

                        var gateProgress = 1.0 - (settings.N_VgStep - (double)i) / settings.N_VgStep;
                        var dsProgress = (1.0 - (settings.N_VdsSweep - (double)j) / settings.N_VdsSweep) / settings.N_VgStep;

                        var totalProgress = (gateProgress + dsProgress) * 100.0;

                        onProgressChanged(new ProgressEventArgs(totalProgress));

                        current_DS_value += d_DS_value;
                    }

                    var ivData = new IV_FET_DataContainer(currentVg, singleIV_FET_CurveData);
                    iv_FET_CurveDataSet.AddLast(ivData);

                    current_DS_value = settings.VdsStart;

                    switch (settings.SMU_SourceMode)
                    {
                        case SMUSourceMode.Voltage:
                            smuVds.Voltage = current_DS_value;
                            break;
                        case SMUSourceMode.Current:
                            smuVds.Current = current_DS_value;
                            break;
                        case SMUSourceMode.ModeNotSet:
                            throw new ArgumentException();
                        default:
                            throw new ArgumentException();
                    }

                    if (i != settings.N_VgStep - 1)
                    {
                        currentVg += dVg;
                        smuVg.Voltage = currentVg;
                    }
                }
            }
            else
            {
                for (int i = 0; i < settings.N_VgStep; i++)
                {
                    onStatusChanged(new StatusEventArgs(string.Format("Measuring I-V curve # {0} out of {1}", (i + 1).ToString(NumberFormatInfo.InvariantInfo), settings.N_VgStep)));
                    onDataArrived(new ExpDataArrivedEventArgs(string.Format("Vg = {0}", currentVg.ToString(NumberFormatInfo.InvariantInfo))));
                    
                    var singleIV_FET_Curve = new LinkedList<IV_FET_Data>();

                    if (!IsRunning)
                        break;

                    switch (settings.SMU_SourceMode)
                    {
                        case SMUSourceMode.Voltage:
                            {
                                var ivData = smuVds.LinearVoltageSweep(settings.VdsStart, settings.VdsStop, settings.N_VdsSweep);

                                onDataArrived(new ExpDataArrivedEventArgs(ivData.ToStringExtension()));

                                var query = from ivDataPoint in ivData
                                            select new IV_FET_Data(ivDataPoint.Voltage, ivDataPoint.Current, double.NaN);
                                foreach(var item in query)
                                    singleIV_FET_CurveData.AddLast(item);
                            } break;
                        case SMUSourceMode.Current:
                            {
                                var ivData = smuVds.LinearCurrentSweep(settings.VdsStart, settings.VdsStop, settings.N_VdsSweep);

                                onDataArrived(new ExpDataArrivedEventArgs(ivData.ToStringExtension()));

                                var query = from ivDataPoint in ivData
                                            select new IV_FET_Data(ivDataPoint.Voltage, ivDataPoint.Current, double.NaN);
                                foreach (var item in query)
                                    singleIV_FET_CurveData.AddLast(item);
                            } break;
                        case SMUSourceMode.ModeNotSet:
                            throw new ArgumentException();
                        default:
                            throw new ArgumentException();
                    }

                    var ivSingleCurveWithDescription = new IV_FET_DataContainer(currentVg, singleIV_FET_CurveData);
                    iv_FET_CurveDataSet.AddLast(ivSingleCurveWithDescription);

                    current_DS_value = settings.VdsStart;

                    if (i != settings.N_VgStep - 1)
                    {
                        currentVg += dVg;
                        smuVg.Voltage = currentVg;
                    }

                    var gateProgress = 1.0 - (settings.N_VgStep - (double)(i + 1)) / settings.N_VgStep;

                    onProgressChanged(new ProgressEventArgs(gateProgress * 100));
                }
            }

            smuVg.SwitchOFF();
            smuVds.SwitchOFF();

            smuVg.Voltage = 0.0;

            switch (settings.SMU_SourceMode)
            {
                case SMUSourceMode.Voltage:
                    smuVds.Voltage = 0.0;
                    break;
                case SMUSourceMode.Current:
                    smuVds.Current = 0.0;
                    break;
                case SMUSourceMode.ModeNotSet:
                    throw new ArgumentException();
                default:
                    throw new ArgumentException();
            }

            SaveToFile(string.Format("{0}\\{1}", settings.IV_FET_DataFilePath, settings.IV_FileName));

            onStatusChanged(new StatusEventArgs("Measurement completed!"));
        }

        public override void Stop()
        {
            IsRunning = false;
        }

        public override void SaveToFile(string FileName)
        {
            var formatBuilder = new StringBuilder();
            var dataBuilder = new StringBuilder();

            var headerBuilder = new StringBuilder();
            var subHeaderBuilder = new StringBuilder();
            var commentBuilder = new StringBuilder();

            var counter = 0;
            for (int i = 0; i < iv_FET_CurveDataSet.Count; i++)
            {
                headerBuilder.AppendFormat("{0}\t{1}\t{2}\t", "V\\-(DS)", "I\\-(D)", "I\\-(G)");
                subHeaderBuilder.AppendFormat("{0}\t{1}\t{2}\t", "V", "A", "A");

                commentBuilder.AppendFormat(
                    "{0}\t{1}\t{2}\t",
                    string.Format("V\\-(G) = {0}", iv_FET_CurveDataSet.ElementAt(i).GateVoltage.ToString(NumberFormatInfo.InvariantInfo)),
                    string.Format("V\\-(G) = {0}", iv_FET_CurveDataSet.ElementAt(i).GateVoltage.ToString(NumberFormatInfo.InvariantInfo)),
                    string.Format("V\\-(G) = {0}", iv_FET_CurveDataSet.ElementAt(i).GateVoltage.ToString(NumberFormatInfo.InvariantInfo)));

                formatBuilder.AppendFormat("{{{0}}}\t{{{1}}}\t{{{2}}}\t", counter.ToString(NumberFormatInfo.InvariantInfo), (counter + 1).ToString(NumberFormatInfo.InvariantInfo), (counter + 2).ToString(NumberFormatInfo.InvariantInfo));
                counter += 3;
            }

            var formatString = string.Join("", formatBuilder.ToString().TrimEnd('\t'), "\r\n");

            var header = string.Join("", headerBuilder.ToString().TrimEnd('\t'), "\r\n");
            var subHeader = string.Join("", subHeaderBuilder.ToString().TrimEnd('\t'), "\r\n");
            var comment = string.Join("", commentBuilder.ToString().TrimEnd('\t'), "\r\n");

            var minLen = iv_FET_CurveDataSet.First.Value.IV_FET_CurveData.Count;

            foreach (var item in iv_FET_CurveDataSet)
                if (item.IV_FET_CurveData.Count < minLen)
                    minLen = item.IV_FET_CurveData.Count;

            for (int i = 0; i < minLen; i++)
            {
                counter = 0;
                var arr = new string[iv_FET_CurveDataSet.Count * 3];
                for (int j = 0; j < iv_FET_CurveDataSet.Count; j++)
                {
                    arr[counter] = iv_FET_CurveDataSet.ElementAt(j).IV_FET_CurveData.ElementAt(i).DrainVoltage.ToString(NumberFormatInfo.InvariantInfo);
                    arr[counter + 1] = iv_FET_CurveDataSet.ElementAt(j).IV_FET_CurveData.ElementAt(i).DrainCurrent.ToString(NumberFormatInfo.InvariantInfo);
                    arr[counter + 2] = iv_FET_CurveDataSet.ElementAt(j).IV_FET_CurveData.ElementAt(i).GateCurrent.ToString(NumberFormatInfo.InvariantInfo);

                    counter += 3;
                }

                dataBuilder.AppendFormat(formatString, arr);
            }

            using (var writer = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)))
            {
                var toWrite = string.Join("", header, subHeader, comment, dataBuilder.ToString());
                writer.Write(toWrite);
            }
        }
    }
}
