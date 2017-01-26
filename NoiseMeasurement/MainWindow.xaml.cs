using NationalInstruments.Analysis;
using NationalInstruments.Analysis.Conversion;
using NationalInstruments.Analysis.Dsp;
using NationalInstruments.Analysis.Dsp.Filters;
using NationalInstruments.Analysis.Math;
using NationalInstruments.Analysis.Monitoring;
using NationalInstruments.Analysis.SignalGeneration;
using NationalInstruments.Analysis.SpectralMeasurements;
using NationalInstruments;
//using NationalInstruments.NI4882;
using NationalInstruments.VisaNS;
using NationalInstruments.NetworkVariable;
using NationalInstruments.NetworkVariable.WindowsForms;
using NationalInstruments.Tdms;
using NationalInstruments.Controls;
using NationalInstruments.Controls.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
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

using Agilent_ExtensionBox;
using Agilent_ExtensionBox.IO;
using Agilent_ExtensionBox.Internal;
using System.IO;
using System.Globalization;
using ExperimentController;
using NoiseMeasurement.Experiments;
using D3HighPerformanceODS;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Threading;

namespace NoiseMeasurement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EnumerableDataSource<Point> ds;
        LinkedList<Point> dList;

        public MainWindow()
        {
            InitializeComponent();

            //ds = new ObsLLPointDataSource(100000);
            //ds.UpdateFrequency = 100000;

            dList = new LinkedList<Point>();
            ds = new EnumerableDataSource<Point>(dList);
            ds.SetXYMapping(p => p);

            var psdGraph = new LineGraph(ds);
            psdGraph.AddToPlotter(myChart);
            myChart.Viewport.FitToView();

            var a = new DefResistanceNoise() as IExperiment;

            a.DataArrived += a_DataArrived;
            a.Start();
        }

        void a_DataArrived(object sender, ExpDataArrivedEventArgs e)
        {
            var ts = new ParameterizedThreadStart(addDataToPlot);
            var th = new Thread(ts);

            th.Start(e.Data);
        }

        private void addDataToPlot(object DataString)
        {
            var data = (string)DataString;
            var query = from dataPoint in data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        select Array.ConvertAll(dataPoint.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries), element => double.Parse(element, NumberFormatInfo.InvariantInfo));

            foreach (var item in query)
                dList.AddLast(new Point(item[0] + 1.0, item[1]));
            //ds.AppendAsync(Dispatcher, new Point(item[0] + 1.0, item[1]));

            Dispatcher.InvokeAsync(new Action(() => 
            {
                ds.RaiseDataChanged();
            }));
        }
    }
}
