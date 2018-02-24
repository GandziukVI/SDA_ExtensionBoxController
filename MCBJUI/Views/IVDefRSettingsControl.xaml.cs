using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MCBJUI
{
	/// <summary>
	/// Interaction logic for IVDefRSettingsControl.xaml
	/// </summary>
	public partial class IVDefRSettingsControl : UserControl
	{
        System.Windows.Forms.FolderBrowserDialog dialog;
		public IVDefRSettingsControl()
		{
            dialog = new System.Windows.Forms.FolderBrowserDialog();
			this.InitializeComponent();
		}

		private void on_cmdOpenClick(object sender, System.Windows.RoutedEventArgs e)
		{
            dialog.SelectedPath = (DataContext as IVDefRSettingsControlModel).FilePath;
            dialog.ShowDialog();
            (DataContext as IVDefRSettingsControlModel).FilePath = dialog.SelectedPath;
		}

        private void on_MCBJ_OpenDataFolder_Click(object sender, RoutedEventArgs e)
        {
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialog.SelectedPath = (DataContext as IVDefRSettingsControlModel).FilePath;
            startInfo.FileName = dialog.SelectedPath;

            Process.Start(startInfo);
        }     
    }
}