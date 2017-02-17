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

        double scanningVoltage = 0.02;
        public double ScanningVoltage
        {
            get { return scanningVoltage; }
            set
            {
                scanningVoltage = value;
                onPropertyChanged("ScanningVoltage");
            }
        }

        private static double conductanceQuantum = 0.0000774809173;

        private double setConductance = 1.0;
        public double SetConductance
        {
            get { return setConductance; }
            set
            {
                if (value > 0)
                    setResistance = 1.0 / conductanceQuantum / value;
                else
                    throw new ArgumentException("Conductance value should me greater than zero.");

                setConductance = value;

                onPropertyChanged("SetConductance");
                onPropertyChanged("SetResistance");
            }
        }


        private static double setResistance = 1.0 / conductanceQuantum;
        public double SetResistance
        {
            get { return setResistance; }
            set
            {
                if (value > 0)
                    setConductance = 1.0 / conductanceQuantum / value;
                else
                    throw new ArgumentException("Resistance value should me greater than zero.");

                setResistance = value;

                onPropertyChanged("SetResistance");
                onPropertyChanged("SetConductance");
            }
        }

        double deviation = 5.0;
        public double Deviation
        {
            get { return deviation; }
            set
            {
                if (value >= 0)
                    deviation = value;
                else
                    throw new ArgumentException("The deviation should have positive value.");

                onPropertyChanged("Deviation");
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

        private double minSpeed = 150.0;
        public double MinSpeed
        {
            get { return minSpeed; }
            set
            {
                if (value >= 0)
                    minSpeed = value;
                else
                    throw new ArgumentException("The minimum speed should have positive value.");

                onPropertyChanged("MinSpeed");
            }
        }


        private double maxSpeed = 300.0;
        public double MaxSpeed
        {
            get { return maxSpeed; }
            set
            {
                if (value >= 0)
                    maxSpeed = value;
                else
                    throw new ArgumentException("The maximum speed should have positive value.");

                onPropertyChanged("MaxSpeed");
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

        private int nAveragesFast=  2;

        public int NAveragesFast
        {
            get { return nAveragesFast; }
            set 
            {
                nAveragesFast = value;
                onPropertyChanged("NAveragesFast");
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

        private string saveFileName = "I-V data.dat";
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
