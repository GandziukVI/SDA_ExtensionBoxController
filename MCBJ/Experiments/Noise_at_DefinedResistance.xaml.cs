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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MCBJ
{
    /// <summary>
    /// Interaction logic for Noise_at_DefinedResistance.xaml
    /// </summary>
    public partial class Noise_at_DefinedResistance : UserControl
    {
        System.Windows.Forms.FolderBrowserDialog dialog;

        public Noise_at_DefinedResistance()
        {
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InitializeComponent();
        }

        private void on_cmdOpenFolderClick(object sender, System.Windows.RoutedEventArgs e)
        {
            dialog.ShowDialog();
            Settings.FilePath = dialog.SelectedPath;
        }
		
		private void on_MCBJ_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        	var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            if (dialog.SelectedPath != string.Empty)
                startInfo.FileName = dialog.SelectedPath;
            else
                startInfo.FileName = Directory.GetCurrentDirectory();

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
    }
}