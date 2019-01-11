using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments.DataHandling
{    
    public class ExperimentLog
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }

        public void WriteToLog(string messageType, string messageString)
        {
            if(string.IsNullOrEmpty(FilePath))
                FilePath = Directory.GetCurrentDirectory();
            if(string.IsNullOrEmpty(FileName))
                FileName = "ExperimentLog.txt";

            var path = FilePath + "\\" + FileName;
            var mode = FileMode.Append;
            if(!File.Exists(path))
                mode = FileMode.Create;

            using(var fs = new FileStream(path, mode, FileAccess.Write))
                using(var sw = new StreamWriter(fs))
                    sw.WriteLine(string.Format("{0}\t{1}", messageType, messageString));                            
        }
    }
}
