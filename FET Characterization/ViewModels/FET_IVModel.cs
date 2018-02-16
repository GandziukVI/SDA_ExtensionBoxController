using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using SourceMeterUnit;
using Keithley26xx;
using System.IO;

namespace FET_Characterization
{
    [Serializable]
    public class FET_IVModel : INotifyPropertyChanged
    {
        public FET_IVModel()
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

        private string keithleyRscName = "GPIB0::26::INSTR";
        public string KeithleyRscName
        {
            get { return keithleyRscName; }
            set
            {
                keithleyRscName = value;
                NotifyPropertyChanged("KeithleyRscName");
            }
        }

        private Keithley26xxB_Channels v_dsChannel = Keithley26xxB_Channels.Channel_A;
        public Keithley26xxB_Channels VdsChannel
        {
            get { return v_dsChannel; }
            set
            {
                v_dsChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        v_gChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        v_gChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("VdsChannel");
                NotifyPropertyChanged("VgChannel");
            }
        }

        private Keithley26xxB_Channels v_gChannel = Keithley26xxB_Channels.Channel_B;
        public Keithley26xxB_Channels VgChannel
        {
            get { return v_gChannel; }
            set
            {
                v_gChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        v_dsChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        v_dsChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("VgChannel");
                NotifyPropertyChanged("VdsChannel");
            }
        }

        private SMUSourceMode smu_SourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode SMU_SourceMode
        {
            get { return smu_SourceMode; }
            set
            {
                smu_SourceMode = value;
                NotifyPropertyChanged("SMU_SourceMode");
            }
        }

        private double v_dsStart;
        public double VdsStart
        {
            get { return v_dsStart; }
            set
            {
                v_dsStart = value;
                NotifyPropertyChanged("VdsStart");
            }
        }

        private double vdsStartValue = 0.0;
        public double VdsStartValue
        {
            get { return vdsStartValue; }
            set
            {
                vdsStartValue = value;
                NotifyPropertyChanged("VdsStartValue");
            }
        }

        private int vdsStartIndex;
        public int VdsStartIndex
        {
            get { return vdsStartIndex; }
            set
            {
                vdsStartIndex = value;
                NotifyPropertyChanged("VdsStartIndex");
            }
        }

        private double v_dsStop;
        public double VdsStop
        {
            get { return v_dsStop; }
            set
            {
                v_dsStop = value;
                NotifyPropertyChanged("VdsStop");
            }
        }

        private double vdsStopValue = -1.0;
        public double VdsStopValue
        {
            get { return vdsStopValue; }
            set
            {
                vdsStopValue = value;
                NotifyPropertyChanged("VdsStopValue");
            }
        }

        private int vdsStopIndex;
        public int VdsStopIndex
        {
            get { return vdsStopIndex; }
            set
            {
                vdsStopIndex = value;
                NotifyPropertyChanged("VdsStopIndex");
            }
        }

        private int n_v_dsSweep = 101;
        public int N_VdsSweep
        {
            get { return n_v_dsSweep; }
            set
            {
                n_v_dsSweep = value;
                NotifyPropertyChanged("N_VdsSweep");
            }
        }

        private double dsCompliance;
        public double DS_Complaince
        {
            get { return dsCompliance; }
            set
            {
                dsCompliance = value;
                NotifyPropertyChanged("DS_Complaince");
            }
        }

        private double dsComplianceValue = 0.001;
        public double DSComplianceValue
        {
            get { return dsComplianceValue; }
            set
            {
                dsComplianceValue = value;
                NotifyPropertyChanged("DSComplianceValue");
            }
        }

        private int dsComplianceIndex;
        public int DSComplianceIndex
        {
            get { return dsComplianceIndex; }
            set
            {
                dsComplianceIndex = value;
                NotifyPropertyChanged("DSComplianceIndex");
            }
        }

        private double v_gStart;
        public double VgStart
        {
            get { return v_gStart; }
            set
            {
                v_gStart = value;
                NotifyPropertyChanged("VgStart");
            }
        }

        private double vgStartValue = 0.0;
        public double VgStartValue
        {
            get { return vgStartValue; }
            set
            {
                vgStartValue = value;
                NotifyPropertyChanged("VgStartValue");
            }
        }

        private int vgStartIndex;
        public int VgStartIndex
        {
            get { return vgStartIndex; }
            set
            {
                vgStartIndex = value;
                NotifyPropertyChanged("VgStartIndex");
            }
        }

        private double v_gStop;
        public double VgStop
        {
            get { return v_gStop; }
            set
            {
                v_gStop = value;
                NotifyPropertyChanged("VgStop");
            }
        }

        private double vgStopValue = -5.0;
        public double VgStopValue
        {
            get { return vgStopValue; }
            set
            {
                vgStopValue = value;
                NotifyPropertyChanged("VgStopValue");
            }
        }

