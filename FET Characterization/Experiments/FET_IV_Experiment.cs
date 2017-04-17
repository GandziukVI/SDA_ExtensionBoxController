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

        LinkedList<IV_Data> ivData;
        LinkedList<IV_Data> leakageData;

        Stopwatch measurementStopwatch;

        public FET_IV_Experiment(ISourceMeterUnit SMU_Vds, ISourceMeterUnit SMU_Vg)
            : base()
        {
            smuVds = SMU_Vds;
            smuVg = SMU_Vg;

            ivData = new LinkedList<IV_Data>();
            leakageData = new LinkedList<IV_Data>();

            measurementStopwatch = new Stopwatch();
        }

        public override void ToDo(object Arg)
        {
            var settings = (FET_IVModel)Arg;

            if (ivData.Count > 0)
                ivData.Clear();
            if (leakageData.Count > 0)
                leakageData.Clear();

            #region Gate SMU settings

            smuVg.SourceMode = SMUSourceMode.Voltage;

            smuVg.Compliance = settings.Gate_Complaince;
            smuVg.Averaging = 10;
            smuVg.NPLC = 1.0;

            smuVg.Voltage = settings.VgStart;

            smuVg.SwitchON();

            onDataArrived(new ExpDataArrivedEventArgs(string.Format("Vg = {0}", settings.VgStart.ToString(NumberFormatInfo.InvariantInfo))));

            #endregion

            #region Drain-Source SMU settings

            smuVds.SourceMode = settings.SMU_SourceMode;

            smuVds.Compliance = settings.DS_Complaince;
            smuVds.Averaging = 10;
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

            var currentVg = settings.VgStart;
            var current_DS_value = settings.VdsStart;

            var dVg = (settings.VgStop - settings.VgStart) / settings.N_VgStep;
            var d_DS_value = (settings.VdsStop - settings.VdsStart) / settings.N_VdsSweep;

            measurementStopwatch.Start();

            if (settings.MeasureLeakage == true)
            {
                for (int i = 0; i < settings.N_VgStep; i++)
                {
                    if (!IsRunning)
                        break;

                    for (int j = 0; j < settings.N_VdsSweep; j++)
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

                        current_DS_value += d_DS_value;
                    }

                    current_DS_value = settings.VdsStart;

                    currentVg += dVg;
                    smuVg.Voltage = currentVg;

                    onDataArrived(new ExpDataArrivedEventArgs(string.Format("Vg = {0}", currentVg.ToString(NumberFormatInfo.InvariantInfo))));
                }
            }

            measurementStopwatch.Stop();

            smuVg.SwitchOFF();
            smuVds.SwitchOFF();
        }
    }
}
