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
	/// Interaction logic for NoiseDefRSettingsControl.xaml
	/// </summary>
	public partial class NoiseDefRSettingsControl : UserControl
	{
		System.Windows.Forms.FolderBrowserDialog dialog;
		
		public NoiseDefRSettingsControl()
		{
            dialog = new System.Windows.Forms.FolderBrowserDialog();
			this.InitializeComponent();            
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

        private void on_cmdOpenFolderClick(object sender, System.Windows.RoutedEventArgs e)
        {
            dialog.SelectedPath = (DataContext as NoiseDefRSettingsControlModel).FilePath;
            dialog.ShowDialog();
            (DataContext as NoiseDefRSettingsControlModel).FilePath = dialog.SelectedPath;
        }
		
		private void on_MCBJ_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
        {        	
            var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialog.SelectedPath = (DataContext as NoiseDefRSettingsControlModel).FilePath;
            startInfo.FileName = dialog.SelectedPath;

            Process.Start(startInfo);
        }        
	}
}