        private int vgStopIndex;
        public int VgStopIndex
        {
            get { return vgStopIndex; }
            set
            {
                vgStopIndex = value;
                NotifyPropertyChanged("VgStopIndex");
            }
        }


        private int n_v_gStep = 6;
        public int N_VgStep
        {
            get { return n_v_gStep; }
            set
            {
                n_v_gStep = value;
                NotifyPropertyChanged("N_VgStep");
            }
        }

        private double gateCompliance;
        public double Gate_Complaince
        {
            get { return gateCompliance; }
            set
            {
                gateCompliance = value;
                NotifyPropertyChanged("Gate_Complaince");
            }
        }

        private double gateComplianceValue = 0.001;
        public double GateComplianceValue
        {
            get { return gateComplianceValue; }
            set
            {
                gateComplianceValue = value;
                NotifyPropertyChanged("GateComplianceValue");
            }
        }

        private int gateComplianceIndex;
        public int GateComplianceIndex
        {
            get { return gateComplianceIndex; }
            set
            {
                gateComplianceIndex = value;
                NotifyPropertyChanged("GateComplianceIndex");
            }
        }

        private double pulseWidth;
        public double PulseWidth
        {
            get { return pulseWidth; }
            set
            {
                pulseWidth = value;
                NotifyPropertyChanged("PulseWidth");
            }
        }

        private double pulseWidthValue = 0.001;
        public double PulseWidthValue
        {
            get { return pulseWidthValue; }
            set
            {
                pulseWidthValue = value;
                NotifyPropertyChanged("PulseWidthValue");
            }
        }

        private int pulseWidthIndex;
        public int PulseWidthIndex
        {
            get { return pulseWidthIndex; }
            set
            {
                pulseWidthIndex = value;
                NotifyPropertyChanged("PulseWidthIndex");
            }
        }


        private double delayTime;
        public double DelayTime
        {
            get { return delayTime; }
            set
            {
                delayTime = value;
                NotifyPropertyChanged("DelayTime");
            }
        }

        private double delayTimeValue = 0.001;
        public double DelayTimeValue
        {
            get { return delayTimeValue; }
            set
            {
                delayTimeValue = value;
                NotifyPropertyChanged("DelayTimeValue");
            }
        }

        private int delayTimeIndex;
        public int DelayTimeIndex
        {
            get { return delayTimeIndex; }
            set
            {
                delayTimeIndex = value;
                NotifyPropertyChanged("DelayTimeIndex");
            }
        }


        private string ivFET_FilePath = Directory.GetCurrentDirectory();
        public string IV_FET_DataFilePath
        {
            get { return ivFET_FilePath; }
            set
            {
                ivFET_FilePath = value;
                NotifyPropertyChanged("IV_FET_DataFilePath");
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

                ivFileName = value;
                NotifyPropertyChanged("IV_FileName");
            }
        }

        private int ke_IV_FET_Averaging = 1;
        public int Ke_IV_FET_Averaging
        {
            get { return ke_IV_FET_Averaging; }
            set
            {
                ke_IV_FET_Averaging = value;
                NotifyPropertyChanged("Ke_IV_FET_Averaging");
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

                ke_IV_FET_NPLC = value;
                NotifyPropertyChanged("Ke_IV_FET_NPLC");
            }
        }

        private double iv_FET_GateDelay;
        public double IV_FET_GateDelay
        {
            get { return iv_FET_GateDelay; }
            set
            {
                iv_FET_GateDelay = value;
                NotifyPropertyChanged("IV_FET_GateDelay");
            }
        }

        private double ivFETGateDelayValue = 2.0;
        public double IVFETGateDelayValue
        {
            get { return ivFETGateDelayValue; }
            set
            {
                ivFETGateDelayValue = value;
                NotifyPropertyChanged("IVFETGateDelayValue");
            }
        }

        private int ivFETGateDelayIndex;
        public int IVFETGateDelayIndex
        {
            get { return ivFETGateDelayIndex; }
            set
            {
                ivFETGateDelayIndex = value;
                NotifyPropertyChanged("IVFETGateDelayIndex");
            }
        }

        // Transfer implementation

        private string transferKeithleyRscName = "GPIB0::26::INSTR";
        public string TransferKeithleyRscName
        {
            get { return transferKeithleyRscName; }
            set
            {
                transferKeithleyRscName = value;
                NotifyPropertyChanged("TransferKeithleyRscName");
            }
        }

        private Keithley26xxB_Channels transfer_v_dsChannel = Keithley26xxB_Channels.Channel_A;
        public Keithley26xxB_Channels TransferVdsChannel
        {
            get { return transfer_v_dsChannel; }
            set
            {
                transfer_v_dsChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        transfer_v_gChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        transfer_v_gChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("TransferVdsChannel");
                NotifyPropertyChanged("TransferVgChannel");
            }
        }

