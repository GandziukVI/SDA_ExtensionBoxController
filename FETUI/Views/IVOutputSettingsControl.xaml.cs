using ControlAssist;
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
using System.Windows.Shapes;

namespace FETUI
{
	/// <summary>
	/// Interaction logic for NoiseIVSettingsControl.xaml
	/// </summary>
	public partial class NoiseIVSettingsControl : UserControl, ISavable
	{
        System.Windows.Forms.FolderBrowserDialog dialogIVMeasurement;

		public NoiseIVSettingsControl()
		{
            dialogIVMeasurement = new System.Windows.Forms.FolderBrowserDialog();
			this.InitializeComponent();
            Load(GetSerializationFileName());
		}

		private void on_cmdOpenFolderIV_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            dialogIVMeasurement.SelectedPath = (DataContext as IVOutputSettingsControlModel).IV_FET_DataFilePath;
            dialogIVMeasurement.ShowDialog();
            (DataContext as IVOutputSettingsControlModel).IV_FET_DataFilePath = dialogIVMeasurement.SelectedPath;
		}

		private void on_IV_FET_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialogIVMeasurement.SelectedPath = (DataContext as IVOutputSettingsControlModel).IV_FET_DataFilePath;
            startInfo.FileName = dialogIVMeasurement.SelectedPath;

            Process.Start(startInfo);
		}

		private void on_IV_FET_SameAsForTransfer_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			// TODO: Add event handler implementation here.
		}

		private void cmdStartIV_Click(object sender, System.Windows.RoutedEventArgs e)
		{
            Save(GetSerializationFileName());
		}

        #region Saving / loading last experiment settings

        private string GetSerializationFileName()
        {
            return Directory.GetCurrentDirectory() + "\\FETCharacterization\\FETIVOutputSettings.bin";
        }

        public void Save(string filePath)
        {
            var path = System.IO.Path.GetDirectoryName(filePath);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var formatter = new BinaryFormatter();

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, DataContext);
            }
        }

        public void Load(string filePath)
        {
            var formatter = new BinaryFormatter();
            if (File.Exists(filePath))
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DataContext = formatter.Deserialize(stream);
                }
            }
        }

        #endregion  
    }
}