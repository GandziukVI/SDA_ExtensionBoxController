using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using ControlAssist;
using System.IO;
using SourceMeterUnit;

namespace MCBJUI
{
	[Serializable]
	public class IVDefRSettingsControlModel : NotifyPropertyChangedBase
	{
		public IVDefRSettingsControlModel()
		{
			
		}

		double scanningVoltage = 0.02;
        public double ScanningVoltage
        {
            get { return scanningVoltage; }
            set
            {
                SetField(ref scanningVoltage, value, "ScanningVoltage");
            }
        }

        private static double conductanceQuantum = 0.0000774809173;

        private double setConductance = 1.0;
        public double SetConductance
        {
            get { return setConductance; }
            set
            {
				var setResistanceValue = 0.0;
                if (value > 0)
                    setResistanceValue = 1.0 / conductanceQuantum / value;
                else
                    throw new ArgumentException("Conductance value should me greater than zero.");


				SetField(ref setConductance, value, "SetConductance");
				SetField(ref setResistance, setResistanceValue, "SetResistance");
            }
        }


        private static double setResistance = 1.0 / conductanceQuantum;
        public double SetResistance
        {
            get { return setResistance; }
            set
            {
				var setConductanceValue = 0.0;
                if (value > 0)
                    setConductanceValue = 1.0 / conductanceQuantum / value;
                else
                    throw new ArgumentException("Resistance value should me greater than zero.");

				SetField(ref setResistance, value, "SetResistance");
				SetField(ref setConductance, setConductanceValue, "SetConductance");
            }
        }

        double deviation = 5.0;
        public double Deviation
        {
            get { return deviation; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The deviation should have positive value.");

                SetField(ref deviation, value, "Deviation");
            }
        }

        double stabilizationTime = 30.0;
        public double StabilizationTime
        {
            get { return stabilizationTime; }
            set
            {
                if(value < 0)
                    throw new ArgumentException("The stabilization time should have positive value.");

                SetField(ref stabilizationTime, value, "StabilizationTime");
            }
        }

        private double minSpeed = 0.049;
        public double MinSpeed
        {
            get { return minSpeed; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The minimum speed should have positive value.");

                SetField(ref minSpeed, value, "MinSpeed");
            }
        }


        private double maxSpeed = 0.098;
        public double MaxSpeed
        {
            get { return maxSpeed; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The maximum speed should have positive value.");

                SetField(ref maxSpeed, value, "MaxSpeed");
            }
        }

        private SMUSourceMode sourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode SourceMode
        {
            get { return sourceMode; }
            set
            {
                SetField(ref sourceMode, value, "SourceMode");
            }
        }

        private double ivMinValue = -0.05;
        public double IVMinValue
        {
            get { return ivMinValue; }
            set
            {
                SetField(ref ivMinValue, value, "IVMinValue");
            }
        }

        private double ivMaxValue = 0.05;
        public double IVMaxValue
        {
            get { return ivMaxValue; }
            set
            {
                SetField(ref ivMaxValue, value, "IVMaxValue");
            }
        }

        private double epsilon = 0.01;
        public double Epsilon
        {
            get { return epsilon; }
            set
            {
				var epsilonValue = 0.0;
                if (value >= 0 && value <= ivMaxValue)
                    epsilonValue = value;
                else if (value > ivMaxValue)
                    throw new ArgumentException("The epsilon value must be lower or equal than IVMaxValue.");
                else
                    throw new ArgumentException("The epsilon should have a positive value.");

                SetField(ref epsilon, epsilonValue, "Epsilon");
            }
        }

        private int nPoints = 101;
        public int NPoints
        {
            get { return nPoints; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The NPoints should have a positive value.");

                SetField(ref nPoints, value, "NPoints");
            }
        }

        private int nCycles = 1;
        public int NCycles
        {
            get { return nCycles; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("The NCycles should have a positive value.");

                SetField(ref nCycles, value, "NCycles");
            }
        }

        private double compliance = 0.00001;
        public double Compliance
        {
            get { return compliance; }
            set
            {
                SetField(ref compliance, Math.Abs(value), "Compliance");
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

        private string filePath = Directory.GetCurrentDirectory();
        public string FilePath
        {
            get { return filePath; }
            set
            {
                SetField(ref filePath, value, "FilePath");
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

                SetField(ref saveFileName, value, "SaveFileName");
            }
        }
	}
}