        private Keithley26xxB_Channels transfer_v_gChannel = Keithley26xxB_Channels.Channel_B;
        public Keithley26xxB_Channels TransferVgChannel
        {
            get { return transfer_v_gChannel; }
            set
            {
                transfer_v_gChannel = value;

                switch (value)
                {
                    case Keithley26xxB_Channels.Channel_A:
                        transfer_v_dsChannel = Keithley26xxB_Channels.Channel_B;
                        break;
                    case Keithley26xxB_Channels.Channel_B:
                        transfer_v_dsChannel = Keithley26xxB_Channels.Channel_A;
                        break;
                    default:
                        break;
                }

                NotifyPropertyChanged("TransferVgChannel");
                NotifyPropertyChanged("TransferVdsChannel");
            }
        }

        private SMUSourceMode transfer_smu_SourceMode = SMUSourceMode.Voltage;
        public SMUSourceMode TransferSMU_SourceMode
        {
            get { return transfer_smu_SourceMode; }
            set
            {
                transfer_smu_SourceMode = value;
                NotifyPropertyChanged("TransferSMU_SourceMode");
            }
        }

        private double transfer_v_dsStart;
        public double TransferVdsStart
        {
            get { return transfer_v_dsStart; }
            set
            {
                transfer_v_dsStart = value;
                NotifyPropertyChanged("TransferVdsStart");
            }
        }

        private double transferVdsStartValue = 0.0;
        public double TransferVdsStartValue
        {
            get { return transferVdsStartValue; }
            set
            {
                transferVdsStartValue = value;
                NotifyPropertyChanged("TransferVdsStartValue");
            }
        }

        private int transferVdsStartIndex;
        public int TransferVdsStartIndex
        {
            get { return transferVdsStartIndex; }
            set
            {
                transferVdsStartIndex = value;
                NotifyPropertyChanged("TransferVdsStartIndex");
            }
        }

        private double transfer_v_dsStop;
        public double TransferVdsStop
        {
            get { return transfer_v_dsStop; }
            set
            {
                transfer_v_dsStop = value;
                NotifyPropertyChanged("TransferVdsStop");
            }
        }

        private double transferVdsStopValue = -1.0;
        public double TransferVdsStopValue
        {
            get { return transferVdsStopValue; }
            set
            {
                transferVdsStopValue = value;
                NotifyPropertyChanged("TransferVdsStopValue");
            }
        }

        private int transferVdsStopIndex;
        public int TransferVdsStopIndex
        {
            get { return transferVdsStopIndex; }
            set
            {
                transferVdsStopIndex = value;
                NotifyPropertyChanged("TransferVdsStopIndex");
            }
        }

        private int transfer_n_v_dsStep = 6;
        public int TransferN_VdsStep
        {
            get { return transfer_n_v_dsStep; }
            set
            {
                transfer_n_v_dsStep = value;
                NotifyPropertyChanged("TransferN_VdsStep");
            }
        }

        private double transfer_dsCompliance;
        public double TransferDS_Complaince
        {
            get { return transfer_dsCompliance; }
            set
            {
                transfer_dsCompliance = value;
                NotifyPropertyChanged("TransferDS_Complaince");
            }
        }

        private double transferDSComplianceValue = 0.001;
        public double TransferDSComplianceValue
        {
            get { return transferDSComplianceValue; }
            set
            {
                transferDSComplianceValue = value;
                NotifyPropertyChanged("TransferDSComplianceValue");
            }
        }

        private int transferDSComplianceIndex;
        public int TransferDSComplianceIndex
        {
            get { return transferDSComplianceIndex; }
            set
            {
                transferDSComplianceIndex = value;
                NotifyPropertyChanged("TransferDSComplianceIndex");
            }
        }
        
        private double transfer_v_gStart;
        public double TransferVgStart
        {
            get { return transfer_v_gStart; }
            set
            {
                transfer_v_gStart = value;
                NotifyPropertyChanged("TransferVgStart");
            }
        }

        private double transferVgStartValue = 0.0;
        public double TransferVgStartValue
        {
            get { return transferVgStartValue; }
            set
            {
                transferVgStartValue = value;
                NotifyPropertyChanged("TransferVgStartValue");
            }
        }

        private int transferVgStartIndex;
        public int TransferVgStartIndex
        {
            get { return transferVgStartIndex; }
            set
            {
                transferVgStartIndex = value;
                NotifyPropertyChanged("TransferVgStartIndex");
            }
        }

        private double transfer_v_gStop;
        public double TransferVgStop
        {
            get { return transfer_v_gStop; }
            set
            {
                transfer_v_gStop = value;
                NotifyPropertyChanged("TransferVgStop");
            }
        }

