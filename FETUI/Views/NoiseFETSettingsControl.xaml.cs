﻿using System;
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

namespace FETUI
{
	/// <summary>
	/// Interaction logic for NoiseFETSettingsControl.xaml
	/// </summary>
	public partial class NoiseFETSettingsControl : UserControl
	{
		System.Windows.Forms.FolderBrowserDialog dialog;
		
		public NoiseFETSettingsControl()
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
			dialog.SelectedPath = (DataContext as NoiseFETSettingsControlModel).FilePath;
            dialog.ShowDialog();
            (DataContext as NoiseFETSettingsControlModel).FilePath = dialog.SelectedPath;
		}

		private void on_FET_OpenDataFolder_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var startInfo = new ProcessStartInfo() { UseShellExecute = true, Verb = "open" };

            dialog.SelectedPath = (DataContext as NoiseFETSettingsControlModel).FilePath;
            startInfo.FileName = dialog.SelectedPath;

            Process.Start(startInfo);
		}
	}
}