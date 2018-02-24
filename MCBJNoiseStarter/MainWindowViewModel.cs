using ControlAssist;
using MCBJUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJNoiseStarter
{
    [Serializable]
    public class MainWindowViewModel : NotifyPropertyChangedBase
    {
        private int nMaxSpectra = 10;
        public int NMaxSpectra
        {
            get { return nMaxSpectra; }
            set
            {
                SetField(ref nMaxSpectra, value, "NMaxSpectra");
            }
        }

        private NoiseDefRSettingsControlModel experimentSettings = new NoiseDefRSettingsControlModel();
        public NoiseDefRSettingsControlModel ExperimentSettings
        {
            get { return experimentSettings; }
            set
            {
                SetField(ref experimentSettings, value, "ExperimentSettings");
            }
        }
    }
}
