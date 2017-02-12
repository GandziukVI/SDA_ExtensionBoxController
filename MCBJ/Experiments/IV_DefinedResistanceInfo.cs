using SourceMeterUnit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBJ.Experiments
{
    public class IV_DefinedResistanceInfo : INotifyPropertyChanged
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

        private SMUSourceMode sourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode SourceMode
        {
            get { return sourceMode; }
            set
            {
                sourceMode = value;
                onPropertyChanged("SourceMode");
            }
        }

        private double ivMinValue = -0.05;
        public double IVMinvalue
        {
            get { return ivMinValue; }
            set
            {
                ivMinValue = value;
                onPropertyChanged("IVMinvalue");
            }
        }

        private double ivMaxValue = 0.05;
        public double IVMaxvalue
        {
            get { return ivMaxValue; }
            set
            {
                ivMaxValue = value;
                onPropertyChanged("IVMaxvalue");
            }
        }

        private double epsilon = 0.01;
        public double Epsilon
        {
            get { return epsilon; }
            set
            {
                if (value >= 0 && value <= ivMaxValue)
                    epsilon = value;
                else if (value > ivMaxValue)
                    throw new ArgumentException("The epsilon value must be lower or equal than IVMaxValue.");
                else
                    throw new ArgumentException("The epsilon should have a positive value.");

                onPropertyChanged("Epsilon");
            }
        }

        private int nPoints = 101;
        public int NPoints
        {
            get { return nPoints; }
            set
            {
                if (value > 0)
                    nPoints = value;
                else
                    throw new ArgumentException("The NPoints should have a positive value.");

                onPropertyChanged("NPoints");
            }
        }

        private int nCycles = 1;
        public int NCycles
        {
            get { return nCycles; }
            set
            {
                if (NCycles > 0)
                    nCycles = value;
                else
                    throw new ArgumentException("The NCycles should have a positive value.");

                onPropertyChanged("NCycles");
            }
        }

        private double compliance = 0.00001;
        public double Compliance
        {
            get { return compliance; }
            set 
            {
                compliance = Math.Abs(value);

                onPropertyChanged("CurrentComplinace");
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
