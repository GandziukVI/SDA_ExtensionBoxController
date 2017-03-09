using System;
using System.Collections.Generic;
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

namespace VoltageApply
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static ApplyVoltageController applyVoltageController;
        public MainWindow()
        {
            applyVoltageController = new ApplyVoltageController();
            applyVoltageController.progressChanged += applyVoltageController_progressChanged;

            InitializeComponent();
        }

        void applyVoltageController_progressChanged(object sender, ProgressChanged_EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                progressBar.Value = e.Progress;
            }));
        }

        private void on_cmdSetVoltagesClick(object sender, RoutedEventArgs e)
        {
            var doJobThread = new Thread(new ThreadStart(doJob));
            doJobThread.Start();
        }

        private void doJob()
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                cmdDoJob.IsEnabled = false;
            }));

            applyVoltageController.Init();

            applyVoltageController.SetGateVoltage(Settings.VGate, Settings.VGateAccuracy);
            applyVoltageController.SetDrainSourceVoltage(Settings.VDrainSource, Settings.VDSAccuracy);

            applyVoltageController.Close();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                cmdDoJob.IsEnabled = true;
            }));
        }
    }
}
