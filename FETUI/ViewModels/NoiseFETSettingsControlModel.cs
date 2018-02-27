using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;

using ControlAssist;
using CustomControls.ViewModels;
using System.IO;
using System.Collections.ObjectModel;

namespace FETUI
{
	[Serializable]
	public class NoiseFETSettingsControlModel : NotifyPropertyChangedBase
	{
		public NoiseFETSettingsControlModel()
		{
			
		}

		private string agilentU2542Ares = "USB0::2391::5912::TW54334510::INSTR";
        public string AgilentU2542AResName
        {
            get { return agilentU2542Ares; }
            set
            {
                SetField(ref agilentU2542Ares, value, "AgilentU2542AResName");
            }
        }

        #region Oscilloscope settings       

        private ExtendedDoubleUpDownViewModel oscilloscopeVoltageRange = new ExtendedDoubleUpDownViewModel() { Value = 200, MultiplierIndex = 1, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel OscilloscopeVoltageRange
        {
            get { return oscilloscopeVoltageRange; }
            set 
            {
                SetField(ref oscilloscopeVoltageRange, value, "OscilloscopeVoltageRange");
            }
        }        

        private ExtendedDoubleUpDownViewModel oscilloscopeTimeRange = new ExtendedDoubleUpDownViewModel() { Value = 1.0, MultiplierIndex = 0, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel OscilloscopeTimeRange
        {
            get { return oscilloscopeTimeRange; }
            set 
            {
                SetField(ref oscilloscopeTimeRange, value, "OscilloscopeTimeRange");
            }
        }        

        private int oscilloscopePointsPerGraph = 1000;
        public int OscilloscopePointsPerGraph
        {
            get { return oscilloscopePointsPerGraph; }
            set 
            {
                SetField(ref oscilloscopePointsPerGraph, value, "OscilloscopePointsPerGraph");
            }
        }

        #endregion

        private bool useVoltageControl = true;
        public bool UseVoltageControl
        {
            get { return useVoltageControl; }
            set 
            {
                SetField(ref useVoltageControl, value, "UseVoltageControl");
            }
        }
        

        private bool isTransferCurveMode = true;
        public bool IsTransferCurveMode
        {
            get { return isTransferCurveMode; }
            set 
            {
                SetField(ref isTransferCurveMode, value, "IsTransferCurveMode");
            }
        }

        private bool isOutputCurveMode = false;
        public bool IsOutputCurveMode
        {
            get { return isOutputCurveMode; }
            set
            {
                SetField(ref isOutputCurveMode, value, "IsOutputCurveMode");
            }
        }

        double[] gateVoltageCollection = new double[] { -3.5 };
        public double[] GateVoltageCollection
        {
            get { return gateVoltageCollection; }
            set
            {
                SetField(ref gateVoltageCollection, value, "GateVoltageCollection");
            }
        }

        double[] dsVoltageCollection = new double[] { -0.1 };
        public double[] DSVoltageCollection
        {
            get { return dsVoltageCollection; }
            set
            {
                SetField(ref dsVoltageCollection, value, "DSVoltageCollection");
            }
        }

        private ExtendedDoubleUpDownViewModel voltageDeviation = new ExtendedDoubleUpDownViewModel() { Value = 1, MultiplierIndex = 1, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VoltageDeviation
        {
            get { return voltageDeviation; }
            set
            {
                SetField(ref voltageDeviation, value, "VoltageDeviation");
            }
        }        
        

        private int nAveragesFast = 2;
        public int NAveragesFast
        {
            get { return nAveragesFast; }
            set
            {
                SetField(ref nAveragesFast, value, "NAveragesFast");
            }
        }

        private int nAveragesSlow = 100;
        public int NAveragesSlow
        {
            get { return nAveragesSlow; }
            set
            {
                SetField(ref nAveragesSlow, value, "NAveragesSlow");
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

                SetField(ref stabilizationTime, value, "StabilizationTime");
            }
        }

        private double ampInputResistance = 1000000.0;
        public double AmpInputResistance
        {
            get { return ampInputResistance;}
            set
            {
                 SetField(ref ampInputResistance, value, "AmpInputResistance");
            }
        }

        private double loadResistance = 5000.0;
        public double LoadResistance
        {
            get { return loadResistance; }
            set
            {
                SetField(ref loadResistance, value, "LoadResistance");
            }
        }

        private int samplingFrequency = 500000;
        public int SamplingFrequency
        {
            get { return samplingFrequency; }
            set
            {
                SetField(ref samplingFrequency, value, "SamplingFrequency");
            }
        }

        private int spectraAveraging = 100;
        public int SpectraAveraging
        {
            get { return spectraAveraging; }
            set
            {
                SetField(ref spectraAveraging, value, "SpectraAveraging");
            }
        }

        private int updateNumber = 1;
        public int UpdateNumber
        {
            get { return updateNumber; }
            set
            {
                SetField(ref updateNumber, value, "UpdateNumber");
            }
        }

        private double kPreAmpl = 180.0;
        public double KPreAmpl
        {
            get { return kPreAmpl; }
            set
            {
                SetField(ref kPreAmpl, value, "KPreAmpl");
            }
        }

        private double kAmpl = 100.0;
        public double KAmpl
        {
            get { return kAmpl; }
            set
            {
                SetField(ref kAmpl, value, "KAmpl");
            }
        }

        private double temperature0 = 297.0;
        public double Temperature0
        {
            get { return temperature0; }
            set
            {
                SetField(ref temperature0, value, "Temperature0");
            }
        }

        private double temperatureE = 297.0;
        public double TemperatureE
        {
            get { return temperatureE; }
            set
            {
                SetField(ref temperatureE, value, "TemperatureE");
            }
        }

        private bool recordTimeTraces = false;
        public bool RecordTimeTraces
        {
            get { return recordTimeTraces; }
            set
            {
                SetField(ref recordTimeTraces, value, "RecordTimeTraces");
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
                SetField(ref recordingFrequency, value, "RecordingFrequency");
            }
        }


        private string filePath = Directory.GetCurrentDirectory();
        public string FilePath
        {
            get { return filePath; }
            set
            {
                SetField(ref filePath, value, "FilePath");
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

                SetField(ref saveFileName, value, "SaveFileName");
            }
        }

        private ObservableCollection<System.Windows.Point> noisePSDData;
        public ObservableCollection<System.Windows.Point> NoisePSDData
        {
            get { return noisePSDData; }
            set
            {
                SetField(ref noisePSDData, value, "NoisePSDData");
            }
        }
	}
}