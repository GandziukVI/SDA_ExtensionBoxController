using ControlAssist;
using CustomControls.ViewModels;
using MCBJUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    [Serializable]
    public class Noise_DefinedResistanceModel : NotifyPropertyChangedBase
    {
        public Noise_DefinedResistanceModel()
        {
        }

        private NoiseDefRSettingsControlModel experimentSettings = new NoiseDefRSettingsControlModel();
        public NoiseDefRSettingsControlModel ExperimentSettings
        {
            get { return experimentSettings; }
            set
            {
                experimentSettings = value;
                SetField(ref experimentSettings, value, "ExperimentSettings");
            }
        }

    }
}
