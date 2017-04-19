using ExperimentController;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments
{
    public class FET_Transfer_Experiment : ExperimentBase
    {
        private ISourceMeterUnit smuVds;
        private ISourceMeterUnit smuVg;

        public FET_Transfer_Experiment(ISourceMeterUnit SMU_Vds, ISourceMeterUnit SMU_Vg)
            : base()
        {
            smuVds = SMU_Vds;
            smuVg = SMU_Vg;
        }

        public override void ToDo(object Arg)
        {
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

            onStatusChanged(new StatusEventArgs("Measurement completed!"));
        }

        public override void Stop()
        {
            IsRunning = false;
        }
    }
}
