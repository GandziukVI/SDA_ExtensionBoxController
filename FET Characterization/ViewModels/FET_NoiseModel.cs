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
    [Serializable]
	public class FET_NoiseModel : INotifyPropertyChanged
	{
		public FET_NoiseModel()
		{
			
		}

		#region INotifyPropertyChanged
        [field:NonSerializedAttribute()]
		public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
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
                NotifyPropertyChanged("AgilentU2542AResName");
            }
        }

        #region Oscilloscope settings       

        private double oscilloscopeVoltageRange;
        public double OscilloscopeVoltageRange
        {
            get { return oscilloscopeVoltageRange; }
            set 
            {
                oscilloscopeVoltageRange = value;
                NotifyPropertyChanged("OscilloscopeVoltageRange");
            }
        }

        private double oscilloscopeVoltageRangeValue = 0.2;
        public double OscilloscopeVoltageRangeValue
        {
            get { return oscilloscopeVoltageRangeValue; }
            set 
            { 
                oscilloscopeVoltageRangeValue = value;
                NotifyPropertyChanged("OscilloscopeVoltageRangeValue");
            }
        }

        private int oscilloscopeVoltageRangeIndex;
        public int OscilloscopeVoltageRangeIndex
        {
            get { return oscilloscopeVoltageRangeIndex; }
            set
            { 
                oscilloscopeVoltageRangeIndex = value;
                NotifyPropertyChanged("OscilloscopeVoltageRangeIndex");
            }
        }

        private double oscilloscopeTimeRange;
        public double OscilloscopeTimeRange
        {
            get { return oscilloscopeTimeRange; }
            set 
            {
                oscilloscopeTimeRange = value;
                NotifyPropertyChanged("OscilloscopeTimeRange");
            }
        }

        private double oscilloscopeTimeRangeValue = 1.0;
        public double OscilloscopeTimeRangeValue
        {
            get { return oscilloscopeTimeRangeValue; }
            set 
            {
                oscilloscopeTimeRangeValue = value;
                NotifyPropertyChanged("OscilloscopeTimeRangeValue");
            }
        }

        private int oscilloscopeTimeRangeIndex;
        public int OscilloscopeTimeRangeIndex
        {
            get { return oscilloscopeTimeRangeIndex; }
            set
            {
                oscilloscopeTimeRangeIndex = value;
                NotifyPropertyChanged("OscilloscopeTimeRangeIndex");
            }
        }

        private int oscilloscopePointsPerGraph;
        public int OscilloscopePointsPerGraph
        {
            get { return oscilloscopePointsPerGraph; }
            set 
            {
                oscilloscopePointsPerGraph = value;
                NotifyPropertyChanged("OscilloscopePointsPerGraph");
            }
        }

        #endregion

        private bool isTransferCurveMode = true;
        public bool IsTransferCurveMode
        {
            get { return isTransferCurveMode; }
            set 
            {
                isTransferCurveMode = value;
                NotifyPropertyChanged("IsTransferCurveMode");
            }
        }

        private bool isOutputCurveMode = false;
        public bool IsOutputCurveMode
        {
            get { return isOutputCurveMode; }
            set
            {
                isOutputCurveMode = value;
                NotifyPropertyChanged("IsOutputCurveMode");
            }
        }

        double[] gateVoltageCollection = new double[] { -3.5 };
        public double[] GateVoltageCollection
        {
            get { return gateVoltageCollection; }
            set
            {
                gateVoltageCollection = value;
                NotifyPropertyChanged("GateVoltageCollection");
            }
        }

        double[] dsVoltageCollection = new double[] { -0.1 };
        public double[] DSVoltageCollection
        {
            get { return dsVoltageCollection; }
            set
            {
                dsVoltageCollection = value;
                NotifyPropertyChanged("DSVoltageCollection");
            }
        }

        private double voltageDeviation;
        public double VoltageDeviation
        {
            get { return voltageDeviation; }
            set
            {
                voltageDeviation = value;
                NotifyPropertyChanged("VoltageDeviation");
            }
        }

        private double voltageDeviationValue = 1.0;
        public double VoltageDeviationValue
        {
            get { return voltageDeviationValue; }
            set 
            {
                voltageDeviationValue = value;
                NotifyPropertyChanged("VoltageDeviationValue");
            }
        }
        
        private int voltageDeviationMultiplierIndex;
        public int VoltageDeviationMultiplierIndex
        {
            get { return voltageDeviationMultiplierIndex; }
            set 
            {
                voltageDeviationMultiplierIndex = value;
                NotifyPropertyChanged("VoltageDeviationMultiplierIndex");
            }
        }
        

        private int nAveragesFast = 2;
        public int NAveragesFast
        {
            get { return nAveragesFast; }
            set
            {
                nAveragesFast = value;
                NotifyPropertyChanged("NAveragesFast");
            }
        }

        private int nAveragesSlow = 100;
        public int NAveragesSlow
        {
            get { return nAveragesSlow; }
            set
            {
                nAveragesSlow = value;
                NotifyPropertyChanged("NAveragesSlow");
            }
        }

        double stabilizationTime = 45.0;
        public double StabilizationTime
        {
            get { return stabilizationTime; }
            set
            {
                if (value >= 0)
                    stabilizationTime = value;
                else
                    throw new ArgumentException("The stabilization time should have positive value.");

                NotifyPropertyChanged("StabilizationTime");
            }
        }

        private double loadResistance = 5000.0;
        public double LoadResistance
        {
            get { return loadResistance; }
            set
            {
                loadResistance = value;
                NotifyPropertyChanged("LoadResistance");
            }
        }

        private int samplingFrequency = 500000;
        public int SamplingFrequency
        {
            get { return samplingFrequency; }
            set
            {
                samplingFrequency = value;
                NotifyPropertyChanged("SamplingFrequency");
            }
        }

        private int spectraAveraging = 100;
        public int SpectraAveraging
        {
            get { return spectraAveraging; }
            set
            {
                spectraAveraging = value;
                NotifyPropertyChanged("SpectraAveraging");
            }
        }

        private int updateNumber = 1;
        public int UpdateNumber
        {
            get { return updateNumber; }
            set
            {
                updateNumber = value;
                NotifyPropertyChanged("UpdateNumber");
            }
        }

        private double kPreAmpl = 178.0;
        public double KPreAmpl
        {
            get { return kPreAmpl; }
            set
            {
                kPreAmpl = value;
                NotifyPropertyChanged("KPreAmpl");
            }
        }

        private double kAmpl = 100.0;
        public double KAmpl
        {
            get { return kAmpl; }
            set
            {
                kAmpl = value;
                NotifyPropertyChanged("KAmpl");
            }
        }

        private double temperature0 = 277.0;
        public double Temperature0
        {
            get { return temperature0; }
            set
            {
                temperature0 = value;
                NotifyPropertyChanged("Temperature0");
            }
        }

        private double temperatureE = 277.0;
        public double TemperatureE
        {
            get { return temperatureE; }
            set
            {
                temperatureE = value;
                NotifyPropertyChanged("TemperatureE");
            }
        }

        private bool recordTimeTraces = false;
        public bool RecordTimeTraces
        {
            get { return recordTimeTraces; }
            set
            {
                recordTimeTraces = value;
                NotifyPropertyChanged("RecordTimeTraces");
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

        private int recordingFrequency = 50000;
        public int RecordingFrequency
        {
            get { return recordingFrequency; }
            set
            {
                value = getClosestValueInArray(value, powersOfTwo);

                recordingFrequency = value;
                NotifyPropertyChanged("RecordingFrequency");
            }
        }


        private string filePath = Directory.GetCurrentDirectory();
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                NotifyPropertyChanged("FilePath");
            }
        }

        private string saveFileName = "Ch (1, 1) T#01.dat";
        public string SaveFileName
        {
            get { return saveFileName; }
            set
            {
                if (!value.EndsWith(".dat"))
                    value += ".dat";

                saveFileName = value;
                NotifyPropertyChanged("SaveFileName");
            }
        }
    }
}