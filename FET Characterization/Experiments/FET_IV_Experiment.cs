using ExperimentController;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FET_Characterization.Experiments
{
    //public class

    public class FET_IV_Experiment : ExperimentBase
    {
        private ISourceMeterUnit smuVds;
        private ISourceMeterUnit smuVg;

        LinkedList<IV_Data> ivData;
        LinkedList<IV_Data> leakageData;

        public FET_IV_Experiment(ISourceMeterUnit SMU_Vds, ISourceMeterUnit SMU_Vg)
            : base()
        {
            smuVds = SMU_Vds;
            smuVg = SMU_Vg;

            ivData = new LinkedList<IV_Data>();
            leakageData = new LinkedList<IV_Data>();
        }

        public override void ToDo(object Arg)
        {
            var settings = (FET_IVModel)Arg;

            #region Gate SMU settings

            smuVg.SourceMode = SMUSourceMode.Voltage;

            smuVg.Compliance = settings.Gate_Complaince;
            smuVg.Averaging = 10;
            smuVg.NPLC = 1.0;

            smuVg.Voltage = settings.VgStart;

            smuVg.SwitchON();

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
            var currentVds = settings.VdsStart;

            var dVg = (settings.VgStop - settings.VgStart) / settings.N_VgStep;
            var dVds = (settings.VdsStop - settings.VdsStart) / settings.N_VdsSweep;

            if (settings.MeasureLeakage == true)
            {
                for (int i = 0; i < settings.N_VgStep; i++)
                {
                    for (int j = 0; j < settings.N_VdsSweep; j++)
                    {

                    }

                    currentVg += dVg;
                    smuVg.Voltage = currentVg;
                }
            }
        }
    }
}
