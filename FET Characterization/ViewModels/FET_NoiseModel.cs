using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Axes.Numeric;
using System.IO;

namespace FET_Characterization
{
	public class FET_NoiseModel : INotifyPropertyChanged
	{
		public FET_NoiseModel()
		{
			
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

        private string agilentU2542Ares = "USB0::2391::5912::TW54334510::INSTR";
        public string AgilentU2542AResName
        {
            get { return agilentU2542Ares; }
            set
            {
                agilentU2542Ares = value;
                onPropertyChanged("AgilentU2542AResName");
            }
        }

        private bool isTransferCurveMode = true;
        public bool IsTransferCurveMode
        {
            get { return isTransferCurveMode; }
            set 
            {
                isTransferCurveMode = value;
                onPropertyChanged("IsTransferCurveMode");
            }
        }

        private bool isOutputCurveMode = false;
        public bool IsOutputCurveMode
        {
            get { return isOutputCurveMode; }
            set
            {
                isOutputCurveMode = value;
                onPropertyChanged("IsOutputCurveMode");
            }
        }

        double[] gateVoltageCollection = new double[] { 0.02 };
        public double[] GateVoltageCollection
        {
            get { return gateVoltageCollection; }
            set
            {
                gateVoltageCollection = value;
                onPropertyChanged("GateVoltageCollection");
            }
        }

        double[] dsVoltageCollection = new double[] { 0.02 };
        public double[] DSVoltageCollection
        {
            get { return dsVoltageCollection; }
            set
            {
                dsVoltageCollection = value;
                onPropertyChanged("DSVoltageCollection");
            }
        }

        private double voltageDeviation = 0.0002;
        public double VoltageDeviation
        {
            get { return voltageDeviation; }
            set
            {
                voltageDeviation = value;
                onPropertyChanged("VoltageDeviation");
            }
        }

        private int nAveragesFast = 1;
        public int NAveragesFast
        {
            get { return nAveragesFast; }
            set
            {
                nAveragesFast = value;
                onPropertyChanged("NAveragesFast");
            }
        }

        private int nAveragesSlow = 100;
        public int NAveragesSlow
        {
            get { return nAveragesSlow; }
            set
            {
                nAveragesSlow = value;
                onPropertyChanged("NAveragesSlow");
            }
        }

        double stabilizationTime = 30.0;
        public double StabilizationTime
        {
            get { return stabilizationTime; }
            set
            {
                if (value >= 0)
                    stabilizationTime = value;
                else
                    throw new ArgumentException("The stabilization time should have positive value.");

                onPropertyChanged("StabilizationTime");
            }
        }

        private double loadResistance = 5000.0;
        public double LoadResistance
        {
            get { return loadResistance; }
            set
            {
                loadResistance = value;
                onPropertyChanged("LoadResistance");
            }
        }

        private int samplingFrequency = 262144;
        public int SamplingFrequency
        {
            get { return samplingFrequency; }
            set
            {
                samplingFrequency = value;
                onPropertyChanged("SamplingFrequency");
            }
        }

        private int nSubSamples = 1;
        public int NSubSamples
        {
            get { return nSubSamples; }
            set
            {
                nSubSamples = value;
                onPropertyChanged("NSubSamples");
            }
        }

        private int spectraAveraging = 100;
        public int SpectraAveraging
        {
            get { return spectraAveraging; }
            set
            {
                spectraAveraging = value;
                onPropertyChanged("SpectraAveraging");
            }
        }

        private int updateNumber = 10;
        public int UpdateNumber
        {
            get { return updateNumber; }
            set
            {
                updateNumber = value;
                onPropertyChanged("UpdateNumber");
            }
        }

        private double kPreAmpl = 178.0;
        public double KPreAmpl
        {
            get { return kPreAmpl; }
            set
            {
                kPreAmpl = value;
                onPropertyChanged("KPreAmpl");
            }
        }

        private double kAmpl = 10.0;
        public double KAmpl
        {
            get { return kAmpl; }
            set
            {
                kAmpl = value;
                onPropertyChanged("KAmpl");
            }
        }

        private double temperature0 = 277.0;
        public double Temperature0
        {
            get { return temperature0; }
            set
            {
                temperature0 = value;
                onPropertyChanged("Temperature0");
            }
        }

        private double temperatureE = 277.0;
        public double TemperatureE
        {
            get { return temperatureE; }
            set
            {
                temperatureE = value;
                onPropertyChanged("TemperatureE");
            }
        }

        private bool recordTimeTraces = false;
        public bool RecordTimeTraces
        {
            get { return recordTimeTraces; }
            set
            {
                recordTimeTraces = value;
                onPropertyChanged("RecordTimeTraces");
            }
        }

        int[] powersOfTwo = { 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536, 131072, 262144 };
        private int getClosestValueInArray(int value, int[] arr)
        {
            var query = (from val in arr
                         select new
                         {
                             diff = Math.Abs(val - value),
                             arrElem = val
                         }).OrderBy(c => c.diff).First();

            return query.arrElem;
        }

        private int recordingFrequency = 262144;
        public int RecordingFrequency
        {
            get { return recordingFrequency; }
            set
            {
                value = getClosestValueInArray(value, powersOfTwo);

                recordingFrequency = value;
                onPropertyChanged("RecordingFrequency");
            }
        }



        private string filePath = Directory.GetCurrentDirectory();
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                onPropertyChanged("FilePath");
            }
        }

        private string saveFileName = "Noise data.dat";
        public string SaveFileName
        {
            get { return saveFileName; }
            set
            {
                if (!value.EndsWith(".dat"))
                    value += ".dat";

                saveFileName = value;
                onPropertyChanged("SaveFileName");
            }
        }
    }
}