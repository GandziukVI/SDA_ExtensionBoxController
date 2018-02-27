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

        private readonly object isInProgressLocker = new object();

        private bool isInProgress = false;        
        public bool IsInProgress
        {
            get 
            {
                lock(isInProgressLocker)
                {
                    return isInProgress; 
                }                
            }
            set 
            {
                lock (isInProgressLocker)
                {
                    isInProgress = value;
                }
            }
        }

        public MainWindow()
        {
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            InitializeComponent();
        }

        private void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            var fileName = GetSerializationFilePath();
            if (File.Exists(fileName))
            {
                var context = DeserializeDataContext(fileName);
                DataContext = context;
                dialog.SelectedPath = context.ExperimentSettings.FilePath;
            }
        }

        private void onWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SerializeDataContext(GetSerializationFilePath());
        }  

        private void on_cmdStartClick(object sender, RoutedEventArgs e)
        {
            IsInProgress = true;

            var filePath = GetSerializationFilePath();            
            SerializeDataContext(filePath);

            Task.Factory.StartNew(new Action(() =>
            {
                var Settings = DeserializeDataContext(filePath);
                var experimentSettings = Settings.ExperimentSettings;

                double[] outerLoopCollection;
                double[] innerLoopCollection;

                if (experimentSettings.IsTransferCurveMode == true)
                {
                    outerLoopCollection = experimentSettings.DSVoltageCollection;
                    innerLoopCollection = experimentSettings.GateVoltageCollection;
                }
                else
                {
                    outerLoopCollection = experimentSettings.GateVoltageCollection;
                    innerLoopCollection = experimentSettings.DSVoltageCollection;
                }

                var innerLoopSelectionList = new List<double[]>();

                var nCompleteSelections = innerLoopCollection.Length / Settings.NMaxSpectra;
                var nResidualSpectra = innerLoopCollection.Length % Settings.NMaxSpectra;

                for (int i = 0; i < nCompleteSelections; i++)
                    innerLoopSelectionList.Add(innerLoopCollection.Where((value, index) => index >= i * Settings.NMaxSpectra && index < (i + 1) * Settings.NMaxSpectra).ToArray());

                if (nResidualSpectra > 0)
                    innerLoopSelectionList.Add(innerLoopCollection.Where((value, index) => index >= nCompleteSelections * Settings.NMaxSpectra && index < nCompleteSelections * Settings.NMaxSpectra + nResidualSpectra).ToArray());

                for (int i = 0; i < outerLoopCollection.Length; i++)
                {
                    if (!IsInProgress)
                        break;
                    for (int j = 0; j < innerLoopSelectionList.Count; j++)
                    {
                        if (!IsInProgress)
                            break;
                        var innerLoopSelection = innerLoopSelectionList[j];

                        if (experimentSettings.IsTransferCurveMode == true)
                        {
                            experimentSettings.DSVoltageCollection = new double[] { outerLoopCollection[i] };
                            experimentSettings.GateVoltageCollection = innerLoopSelection;
                        }
                        else if (experimentSettings.IsOutputCurveMode == true)
                        {
                            experimentSettings.DSVoltageCollection = innerLoopSelection;
                            experimentSettings.GateVoltageCollection = new double[] { outerLoopCollection[i] };
                        }

                        var noiseFilePath = GetNoiseSerializationFilePath(); ;
                        var noiseFileDir = System.IO.Path.GetDirectoryName(noiseFilePath);

                        if (!Directory.Exists(noiseFileDir))
                            Directory.CreateDirectory(noiseFileDir);

                        SerializeDataContext(noiseFilePath, experimentSettings);

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
            IsInProgress = false;
        }

        private void on_cmdOpenFolderClick(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
            (DataContext as FET_NoiseModel).ExperimentSettings.FilePath = dialog.SelectedPath;
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

        string GetSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\NoiseFETStarter.bin";
        }

        string GetNoiseSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\FETCharacterization\\FETNoiseSettings.bin";
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
