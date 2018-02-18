using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    [Serializable]
    public class Noise_DefinedResistanceModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        [field: NonSerializedAttribute()]
        public event PropertyChangedEventHandler PropertyChanged;

        void onPropertyChanged(string PropertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(PropertyName));
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


        double[] scanningVoltageCollection = new double[] { 0.02 };
        public double[] ScanningVoltageCollection
        {
            get { return scanningVoltageCollection; }
            set
            {
                scanningVoltageCollection = value;
                onPropertyChanged("ScanningVoltageCollection");
            }
        }

        private double voltageDeviation;
        public double VoltageDeviation
        {
            get { return voltageDeviation; }
            set
            {
                voltageDeviation = value;
                onPropertyChanged("VoltageDeviation");
            }
        }

        private double voltageDeviationValue = 0.0002;
        public double VoltageDeviationValue
        {
            get { return voltageDeviationValue; }
            set 
            {
                voltageDeviationValue = value;
                onPropertyChanged("VoltageDeviationValue");
            }
        }

        private int voltageDeviationIndex;
        public int VoltageDeviationIndex
        {
            get { return voltageDeviationIndex; }
            set 
            {
                voltageDeviationIndex = value;
                onPropertyChanged("VoltageDeviationIndex");
            }
        }


        private double minVoltageTreshold;
        public double MinVoltageTreshold
        {
            get { return minVoltageTreshold; }
            set
            {
                minVoltageTreshold = value;
                onPropertyChanged("MinVoltageTreshold");
            }
        }

        private double minVoltageTresholdValue = 0.005;
        public double MinVoltageTresholdValue
        {
            get { return minVoltageTresholdValue; }
            set
            {
                minVoltageTresholdValue = value;
                onPropertyChanged("MinVoltageTresholdValue");
            }
        }

        private int minVoltageTresholdIndex;
        public int MinVoltageTresholdIndex
        {
            get { return minVoltageTresholdIndex; }
            set 
            {
                minVoltageTresholdIndex = value;
                onPropertyChanged("MinVoltageTresholdIndex");
            }
        }

        private double voltageTreshold;
        public double VoltageTreshold
        {
            get { return voltageTreshold; }
            set
            {
                voltageTreshold = value;
                onPropertyChanged("VoltageTreshold");
            }
        }

        private double voltageTresholdValue = 0.1;
        public double VoltageTresholdValue
        {
            get { return voltageTresholdValue; }
            set 
            {
                voltageTresholdValue = value;
                onPropertyChanged("VoltageTresholdValue");
            }
        }

        private int voltageTresholdIndex;        
        public int VoltageTresholdIndex
        {
            get { return voltageTresholdIndex; }
            set 
            {
                voltageTresholdIndex = value;
                onPropertyChanged("VoltageTresholdIndex");
            }
        }

        private static double conductanceQuantum = 0.0000774809173;

        private double[] setConductanceCollection = new double[] { 50.0, 45.0, 40.0, 35.0, 30.0, 25.0, 20.0, 15.0, 14.0, 13.0, 12.0, 11.0, 10.0, 9.0, 8.0, 7.0, 6.0, 5.0, 4.0, 3.0, 2.0, 1.0 };
        public double[] SetConductanceCollection
        {
            get { return setConductanceCollection; }
            set
            {
                if (setResistanceCollection.Length != value.Length)
                    setResistanceCollection = new double[value.Length];

                for (int i = 0; i < value.Length; i++)
                    if (value[i] > 0)
                        setResistanceCollection[i] = 1.0 / conductanceQuantum / value[i];
                    else
                        throw new ArgumentException("Conductance value should me greater than zero.");

                setConductanceCollection = value;

                onPropertyChanged("SetConductanceCollection");
                onPropertyChanged("SetResistanceCollection");
            }
        }

        private static double[] setResistanceCollection = new double[] 
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
                if (setConductanceCollection.Length != value.Length)
                    setConductanceCollection = new double[value.Length];
                for (int i = 0; i < value.Length; i++)
                    if (value[i] > 0)
                        setConductanceCollection[i] = 1.0 / conductanceQuantum / value[i];
                    else
                        throw new ArgumentException("Resistance value should me greater than zero.");

                setResistanceCollection = value;

                onPropertyChanged("SetResistanceCollection");
                onPropertyChanged("SetConductanceCollection");
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

                onPropertyChanged("ConductanceDeviation");
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

                onPropertyChanged("MotionMinSpeed");
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

                onPropertyChanged("MotionMaxSpeed");
            }
        }

        private double motorMinPos = 0.0;
        public double MotorMinPos
        {
            get { return motorMinPos; }
            set
            {
                motorMinPos = value;
                onPropertyChanged("MotorMinPos");
            }
        }

        private double motorMaxPos = 15.0;
        public double MotorMaxPos
        {
            get { return motorMaxPos; }
            set
            {
                motorMaxPos = value;
                onPropertyChanged("MotorMaxPos");
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

        private int nAveragesFast = 2;
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

        private int updateNumber = 1;
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

        private double kAmpl = 100.0;
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
