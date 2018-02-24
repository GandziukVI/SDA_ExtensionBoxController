using ControlAssist;
using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBJUI;

namespace MCBJ.Experiments
{
    [Serializable]
    public class IV_DefinedResistanceModel : NotifyPropertyChangedBase
    {
        public IV_DefinedResistanceModel() { }

        private IVDefRSettingsControlModel experimentSettings = new IVDefRSettingsControlModel();
        public IVDefRSettingsControlModel ExperimentSettigns
        {
            get { return experimentSettings; }
            set 
            {
                SetField(ref experimentSettings, value, "ExperimentSettigns");
            }
        }
    }
}
