using ExperimentController;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            GateVoltage = gateVoltage;
            DrainCurrent = drainCurrent;
            GateCurrent = gateCurrent;
        }

        public override string ToString()
        {
            return string.Join("\t", GateVoltage.ToString(NumberFormatInfo.InvariantInfo), DrainCurrent.ToString(NumberFormatInfo.InvariantInfo), GateCurrent.ToString(NumberFormatInfo.InvariantInfo));
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

        public override string ToString()
        {
            var header = "V\\-(G)\tI\\-(D)\tI\\-(G)";
            var subHeader = "V\tA\tA";
            var comment = string.Format("V\\-(DS) = {0}\tV\\-(DS) = {0}\tV\\-(DS) = {0}", DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo));

            var dataBuilder = new StringBuilder();

            foreach (var dataPoint in TransferCurveData)
                dataBuilder.AppendFormat("{0}\r\n", dataPoint.ToString());

            return string.Join("\r\n", header, subHeader, comment, dataBuilder.ToString());
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
            smuVg.Averaging = settings.Ke_Transfer_Averaging;
            smuVg.NPLC = settings.Ke_Transfer_NPLC;

            smuVg.Voltage = settings.TransferVgStart;

            smuVg.SwitchON();

            #endregion

            #region Drain-Source SMU settings

            smuVds.SourceMode = settings.TransferSMU_SourceMode;

            smuVds.Compliance = settings.TransferDS_Complaince;
            smuVds.Averaging = settings.Ke_Transfer_Averaging;
            smuVds.NPLC = settings.Ke_Transfer_NPLC;

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

                    singleTransferCurveData.AddLast(new TransferData(gateVoltage, drainCurrent, leakageCurrent));

                    onStatusChanged(new StatusEventArgs(string.Format("Measuring Transfer Curve # {0} out of {1}. Leakage current I = {2}", (i + 1).ToString(NumberFormatInfo.InvariantInfo), settings.TransferN_VdsStep, leakageCurrent.ToString("E4", NumberFormatInfo.InvariantInfo))));

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

                Thread.Sleep((int)(settings.Transfer_VdsDelay * 1000));
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

            SaveToFile(string.Format("{0}\\{1}", settings.TransferDataFilePath, settings.Transfer_FileName));

            onStatusChanged(new StatusEventArgs("Measurement completed!"));
        }

        public override void Stop()
        {
            IsRunning = false;
        }

        public override void SaveToFile(string FileName)
        {
            #region Saving all the data into a single file

            var formatBuilder = new StringBuilder();
            var dataBuilder = new StringBuilder();

            var headerBuilder = new StringBuilder();
            var subHeaderBuilder = new StringBuilder();
            var commentBuilder = new StringBuilder();

            var counter = 0;
            for (int i = 0; i < transferCurveDataSet.Count; i++)
            {
                headerBuilder.AppendFormat("{0}\t{1}\t{2}\t", "V\\-(G)", "I\\-(D)", "I\\-(G)");
                subHeaderBuilder.AppendFormat("{0}\t{1}\t{2}\t", "V", "A", "A");

                commentBuilder.AppendFormat(
                    "{0}\t{1}\t{2}\t",
                    string.Format("V\\-(DS) = {0}", transferCurveDataSet.ElementAt(i).DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo)),
                    string.Format("V\\-(DS) = {0}", transferCurveDataSet.ElementAt(i).DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo)),
                    string.Format("V\\-(DS) = {0}", transferCurveDataSet.ElementAt(i).DrainSourceVoltage.ToString(NumberFormatInfo.InvariantInfo)));

                formatBuilder.AppendFormat("{{{0}}}\t{{{1}}}\t{{{2}}}\t", counter.ToString(NumberFormatInfo.InvariantInfo), (counter + 1).ToString(NumberFormatInfo.InvariantInfo), (counter + 2).ToString(NumberFormatInfo.InvariantInfo));
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
                    arr[counter + 2] = transferCurveDataSet.ElementAt(j).TransferCurveData.ElementAt(i).GateCurrent.ToString(NumberFormatInfo.InvariantInfo);

                    counter += 3;
                }

                dataBuilder.AppendFormat(formatString, arr);
            }

            using (var writer = new StreamWriter(new FileStream(FileName, FileMode.Create, FileAccess.Write)))
            {
                var toWrite = string.Join("", header, subHeader, comment, dataBuilder.ToString());
                writer.Write(toWrite);
            }

            #endregion

            #region Saving the data into separate files

            var singleCurveDirName = Path.GetFileNameWithoutExtension(FileName);
            var singleCurveDirInfo = new DirectoryInfo(singleCurveDirName);

            if (!singleCurveDirInfo.Exists)
                singleCurveDirInfo.Create();

            foreach (var item in transferCurveDataSet)
            {
                var singleCurveDataFileName = string.Format(
                    "{0} {1}{2}",
                    string.Join("\\", singleCurveDirName, singleCurveDirInfo.Name),
                    string.Format("V\\-(DS) = {0} V", Math.Round(item.DrainSourceVoltage, 3).ToString("G3", NumberFormatInfo.InvariantInfo)),
                    Path.GetExtension(FileName)
                    );

                using (var sw = new StreamWriter(new FileStream(singleCurveDataFileName, FileMode.Create, FileAccess.Write)))
                {
                    sw.Write(item.ToString());
                }
            }

            #endregion
        }
    }
}
