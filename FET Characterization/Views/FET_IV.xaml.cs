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

namespace FET_Characterization
{
    /// <summary>
    /// Interaction logic for FET_IV.xaml
    /// </summary>
    public partial class FET_IV : UserControl
    {
        System.Windows.Forms.FolderBrowserDialog dialogIVMeasurement;
        System.Windows.Forms.FolderBrowserDialog dialogTransferMeasurement;

        public FET_IV()
        {
            dialogIVMeasurement = new System.Windows.Forms.FolderBrowserDialog();
            dialogTransferMeasurement = new System.Windows.Forms.FolderBrowserDialog();

            this.InitializeComponent();
        }

        #region Output IV section

        private void on_cmdOpenFolderIV_Click(object sender, RoutedEventArgs e)
        {
            dialogIVMeasurement.SelectedPath = (DataContext as FET_IVModel).IV_FET_DataFilePath;
            dialogIVMeasurement.ShowDialog();
            (DataContext as FET_IVModel).IV_FET_DataFilePath = dialogIVMeasurement.SelectedPath;
        }

        private void on_IV_FET_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialogIVMeasurement.SelectedPath = (DataContext as FET_IVModel).IV_FET_DataFilePath;
            startInfo.FileName = dialogIVMeasurement.SelectedPath;

            Process.Start(startInfo);
        }

        private void on_IV_FET_SameAsForTransfer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dialogIVMeasurement.SelectedPath = dialogTransferMeasurement.SelectedPath;
            (DataContext as FET_IVModel).IV_FET_DataFilePath = dialogIVMeasurement.SelectedPath;
        }

        private void cmdStartIV_Click(object sender, RoutedEventArgs e)
        {
            SerializeIVExperimentDataContext();
        }

        #endregion

        #region Transfer section

        private void on_cmdOpenFolderTransfer(object sender, RoutedEventArgs e)
        {
            dialogTransferMeasurement.SelectedPath = (DataContext as FET_IVModel).TransferDataFilePath;
            dialogTransferMeasurement.ShowDialog();
            (DataContext as FET_IVModel).TransferDataFilePath = dialogTransferMeasurement.SelectedPath;
        }

        private void on_Transfer_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialogTransferMeasurement.SelectedPath = (DataContext as FET_IVModel).TransferDataFilePath;
            startInfo.FileName = dialogTransferMeasurement.SelectedPath;

            Process.Start(startInfo);
        }

        private void on_Transfer_SameAsForIV_FET_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dialogTransferMeasurement.SelectedPath = dialogIVMeasurement.SelectedPath;
            (DataContext as FET_IVModel).TransferDataFilePath = dialogTransferMeasurement.SelectedPath;
        }

        private void cmdStartTransfer_Click(object sender, RoutedEventArgs e)
        {
            SerializeIVExperimentDataContext();
        }

        #endregion

        #region Saving / loading last experiment settings

        private void onFETIVControlLoaded(object sender, RoutedEventArgs e)
        {
            DeserializeIVExperimentDataContext();
        }

        private void SerializeIVExperimentDataContext()
        {
            var path = Directory.GetCurrentDirectory() + "\\FET Characterization";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = path + "\\FETIVSettings.bin";
            var formatter = new BinaryFormatter();

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, DataContext);
            }
        }

        private void DeserializeIVExperimentDataContext()
        {
            var path = Directory.GetCurrentDirectory() + "\\FET Characterization\\FETIVSettings.bin";

            var formatter = new BinaryFormatter();
            if (File.Exists(path))
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    DataContext = formatter.Deserialize(stream);
                }
            }
        }

        #endregion        
    }
}