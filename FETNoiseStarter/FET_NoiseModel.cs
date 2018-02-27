using ControlAssist;
using FETUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FETNoiseStarter
{
    [Serializable]
    class FET_NoiseModel : NotifyPropertyChangedBase
    {
        public FET_NoiseModel()
        {
        }

        private int nMaxSpectra = 10;
        public int NMaxSpectra
        {
            get { return nMaxSpectra; }
            set
            {
                SetField(ref nMaxSpectra, value, "NMaxSpectra");
            }
        }


        private NoiseFETSettingsControlModel experimentSettings = new NoiseFETSettingsControlModel();
        public NoiseFETSettingsControlModel ExperimentSettings
        {
            get { return experimentSettings; }
            set
            {
                SetField(ref experimentSettings, value, "ExperimentSettings");
            }
        }

    }
}
