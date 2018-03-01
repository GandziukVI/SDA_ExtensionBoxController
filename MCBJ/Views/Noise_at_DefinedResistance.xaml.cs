using ControlAssist;
using MCBJ.Experiments;
using MCBJUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCBJ
{
    /// <summary>
    /// Interaction logic for Noise_at_DefinedResistance.xaml
    /// </summary>
    public partial class Noise_at_DefinedResistance : UserControl, ISavable
    {
        public Noise_at_DefinedResistance()
        {
            this.InitializeComponent();
            Load(GetNoiseSerializationFilePath());
        }        

        private void onMCBJNoiseLoaded(object sender, RoutedEventArgs e)
        {
            //Load(GetNoiseSerializationFilePath());
        }

        private void cmdStart_Click(object sender, RoutedEventArgs e)
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
            return Directory.GetCurrentDirectory() + "\\MCBJCharacterization\\MCBJNoiseSettings.bin";
        }

        void SerializeDataContext(string filePath)
        {
            var formatter = new BinaryFormatter();
            var dir = System.IO.Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, (DataContext as Noise_DefinedResistanceModel).ExperimentSettings);
            }
        }

        void DeserializeDataContext(string filePath)
        {
            var formatter = new BinaryFormatter();
            if(File.Exists(filePath))
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var context = formatter.Deserialize(stream) as NoiseDefRSettingsControlModel;
                    (DataContext as Noise_DefinedResistanceModel).ExperimentSettings = context;
                }
            }
        }        
    }
}