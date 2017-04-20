using ExperimentController;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments
{
    class TransferData
    {
        public double GateVoltage { get; set; }
        public double DrainCurrent { get; set; }
        public double GateCurrent { get; set; }

        public TransferData(double gateVoltage, double drainCurrent, double gateCurrent)
        {
            GateVoltage = GateVoltage;
            DrainCurrent = drainCurrent;
            GateCurrent = gateCurrent;
        }

        public override string ToString()
        {
            return string.Join("\t", GateVoltage, DrainCurrent, GateCurrent);
        }
    }

    class TransferDataContainer
    {
        public double DrainSourceVoltage { get; set; }
        public LinkedList<TransferData> TransferCurveData { get; set; }

        public TransferDataContainer(double drainSourceVoltage)
        {
            DrainSourceVoltage = drainSourceVoltage;
            TransferCurveData = new LinkedList<TransferData>();
        }

        public TransferDataContainer(double drainSourceVoltage, LinkedList<TransferData> data)
        {
            DrainSourceVoltage = drainSourceVoltage;
            TransferCurveData = data;
        }
    }

    public class FET_Transfer_Experiment : ExperimentBase
    {
        private ISourceMeterUnit smuVds;
        private ISourceMeterUnit smuVg;

        private LinkedList<TransferData> singleTransferCurveData;
        private LinkedList<TransferDataContainer> transferCurveDataSet;

        public FET_Transfer_Experiment(ISourceMeterUnit SMU_Vds, ISourceMeterUnit SMU_Vg)
            : base()
        {
            smuVds = SMU_Vds;
            smuVg = SMU_Vg;
        }

        public override void ToDo(object Arg)
        {
            transferCurveDataSet = new LinkedList<TransferDataContainer>();

            var settings = (FET_IVModel)Arg;

            #region Gate SMU settings

            smuVg.SourceMode = SMUSourceMode.Voltage;

            smuVg.Compliance = settings.TransferGate_Complaince;
            smuVg.Averaging = 1;
            smuVg.NPLC = 1.0;

            smuVg.Voltage = settings.TransferVgStart;

            smuVg.SwitchON();

            #endregion

            #region Drain-Source SMU settings

            smuVds.SourceMode = settings.TransferSMU_SourceMode;

            smuVds.Compliance = settings.TransferDS_Complaince;
            smuVds.Averaging = 1;
            smuVds.NPLC = 1.0;

            switch (settings.TransferSMU_SourceMode)
            {
                case SMUSourceMode.Voltage:
                    smuVds.Voltage = settings.TransferVdsStart;
                    break;
                case SMUSourceMode.Current:
                    smuVds.Current = settings.TransferVdsStart;
                    break;
                case SMUSourceMode.ModeNotSet:
                    break;
                default:
                    break;
            }

            smuVds.SwitchON();

            #endregion

            #region General settings

            var currentVg = settings.TransferVgStart;
            var current_DS_value = settings.TransferVdsStart;

            var dVg = (settings.TransferVgStop - settings.TransferVgStart) / (settings.TransferN_VgSweep - 1);
            var d_DS_value = (settings.TransferVdsStop - settings.TransferVdsStart) / (settings.TransferN_VdsStep - 1);

            #endregion

            for (int i = 0; i < settings.TransferN_VdsStep; i++)
            {
                onStatusChanged(new StatusEventArgs(string.Format("Measuring Transfer Curve # {0} out of {1}", (i + 1).ToString(NumberFormatInfo.InvariantInfo), settings.TransferN_VdsStep)));
                onDataArrived(new ExpDataArrivedEventArgs(string.Format("Vds = {0}", current_DS_value.ToString(NumberFormatInfo.InvariantInfo))));

                if (!IsRunning)
                    break;

                singleTransferCurveData = new LinkedList<TransferData>();

                for (int j = 0; j <= settings.TransferN_VgSweep; j++)
                {
                    if (!IsRunning)
                        break;

                    smuVg.Voltage = currentVg;

                    var gateVoltage = currentVg;
                    var drainCurrent = smuVds.Current;
                    var leakageCurrent = smuVg.Current;

                    onDataArrived(new ExpDataArrivedEventArgs(string.Format(
                        "{0}\t{1}\t{2}\r\n",
                        gateVoltage.ToString(NumberFormatInfo.InvariantInfo),
                        drainCurrent.ToString(NumberFormatInfo.InvariantInfo),
                        leakageCurrent.ToString(NumberFormatInfo.InvariantInfo))));

                    currentVg += dVg;

                    var gateProgress = (1.0 - (settings.TransferN_VgSweep - (double)j) / settings.TransferN_VgSweep) / settings.TransferN_VdsStep;
                    var dsProgress = 1.0 - (settings.TransferN_VdsStep - (double)i) / settings.TransferN_VdsStep;

                    var totalProgress = (gateProgress + dsProgress) * 100.0;

                    onProgressChanged(new ProgressEventArgs(totalProgress));
                }

                var trData = new TransferDataContainer(current_DS_value, singleTransferCurveData);

                transferCurveDataSet.AddLast(trData);

                currentVg = settings.TransferVgStart;
                smuVg.Voltage = currentVg;

                if (i != settings.TransferN_VdsStep - 1)
                {
                    current_DS_value += d_DS_value;

                    switch (settings.TransferSMU_SourceMode)
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

            SaveToFile(settings.Transfer_FileName);

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
            for (int i = 0; i < transferCurveDataSet.Count; i++)
            {
                headerBuilder.AppendFormat("{0}\t{1}\t{2}\t", "V\\-(G)", "I\\-(DS)", "I\\-(G)");
                subHeaderBuilder.AppendFormat("{{0}}\t{{1}}\t{{2}}\t", "V", "A", "A");

                commentBuilder.AppendFormat(
                    "{{0}}\t{{1}}\t{{2}}\t", 
                    string.Format("V\\-(DS) = {0}", transferCurveDataSet.ElementAt(i).DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo)),
                    string.Format("V\\-(DS) = {0}", transferCurveDataSet.ElementAt(i).DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo)),
                    string.Format("V\\-(DS) = {0}", transferCurveDataSet.ElementAt(i).DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo)));

                formatBuilder.AppendFormat("{{0}}\t{{1}}\t{{2}}\t", counter.ToString(NumberFormatInfo.InvariantInfo), (counter + 1).ToString(NumberFormatInfo.InvariantInfo), (counter + 2).ToString(NumberFormatInfo.InvariantInfo));
                counter += 3;
            }

            var formatString = string.Join("", formatBuilder.ToString().TrimEnd('\t'), "\r\n");

            var header = string.Join("", headerBuilder.ToString().TrimEnd('\t'), "\r\n");
            var subHeader = string.Join("", subHeaderBuilder.ToString().TrimEnd('\t'), "\r\n");
            var comment = string.Join("", commentBuilder.ToString().TrimEnd('\t'), "\r\n");

            var minLen = transferCurveDataSet.First.Value.TransferCurveData.Count;

            foreach (var item in transferCurveDataSet)
                if (item.TransferCurveData.Count < minLen)
                    minLen = item.TransferCurveData.Count;

            for (int i = 0; i < minLen; i++)
            {
                counter = 0;
                var arr = new string[transferCurveDataSet.Count * 3];
                for (int j = 0; j < transferCurveDataSet.Count; j++)
                {
                    arr[counter] = transferCurveDataSet.ElementAt(j).TransferCurveData.ElementAt(i).GateVoltage.ToString(NumberFormatInfo.InvariantInfo);
                    arr[counter + 1] = transferCurveDataSet.ElementAt(j).TransferCurveData.ElementAt(i).DrainCurrent.ToString(NumberFormatInfo.InvariantInfo);
                    arr[counter + 3] = transferCurveDataSet.ElementAt(j).TransferCurveData.ElementAt(i).DrainCurrent.ToString(NumberFormatInfo.InvariantInfo);

                    counter += 3;
                }

                dataBuilder.AppendFormat(formatString, arr);
            }

            using (var writer = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)))
            {
                var toWrite = string.Join("", header, subHeader, dataBuilder.ToString());
                writer.Write(toWrite);
            }
        }
    }
}
