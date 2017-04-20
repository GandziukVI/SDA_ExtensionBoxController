using ExperimentController;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments
{
    public class FET_IV_Experiment : ExperimentBase
    {
        private ISourceMeterUnit smuVds;
        private ISourceMeterUnit smuVg;

        public FET_IV_Experiment(ISourceMeterUnit SMU_Vds, ISourceMeterUnit SMU_Vg)
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

                    if (!IsRunning)
                        break;

                    switch (settings.SMU_SourceMode)
                    {
                        case SMUSourceMode.Voltage:
                            {
                                var ivData = smuVds.LinearVoltageSweep(settings.VdsStart, settings.VdsStop, settings.N_VdsSweep);

                                onDataArrived(new ExpDataArrivedEventArgs(ivData.ToStringExtension()));
                            } break;
                        case SMUSourceMode.Current:
                            {
                                var ivData = smuVds.LinearCurrentSweep(settings.VdsStart, settings.VdsStop, settings.N_VdsSweep);

                                onDataArrived(new ExpDataArrivedEventArgs(ivData.ToStringExtension()));
                            } break;
                        case SMUSourceMode.ModeNotSet:
                            throw new ArgumentException();
                        default:
                            throw new ArgumentException();
                    }


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

            onStatusChanged(new StatusEventArgs("Measurement completed!"));
        }

        public override void Stop()
        {
            IsRunning = false;
        }

        public override void SaveToFile(string FileName)
        {
            
        }
    }
}
