using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using SourceMeterUnit;
using Keithley26xx;
using System.IO;
using ControlAssist;
using CustomControls.ViewModels;

namespace FET_Characterization
{
    [Serializable]
    public class FET_IVModel : NotifyPropertyChangedBase
    {
        public FET_IVModel()
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

        // Transfer implementation

        private string transferKeithleyRscName = "GPIB0::26::INSTR";
        public string TransferKeithleyRscName
        {
            get { return transferKeithleyRscName; }
            set
            {
                SetField(ref transferKeithleyRscName, value, "TransferKeithleyRscName");
            }
        }

        private Keithley26xxB_Channels transfer_v_dsChannel = Keithley26xxB_Channels.Channel_A;
        public Keithley26xxB_Channels TransferVdsChannel
        {
            get { return transfer_v_dsChannel; }
            set
            {
                var transferVGChannelValue = Keithley26xxB_Channels.Channel_B;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        transferVGChannelValue = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        transferVGChannelValue = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                SetField(ref transfer_v_dsChannel, value, "TransferVdsChannel");
                SetField(ref transfer_v_gChannel, transferVGChannelValue, "TransferVgChannel");
            }
        }

        private Keithley26xxB_Channels transfer_v_gChannel = Keithley26xxB_Channels.Channel_B;
        public Keithley26xxB_Channels TransferVgChannel
        {
            get { return transfer_v_gChannel; }
            set
            {
                var transferVDSChannelValue = Keithley26xxB_Channels.Channel_A;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        transferVDSChannelValue = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        transferVDSChannelValue = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                SetField(ref transfer_v_gChannel, value, "TransferVgChannel");
                SetField(ref transfer_v_dsChannel, transferVDSChannelValue, "TransferVdsChannel");
            }
        }

        private SMUSourceMode transfer_smu_SourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode TransferSMU_SourceMode
        {
            get { return transfer_smu_SourceMode; }
            set
            {
                SetField(ref transfer_smu_SourceMode, value, "TransferSMU_SourceMode");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_v_dsStart = new ExtendedDoubleUpDownViewModel() { Value = -0.1, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel TransferVdsStart
        {
            get { return transfer_v_dsStart; }
            set
            {
                SetField(ref transfer_v_dsStart, value, "TransferVdsStart");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_v_dsStop = new ExtendedDoubleUpDownViewModel() { Value = -1.0, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel TransferVdsStop
        {
            get { return transfer_v_dsStop; }
            set
            {
                SetField(ref transfer_v_dsStop, value, "TransferVdsStop");
            }
        }       

        private int transfer_n_v_dsStep = 6;
        public int TransferN_VdsStep
        {
            get { return transfer_n_v_dsStep; }
            set
            {
                SetField(ref transfer_n_v_dsStep, value, "TransferN_VdsStep");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_dsCompliance = new ExtendedDoubleUpDownViewModel() { Value = 100, MultiplierIndex = 2, UnitAlias = "A" };
        public ExtendedDoubleUpDownViewModel TransferDS_Complaince
        {
            get { return transfer_dsCompliance; }
            set
            {
                SetField(ref transfer_dsCompliance, value, "TransferDS_Complaince");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_v_gStart = new ExtendedDoubleUpDownViewModel() { Value = 0.0, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel TransferVgStart
        {
            get { return transfer_v_gStart; }
            set
            {
                SetField(ref transfer_v_gStart, value, "TransferVgStart");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_v_gStop = new ExtendedDoubleUpDownViewModel() { Value = -3.5, MultiplierIndex = 0, UnitAlias = "V" };
        public ExtendedDoubleUpDownViewModel TransferVgStop
        {
            get { return transfer_v_gStop; }
            set
            {
                SetField(ref transfer_v_gStop, value, "TransferVgStop");
            }
        }

    
        private int transfer_n_v_gSweep = 101;
        public int TransferN_VgSweep
        {
            get { return transfer_n_v_gSweep; }
            set
            {
                SetField(ref transfer_n_v_gSweep, value, "TransferN_VgSweep");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_gateCompliance = new ExtendedDoubleUpDownViewModel() { Value = 10, MultiplierIndex = 2, UnitAlias = "A" };
        public ExtendedDoubleUpDownViewModel TransferGate_Complaince
        {
            get { return transfer_gateCompliance; }
            set
            {
                SetField(ref transfer_gateCompliance, value, "TransferGate_Complaince");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_pulseWidth = new ExtendedDoubleUpDownViewModel() { Value = 100, MultiplierIndex = 2, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel TransferPulseWidth
        {
            get { return transfer_pulseWidth; }
            set
            {
                SetField(ref transfer_pulseWidth, value, "TransferPulseWidth");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_delayTime = new ExtendedDoubleUpDownViewModel() { Value = 100, MultiplierIndex = 2, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel TransferDelayTime
        {
            get { return transfer_delayTime; }
            set
            {
                SetField(ref transfer_delayTime, value, "TransferDelayTime");
            }
        }

        private string transferFilePath = string.Empty;
        public string TransferDataFilePath
        {
            get
            {
                if (!string.IsNullOrEmpty(transferFilePath))
                    return transferFilePath;
                else 
                    return Directory.GetCurrentDirectory();
            }
            set
            {
                SetField(ref transferFilePath, value, "TransferDataFilePath");
            }
        }

        private string transferFileName = "Ch (1, 1) T #01 Transfer.dat";
        public string Transfer_FileName
        {
            get { return transferFileName; }
            set
            {
                if (!value.EndsWith(".dat"))
                    value += ".dat";

                SetField(ref transferFileName, value, "Transfer_FileName");
            }
        }

        private int ke_Transfer_Averaging = 1;
        public int Ke_Transfer_Averaging
        {
            get { return ke_Transfer_Averaging; }
            set
            {
                SetField(ref ke_Transfer_Averaging, value, "Ke_Transfer_Averaging");
            }
        }

        private double ke_Transfer_NPLC = 1.0;
        public double Ke_Transfer_NPLC
        {
            get { return ke_Transfer_NPLC; }
            set
            {
                if (value < 0.01)
                    value = 0.01;
                else if (value > 25)
                    value = 25;

                SetField(ref ke_Transfer_NPLC, value, "Ke_Transfer_NPLC");
            }
        }

        private ExtendedDoubleUpDownViewModel transfer_VdsDelay = new ExtendedDoubleUpDownViewModel() { Value = 2, MultiplierIndex = 0, UnitAlias = "s" };
        public ExtendedDoubleUpDownViewModel Transfer_VdsDelay
        {
            get { return transfer_VdsDelay; }
            set
            {
                SetField(ref transfer_VdsDelay, value, "Transfer_VdsDelay");
            }
        }
        
        private bool measureLeakage = true;
        public bool MeasureLeakage
        {
            get { return measureLeakage; }
            set
            {
                SetField(ref measureLeakage, value, "MeasureLeakage");
            }
        }
    }
}