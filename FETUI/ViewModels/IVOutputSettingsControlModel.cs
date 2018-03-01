using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using ControlAssist;
using CustomControls.ViewModels;
using System.IO;
using Keithley26xx;
using SourceMeterUnit;

namespace FETUI
{
	[Serializable]
	public class IVOutputSettingsControlModel : NotifyPropertyChangedBase
	{
        public IVOutputSettingsControlModel()
		{
		}
		
		private string keithleyRscName = "GPIB0::26::INSTR";
        public string KeithleyRscName
        {
            get { return keithleyRscName; }
            set
            {
                SetField(ref keithleyRscName, value, "KeithleyRscName");
            }
        }

        private Keithley26xxB_Channels v_dsChannel = Keithley26xxB_Channels.Channel_A;
        public Keithley26xxB_Channels VdsChannel
        {
            get { return v_dsChannel; }
            set
            {                
                var vgChannelValue = Keithley26xxB_Channels.Channel_B;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        vgChannelValue = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        vgChannelValue = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                SetField(ref v_dsChannel, value, "VdsChannel");
                SetField(ref v_gChannel, vgChannelValue, "VgChannel");
            }
        }

        private Keithley26xxB_Channels v_gChannel = Keithley26xxB_Channels.Channel_B;
        public Keithley26xxB_Channels VgChannel
        {
            get { return v_gChannel; }
            set
            {
                var vdsChannelValue = Keithley26xxB_Channels.Channel_A;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        vdsChannelValue = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        vdsChannelValue = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                SetField(ref v_gChannel, value, "VgChannel");
                SetField(ref v_dsChannel, vdsChannelValue, "VdsChannel");
            }
        }

        private SMUSourceMode smu_SourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode SMU_SourceMode
        {
            get { return smu_SourceMode; }
            set
            {
                var vdsStartValue = VdsStart;
                var vdsStopValue = VdsStop;

                var dsComplianceValue = DS_Complaince;

                switch (value)
                {
                    case SMUSourceMode.Voltage:
                       {
                            vdsStartValue.UnitAlias = "V";
                            vdsStopValue.UnitAlias = "V";

                            dsComplianceValue.UnitAlias = "A";
                       }
                    break;
                    case SMUSourceMode.Current:
                    {
                        vdsStartValue.UnitAlias = "A";
                        vdsStopValue.UnitAlias = "A";

                        dsComplianceValue.UnitAlias = "V";
                    }
                    break;
                }

                SetField(ref v_dsStart, vdsStartValue, "VdsStart");
                SetField(ref v_dsStop, vdsStopValue, "VdsStop");

                SetField(ref dsCompliance, dsComplianceValue, "DS_Complaince");

                SetField(ref smu_SourceMode, value, "SMU_SourceMode");
            }
        }

        private ExtendedDoubleUpDownViewModel v_dsStart = new ExtendedDoubleUpDownViewModel() { Value = 0.0, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VdsStart
        {
            get { return v_dsStart; }
            set
            {
                SetField(ref v_dsStart, value, "VdsStart");
            }
        }       

        private ExtendedDoubleUpDownViewModel v_dsStop = new ExtendedDoubleUpDownViewModel() { Value = -1.0, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VdsStop
        {
            get { return v_dsStop; }
            set
            {
                SetField(ref v_dsStop, value, "VdsStop");
            }
        }       

        private int n_v_dsSweep = 101;
        public int N_VdsSweep
        {
            get { return n_v_dsSweep; }
            set
            {
                SetField(ref n_v_dsSweep, value, "N_VdsSweep");        
            }
        }

        private ExtendedDoubleUpDownViewModel dsCompliance = new ExtendedDoubleUpDownViewModel() { Value = 100.0, MultiplierIndex = 2, UnitAlias = "A" };
        public ExtendedDoubleUpDownViewModel DS_Complaince
        {
            get { return dsCompliance; }
            set
            {
                SetField(ref dsCompliance, value, "DS_Complaince");
            }
        }

        private ExtendedDoubleUpDownViewModel v_gStart = new ExtendedDoubleUpDownViewModel() { Value = -1.5, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VgStart
        {
            get { return v_gStart; }
            set
            {
                SetField(ref v_gStart, value, "VgStart");
            }
        }

        private ExtendedDoubleUpDownViewModel v_gStop = new ExtendedDoubleUpDownViewModel() { Value = -3.5, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel VgStop
        {
            get { return v_gStop; }
            set
            {
                SetField(ref v_gStop, value, "VgStop");
            }
        }    

        private int n_v_gStep = 6;
        public int N_VgStep
        {
            get { return n_v_gStep; }
            set
            {
                SetField(ref n_v_gStep, value, "N_VgStep");
            }
        }

        private ExtendedDoubleUpDownViewModel gateCompliance = new ExtendedDoubleUpDownViewModel() { Value = 10, MultiplierIndex = 2, UnitAlias = "A" };
        public ExtendedDoubleUpDownViewModel Gate_Complaince
        {
            get { return gateCompliance; }
            set
            {
                SetField(ref gateCompliance, value, "Gate_Complaince");
            }
        }        

        private ExtendedDoubleUpDownViewModel pulseWidth = new ExtendedDoubleUpDownViewModel() { Value = 100, MultiplierIndex = 2, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel PulseWidth
        {
            get { return pulseWidth; }
            set
            {
                SetField(ref pulseWidth, value, "PulseWidth");
            }
        }       


        private ExtendedDoubleUpDownViewModel delayTime = new ExtendedDoubleUpDownViewModel() { Value = 100, MultiplierIndex = 2, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel DelayTime
        {
            get { return delayTime; }
            set
            {
                SetField(ref delayTime, value, "DelayTime");
            }
        }        

        private string ivFET_FilePath;
        public string IV_FET_DataFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(ivFET_FilePath))
                    return ivFET_FilePath;
                else
                    return Directory.GetCurrentDirectory();
            }
            set
            {
                SetField(ref ivFET_FilePath, value, "IV_FET_DataFilePath");
            }
        }

        private string ivFileName = "Ch (1, 1) T #01 IV FG.dat";
        public string IV_FileName
        {
            get { return ivFileName; }
            set
            {
                if (!value.EndsWith(".dat"))
                    value += ".dat";

                SetField(ref ivFileName, value, "IV_FileName");
            }
        }

        private int ke_IV_FET_Averaging = 1;
        public int Ke_IV_FET_Averaging
        {
            get { return ke_IV_FET_Averaging; }
            set
            {
                SetField(ref ke_IV_FET_Averaging, value, "Ke_IV_FET_Averaging");
            }
        }

        private double ke_IV_FET_NPLC = 1.0;
        public double Ke_IV_FET_NPLC
        {
            get { return ke_IV_FET_NPLC; }
            set
            {
                if (value < 0.01)
                    value = 0.01;
                else if (value > 25)
                    value = 25;

                SetField(ref ke_IV_FET_NPLC, value, "Ke_IV_FET_NPLC");
            }
        }

        private ExtendedDoubleUpDownViewModel iv_FET_GateDelay = new ExtendedDoubleUpDownViewModel() { Value = 2, MultiplierIndex = 0, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel IV_FET_GateDelay
        {
            get { return iv_FET_GateDelay; }
            set
            {
                SetField(ref iv_FET_GateDelay, value, "IV_FET_GateDelay");
            }
        }       
	}
}