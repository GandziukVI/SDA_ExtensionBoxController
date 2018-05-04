using DeviceIO;
using MCBJNoiseStarter.Experiments;
using MCS_Faulhaber;
using MotionManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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

namespace MCBJNoiseStarter
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
                lock (isInProgressLocker)
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

        private void onWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SerializeDataContext(GetSerializationFilePath());
        }

        private void on_cmdOpenFolderClick(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
            (DataContext as MainWindowViewModel).ExperimentSettings.FilePath = dialog.SelectedPath;
        }

        private void on_MCBJ_OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            if (dialog.SelectedPath != string.Empty)
                startInfo.FileName = dialog.SelectedPath;
            else
                startInfo.FileName = Directory.GetCurrentDirectory();

            Process.Start(startInfo);
        }

        private void on_cmdStartClick(object sender, RoutedEventArgs e)
        {
            IsInProgress = true;
            var filePath = GetSerializationFilePath();
            SerializeDataContext(filePath);

            Task.Factory.StartNew(new Action(() =>
            {
                var Settings = DeserializeDataContext(filePath);

                var innerLoopCollection = Settings.ExperimentSettings.ScanningVoltageCollection;
                var outerLoopCollection = Settings.ExperimentSettings.SetConductanceCollection;

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
                    for (int j = 0; j < innerLoopSelectionList.Count; )
                    {
                        if (!IsInProgress)
                            break;

                        var innerLoopSelection = innerLoopSelectionList[j];

                        Settings.ExperimentSettings.SetConductanceCollection = new double[] { outerLoopCollection[i] };
                        Settings.ExperimentSettings.ScanningVoltageCollection = innerLoopSelection;

                        var noiseFilePath = GetNoiseSerializationFilePath();
                        var noiseFileDir = System.IO.Path.GetDirectoryName(noiseFilePath);

                        if (!Directory.Exists(noiseFileDir))
                            Directory.CreateDirectory(noiseFileDir);

                        SerializeDataContext(noiseFilePath, Settings.ExperimentSettings);

                        ++j;

                        using (var process = Process.Start("MCBJ.exe", "MCBJNoise"))
                        {
                            process.WaitForExit();
                            if (process.ExitCode != 0)
                                --j;
                        }
                    }
                }

                using (var driver = new SerialDevice("COM1", 115200, Parity.None, 8, StopBits.One) as IDeviceIO)
                {
                    var motionController = new SA_2036U012V(driver) as IMotionController1D;

                    motionController.Enabled = true;
                    motionController.SetVelosity(4.8);
                    motionController.SetPosition(0.0);
                    motionController.Enabled = false;
                }

                MessageBox.Show("The measurement is done!", "Measurement Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }));
        }

        private void on_cmdStopClick(object sender, RoutedEventArgs e)
        {
            IsInProgress = false;
        }

        string GetSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\NoiseMCBJStarter.bin";
        }

        string GetNoiseSerializationFilePath()
        {
            return Directory.GetCurrentDirectory() + "\\MCBJCharacterization\\MCBJNoiseSettings.bin";
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
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                formatter.Serialize(stream, context);
            }
        }

        MainWindowViewModel DeserializeDataContext(string filePath)
        {
            MainWindowViewModel result;
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                result = formatter.Deserialize(stream) as MainWindowViewModel;
            }

            return result;
        }
    }
}
