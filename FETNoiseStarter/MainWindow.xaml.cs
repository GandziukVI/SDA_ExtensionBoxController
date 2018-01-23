using FETNoiseStarter.Experiments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
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

        private void on_cmdOpenFolderClick(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
            Settings.FilePath = dialog.SelectedPath;
        }

        private void on_cmdStartClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                var argumentsString = string.Format("{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10} {11} {12} {13} {14} {15} {16} \"{17}\" \"{18}\"",
                "FETNoise",
                Settings.AgilentU2542AResName,
                Settings.IsTransferCurveMode ? "Transfer" : "Output",
                Settings.VoltageDeviation.ToString(NumberFormatInfo.InvariantInfo),
                Settings.NAveragesFast.ToString(NumberFormatInfo.InvariantInfo),
                Settings.NAveragesSlow.ToString(NumberFormatInfo.InvariantInfo),
                Settings.StabilizationTime.ToString(NumberFormatInfo.InvariantInfo),
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
                Settings.FilePath,
                Settings.SaveFileName);

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
                    foreach (var innerLoopSelection in innerLoopSelectionList)
                    {
                        var dataBytesOuterLoop = Encoding.ASCII.GetBytes(outerLoopCollection[i].ToString(NumberFormatInfo.InvariantInfo));
                        var dataBytesInnerLoop = Encoding.ASCII.GetBytes((string)converter.Convert(innerLoopSelection, typeof(string), null, CultureInfo.InvariantCulture));

                        int VgSetLen = 0;
                        byte[] VgSet = { 0 };

                        int VdsSetLen = 0;
                        byte[] VdsSet = { 0 };

                        if (Settings.IsTransferCurveMode == true)
                        {
                            VgSetLen = dataBytesInnerLoop.Length;
                            VgSet = dataBytesInnerLoop;

                            VdsSetLen = dataBytesOuterLoop.Length;
                            VdsSet = dataBytesOuterLoop;
                        }
                        else if (Settings.IsOutputCurveMode == true)
                        {
                            VgSetLen = dataBytesOuterLoop.Length;
                            VgSet = dataBytesOuterLoop;

                            VdsSetLen = dataBytesInnerLoop.Length;
                            VdsSet = dataBytesInnerLoop;
                        }

                        using (var mmfVg = MemoryMappedFile.CreateNew(@"VgSet", VgSetLen + sizeof(Int32), MemoryMappedFileAccess.ReadWrite))
                        using (var mmfVds = MemoryMappedFile.CreateNew(@"VdsSet", VgSetLen + sizeof(Int32), MemoryMappedFileAccess.ReadWrite))
                        {
                            using (var mmfVgStream = mmfVg.CreateViewStream(0, VgSetLen + sizeof(Int32), MemoryMappedFileAccess.Write))
                            using (var mmfVdsStream = mmfVds.CreateViewStream(0, VdsSetLen + sizeof(Int32), MemoryMappedFileAccess.Write))
                            {
                                var VgSetLengthBytes = BitConverter.GetBytes(VgSetLen);
                                mmfVgStream.Write(VgSetLengthBytes, 0, VgSetLengthBytes.Length);
                                mmfVgStream.Write(VgSet, 0, VgSet.Length);

                                var VdsSetLengthBytes = BitConverter.GetBytes(VdsSetLen);
                                mmfVdsStream.Write(VdsSetLengthBytes, 0, VdsSetLengthBytes.Length);
                                mmfVdsStream.Write(VdsSet, 0, VdsSet.Length);
                            }

                            var process = Process.Start("FET Characterization.exe", argumentsString);
                            process.WaitForExit();
                        }
                    }
                }
            }));
        }

        private void on_cmdStopClick(object sender, RoutedEventArgs e)
        {

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
    }
}
