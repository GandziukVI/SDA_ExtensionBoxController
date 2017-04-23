using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private void on_cmdOpenFolderIV_Click(object sender, RoutedEventArgs e)
        {
            dialogIVMeasurement.ShowDialog();
            Settings.IV_FET_DataFilePath = dialogIVMeasurement.SelectedPath;
        }

        private void on_cmdOpenFolderTransfer(object sender, RoutedEventArgs e)
        {
            dialogTransferMeasurement.ShowDialog();
            Settings.TransferDataFilePath = dialogTransferMeasurement.SelectedPath;
        }

        private void on_IV_FET_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            if (dialogIVMeasurement.SelectedPath != string.Empty)
                startInfo.FileName = dialogIVMeasurement.SelectedPath;
            else
                startInfo.FileName = Directory.GetCurrentDirectory();

            Process.Start(startInfo);
        }

        private void on_IV_FET_SameAsForTransfer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dialogIVMeasurement.SelectedPath = dialogTransferMeasurement.SelectedPath;
            Settings.IV_FET_DataFilePath = dialogIVMeasurement.SelectedPath;
        }

        private void on_Transfer_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            if (dialogTransferMeasurement.SelectedPath != string.Empty)
                startInfo.FileName = dialogTransferMeasurement.SelectedPath;
            else
                startInfo.FileName = Directory.GetCurrentDirectory();

            Process.Start(startInfo);
        }

        private void on_Transfer_SameAsForIV_FET_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dialogTransferMeasurement.SelectedPath = dialogIVMeasurement.SelectedPath;
            Settings.TransferDataFilePath = dialogTransferMeasurement.SelectedPath;
        }
    }
}