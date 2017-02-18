using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    public class Noise_DefinedResistanceInfo : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        void onPropertyChanged(string PropertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(PropertyName));
        }

        #endregion

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

        private double voltageTreshold = 0.15;
        public double VoltageTreshold
        {
            get { return voltageTreshold; }
            set
            {
                voltageTreshold = value;
                onPropertyChanged("VoltageTreshold");
            }
        }

        private static double conductanceQuantum = 0.0000774809173;

        private double[] setConductanceCollection = new double[] { 1.0 };
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

        private static double[] setResistanceCollection = new double[] { 1.0 / conductanceQuantum };
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

        double conductanceDeviation = 5.0;
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

        private double motionMinSpeed = 150.0;
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

        private double motionMaxSpeed = 300.0;
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
                nAveragesFast = value;
                onPropertyChanged("NAveragesSlow");
            }
        }

        private int samplingFrequency = 200000;
        public int SamplingFrequency
        {
            get { return samplingFrequency; }
            set 
            {
                samplingFrequency = value;
                onPropertyChanged("SamplingFrequency");
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

        private double kPreAmpl;
        public double KPreAmpl
        {
            get { return kPreAmpl; }
            set 
            {
                kPreAmpl = value;
                onPropertyChanged("KPreAmpl");
            }
        }

        private double kAmpl;
        public double KAmpl
        {
            get { return kAmpl; }
            set
            {
                kAmpl = value;
                onPropertyChanged("KAmpl");
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
