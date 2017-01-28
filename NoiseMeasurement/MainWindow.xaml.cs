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

            var r = new Random();

            var x = new Point[1000];
            for (int i = 0; i < x.Length; i++)
                x[i] = new Point(i + 1, r.NextDouble());


            var y = SelectNPointsPerDecade(ref x, 100);


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
            dList.Clear();

            var data = (string)DataString;
            var query = from dataPoint in data.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                        select Array.ConvertAll(dataPoint.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries), element => double.Parse(element, NumberFormatInfo.InvariantInfo));

            foreach (var item in query)
                dList.AddLast(new Point(item[0] + 1.0, item[1]));

            Dispatcher.InvokeAsync(new Action(() =>
            {
                ds.RaiseDataChanged();
            }));
        }

        double[] SelectPoints(ref double[] arr, int N)
        {
            return arr.Where((value, index) => index % N == 0).ToArray();
        }

        Point[] SelectPoints(ref Point[] arr, int N)
        {
            return arr.Where((value, index) => index % N == 0).ToArray();
        }

        Point[] SelectNPointsPerDecade(ref Point[] arr, int N)
        {
            var minFreq = arr[0].X;
            var maxFreq = arr[arr.Length - 1].X;

            var nDecades = (int)Math.Log10(maxFreq);

            var res = new Point[] { };

            for (int i = 0; i < nDecades; i++)
            {
                var minXPerDecade = arr[arr.Length / nDecades * i].X;
                var maxXPerDecade = arr[arr.Length / nDecades * (i + 1)].X;

                var selectRangeArr = (from p in arr
                                      where p.X >= minXPerDecade && p.X < maxXPerDecade
                                      select p).ToArray();

                if (selectRangeArr.Length > N)
                {
                    var factor = (int)(Math.Ceiling((double)selectRangeArr.Length / N));
                    selectRangeArr = SelectPoints(ref selectRangeArr, factor);
                }

                res = res.Concat(selectRangeArr).ToArray();
            }


            return res;
        }
    }

}
