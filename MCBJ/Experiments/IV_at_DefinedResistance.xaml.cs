using MCBJ.Experiments;
using System;
using System.Collections.Generic;
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
	/// Interaction logic for IV_at_DefinedResistance.xaml
	/// </summary>
	public partial class IV_at_DefinedResistance : UserControl
	{
        System.Windows.Forms.FolderBrowserDialog dialog;

		public IV_at_DefinedResistance()
		{
            dialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InitializeComponent();
		}

        private void on_cmdOpenClick(object sender, RoutedEventArgs e)
        {
            dialog.ShowDialog();
            Settings.FilePath = dialog.SelectedPath;
        }
	}
}