using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using System.IO;
using ControlAssist;
using CustomControls.ViewModels;
using FETUI;

namespace FET_Characterization
{
    [Serializable]
	public class FET_NoiseModel : NotifyPropertyChangedBase
	{
		public FET_NoiseModel()
		{
			
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