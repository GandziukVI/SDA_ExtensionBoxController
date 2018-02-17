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

namespace FET_Characterization
{
    /// <summary>
    /// Interaction ligic for FET_Noise.xaml
    /// </summary>
    public partial class FET_Noise : UserControl
    {
        System.Windows.Forms.FolderBrowserDialog dialog;

        public FET_Noise()
        {
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InitializeComponent();
        }

        private void on_cmdOpenFolderClick(object sender, System.Windows.RoutedEventArgs e)
        {
            dialog.SelectedPath = (DataContext as FET_NoiseModel).FilePath;
            dialog.ShowDialog();
            (DataContext as FET_NoiseModel).FilePath = dialog.SelectedPath;
        }

        private void on_FET_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialog.SelectedPath = (DataContext as FET_NoiseModel).FilePath;
            startInfo.FileName = dialog.SelectedPath;

            Process.Start(startInfo);
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

        private void cmdStart_Click(object sender, RoutedEventArgs e)
        {
            SerializeNoiseExperimentDataContext();
        }

        #region Saving / loading last experiment settings

        private void onFETNoiseControlLoaded(object sender, RoutedEventArgs e)
        {
            DeserializeNoiseExperimentDataContext();
        }

        private void SerializeNoiseExperimentDataContext()
        {
            var path = Directory.GetCurrentDirectory() + "\\FET Characterization";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            var filePath = path + "\\FETNoiseSettings.bin";
            var formatter = new BinaryFormatter();

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, DataContext);
            }
        }

        private void DeserializeNoiseExperimentDataContext()
        {
            var path = Directory.GetCurrentDirectory() + "\\FET Characterization\\FETNoiseSettings.bin";

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