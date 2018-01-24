using MCBJNoiseStarter.Experiments;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
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

        private void on_cmdOpenFolderClick(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
            Settings.FilePath = dialog.SelectedPath;
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
            Dispatcher.BeginInvoke(new Action(() => 
            {
                var fPath = Settings.FilePath.EndsWith("\\") ? Settings.FilePath.Substring(0, Settings.FilePath.Length - 2) : Settings.FilePath;
                var argumentsString = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} {17} {18} {19} {20} {21} {22} \"{23}\" \"{24}\"",
                "MCBJNoise",
                Settings.AgilentU2542AResName,
                Settings.VoltageDeviation.ToString(NumberFormatInfo.InvariantInfo),
                Settings.MinVoltageTreshold.ToString(NumberFormatInfo.InvariantInfo),
                Settings.VoltageTreshold.ToString(NumberFormatInfo.InvariantInfo),
                Settings.ConductanceDeviation.ToString(NumberFormatInfo.InvariantInfo),
                Settings.StabilizationTime.ToString(NumberFormatInfo.InvariantInfo),
                Settings.MotionMinSpeed.ToString(NumberFormatInfo.InvariantInfo),
                Settings.MotionMaxSpeed.ToString(NumberFormatInfo.InvariantInfo),
                Settings.MotorMinPos.ToString(NumberFormatInfo.InvariantInfo),
                Settings.MotorMaxPos.ToString(NumberFormatInfo.InvariantInfo),
                Settings.NAveragesFast.ToString(NumberFormatInfo.InvariantInfo),
                Settings.NAveragesSlow.ToString(NumberFormatInfo.InvariantInfo),
                Settings.LoadResistance.ToString(NumberFormatInfo.InvariantInfo),
                Settings.NSubSamples.ToString(NumberFormatInfo.InvariantInfo),
                Settings.SpectraAveraging.ToString(NumberFormatInfo.InvariantInfo),
                Settings.UpdateNumber.ToString(NumberFormatInfo.InvariantInfo),
                Settings.KPreAmpl.ToString(NumberFormatInfo.InvariantInfo),
                Settings.KAmpl.ToString(NumberFormatInfo.InvariantInfo),
                Settings.Temperature0.ToString(NumberFormatInfo.InvariantInfo),
                Settings.TemperatureE.ToString(NumberFormatInfo.InvariantInfo),
                Settings.RecordTimeTraces ? "y" : "n",
                Settings.RecordingFrequency.ToString(NumberFormatInfo.InvariantInfo),
                fPath,
                Settings.SaveFileName);

                MessageBox.Show(Settings.FilePath);
                MessageBox.Show(Settings.SaveFileName);

                var innerLoopCollection = Settings.ScanningVoltageCollection;
                var outerLoopCollection = Settings.SetConductanceCollection;

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
                    foreach (var innerLoopSelection in innerLoopSelectionList)
                    {
                        byte[] scanningVoltagesSetBytes = Encoding.ASCII.GetBytes((string)converter.Convert(innerLoopSelection, typeof(string), null, CultureInfo.InvariantCulture));
                        int scanningVoltagesSetLen = scanningVoltagesSetBytes.Length;

                        byte[] conductancesSetBytes = Encoding.ASCII.GetBytes(outerLoopCollection[i].ToString(NumberFormatInfo.InvariantInfo));
                        int conductancesSetLen = conductancesSetBytes.Length;

                        using (var mmfVdsSet = MemoryMappedFile.CreateNew(@"VdsSet", scanningVoltagesSetLen + sizeof(Int32), MemoryMappedFileAccess.ReadWrite))
                        using (var mmfConductanceSet = MemoryMappedFile.CreateNew(@"ConductanceSet", scanningVoltagesSetLen + sizeof(Int32), MemoryMappedFileAccess.ReadWrite))
                        {
                            using (var mmfVdsSetStream = mmfVdsSet.CreateViewStream(0, scanningVoltagesSetLen + sizeof(Int32), MemoryMappedFileAccess.Write))
                            using (var mmfConductanceSetStream = mmfConductanceSet.CreateViewStream(0, conductancesSetLen + sizeof(Int32), MemoryMappedFileAccess.Write))
                            {
                                var VdsSetLengthBytes = BitConverter.GetBytes(scanningVoltagesSetLen);
                                mmfVdsSetStream.Write(VdsSetLengthBytes, 0, VdsSetLengthBytes.Length);
                                mmfVdsSetStream.Write(scanningVoltagesSetBytes, 0, scanningVoltagesSetBytes.Length);

                                var ConductanceSetLengthBytes = BitConverter.GetBytes(conductancesSetLen);
                                mmfConductanceSetStream.Write(ConductanceSetLengthBytes, 0, ConductanceSetLengthBytes.Length);
                                mmfConductanceSetStream.Write(conductancesSetBytes, 0, conductancesSetBytes.Length);
                            }

                            var process = Process.Start("MCBJ.exe", argumentsString);
                            process.WaitForExit();
                        }
                    }
                }
            }));
        }

        private void on_cmdStopClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
