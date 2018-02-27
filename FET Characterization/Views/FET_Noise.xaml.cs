using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ControlAssist;

namespace FET_Characterization
{
    /// <summary>
    /// Interaction ligic for FET_Noise.xaml
    /// </summary>
    public partial class FET_Noise : UserControl, ISavable
    {
        public FET_Noise()
        {
            this.InitializeComponent();
        }
       
        #region Saving / loading last experiment settings

        private void onFETNoiseControlLoaded(object sender, RoutedEventArgs e)
        {
            Load(GetNoiseSerializationFilePath());
        }

        private void cmdStart_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Save(GetNoiseSerializationFilePath());
        } 

        public void Save(string filePath)
        {
            SerializeDataContext(filePath);
        }

        public void Load(string filePath)
        {
            if (File.Exists(filePath))
                DeserializeDataContext(filePath);
        }

        string GetNoiseSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\FETCharacterization\\FETNoiseSettings.bin";
        }

        void SerializeDataContext(string filePath)
        {
            var formatter = new BinaryFormatter();
            var dir = System.IO.Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, (DataContext as FET_NoiseModel).ExperimentSettings);
            }
        }

        void DeserializeDataContext(string filePath)
        {
            var formatter = new BinaryFormatter();
            if (File.Exists(filePath))
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var context = formatter.Deserialize(stream) as FETUI.NoiseFETSettingsControlModel;
                    (DataContext as FET_NoiseModel).ExperimentSettings = context;
                }
            }
        }       

        #endregion                    
    }
}