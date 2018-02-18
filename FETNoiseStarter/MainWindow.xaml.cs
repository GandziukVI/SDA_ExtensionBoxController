using FETNoiseStarter.Experiments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FETNoiseStarter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Forms.FolderBrowserDialog dialog;

        public MainWindow()
        {
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            InitializeComponent();
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

        private void on_cmdStartClick(object sender, RoutedEventArgs e)
        {
            var filePath = GetSerializationFilePath();
            
            SerializeDataContext(filePath);

            Task.Factory.StartNew(new Action(() =>
            {
                var Settings = DeserializeDataContext(filePath);                

                double[] outerLoopCollection;
                double[] innerLoopCollection;

                if (Settings.IsTransferCurveMode == true)
                {
                    outerLoopCollection = Settings.DSVoltageCollection;
                    innerLoopCollection = Settings.GateVoltageCollection;
                }
                else
                {
                    outerLoopCollection = Settings.GateVoltageCollection;
                    innerLoopCollection = Settings.DSVoltageCollection;
                }

                var innerLoopSelectionList = new List<double[]>();

                var nCompleteSelections = innerLoopCollection.Length / Settings.NMaxSpectra;
                var nResidualSpectra = innerLoopCollection.Length % Settings.NMaxSpectra;

                for (int i = 0; i < nCompleteSelections; i++)
                    innerLoopSelectionList.Add(innerLoopCollection.Where((value, index) => index >= i * Settings.NMaxSpectra && index < (i + 1) * Settings.NMaxSpectra).ToArray());

                if (nResidualSpectra > 0)
                    innerLoopSelectionList.Add(innerLoopCollection.Where((value, index) => index >= nCompleteSelections * Settings.NMaxSpectra && index < nCompleteSelections * Settings.NMaxSpectra + nResidualSpectra).ToArray());

                var converter = new ValueCollectionConverter();

                for (int i = 0; i < outerLoopCollection.Length; i++)
                {
                    for (int j = 0; j < innerLoopSelectionList.Count; j++)
                    {
                        var innerLoopSelection = innerLoopSelectionList[j];                        

                        if (Settings.IsTransferCurveMode == true)
                        {
                            Settings.DSVoltageCollection = new double[] { outerLoopCollection[i] };
                            Settings.GateVoltageCollection = innerLoopSelection;
                        }
                        else if (Settings.IsOutputCurveMode == true)
                        {
                            Settings.DSVoltageCollection = innerLoopSelection;
                            Settings.GateVoltageCollection = new double[] { outerLoopCollection[i] };
                        }
                        
                        var noiseFilePath = Directory.GetCurrentDirectory() + "\\FET Characterization";
                        var noiseFileDir = System.IO.Path.GetDirectoryName(noiseFilePath);

                        if (!Directory.Exists(noiseFileDir))
                            Directory.CreateDirectory(noiseFileDir);
                        
                        SerializeDataContext(noiseFilePath, Settings);

                        using (var process = Process.Start("FET Characterization.exe", "FETNoise"))
                        {
                            process.WaitForExit();
                            if (process.ExitCode != 0)
                                --j;
                        }
                    }
                }
            }));
        }

        private void on_cmdStopClick(object sender, RoutedEventArgs e)
        {

        }

        private void on_cmdOpenFolderClick(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
            (DataContext as FET_NoiseModel).FilePath = dialog.SelectedPath;
        }

        private void on_OpenDataFolderClick(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            if (dialog.SelectedPath != string.Empty)
                startInfo.FileName = dialog.SelectedPath;
            else
                startInfo.FileName = Directory.GetCurrentDirectory();

            Process.Start(startInfo);
        }

        private void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            var fileName = GetSerializationFilePath();
            if (File.Exists(fileName))
            {
                var context = DeserializeDataContext(fileName);
                DataContext = context;
                dialog.SelectedPath = context.FilePath;
            }
        }

        private void onWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SerializeDataContext(GetSerializationFilePath());
        }       

        string GetSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\NoiseFETStarter.bin";
        }

        string GetNoiseSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\FET Characterization\\FETNoiseSettings.bin";
        }

        void SerializeDataContext(string filePath)
        {            
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, DataContext);
            }
        }

        void SerializeDataContext(string filePath, object context)
        {
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                formatter.Serialize(stream, context);
            }
        }

        FET_NoiseModel DeserializeDataContext(string filePath)
        {
            FET_NoiseModel result;
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = formatter.Deserialize(stream) as FET_NoiseModel;
            }

            return result;
        }        
    }
}
