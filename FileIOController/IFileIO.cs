using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileIOController
{
    public interface IFileIO : IDisposable
    {
        string DataHeader { get; set; }
        string DataFormat { get; set; }
        string ReadDataFile(string FileName);
        void WriteDataAsync(string Data);
        void WriteDataAsync(params double[] Data);
        void DataArrived(string Data);
        void DataArrived(params double[] Data);
    }
}