        private double transferVgStopValue = -5.0;
        public double TransferVgStopValue
        {
            get { return transferVgStopValue; }
            set
            {
                transferVgStopValue = value;
                NotifyPropertyChanged("TransferVgStopValue");
            }
        }

        private int transferVgStopIndex;
        public int TransferVgStopIndex
        {
            get { return transferVgStopIndex; }
            set
            {
                transferVgStopIndex = value;
                NotifyPropertyChanged("TransferVgStopIndex");
            }
        }

        private int transfer_n_v_gSweep = 101;
        public int TransferN_VgSweep
        {
            get { return transfer_n_v_gSweep; }
            set
            {
                transfer_n_v_gSweep = value;
                NotifyPropertyChanged("TransferN_VgSweep");
            }
        }

        private double transfer_gateCompliance;
        public double TransferGate_Complaince
        {
            get { return transfer_gateCompliance; }
            set
            {
                transfer_gateCompliance = value;
                NotifyPropertyChanged("TransferGate_Complaince");
            }
        }

        private double transferGateComplianceValue = 0.001;
        public double TransferGateComplianceValue
        {
            get { return transferGateComplianceValue; }
            set
            {
                transferGateComplianceValue = value;
                NotifyPropertyChanged("TransferGateComplianceValue");
            }
        }

        private int transferGateComplianceIndex;
        public int TransferGateComplianceIndex
        {
            get { return transferGateComplianceIndex; }
            set
            {
                transferGateComplianceIndex = value;
                NotifyPropertyChanged("TransferGateComplianceIndex");
            }
        }

        private double transfer_pulseWidth;
        public double TransferPulseWidth
        {
            get { return transfer_pulseWidth; }
            set
            {
                transfer_pulseWidth = value;
                NotifyPropertyChanged("TransferPulseWidth");
            }
        }

        private double transferPulseWidthValue = 0.001;
        public double TransferPulseWidthValue
        {
            get { return transferPulseWidthValue; }
            set 
            {
                transferPulseWidthValue = value;
                NotifyPropertyChanged("TransferPulseWidthValue");
            }
        }

        private int transferPulseWidthIndex;
        public int TransferPulseWidthIndex
        {
            get { return transferPulseWidthIndex; }
            set
            {
                transferPulseWidthIndex = value;
                NotifyPropertyChanged("TransferPulseWidthIndex");
            }
        }

        private double transfer_delayTime;
        public double TransferDelayTime
        {
            get { return transfer_delayTime; }
            set
            {
                transfer_delayTime = value;
                NotifyPropertyChanged("TransferDelayTime");
            }
        }

        private double transferDelayTimeValue = 0.001;
        public double TransferDelayTimeValue
        {
            get { return transferDelayTimeValue; }
            set 
            {
                transferDelayTimeValue = value;
                NotifyPropertyChanged("TransferDelayTimeValue");
            }
        }

        private int transferDelayTimeIndex;
        public int TransferDelayTimeIndex
        {
            get { return transferDelayTimeIndex; }
            set
            {
                transferDelayTimeIndex = value;
                NotifyPropertyChanged("TransferDelayTimeIndex");
            }
        }
        

        private string transferFilePath = Directory.GetCurrentDirectory();
        public string TransferDataFilePath
        {
            get { return transferFilePath; }
            set
            {
                transferFilePath = value;
                NotifyPropertyChanged("TransferDataFilePath");
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

                transferFileName = value;
                NotifyPropertyChanged("Transfer_FileName");
            }
        }

        private int ke_Transfer_Averaging = 1;
        public int Ke_Transfer_Averaging
        {
            get { return ke_Transfer_Averaging; }
            set
            {
                ke_Transfer_Averaging = value;
                NotifyPropertyChanged("Ke_Transfer_Averaging");
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

                ke_Transfer_NPLC = value;
                NotifyPropertyChanged("Ke_Transfer_NPLC");
            }
        }

        private double transfer_VdsDelay;
        public double Transfer_VdsDelay
        {
            get { return transfer_VdsDelay; }
            set
            {
                transfer_VdsDelay = value;
                NotifyPropertyChanged("Transfer_VdsDelay");
            }
        }

        private double transferVdsDelayValue = 2.0;
        public double TransferVdsDelayValue
        {
            get { return transferVdsDelayValue; }
            set 
            {
                transferVdsDelayValue = value;
                NotifyPropertyChanged("TransferVdsDelayValue");
            }
        }

        private int transferVdsDelayIndex;
        public int TransferVdsDelayIndex
        {
            get { return transferVdsDelayIndex; }
            set 
            {
                transferVdsDelayIndex = value;
                NotifyPropertyChanged("TransferVdsDelayIndex");
            }
        }

        private bool measureLeakage = true;
        public bool MeasureLeakage
        {
            get { return measureLeakage; }
            set
            {
                measureLeakage = value;
                NotifyPropertyChanged("MeasureLeakage");
            }
        }
    }
}