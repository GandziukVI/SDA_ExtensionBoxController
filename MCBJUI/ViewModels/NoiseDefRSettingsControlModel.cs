using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using System.IO;
using ControlAssist;
using CustomControls.ViewModels;
using System.Collections.ObjectModel;

namespace MCBJUI
{
	[Serializable]
	public class NoiseDefRSettingsControlModel : NotifyPropertyChangedBase
	{
		public NoiseDefRSettingsControlModel()
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


        double[] scanningVoltageCollection = new double[] { 0.02 };
        public double[] ScanningVoltageCollection
        {
            get { return scanningVoltageCollection; }
            set
            {
                SetField(ref scanningVoltageCollection, value, "ScanningVoltageCollection");
            }
        }

        private ExtendedDoubleUpDownViewModel voltageDeviation = new ExtendedDoubleUpDownViewModel() { Value = 0.5, MultiplierIndex = 1, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VoltageDeviation
        {
            get { return voltageDeviation; }
            set
            {
                SetField(ref voltageDeviation, value, "VoltageDeviation");
            }
        }


        private ExtendedDoubleUpDownViewModel minVoltageTreshold = new ExtendedDoubleUpDownViewModel() { Value = 5, MultiplierIndex = 1, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel MinVoltageTreshold
        {
            get { return minVoltageTreshold; }
            set
            {
                SetField(ref minVoltageTreshold, value, "MinVoltageTreshold");
            }
        }

        private ExtendedDoubleUpDownViewModel voltageTreshold = new ExtendedDoubleUpDownViewModel() { Value = 100, MultiplierIndex = 1, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VoltageTreshold
        {
            get { return voltageTreshold; }
            set
            {
                SetField(ref voltageTreshold, value, "VoltageTreshold");
            }
        }

        private static double conductanceQuantum = 0.0000774809173;

        private double[] setConductanceCollection = new double[] { 50.0, 45.0, 40.0, 35.0, 30.0, 25.0, 20.0, 15.0, 14.0, 13.0, 12.0, 11.0, 10.0, 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0 };
        public double[] SetConductanceCollection
        {
            get { return setConductanceCollection; }
            set
            {
                var setResistanceCollectionValue = new double[value.Length];

                for (int i = 0; i < value.Length; i++)
                    if (value[i] > 0)
                        setResistanceCollectionValue[i] = 1.0 / conductanceQuantum / value[i];
                    else
                        throw new ArgumentException("Conductance value should me greater than zero.");

                SetField(ref setConductanceCollection, value, "SetConductanceCollection");
                SetField(ref setResistanceCollection, setResistanceCollectionValue, "SetResistanceCollection");
            }
        }

        private double[] setResistanceCollection = new double[]
        {
            1.0 / (50.0 * conductanceQuantum), 
            1.0 / (45.0 * conductanceQuantum),
            1.0 / (40.0 * conductanceQuantum),
            1.0 / (35.0 * conductanceQuantum),
            1.0 / (30.0 * conductanceQuantum),
            1.0 / (25.0 * conductanceQuantum),
            1.0 / (20.0 * conductanceQuantum),
            1.0 / (15.0 * conductanceQuantum),
            1.0 / (14.0 * conductanceQuantum),
            1.0 / (13.0 * conductanceQuantum),
            1.0 / (12.0 * conductanceQuantum),
            1.0 / (11.0 * conductanceQuantum),
            1.0 / (10.0 * conductanceQuantum),
            1.0 / (9.0 * conductanceQuantum),
            1.0 / (8.0 * conductanceQuantum),
            1.0 / (7.0 * conductanceQuantum),
            1.0 / (6.0 * conductanceQuantum),
            1.0 / (5.0 * conductanceQuantum),
            1.0 / (4.0 * conductanceQuantum),
            1.0 / (3.0 * conductanceQuantum),
            1.0 / (2.0 * conductanceQuantum),
            1.0 / (1.0 * conductanceQuantum)
        };

        public double[] SetResistanceCollection
        {
            get { return setResistanceCollection; }
            set
            {
                var setConductanceCollectionValue = new double[value.Length];

                for (int i = 0; i < value.Length; i++)
                    if (value[i] > 0)
                        setConductanceCollectionValue[i] = 1.0 / conductanceQuantum / value[i];
                    else
                        throw new ArgumentException("Resistance value should me greater than zero.");

                SetField(ref setResistanceCollection, value, "SetResistanceCollection");
                SetField(ref setConductanceCollection, setConductanceCollectionValue, "SetConductanceCollection");
            }
        }

        double conductanceDeviation = 3.0;
        public double ConductanceDeviation
        {
            get { return conductanceDeviation; }
            set
            {
                if (value >= 0)
                    conductanceDeviation = value;
                else
                    throw new ArgumentException("The deviation should have positive value.");

                SetField(ref conductanceDeviation, value, "ConductanceDeviation");
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

                SetField(ref stabilizationTime, value, "StabilizationTime");
            }
        }

        private double ampInputResistance = 1e6;
        public double AmpInputResistance
        {
            get { return ampInputResistance; }
            set 
            {
                SetField(ref ampInputResistance, value, "AmpInputResistance");
            }
        }



        private double motionMinSpeed = 0.02;
        public double MotionMinSpeed
        {
            get { return motionMinSpeed; }
            set
            {
                if (value >= 0)
                    motionMinSpeed = value;
                else
                    throw new ArgumentException("The minimum speed should have positive value.");

                SetField(ref motionMinSpeed, value, "MotionMinSpeed");
            }
        }

        private double motionMaxSpeed = 0.04;
        public double MotionMaxSpeed
        {
            get { return motionMaxSpeed; }
            set
            {
                if (value >= 0)
                    motionMaxSpeed = value;
                else
                    throw new ArgumentException("The maximum speed should have positive value.");

                SetField(ref motionMaxSpeed, value, "MotionMaxSpeed");
            }
        }

        private double motorMinPos = 0.0;
        public double MotorMinPos
        {
            get { return motorMinPos; }
            set
            {
                SetField(ref motorMinPos, value, "MotorMinPos");
            }
        }

        private double motorMaxPos = 15.0;
        public double MotorMaxPos
        {
            get { return motorMaxPos; }
            set
            {
                SetField(ref motorMaxPos, value, "MotorMaxPos");
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
                SetField(ref filePath, value, FilePath);
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

                SetField(ref saveFileName, value, "SaveFileName");
            }
        }

        private ObservableCollection<System.Windows.Point> noisePSDData = new ObservableCollection<System.Windows.Point>();
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