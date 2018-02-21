using MCBJ.Experiments;
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
    public partial class Noise_at_DefinedResistance : UserControl
    {
        System.Windows.Forms.FolderBrowserDialog dialog;

        public Noise_at_DefinedResistance()
        {
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InitializeComponent();
        }

        private void SelectAddress(object sender, System.Windows.RoutedEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TextBox tb = (sender as TextBox);

            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }  

        private void on_cmdOpenFolderClick(object sender, System.Windows.RoutedEventArgs e)
        {
            dialog.ShowDialog();
            (DataContext as Noise_DefinedResistanceModel).FilePath = dialog.SelectedPath;
        }
		
		private void on_MCBJ_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {        	
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialog.SelectedPath = (DataContext as Noise_DefinedResistanceModel).FilePath;
            startInfo.FileName = dialog.SelectedPath;

            Process.Start(startInfo);
        }        

        private void onMCBJNoiseLoaded(object sender, RoutedEventArgs e)
        {
            var fileName = GetNoiseSerializationFilePath();
            if (File.Exists(fileName))
            {
                DeserializeDataContext(fileName);
                dialog.SelectedPath = (DataContext as Noise_DefinedResistanceModel).FilePath;
            }
        }

        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {
            SerializeDataContext(GetNoiseSerializationFilePath());
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
                formatter.Serialize(stream, DataContext);
            }
        }

        void DeserializeDataContext(string filePath)
        {
            var formatter = new BinaryFormatter();
            if(File.Exists(filePath))
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DataContext = formatter.Deserialize(stream);
                }
            }
        }              
    }
}