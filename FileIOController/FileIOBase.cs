using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileIOController
{
    public class FileIOBase : IFileIO
    {
        private string dataHeader = "";
        public string DataHeader
        {
            get { return dataHeader; }
            set { dataHeader = value; }
        }

        private string dataFormat = "";
        public string DataFormat
        {
            get { return dataFormat; }
            set { dataFormat = value; }
        }

        public virtual string ReadDataFile(string FileName)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteDataAsync(string Data)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteDataAsync(params double[] Data)
        {
            throw new NotImplementedException();
        }

        public void DataArrived(string Data)
        {
            WriteDataAsync(Data);
        }

        public void DataArrived(params double[] Data)
        {
            WriteDataAsync(Data);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
