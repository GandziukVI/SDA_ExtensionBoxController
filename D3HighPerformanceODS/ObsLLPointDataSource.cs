using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Threading;

using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Threading;

namespace D3HighPerformanceODS
{
    public class ObsLLPointDataSource : IPointDataSource, INotifyPropertyChanged
    {
        private int counter = 0;

        private int updateFrequency = 1;
        public int UpdateFrequency
        {
            get { return updateFrequency; }
            set { updateFrequency = value; }
        }


        private int collectionSize = 1000;
        public int CollectionSize
        {
            get { return collectionSize; }
            set { collectionSize = value; }
        }

        public LinkedList<Point> Data { get; private set; }
        public EnumerableDataSource<Point> DataSource { get; private set; }

        private static object DataLocker = new object();

        public ObsLLPointDataSource()
        {
            Data = new LinkedList<Point>();
            DataSource = new EnumerableDataSource<Point>(Data);

            DataSource.SetXYMapping(p => p);
        }

        public ObsLLPointDataSource(int ColSize)
            : this()
        {
            collectionSize = ColSize;
        }

        public event EventHandler DataChanged;

        public IPointEnumerator GetEnumerator(DependencyObject context)
        {
            return DataSource.GetEnumerator(context);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task OnPropertyChanged(PropertyChangedEventArgs e)
        {
            await Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var handler = PropertyChanged;
                if (handler != null)
                    handler(this, e);
            }));
        }
        private async void OnPropertyChanged(string PropertyName)
        {
            await OnPropertyChanged(new PropertyChangedEventArgs(PropertyName));
        }

        #endregion

        public void Clear()
        {
            Data.Clear();
        }

        public async Task AddLastAsync(Dispatcher d, Point p)
        {
            if (Data.Count > collectionSize)
                Data.RemoveFirst();

            Data.AddLast(p);

            if (counter % updateFrequency == 0)
            {
                counter = 0;

                await d.InvokeAsync(new Action(() =>
                {
                    DataChanged(this, new EventArgs());
                }));
            }

            Interlocked.Increment(ref counter);
        }

        public async Task AddRangeAsync(Dispatcher d, IEnumerable<Point> collection)
        {
            var len = collection.Count();

            if (len > Data.Count)
                for (int i = 0; i < len - Data.Count; i++)
                    Data.RemoveFirst();

            foreach (var item in collection)
                Data.AddLast(item);

            await d.InvokeAsync(new Action(() =>
                {
                    DataChanged(this, new EventArgs());
                }));
        }

        public async Task SetDataRangeAsync(Dispatcher d, IEnumerable<Point> collection)
        {
            Data.Clear();

            foreach (var item in collection)
                Data.AddLast(item);            

            await d.InvokeAsync(new Action(() =>
            {
                DataChanged(this, new EventArgs());
            }));
        }
    }
}
